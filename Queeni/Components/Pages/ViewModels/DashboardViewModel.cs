using AutoMapper;
using Queeni.Components.Library.AI;
using Queeni.Components.Library.Enumerations;
using Queeni.Components.Library.Extensions;
using Queeni.Components.Library.Interfaces;
using Queeni.Components.Library.Services;
using Queeni.Components.Models;
using Queeni.Components.Pages.ViewModels;
using Queeni.Data.Interfaces;
using Queeni.Data.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using IO.Ably;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenAI;
using OpenAI.Chat;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.InteractiveChat;
using Syncfusion.Blazor.Kanban;
using Syncfusion.Blazor.RichTextEditor;
using Syncfusion.Blazor.Spinner;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatMessage = OpenAI.Chat.ChatMessage;
using Model = Queeni.Data.Models.TaskModel;
using ViewModel = Queeni.Components.Models.TaskViewModel;

namespace Queeni.Components.Pages.ViewModels
{
    public partial class DashboardViewModel : GenericBaseViewModel<Model, ViewModel>
    {
        [ObservableProperty]
        private bool isOpen = false;

        [ObservableProperty]
        private string isClosed = string.Empty;

        [ObservableProperty]
        private string selectedDisplayDate = string.Empty;

        [ObservableProperty]
        private bool showAIAssistant = false;

        public Models.CategoryViewModel SelectedCategory = new Models.CategoryViewModel();

        //public ObservableCollection<DataFile> DataFiles = new ObservableCollection<DataFile>();

        public ObservableCollection<Models.CategoryViewModel> Categories = new ObservableCollection<Models.CategoryViewModel>();

        public ObservableCollection<ColumnModel> Columns = new ObservableCollection<ColumnModel>();

        public ObservableCollection<string> Colors = new ObservableCollection<string>();
        public SfKanban<TaskViewModel> KanbanControl { get; set; } = null;

        private readonly IOpenAIConversation _conversation;

        public DashboardViewModel(IMapper mapper, IRealtimeSyncService sync, IUowData data, IOpenAIConversation conversation)
            : base(mapper, sync, data, data.Tasks)
        {
            _conversation = conversation;

            sync.InitAsync();
            sync.MessageReceived += Sync_MessageReceived;

            var colorType = typeof(Colors); // <-- ВАЖНО: Microsoft.Maui.Graphics.Colors
            var fields = colorType.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                Colors.Add(field.Name);
            }
        }

        private async void Sync_MessageReceived(object? sender, SyncMessage message)
        {
            var item = message.Item;
            var isCategory = false;
            if (item.Id > 0 && item.CategoryId == 0)
                isCategory = true;

            // update logic
            switch (message.MessageType)
            {
                case Library.Enumerations.MessageTypes.ReadOnly:
                    {
                        if (isCategory)
                        {
                            Categories.Add(new CategoryViewModel
                            {
                                Id = item.Id,
                                Title = item.Title,
                            });
                        }
                        else
                        {
                            Items.Add(item);
                        }
                    }
                    break;
                case Library.Enumerations.MessageTypes.AddOrUpdate:
                    {
                        if (isCategory)
                        {

                            SelectedCategory = new CategoryViewModel()
                            {
                                Title = message.Item.Title
                            };
                            await CreateCategoryAsync(false);
                        }
                        else
                        {
                            await CreateOrUpdate(_mapper.Map<ViewModel>(message.Item), false);
                        }
                    }
                    break;
                case Library.Enumerations.MessageTypes.Delete:
                    {
                        Context.Delete(message.Id);
                        Data.SaveChanges();
                    }
                    break;
            }
        }

        public async Task<bool> LoadData(bool loadTasks = false)
        {
            await AppCache.BusyIndicator.RunAsync(async () =>
            {
                Categories.Clear();
                Columns.Clear();

                var result = await Data.Categories.All();
                if (result != null)
                {
                    Categories.AddRange(_mapper.Map<List<CategoryViewModel>>(result));
                    foreach (var item in Categories)
                    {
                        Columns.Add(new ColumnModel()
                        {
                            HeaderText = item.Title,
                            KeyField = new List<string>()
                            {
                                item.Id.ToString()
                            },
                            AllowToggle = true
                        });
                    }
                    await ReadMulti();
                }
            }, "Loading tasks...");
            return true;
        }
        public async Task SaveChanges()
        {
            if (KanbanControl != null && Items.Any())
            {
                await App.SaveChanges();
            }
        }

        #region Kanban
        public async Task AddCategory()
        {
            SelectedCategory = new Models.CategoryViewModel();
            OpenDialog();
        }

        public async Task AddTask()
        {
            if (KanbanControl != null)
            {
                await KanbanControl.OpenDialogAsync(CurrentAction.Add, new TaskViewModel());
            }
        }

        public async void ActionCompleteHandler(ActionEventArgs<TaskViewModel> args)
        {
            switch (args.RequestType)
            {
                case "cardAdd":
                    {
                        await AddOrUpdate(args.AddedRecords, true);
                    }
                    break;
                case "cardChange":
                    {
                        await AddOrUpdate(args.ChangedRecords, true);
                    }
                    break;
                case "cardRemove":
                    {
                        await AppCache.BusyIndicator.RunAsync(async () =>
                        {
                            foreach (var item in args.DeletedRecords)
                            {
                                Context.Delete(item.Id);
                            }
                            Data.SaveChanges();
                        }, "Saving data...");
                    }
                    break;

            }
        }

        private async Task AddOrUpdate(IEnumerable<ViewModel> records, bool isNotifySend)
        {
            await AppCache.BusyIndicator.RunAsync(async () =>
            {
                foreach (var item in records)
                {
                    await CreateOrUpdate(item, isNotifySend);
                }
                Data.SaveChanges();
            }, "Saving data...");
        }

        #endregion

        #region Dialog
        public void OpenDialog()
        {
            IsOpen = true;
        }

        public async Task SaveCategory()
        {
            await AppCache.BusyIndicator.RunAsync(async () =>
            {
                var model = CreateCategoryAsync(true);

                IsOpen = false;
                this.IsClosed = "Ok Clicked";
            }, "Saving data...");
        }

        private async Task<CategoryModel> CreateCategoryAsync(bool isNotifySend)
        {
            var model = _mapper.Map<Data.Models.CategoryModel>(SelectedCategory);
            Data.Categories.Add(model);
            Data.SaveChanges();
            SelectedCategory = new CategoryViewModel();

            if (model.Id > 0)
            {
                Columns.Add(new ColumnModel()
                {
                    HeaderText = model.Title,
                    KeyField = new List<string>()
                            {
                                model.Id.ToString()
                            },
                    AllowToggle = true
                });
                Categories.Add(_mapper.Map<CategoryViewModel>(model));

                if (isNotifySend)
                {
                    await _sync.NotifyItemCreatedOrUpdateAsync(new ViewModel()
                    {
                        Id = model.Id,
                        Title = model.Title,
                    });
                }
            }

            return model;
        }

        public void CancelSaveCategory()
        {
            IsOpen = false;
            this.IsClosed = "Cancel Clicked";
        }

        #endregion

        public override async Task<ViewModel> CreateOrUpdate(ViewModel model, bool isNotifySend, string id = null)
        {
            if (string.IsNullOrEmpty(model.Priority))
                model.Priority = "Blue";

            var result = await base.CreateOrUpdate(model, isNotifySend, id);
            var x = Items.FirstOrDefault(x => x.Id == result.Id);
            if (x == null)
            {
                Items.Add(_mapper.Map<ViewModel>(result));
            }
            else
            {
                _mapper.Map(result, x);
            }

            if (isNotifySend)
                await _sync.NotifyItemCreatedOrUpdateAsync(result);

            return result;
        }
        internal override async Task<Model> CreateOrUpdateData(ViewModel item, string id)
        {
            var model = _mapper.Map<Model>(item);
            Context.AddOrUpdate(model);
            Data.SaveChanges();
            return model;
        }

        protected override Task<bool> DeleteData(ViewModel model)
        {
            throw new NotImplementedException();
        }

        public async Task PromptRequest(AssistViewPromptRequestedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(AppCache.Settings.OpenAiApiKey))
                {
                    args.Response = "please set openai api key first";
                    return;
                }

                Environment.SetEnvironmentVariable("OPENAI_API_KEY", AppCache.Settings.OpenAiApiKey);
                var assistant = new OpenAIAssistant(Environment.GetEnvironmentVariable("OPENAI_API_KEY"), _mapper, _conversation);
                assistant.SetCategories(Categories);

                var result = await assistant.CallOpenAiAsync(args.Prompt,
                    async (categoryVm) =>
                    {
                        SelectedCategory = categoryVm;
                        var result = await CreateCategoryAsync(true);
                        assistant.SetCategories(Categories);
                        return result;
                    },
                    async (taskVm) =>
                    {
                        return await CreateOrUpdate(taskVm, true);
                    });
                // await CallOpenAiAsync(args.Prompt);
                string htmlResult = Markdown.ToHtml(result); // Markdown → HTML

                args.Response = htmlResult;
                //args.Response = result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public async Task<string?> CallOpenAiAsync(string userInput)
        //{
        //    Environment.SetEnvironmentVariable("OPENAI_API_KEY", "");
        //    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        //    var url = "https://api.openai.com/v1/chat/completions";

        //    var categoriesString = string.Join(", ", Categories.Select(c => $"id={c.Id},title={c.Title}"));

        //    var systemMessage = $"You are a task management assistant. Recognize requests to create categories or tasks. Аlways reply short in a playful, friendly, and slightly cheeky tone. Use fun metaphors or emojis where appropriate." +
        //            $"Current categories: {categoriesString}. " +
        //            "If the user is simply chatting, respond normally in the language the user is currently using. ";

        //    var inputMessages = new List<object>
        //        {
        //            new { role = "system", content = systemMessage },
        //            new { role = "user", content = userInput }
        //        };

        //    var tools = ChatTools.GetOpenAIFunctions();

        //    var requestBody = new
        //    {
        //        model = "gpt-4.1-mini-2025-04-14",
        //        temperature = 0.8,
        //        messages = inputMessages,
        //        tools = new[]
        //        {
        //            ChatTools.GetCreateCategoryFunction()
        //        }
        //    };

        //    using var httpClient = new HttpClient();
        //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        //    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync(url, content);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        var error = await response.Content.ReadAsStringAsync();
        //        return $"❌ OpenAI API error: {error}";
        //    }

        //    var responseString = await response.Content.ReadAsStringAsync();
        //    using var doc = JsonDocument.Parse(responseString);
        //    var root = doc.RootElement;

        //    var message = root.GetProperty("choices")[0].GetProperty("message");
        //    inputMessages.Add(message);

        //    if (message.TryGetProperty("tool_calls", out var toolCalls) && toolCalls.GetArrayLength() > 0)
        //    {
        //        foreach (var toolCall in toolCalls.EnumerateArray())
        //        {
        //            var functionName = toolCall.GetProperty("function").GetProperty("name").GetString();
        //            var arguments = toolCall.GetProperty("function").GetProperty("arguments").GetRawText();

        //            if (functionName == "CreateCategory")
        //            {
        //                var innerJson = JsonSerializer.Deserialize<string>(arguments);
        //                var args = JsonSerializer.Deserialize<CreateCategoryArgs>(innerJson);
        //                SelectedCategory = new CategoryViewModel { Title = args!.name };
        //                var model = await CreateCategoryAsync(true);

        //                bool isCreated = model.Id > 0;
        //                inputMessages.Add(new
        //                {
        //                    role = "tool",
        //                    tool_call_id = toolCall.GetProperty("id").GetString(),
        //                    content = JsonSerializer.Serialize(new { success = isCreated, message = isCreated ? $"Category '{model.Title}' created." : "Failed to create category." })
        //                });
        //            }
        //            else if (functionName == "CreateTask")
        //            {
        //                var args = JsonSerializer.Deserialize<TaskViewModel>(arguments);
        //                if (args == null || string.IsNullOrWhiteSpace(args.Title) || string.IsNullOrWhiteSpace(args.Description))
        //                {
        //                    inputMessages.Add(new
        //                    {
        //                        role = "tool",
        //                        tool_call_id = toolCall.GetProperty("id").GetString(),
        //                        content = JsonSerializer.Serialize(new { success = false, message = "Missing title or description." })
        //                    });
        //                }

        //                int? categoryId = Categories.FirstOrDefault(c => c.Title.Equals(args.CategoryTitle, StringComparison.OrdinalIgnoreCase))?.Id;

        //                if (categoryId == null)
        //                {
        //                    inputMessages.Add(new
        //                    {
        //                        role = "tool",
        //                        tool_call_id = toolCall.GetProperty("id").GetString(),
        //                        content = JsonSerializer.Serialize(new { success = false, message = $"Category '{args.CategoryTitle}' not found." })
        //                    });
        //                }

        //                args.CategoryId = categoryId.Value;
        //                var taskResult = await CreateOrUpdate(_mapper.Map<ViewModel>(args), true);
        //                inputMessages.Add(new
        //                {
        //                    role = "tool",
        //                    tool_call_id = toolCall.GetProperty("id").GetString(),
        //                    content = JsonSerializer.Serialize(new { success = taskResult.Id > 0, message = $"Task '{taskResult.Title}' created." })
        //                });
        //            }
        //        }

        //        var backContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        //        var backResponse = await httpClient.PostAsync(url, backContent);
        //        if (!backResponse.IsSuccessStatusCode)
        //        {
        //            var error = await backResponse.Content.ReadAsStringAsync();
        //            return $"❌ OpenAI API error: {error}";
        //        }
        //        var backResponseString = await backResponse.Content.ReadAsStringAsync();
        //        var decodedContent = Encoding.UTF8.GetString(Encoding.Default.GetBytes(backResponseString));
        //        var rootResult = JsonSerializer.Deserialize<JsonElement>(decodedContent);

        //        // Извличане на 'content' от съобщението
        //        var contentResultString = rootResult
        //            .GetProperty("choices")[0]
        //            .GetProperty("message")
        //            .GetProperty("content")
        //            .GetString();

        //        try
        //        {
        //            var jsonContent = JsonSerializer.Deserialize<JsonElement>(contentResultString);

        //            // Ако е JSON, ще можеш да извлечеш резултата от него
        //            if (jsonContent.TryGetProperty("result", out var resultProperty))
        //            {
        //                return resultProperty.GetString(); // Връщаш резултата от JSON
        //            }
        //            else
        //            {
        //                return contentResultString; // Ако няма поле "result", просто връщаш текста
        //            }
        //        }
        //        catch (JsonException)
        //        {
        //            // Ако не е JSON, връщаш текста директно
        //            return contentResultString;
        //        }
        //    }
        //    return message.GetProperty("content").GetString();
        //}

        //private class CreateCategoryArgs
        //{
        //    public string name { get; set; } = "";
        //}


        //public async Task<string?> CallOpenAiAsync(string userInput)
        //{
        //    Environment.SetEnvironmentVariable("OPENAI_API_KEY", "");
        //    ChatClient client = new(model: "gpt-4.1", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

        //    var categoriesString = string.Join(", ", Categories.Select(c => $"id={c.Id},title={c.Title}"));

        //    var systemMessage = $"You are a task management assistant. Recognize requests to create categories or tasks and respond with JSON using the appropriate tools. " +
        //            $"Current categories: {categoriesString}. " +
        //            "If the user is simply chatting, respond normally in the language the user is currently using. " +
        //            "Match the language of the user's latest message — if they write in English, respond in English; if they write in Bulgarian, respond in Bulgarian; etc.";


        //    // Съобщения
        //    var messages = new List<ChatMessage>
        //    {
        //        ChatMessage.CreateSystemMessage(systemMessage),
        //        ChatMessage.CreateUserMessage(userInput)
        //    };

        //    ChatCompletionOptions options = new()
        //    {
        //        Tools = { ChatTools.getCreateCategoryTool, ChatTools.getCreateTaskTool }
        //    };

        //    bool requiresAction;

        //    do
        //    {
        //        requiresAction = false;
        //        ChatCompletion completion = client.CompleteChat(messages, options);

        //        switch (completion.FinishReason)
        //        {
        //            case ChatFinishReason.Stop:
        //                {
        //                    // Add the assistant message to the conversation history.
        //                    messages.Add(new AssistantChatMessage(completion));
        //                    break;
        //                }

        //            case ChatFinishReason.ToolCalls:
        //                {
        //                    // First, add the assistant message with tool calls to the conversation history.
        //                    messages.Add(new AssistantChatMessage(completion));

        //                    // Then, add a new tool message for each tool call that is resolved.
        //                    foreach (ChatToolCall toolCall in completion.ToolCalls)
        //                    {
        //                        switch (toolCall.FunctionName.ToEnum<FunctionNames>())
        //                        {
        //                            case FunctionNames.CreateCategory:
        //                                {
        //                                    await HandleCreateCategoryToolAsync(toolCall, messages);
        //                                    break;
        //                                }

        //                            case FunctionNames.CreateTask:
        //                                {
        //                                    using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
        //                                    var json = argumentsJson.RootElement.GetRawText();

        //                                    var jsonOptions = new JsonSerializerOptions
        //                                    {
        //                                        PropertyNameCaseInsensitive = true
        //                                    };
        //                                    var taskModel = JsonSerializer.Deserialize<TaskViewModel>(json, jsonOptions);


        //                                    if (taskModel == null || string.IsNullOrEmpty(taskModel.Title) || string.IsNullOrEmpty(taskModel.Description) || string.IsNullOrEmpty(taskModel.CategoryTitle))
        //                                    {
        //                                        throw new ArgumentNullException("title, description or category", "Both title and description are required.");
        //                                    }

        //                                    int? categoryId = null;
        //                                    if (!string.IsNullOrEmpty(taskModel.CategoryTitle))
        //                                    {
        //                                        string categoryName = taskModel.CategoryTitle;
        //                                        // Match the category name (assuming categories is a dictionary or list)
        //                                        categoryId = Categories.FirstOrDefault(c => c.Title.Equals(categoryName, StringComparison.OrdinalIgnoreCase))?.Id;

        //                                        if (categoryId == null)
        //                                        {
        //                                            // If no match, ask the user whether to create a new category or assign it to a default category
        //                                            messages.Add(new ToolChatMessage(
        //                                                toolCall.Id,
        //                                                $"Category '{categoryName}' not found. Would you like to create a new category with this name, or assign it to the 'Other' category?"
        //                                            ));
        //                                        }
        //                                    }

        //                                    var taskResult = await CreateOrUpdate(_mapper.Map<ViewModel>(taskModel), true);
        //                                    bool isCreated = taskResult.Id > 0;
        //                                    messages.Add(new ToolChatMessage(toolCall.Id, isCreated.ToString()));

        //                                    break;
        //                                }
        //                            default:
        //                                {
        //                                    // Handle other unexpected calls.
        //                                    throw new NotImplementedException();
        //                                }
        //                        }
        //                    }

        //                    requiresAction = true;
        //                    break;
        //                }

        //            case ChatFinishReason.Length:
        //                throw new NotImplementedException("Incomplete model output due to MaxTokens parameter or token limit exceeded.");

        //            case ChatFinishReason.ContentFilter:
        //                throw new NotImplementedException("Omitted content due to a content filter flag.");

        //            case ChatFinishReason.FunctionCall:
        //                throw new NotImplementedException("Deprecated in favor of tool calls.");

        //            default:
        //                throw new NotImplementedException(completion.FinishReason.ToString());
        //        }
        //    } while (requiresAction);

        //    var result = messages.ToResult<AssistantChatMessage>();
        //    return result;
        //    //var messages = new List<ChatMessage>
        //    //{
        //    //    new SystemChatMessage("You are a helpful assistant."),
        //    //    new UserChatMessage("Създай категория Домашни")
        //    //};

        //    //ChatCompletion completion = client.CompleteChat(prompt);

        //    //if (result.IsCommand)
        //    //{
        //    //    var rawJson = completion.Content[0].Text.Trim();
        //    //    try
        //    //    {
        //    //        var response = JsonSerializer.Deserialize<TaskViewModel>(rawJson, new JsonSerializerOptions
        //    //        {
        //    //            PropertyNameCaseInsensitive = true
        //    //        });

        //    //        if (result != null)
        //    //        {
        //    //            //if (result.MatchedCommand.Type == CommandDefinitionTypes.Category)
        //    //            //{
        //    //            //    await AddCategory
        //    //            //}
        //    //            //else if (result.MatchedCommand.Type == CommandDefinitionTypes.Task)
        //    //            //{
        //    //            //    response.CategoryId = 1;
        //    //            //    await AddOrUpdate(new List<ViewModel>() { response });
        //    //            //}
        //    //        }

        //    //        return "done";
        //    //    }
        //    //    catch (JsonException ex)
        //    //    {
        //    //        Console.WriteLine("❌ Failed to parse JSON:");
        //    //        Console.WriteLine(rawJson);
        //    //        Console.WriteLine(ex);
        //    //        return null;
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    return completion.Content[0].Text.Trim();
        //    //}

        //}

        private async Task HandleCreateCategoryToolAsync(ChatToolCall toolCall, List<ChatMessage> messages)
        {
            using JsonDocument argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);
            string name = argumentsJson.RootElement.GetProperty("name").GetString() ?? string.Empty;

            SelectedCategory = new CategoryViewModel
            {
                Title = name
            };

            try
            {
                var model = await CreateCategoryAsync(true);
                bool isCreated = model.Id > 0;
                messages.Add(new ToolChatMessage(toolCall.Id, $"Category={model.Id}:{model.Title}"));
            }
            catch (Exception ex)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, $"ERROR: {ex.Message}\n{ex.StackTrace}"));
            }
        }

        public void ToggleAIAssistant()
        {
            ShowAIAssistant = !ShowAIAssistant;
        }
    }
}

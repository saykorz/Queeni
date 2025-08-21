using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using IO.Ably;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OpenAI;
using OpenAI.Chat;
using Queeni.Components.Library.AI;
using Queeni.Components.Library.Enumerations;
using Queeni.Components.Library.Extensions;
using Queeni.Components.Library.Interfaces;
using Queeni.Components.Library.Services;
using Queeni.Components.Models;
using Queeni.Components.Pages.ViewModels;
using Queeni.Data;
using Queeni.Data.Interfaces;
using Queeni.Data.Models;
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
using System.Text.RegularExpressions;
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
        private readonly BusyIndicatorService _busyIndicator;

        public DashboardViewModel(IMapper mapper, IRealtimeSyncService sync, IUowData data, IOpenAIConversation conversation, BusyIndicatorService busyIndicator)
            : base(mapper, sync, data, data.Tasks)
        {
            _conversation = conversation;
            _busyIndicator = busyIndicator;

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
            await _busyIndicator.RunAsync(async () =>
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
                await App.SaveChanges(_busyIndicator);
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
                        await _busyIndicator.RunAsync(async () =>
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
            await _busyIndicator.RunAsync(async () =>
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
            await _busyIndicator.RunAsync(async () =>
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
                    // Check if user included API key in the response
                    if (!string.IsNullOrEmpty(args.Prompt) && args.Prompt.StartsWith("sk-"))
                    {
                        try
                        {
                            var isValid = await TestApiKeyWithOpenAI(args.Prompt);
                            if (isValid)
                            {
                                AppCache.Settings.OpenAiApiKey = args.Prompt;
                                await QueeniConfigManager.SaveAsync(AppCache.Settings);

                                args.Response = "API key has been successfully saved! Welcome to Queeni!";
                                return;
                            }
                            else
                            {
                                args.Response = "Invalid API key format. Please use format: OPENAI_API_KEY=sk-xxxx";
                                return;
                            }
                        }
                        catch
                        {
                            args.Response = "Error processing API key. Please try again.";
                            return;
                        }
                    }

                    // Show instructions if no key provided
                    args.Response = @"
        <div class='api-key-instructions'>
            <p>To use this feature, please enter your OpenAI API key:</p>
            <p><strong>Format:</strong> <code>OPENAI_API_KEY=sk-gdhw456757itufghgf33e</code></p>
            <br/>
            <p>How to get your API key:</p>
            <ol>
                <li>Go to <a href='https://platform.openai.com' target='_blank'>platform.openai.com</a></li>
                <li>Log in or create an account</li>
                <li>Click your profile → 'View API keys'</li>
                <li>Click 'Create new secret key'</li>
                <li>Copy the key and paste it here</li>
            </ol>
        </div>";
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

        private async Task<bool> TestApiKeyWithOpenAI(string apiKey)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                // Тестова заявка към OpenAI API (много лека - листване на модели)
                var response = await httpClient.GetAsync("https://api.openai.com/v1/models");

                // Успех ако получим 200 OK
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
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

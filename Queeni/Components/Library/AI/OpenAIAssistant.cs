using AutoMapper;
using Queeni.Components.Library.Extensions;
using Queeni.Components.Library.Interfaces;
using Queeni.Components.Models;
using Queeni.Data.Models;
using IO.Ably;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.Maui.Core.Carousel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.AI
{
    public class OpenAIAssistant
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly IOpenAIConversation _conversation;
        private readonly List<CategoryViewModel> _categories = new();
        private readonly IMapper _mapper;

        public OpenAIAssistant(string apiKey, IMapper mapper, IOpenAIConversation conversation)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _mapper = mapper;
            _conversation = conversation;
        }

        public async Task<string> CallOpenAiAsync(string userInput, Func<CategoryViewModel, Task<CategoryModel>> createCategoryAsync, Func<TaskViewModel, Task<TaskViewModel>> createTaskAsync)
        {
            UpdateSystemMessage();
            _conversation.AddUserMessage(userInput);

            var tools = new[] {
                ChatTools.GetDeviceLocationAsync(),
                ChatTools.GetCreateCategoryResponsesFunction(),
                ChatTools.GetCreateTaskResponsesFunction()
            };

            var response = await SendRequestAsync(createCategoryAsync, createTaskAsync, tools);

            //response += await ToolResponses(tools);
            return response;
        }

        private async Task<string> SendRequestAsync(Func<CategoryViewModel, Task<CategoryModel>> createCategoryAsync, Func<TaskViewModel, Task<TaskViewModel>> createTaskAsync, object[] tools)
        {
            var isFinal = false;
            var messagesToShow = new List<string>();

            try
            {
                var requestBody = new
                {
                    model = "gpt-4.1-mini-2025-04-14",//"gpt-4o-mini-2024-07-18",
                    // 
                    temperature = 0.8,
                    input = _conversation.Messages,
                    tools
                };

                dynamic responseContent = await PostToOpenAIAsync(requestBody);
                foreach(var toolCall in responseContent.output)
                {
                    var type = toolCall.type;
                    if (type == "message")
                    {
                        var contents = toolCall.content;
                        foreach (var part in contents)
                        {
                            if (part.type == "output_text")
                            {
                                messagesToShow.Add(part.text.ToString());
                            }
                        }

                        if (toolCall.status == "completed")
                        {
                            isFinal = true;
                        }
                    }
                    else if (type == "function_call")
                    {
                        var functionName = toolCall.name.ToString();
                        var arguments = toolCall.arguments.ToString();
                        var call_id = toolCall.call_id.ToString();

                        _conversation.Messages.Add(new
                        {
                            type = "function_call",
                            call_id = call_id,
                            name = functionName,
                            arguments = arguments
                        });

                        if (functionName == "CreateCategory")
                        {
                            await HandleCreateCategoryAsync(createCategoryAsync, arguments, call_id);
                            UpdateSystemMessage();
                        }
                        else if (functionName == "CreateTask")
                        {
                            await HandleCreateTaskAsync(createTaskAsync, arguments, call_id);
                        }
                    }

                }

                //var toolCalls = responseContent.GetProperty("output");
                //foreach (var toolCall in toolCalls.EnumerateArray())
                //{
                //    var type = toolCall.GetProperty("type").GetString();
                //    if (type == "message")
                //    {
                //        var content = toolCall.GetProperty("content").EnumerateArray();
                //        foreach (var part in content)
                //        {
                //            if (part.GetProperty("type").GetString() == "output_text")
                //            {
                //                messagesToShow.Add(part.GetProperty("text").GetString());
                //            }
                //        }

                //        var status = toolCall.GetProperty("status").GetString();
                //        if (status == "completed")
                //        {
                //            isFinal = true;
                //        }
                //    }
                //    else if (type == "function_call")
                //    {
                //        var functionName = toolCall.GetProperty("name").GetString();
                //        var arguments = toolCall.GetProperty("arguments").GetRawText();
                //        var call_id = toolCall.GetProperty("call_id").GetString();

                //        _messages.Add(new
                //        {
                //            type = "function_call",
                //            call_id = call_id,
                //            name = functionName,
                //            arguments = arguments
                //        });

                //        if (functionName == "CreateCategory")
                //        {
                //            await HandleCreateCategoryAsync(createCategoryAsync, arguments, call_id);
                //            UpdateSystemMessage();
                //        }
                //        else if (functionName == "CreateTask")
                //        {
                //            await HandleCreateTaskAsync(createTaskAsync, arguments, call_id);
                //        }
                //    }
                //}

                if (!isFinal)
                {
                    var finalMessage = await SendRequestAsync(createCategoryAsync, createTaskAsync, tools);
                    messagesToShow.Add(finalMessage);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return string.Join("\n\n", messagesToShow);
        }

        //private async Task<string> ToolResponses(object[] tools)
        //{
        //    var secondRequestBody = new
        //    {
        //        model = "gpt-4.1-mini-2025-04-14",
        //        temperature = 0.8,
        //        input = _messages,
        //        tools
        //    };

        //    var backResponse = await PostToOpenAIAsync(secondRequestBody);
        //    return ExtractTextFromOpenAIResponse(backResponse);
        //}

        private async Task<dynamic> PostToOpenAIAsync(object body)
        {
            try
            {
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/responses", content);

                var responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"OpenAI API error: {response.StatusCode} - {responseString}");
                return JsonConvert.DeserializeObject<dynamic>(responseString);

                //return JsonDocument.Parse(responseString).RootElement;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateSystemMessage()
        {
            try
            {
                var categoriesString = string.Join("|", _categories.Select(c => $"id={c.Id},title={c.Title}"));
                var systemMessage = $"You are a task management assistant. Recognize requests to create categories or tasks. " +
                                    $"Current categories: {categoriesString}.Today's date is {DateTime.UtcNow:MMMM d, yyyy} " + 
                                    $"When the user request involves both creating a category and a task, you must include both function calls in a single response. First, call the CreateCategory function, and then call the CreateTask function, placing the task in the newly created category." +
                                    $"Always reply short in a playful, friendly, and slightly cheeky tone. Use fun metaphors or emojis where appropriate. " +
                                    $"If the user is simply chatting, respond normally in the language the user is currently using.";
                _conversation.SetSystemMessage(systemMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetCategories(IEnumerable<CategoryViewModel> categories)
        {
            _categories.Clear();
            _categories.AddRange(categories);
        }

        #region Functions

        private async Task HandleCreateTaskAsync(Func<TaskViewModel, Task<TaskViewModel>> createTaskAsync, string arguments, string? toolCallId)
        {
            try
            {
                var model = arguments.DeserializeFromJson<TaskViewModel>();
                if (model == null || string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Description))
                {
                    _conversation.Messages.Add(new { role = "tool", tool_call_id = toolCallId, content = false });
                    return;
                }

                if (model.CategoryId == 0)
                {
                    var categoryId = _categories.FirstOrDefault(c => c.Title.Equals(model.CategoryTitle, StringComparison.OrdinalIgnoreCase))?.Id;
                    if (categoryId == null)
                    {
                        _conversation.Messages.Add(new { role = "tool", tool_call_id = toolCallId, content = false });
                        return;
                    }
                    model.CategoryId = categoryId.Value;
                }
                else
                {
                    if (_categories.All(x => x.Id != model.CategoryId))
                    {
                        model.CategoryId = _categories.Last().Id;
                    }
                }

                var item = _mapper.Map<TaskViewModel>(model);
                var result = await createTaskAsync(item);
                bool created = result.Id > 0;

                _conversation.Messages.Add(new
                {
                    type = "function_call_output",
                    call_id = toolCallId,
                    output = created.ToString()// $"taskId={result.Id},taskTitle={result.Title}"// JsonSerializer.Serialize(new { success = created, message = created ? $"Category '{result.Title}' created." : "Failed to create category." })
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task HandleCreateCategoryAsync(Func<CategoryViewModel, Task<CategoryModel>> createCategoryAsync, string arguments, string? toolCallId)
        {
            try
            {
                var model = arguments.DeserializeFromJson<CategoryViewModel>();
                var result = await createCategoryAsync(model!);

                bool created = result.Id > 0;
                _conversation.Messages.Add(new
                {
                    type = "function_call_output",
                    call_id = toolCallId,
                    output = created.ToString()// JsonSerializer.Serialize(new { success = created, message = created ? $"Category '{result.Title}' created." : "Failed to create category." })
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public static string? ExtractTextFromOpenAIResponse(JsonElement backResponse)
        //{
        //    try
        //    {
        //        var output = backResponse.GetProperty("output");
        //        if (output.GetArrayLength() == 0) return null;

        //        var content = output[0].GetProperty("content");
        //        if (content.GetArrayLength() == 0) return null;

        //        var text = content[0].GetProperty("text").GetString();
        //        return text;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"⚠️ Oops! Failed to extract text: {ex.Message}");
        //        return null;
        //    }
        //}

        #endregion
    }
}

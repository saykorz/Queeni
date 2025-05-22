using Queeni.Components.Library.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.AI
{
    public class OpenAIConversation : IOpenAIConversation
    {
        public List<object> Messages { get; } = new();

        public void AddUserMessage(string text) => Messages.Add(JObject.FromObject(new { role = "user", content = text }));

        public void AddAssistantMessage(string text) => Messages.Add(JObject.FromObject(new { role = "assistant", content = text }));

        public void SetSystemMessage(string content)
        {
            var existingSystem = Messages.OfType<JObject>()
                .FirstOrDefault(m => m["role"]?.ToString() == "system");

            if (existingSystem != null)
                Messages.Remove(existingSystem);

            var newSystemMessage = new JObject
            {
                ["role"] = "system",
                ["content"] = content
            };

            Messages.Insert(0, newSystemMessage);

            var existingTools = Messages.OfType<JObject>()
                .Where(m => m["role"]?.ToString() == "tool" || m["type"]?.ToString() == "function_call_output")
                .ToList();

            foreach (var tool in existingTools)
            {
                Messages.Remove(tool);
            }
        }
    }
}

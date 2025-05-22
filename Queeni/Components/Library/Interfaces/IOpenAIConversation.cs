using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Interfaces
{
    public interface IOpenAIConversation
    {
        List<object> Messages { get; }
        void AddUserMessage(string text);
        void AddAssistantMessage(string text);
        void SetSystemMessage(string content);
    }
}

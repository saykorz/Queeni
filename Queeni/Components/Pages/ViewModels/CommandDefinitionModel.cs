using Queeni.Components.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Pages.ViewModels
{
    public class CommandDefinitionModel
    {
        public string Command { get; set; }         // eg. "Create"
        public string Example { get; set; }         // eg. "Create a task..."
        public string Description { get; set; }     // eg. "Adds a scheduled task."
        public CommandDefinitionTypes Type { get; set; }            // "Task" or "Category"
    }
}

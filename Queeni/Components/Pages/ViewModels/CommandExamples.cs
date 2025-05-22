using Queeni.Components.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Pages.ViewModels
{
    public static class CommandExamples
    {
        public static List<CommandDefinitionModel> All => new List<CommandDefinitionModel>
    {
        new CommandDefinitionModel
        {
            Command = "Create",
            Example = "Create a task to clean the queen’s chamber at 7 AM tomorrow.",
            Description = "Adds a scheduled task.",
            Type = CommandDefinitionTypes.Task
        },
        new CommandDefinitionModel
        {
            Command = "Remind",
            Example = "Remind me to check food supplies tonight.",
            Description = "Creates a reminder-style task.",
            Type = CommandDefinitionTypes.Task
        },
        new CommandDefinitionModel
        {
            Command = "Add",
            Example = "Add task: patrol the east tunnel.",
            Description = "Simple unscheduled task.",
            Type = CommandDefinitionTypes.Task
        },
        new CommandDefinitionModel
        {
            Command = "Make",
            Example = "Make a task named “repair bridge” for next Friday.",
            Description = "Task with a custom title and date.",
            Type = CommandDefinitionTypes.Task
        },
        new CommandDefinitionModel
        {
            Command = "Create",
            Example = "Create a category called “Nest Maintenance”.",
            Description = "Adds a new category.",
            Type = CommandDefinitionTypes.Category
        },
        new CommandDefinitionModel
        {
            Command = "Add",
            Example = "Add group 'Supply Duties'.",
            Description = "Alternate phrasing.",
            Type = CommandDefinitionTypes.Category
        },
        new CommandDefinitionModel
        {
            Command = "Make",
            Example = "Make folder “Defense Squad”.",
            Description = "Interprets 'folder' as category.",
            Type = CommandDefinitionTypes.Category
        }
    };
    }
}

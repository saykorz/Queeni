using Queeni.Components.Library.Interfaces;
using Queeni.Components.Models;
using Queeni.Components.Pages.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Library.Services
{
    public class CommandParser : ICommandParser
    {
        private readonly IEnumerable<CommandDefinitionModel> _commands;

        public CommandParser(IEnumerable<CommandDefinitionModel> commands)
        {
            _commands = commands;
        }

        public CommandParseResult Parse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return new CommandParseResult { IsCommand = false, OriginalInput = userInput };

            var lowered = userInput.ToLower();

            // Пример: Търсим команда, която започва с "create", "add", и т.н.
            var match = _commands.FirstOrDefault(cmd =>
                lowered.StartsWith(cmd.Command.ToLower()) &&
                (lowered.Contains("task") || lowered.Contains("category")));

            return new CommandParseResult
            {
                IsCommand = match != null,
                MatchedCommand = match,
                OriginalInput = userInput
            };
        }
    }

}

using Queeni.Components.Pages.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Models
{
    public class CommandParseResult
    {
        public bool IsCommand { get; set; }
        public CommandDefinitionModel MatchedCommand { get; set; }
        public string OriginalInput { get; set; }
    }

}

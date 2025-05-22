using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Pages.ViewModels
{
    public class AiTaskResult
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public List<string> Tags { get; set; } = new();
        public string Color { get; set; } = "Black";
        public string Category { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Components.Models
{
    public class TaskMessage
    {
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string CardTags { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
    }
}

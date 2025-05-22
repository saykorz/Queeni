using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data.Models
{
    public class TaskModel: BaseTitleModel
    {
        public required string Description { get; set; }

        public string Priority { get; set; } = "Black";

        public string CardTags { get; set; } = string.Empty;

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public CategoryModel? Category { get; set; }
        
    }
}

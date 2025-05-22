using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data.Models
{
    public class CategoryModel: BaseTitleModel
    {
        public ICollection<TaskModel> Tasks { get; set; } = new HashSet<TaskModel>();
    }
}

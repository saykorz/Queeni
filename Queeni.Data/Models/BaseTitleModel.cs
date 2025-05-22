using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data.Models
{
    public class BaseTitleModel: EntityModel
    {
        public required string Title { get; set; }
    }
}

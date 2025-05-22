using Queeni.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data.Interfaces
{
    public interface IUowData: IDisposable
    {
        ApplicationDbContext Context { get; }

        IRepository<CategoryModel> Categories { get; }

        IRepository<TaskModel> Tasks { get; }

        int SaveChanges();
    }
}

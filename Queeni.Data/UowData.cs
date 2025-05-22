using Queeni.Data.Interfaces;
using Queeni.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data
{
    public class UowData : IUowData
    {
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

        private readonly ApplicationDbContext _context;

        //public UowData()
        //    : this(new ApplicationDbContext())
        //{

        //}

        public UowData(ApplicationDbContext context)
        {
            Context = context;
        }

        private IRepository<T> GetRepository<T>() where T : class, IEntityModel
        {
            if (!_repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(BaseRepository<T>);

                // Custom logic for specific repositories
                //if (typeof(T) == typeof(ApplicationUser))
                //{
                //    type = typeof(UserRepository);
                //}

                _repositories.Add(typeof(T), Activator.CreateInstance(type, Context));
            }

            return (IRepository<T>)_repositories[typeof(T)];
        }

        public ApplicationDbContext Context { get; private set; }

        // Repositories
        //public IUsersRepository Users => (UserRepository)GetRepository<ApplicationUser>();
        public IRepository<CategoryModel> Categories => GetRepository<CategoryModel>();

        public IRepository<TaskModel> Tasks => GetRepository<TaskModel>();

        public int SaveChanges()
        {
            try
            {
                return Context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                var errorMessage = new StringBuilder();
                foreach (var entry in dbEx.Entries)
                {
                    errorMessage.AppendLine($"Entity of type {entry.Entity.GetType().Name} failed to update.");
                }
                throw new Exception(errorMessage.ToString(), dbEx);
            }
        }

        public void Dispose()
        {
            if (Context.Database.GetDbConnection() is SqliteConnection sqliteConnection)
            {
                SqliteConnection.ClearPool(sqliteConnection);
            }
            Context.Dispose();
        }
    }
}

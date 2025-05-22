using Queeni.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace Queeni.Data
{
    public class GenericRepository<T> : BaseRepository<T>
        where T : class, IEntityModel, new()
    {
        public GenericRepository(DbContext context) : base(context)
        {
        }

        public override void Delete(int id)
        {
            var entity = new T { Id = id };
            Context.Entry(entity).State = EntityState.Deleted;
        }
    }
}

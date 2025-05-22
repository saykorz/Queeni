using Queeni.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Queeni.Data
{
    public class BaseRepository<T> : IDisposable, IRepository<T>
        where T : class, IEntityModel
    {
        public BaseRepository(DbContext context)
        {
            Context = context ?? throw new ArgumentException("An instance of DbContext is required to use this repository.", nameof(context));
            DbSet = Context.Set<T>();
        }

        protected DbSet<T> DbSet { get; set; }
        protected DbContext Context { get; set; }

        public virtual IQueryable<T> All()
        {
            return DbSet.AsQueryable();
        }

        public Task<IQueryable<T>> All(Expression<Func<T, bool>> predicate = null)
        {
            return Task.FromResult(
                predicate != null ? DbSet.Where(predicate).AsQueryable() : DbSet.AsQueryable()
            );
        }

        public async Task<IQueryable<T>> AllIncluding(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = await All(predicate);
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public virtual T GetById(int id) => DbSet.Find(id);
        public virtual T GetById(string id) => DbSet.Find(id);
        public virtual T GetById(Guid id) => DbSet.Find(id);

        public virtual void Add(T entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Add(ICollection<T> entities)
        {
            DbSet.AddRange(entities);
        }

        public virtual void Update(T entity)
        {
            DbSet.Update(entity);
        }

        public virtual void Update(ICollection<T> entities)
        {
            DbSet.UpdateRange(entities);
        }

        // Simple AddOrUpdate implementation (manual, based on key existence)
        public virtual void AddOrUpdate(T entity)
        {
            var existingEntity = Context.Find<T>(entity.Id); // или какъвто е ключът ти

            if (existingEntity != null)
            {
                Context.Entry(existingEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                Context.Add(entity);
            }
        }

        public virtual void AddOrUpdate(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                AddOrUpdate(entity);
            }
        }

        public virtual void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                Delete(entity);
            }
        }

        public virtual void Delete(T entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Delete(ICollection<T> entities)
        {
            DbSet.RemoveRange(entities);
        }

        public virtual void Detach(T entity)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                Context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
    }
}
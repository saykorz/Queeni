using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Queeni.Data.Interfaces
{
    public interface IRepository<T> where T : class, IEntityModel
    {
        Task<IQueryable<T>> All(Expression<Func<T, bool>> predicate = null);

        Task<IQueryable<T>> AllIncluding(Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includeProperties);

        T GetById(int id);

        T GetById(string id);

        T GetById(Guid id);

        void Add(T entity);

        void Add(ICollection<T> entities);

        void Update(T entity);

        void Update(ICollection<T> entities);

        void AddOrUpdate(T entity);

        void AddOrUpdate(ICollection<T> entities);

        void Delete(T entity);

        void Delete(ICollection<T> entities);

        void Delete(int id);

        void Detach(T entity);

        T Create();
    }
}

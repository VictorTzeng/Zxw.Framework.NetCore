using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public interface IRepository<T, TKey>:IDisposable where T : class, IBaseModel<TKey>
    {
        void Add(T entity);
        Task AddAsync(T entity);
        void AddRange(ICollection<T> entities);
        Task AddRangeAsync(ICollection<T> entities);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Count(Expression<Func<T, bool>> @where = null);
        Task<int> CountAsync(Expression<Func<T, bool>> @where = null);
        void Delete(TKey key);
        void Delete(Expression<Func<T, bool>> @where);
        Task DeleteAsync(Expression<Func<T, bool>> @where);
        void Edit(T entity);
        void EditRange(ICollection<T> entities);
        bool Exist(Expression<Func<T, bool>> @where = null);
        Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters);
        T GetSingle(TKey key);
        Task<T> GetSingleAsync(TKey key);
        T GetSingle(TKey key, params Expression<Func<T, object>>[] includes);
        Task<T> GetSingleAsync(TKey key, params Expression<Func<T, object>>[] includes);
        T GetSingle(Expression<Func<T, bool>> @where = null);
        Task<T> GetSingleAsync(Expression<Func<T, bool>> @where = null);
        T GetSingle(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        Task<T> GetSingleAsync(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null);
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true,
            params Func<T, object>[] @orderby);
        IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new();
        void Update(T model, params string[] updateColumns);
        void Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
        Task UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
    }
}

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
        int Add(T entity, bool withTrigger = false);
        Task<int> AddAsync(T entity, bool withTrigger = false);
        int AddRange(ICollection<T> entities, bool withTrigger = false);
        Task<int> AddRangeAsync(ICollection<T> entities, bool withTrigger = false);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Count(Expression<Func<T, bool>> @where = null);
        Task<int> CountAsync(Expression<Func<T, bool>> @where = null);
        int Delete(TKey key, bool withTrigger = false);
        int Delete(Expression<Func<T, bool>> @where);
        Task<int> DeleteAsync(Expression<Func<T, bool>> @where);
        int Edit(T entity, bool withTrigger = false);
        int EditRange(ICollection<T> entities, bool withTrigger = false);
        bool Exist(Expression<Func<T, bool>> @where = null);
        Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters);
        T GetSingle(TKey key);
        T GetSingle(TKey key, Func<IQueryable<T>, IQueryable<T>> includeFunc);
        Task<T> GetSingleAsync(TKey key);
        T GetSingleOrDefault(Expression<Func<T, bool>> @where = null);
        Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null);
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null);
        IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true,
            params Func<T, object>[] @orderby);
        IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new();
        int Update(T model, bool withTrigger = false, params string[] updateColumns);
        int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
        Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
    }
}

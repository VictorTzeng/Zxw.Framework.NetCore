using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    [FromDbContextFactoryInterceptor]
    public interface IRepository<T, in TKey>: ITransientDependency, IDisposable where T : IBaseModel<TKey>
    {
        #region Insert

        int Add(T entity);
        Task<int> AddAsync(T entity);
        int AddRange(ICollection<T> entities);
        Task<int> AddRangeAsync(ICollection<T> entities);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int AddBySql(string sql);

        #endregion

        #region Delete

        int Delete(TKey key);
        int Delete(Expression<Func<T, bool>> @where);
        Task<int> DeleteAsync(Expression<Func<T, bool>> @where);
        int DeleteBySql(string sql);
        #endregion

        #region Update

        int Edit(T entity);
        int EditRange(ICollection<T> entities);
        int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Update(T model, params string[] updateColumns);
        int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
        Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory);
        int UpdateBySql(string sql);

        #endregion

        #region Query

        int Count(Expression<Func<T, bool>> @where = null);
        Task<int> CountAsync(Expression<Func<T, bool>> @where = null);
        bool Exist(Expression<Func<T, bool>> @where = null);
        Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null);
        T GetSingle(TKey key);
        T GetSingle(TKey key, Func<IQueryable<T>, IQueryable<T>> includeFunc);
        Task<T> GetSingleAsync(TKey key);
        T GetSingleOrDefault(Expression<Func<T, bool>> @where = null);
        Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null);
        IList<T> Get(Expression<Func<T, bool>> @where = null);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null);
        IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true,
            params Expression<Func<T, object>>[] @orderby);

        List<T> GetBySql(string sql);

        List<TView> GetViews<TView>(string sql);
        List<TView> GetViews<TView>(string viewName, Func<TView, bool> where);

        #endregion
    }
}

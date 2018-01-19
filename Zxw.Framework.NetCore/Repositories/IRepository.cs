using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public interface IRepository<T, TKey>:IDisposable where T : class, IBaseModel<TKey>
    {
        void Add(T entity);
        void AddRange(ICollection<T> entities);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Count(Expression<Func<T, bool>> @where = null);
        void Delete(TKey key);
        void Delete(Expression<Func<T, bool>> @where);
        void Edit(T entity);
        void EditRange(ICollection<T> entities);
        bool Exist(Expression<Func<T, bool>> @where = null);
        bool Exist(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        T GetSingle(TKey key);
        T GetSingle(TKey key, params Expression<Func<T, object>>[] includes);
        T GetSingle(Expression<Func<T, bool>> @where = null);
        T GetSingle(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null);
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes);
        IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true,
            params Func<T, object>[] @orderby);
        IQueryable<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        void Update(T model, params string[] updateColumns);
    }
}

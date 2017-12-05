using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.IRepositories
{
    public interface IRepository<T, TKey>:IDisposable where T : class, IBaseModel<TKey>
    {
        int Add(T entity);
        int AddRange(ICollection<T> entities);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int Count(Expression<Func<T, bool>> @where = null);
        int Delete(TKey key);
        int Delete(Expression<Func<T, bool>> @where);
        int Edit(T entity);
        int EditRange(ICollection<T> entities);
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
        int Save();
        IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Update(T model, params string[] updateColumns);
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.Models;

namespace Zxw.Framework.IRepositories
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
        bool Exist<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        T GetSingle(TKey key);
        T GetSingle<TProperty>(TKey key, params Expression<Func<T, TProperty>>[] includes);
        IList<T> Get(Expression<Func<T, bool>> @where = null);
        IList<T> Get<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        IList<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex,
            Expression<Func<T, T>> @orderby = null, bool asc = true);
        IList<T> GetByPagination<TProperty>(Expression<Func<T, bool>> @where, int pageSize, int pageIndex,
            Expression<Func<T, T>> @orderby = null, bool asc = true, params Expression<Func<T, TProperty>>[] includes);
        int Save();
        IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        int Update(T model, params string[] updateColumns);
    }
}

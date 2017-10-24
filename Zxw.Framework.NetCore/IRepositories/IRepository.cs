using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.IRepositories
{
    public interface IRepository<T, TKey>:IDisposable where T : class, IBaseModel<TKey>
    {
        void Add(T entity);
        void AddRange(ICollection<T> entities);
        void BulkInsert(IList<T> entities, string destinationTableName = null);
        int Count(Expression<Func<T, bool>> @where = null);
        void Delete(TKey key);
        void Delete(Expression<Func<T, bool>> @where);
        void Edit(T entity);
        void EditRange(ICollection<T> entities);
        bool Exist(Expression<Func<T, bool>> @where = null);
        bool Exist<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        T GetSingle(TKey key);
        T GetSingle<TProperty>(TKey key, params Expression<Func<T, TProperty>>[] includes);
        T GetSingle(Expression<Func<T, bool>> @where = null);
        T GetSingle<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        IList<T> Get(Expression<Func<T, bool>> @where = null);
        IList<T> Get<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        IList<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex,
            Expression<Func<T, T>> @orderby = null, bool asc = true);
        IList<T> GetByPagination<TProperty>(Expression<Func<T, bool>> @where, int pageSize, int pageIndex,
            Expression<Func<T, T>> @orderby = null, bool asc = true, params Expression<Func<T, TProperty>>[] includes);
        //int Save();
        IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        void Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp);
        void Update(T model, params string[] updateColumns);
    }
}

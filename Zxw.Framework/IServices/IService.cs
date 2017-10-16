using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.Models;

namespace Zxw.Framework.IServices
{
    public interface IService<T, in TKey>: IDisposable where T:IBaseModel<TKey>
    {
        int Count(Expression<Func<T, bool>> where = null);

        bool Exists(Expression<Func<T, bool>> where = null);
        bool Exists<TProperty>(Expression<Func<T, bool>> where = null, params Expression<Func<T, TProperty>>[] includes);

        T GetSingle(TKey key);

        T GetSingle<TProperty>(TKey key, params Expression<Func<T, TProperty>>[] includes);

        IList<T> Get(Expression<Func<T, bool>> where = null);
        IList<T> Get<TProperty>(Expression<Func<T, bool>> where = null, params Expression<Func<T, TProperty>>[] includes);

        IList<T> GetByPagination(Expression<Func<T, bool>> where, int pageSize, int pageIndex,
            Expression<Func<T, T>> orderby = null, bool asc = true);
        IList<T> GetByPagination<TProperty>(Expression<Func<T, bool>> where, int pageSize, int pageIndex,
            Expression<Func<T, T>> orderby = null, bool asc = true, params Expression<Func<T, TProperty>>[] includes);
        int Add(T entity);

        int AddRange(ICollection<T> entities);

        int Edit(T entity);

        int EditRange(ICollection<T> entities);

        int Update(Expression<Func<T, bool>> where, Expression<Func<T, T>> updateExp);

        int Update(T model, params string[] updateColumns);

        int Delete(TKey key);

        int Delete(Expression<Func<T, bool>> where);

        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);

        IList<TView> CustomQuery<TView>(string sql, params object[] parameters) where TView : class, new();
    }
}
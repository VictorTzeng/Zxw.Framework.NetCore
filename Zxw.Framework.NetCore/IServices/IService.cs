using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.IServices
{
    public interface IService<T, in TKey>: IDisposable where T:IBaseModel<TKey>
    {
        int Count(Expression<Func<T, bool>> where = null);

        bool Exists(Expression<Func<T, bool>> where = null);
        bool Exists(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes);

        T GetSingle(TKey key);

        T GetSingle(TKey key, params Expression<Func<T, object>>[] includes);

        IList<T> Get(Expression<Func<T, bool>> where = null);
        IList<T> Get(Expression<Func<T, bool>> where = null, params Expression<Func<T, object>>[] includes);

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
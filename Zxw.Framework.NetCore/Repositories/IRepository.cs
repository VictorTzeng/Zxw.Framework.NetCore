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
        IQueryable<T> Get(Expression<Func<T, bool>> @where = null);
        IQueryable<T> Get<TProperty>(Expression<Func<T, bool>> @where = null, params Expression<Func<T, TProperty>>[] includes);
        IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true,
            params Func<T, object>[] @orderby);
        //int Save();
        IQueryable<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new();
        void Update(T model, params string[] updateColumns);
    }
}

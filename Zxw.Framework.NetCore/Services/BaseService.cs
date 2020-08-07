using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Repositories;

namespace Zxw.Framework.NetCore.Services
{
    public abstract class BaseService<T,TKey>:IService<T,TKey> where T:class,IBaseModel<TKey>
    {
        public IRepository<T,TKey> Repository { get; set; }
        protected BaseService(IRepository<T,TKey> repository)
        {
            Repository = repository;
        }

        public virtual int Add(T model)
        {
            return Repository.Add(model);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            return await Repository.AddAsync(entity);
        }

        public virtual int AddRange(IList<T> models)
        {
            return Repository.AddRange(models);
        }

        public virtual async Task<int> AddRangeAsync(IList<T> entities)
        {
            return await Repository.AddRangeAsync(entities);
        }

        public virtual void BulkInsert(IList<T> entities)
        {
            Repository.BulkInsert(entities, typeof(T).Name);
        }

        public virtual int Edit(T model)
        {
            return Repository.Edit(model);
        }

        public virtual int EditRange(ICollection<T> entities)
        {
            return Repository.EditRange(entities);
        }

        public virtual int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return Repository.BatchUpdate(where, updateExp);
        }

        public virtual async Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return await Repository.BatchUpdateAsync(where, updateExp);
        }

        public virtual int Update(T model, params string[] updateColumns)
        {
            return Repository.Update(model, updateColumns);
        }

        public virtual int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return Repository.Update(where, updateFactory);
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return await Repository.UpdateAsync(where, updateFactory);
        }

        public virtual int Delete(TKey key)
        {
            return Repository.Delete(key);
        }

        public virtual int Delete(Expression<Func<T, bool>> @where)
        {
            return Repository.Delete(where);
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return await Repository.DeleteAsync(where);
        }

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return Repository.Count(where);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return await Repository.CountAsync(where);
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return Repository.Exist(where);
        }

        public virtual async Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            return await Repository.ExistAsync(where);
        }

        public virtual T GetSingle(TKey key)
        {
            return Repository.GetSingle(key);
        }

        public virtual async Task<T> GetSingleAsync(TKey key)
        {
            return await Repository.GetSingleAsync(key);
        }

        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> @where = null)
        {
            return Repository.GetSingleOrDefault(where);
        }

        public virtual async Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null)
        {
            return await Repository.GetSingleOrDefaultAsync(where);
        }

        public virtual IList<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return Repository.Get(where);
        }

        public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return await Repository.GetAsync(where);
        }

        public virtual IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true, params Expression<Func<T, object>>[] @orderby)
        {
            return Repository.GetByPagination(where, pageSize, pageIndex, asc, orderby);
        }

        public void Dispose()
        {
            Repository?.Dispose();
        }
    }
}

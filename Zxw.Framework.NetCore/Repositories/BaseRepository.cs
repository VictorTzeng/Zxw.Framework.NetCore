using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public abstract class BaseRepository<T, TKey>:IRepository<T, TKey> where T : BaseModel<TKey>
    {
        protected readonly IEfDbContext DbContext;

        protected DbSet<T> DbSet => DbContext.GetDbSet<T>();

        protected BaseRepository(IEfDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbContext.EnsureCreatedAsync();
        }

        public virtual int Add(T entity, bool withTrigger = false)
        {
            return DbContext.Add(entity, withTrigger);
        }

        public virtual async Task<int> AddAsync(T entity, bool withTrigger = false)
        {
            return await DbContext.AddAsync(entity, withTrigger);
        }

        public virtual int AddRange(ICollection<T> entities, bool withTrigger = false)
        {
            return DbContext.AddRange(entities, withTrigger);
        }

        public virtual async Task<int> AddRangeAsync(ICollection<T> entities, bool withTrigger = false)
        {
            return await DbContext.AddRangeAsync(entities, withTrigger);
        }

        public virtual void BulkInsert(IList<T> entities, string destinationTableName = null)
        {
            DbContext.BulkInsert<T, TKey>(entities, destinationTableName);
        }

        /// <summary>
        /// update query datas by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="updateExp"></param>
        /// <returns></returns>
        public virtual int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return DbContext.Update(where, updateExp);
        }

        public virtual async Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return await DbContext.UpdateAsync(@where, updateExp);
        }

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Count(where);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.CountAsync(where);
        }

        public virtual int Delete(TKey key, bool withTrigger = false)
        {
            return DbContext.Delete<T,TKey>(key, withTrigger);
        }

        public virtual int Delete(Expression<Func<T, bool>> @where)
        {
            return DbContext.Delete(where);
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return await DbContext.DeleteAsync(where);
        }

        public virtual int Edit(T entity, bool withTrigger = false)
        {
            return DbContext.Edit<T,TKey>(entity, withTrigger);
        }

        public virtual int EditRange(ICollection<T> entities, bool withTrigger = false)
        {
            return DbContext.EditRange(entities, withTrigger);
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Exist(where);
        }

        public virtual async Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.ExistAsync(where);
        }

        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return DbContext.ExecuteSqlWithNonQuery(sql, parameters);
        }

        public virtual async Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return await DbContext.ExecuteSqlWithNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// 根据主键获取实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetSingle(TKey key)
        {
            return DbSet.Find(key);
        }

        public T GetSingle(TKey key, Func<IQueryable<T>, IQueryable<T>> includeFunc)
        {
            if (includeFunc == null) return GetSingle(key);
            return includeFunc(DbSet.Where(m => m.Id.Equal(key))).AsNoTracking().FirstOrDefault();
        }

        /// <summary>
        /// 根据主键获取实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual async Task<T> GetSingleAsync(TKey key)
        {
            return await DbContext.FindAsync<T,TKey>(key);
        }

        /// <summary>
        /// 获取单个实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.GetSingleOrDefault(@where);
        }

        /// <summary>
        /// 获取单个实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual async Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.GetSingleOrDefaultAsync(where);
        }

        /// <summary>
        /// 获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return (@where != null ? DbSet.Where(@where).AsNoTracking() : DbSet.AsNoTracking());
        }

        /// <summary>
        /// 获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbSet.Where(where).ToListAsync();
        }

        /// <summary>
        /// 分页获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true, params Func<T, object>[] @orderby)
        {
            var filter = Get(where).AsEnumerable();
            if (orderby != null)
            {
                foreach (var func in orderby)
                {
                    filter = asc ? filter.OrderBy(func) : filter.OrderByDescending(func);
                }
            }
            return filter.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }


        public virtual IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return DbContext.SqlQuery<T, TView>(sql, parameters);
        }

        public virtual async Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return await DbContext.SqlQueryAsync<T,TView>(sql, parameters);
        }

        public virtual int Update(T model, bool withTrigger = false, params string[] updateColumns)
        {
            DbContext.Update(model, withTrigger, updateColumns);
            return DbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return DbContext.Update(where, updateFactory);
        }

        public virtual async Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return await DbContext.UpdateAsync(where, updateFactory);
        }

        public virtual void Dispose()
        {
            
        }
    }
}


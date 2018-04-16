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
    public abstract class BaseRepository<T, TKey>:IRepository<T, TKey> where T : class, IBaseModel<TKey>
    {
        protected readonly IEfDbContext DbContext;

        protected DbSet<T> DbSet => DbContext.GetDbSet<T>();

        protected BaseRepository(IEfDbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbContext.EnsureCreated();
        }

        public virtual int Add(T entity)
        {
            DbContext.Add(entity);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> AddAsync(T entity)
        {
            DbContext.AddAsync(entity);
            return DbContext.SaveChangesAsync();
        }

        public virtual int AddRange(ICollection<T> entities)
        {
            DbContext.AddRange(entities);
            return DbContext.SaveChanges();
        }

        public virtual Task<int> AddRangeAsync(ICollection<T> entities)
        {
            DbContext.AddRangeAsync(entities);
            return DbContext.SaveChangesAsync();
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

        public virtual Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return DbContext.UpdateAsync(@where, updateExp);
        }

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Count(where);
        }

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.CountAsync(where);
        }

        public virtual int Delete(TKey key)
        {
            DbContext.Delete<T,TKey>(key);
            return DbContext.SaveChanges();
        }

        public virtual int Delete(Expression<Func<T, bool>> @where)
        {
            return DbContext.Delete(where);
        }

        public virtual Task<int> DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return DbContext.DeleteAsync(where);
        }

        public virtual int Edit(T entity)
        {
            DbContext.Edit(entity);
            return DbContext.SaveChanges();
        }

        public virtual int EditRange(ICollection<T> entities)
        {
            DbContext.EditRange(entities);
            return DbContext.SaveChanges();
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Exist(where);
        }

        public virtual Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.ExistAsync(where);
        }

        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return DbContext.ExecuteSqlWithNonQuery(sql, parameters);
        }

        public virtual Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return DbContext.ExecuteSqlWithNonQueryAsync(sql, parameters);
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
        public virtual Task<T> GetSingleAsync(TKey key)
        {
            return DbContext.FindAsync<T,TKey>(key);
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
        public virtual Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.GetSingleOrDefaultAsync(where);
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
        public virtual Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return DbSet.Where(where).ToListAsync();
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

        public virtual Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return DbContext.SqlQueryAsync<T,TView>(sql, parameters);
        }

        public virtual int Update(T model, params string[] updateColumns)
        {
            DbContext.Update(model, updateColumns);
            return DbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return DbContext.Update(where, updateFactory);
        }

        public virtual Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return DbContext.UpdateAsync(where, updateFactory);
        }

        public virtual void Dispose()
        {
            if (DbContext != null)
            {
                DbContext.Dispose();
            }
        }
    }
}


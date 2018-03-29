using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public abstract class BaseRepository<T, TKey>:IRepository<T, TKey> where T : class, IBaseModel<TKey>
    {
        protected readonly IEfDbContext _dbContext;
        private readonly DbSet<T> _set;

        protected BaseRepository(IEfDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbContext.GetDatabase().EnsureCreated();
            _set = dbContext.GetDbSet<T>();
        }

        public virtual int Add(T entity)
        {
            _set.Add(entity);
            return _dbContext.SaveChanges();
        }

        public virtual Task<int> AddAsync(T entity)
        {
            _dbContext.AddAsync(entity);
            return _dbContext.SaveChangesAsync();
        }

        public virtual int AddRange(ICollection<T> entities)
        {
            _set.AddRange(entities);
            return _dbContext.SaveChanges();
        }

        public virtual Task<int> AddRangeAsync(ICollection<T> entities)
        {
            _dbContext.AddRangeAsync(entities);
            return _dbContext.SaveChangesAsync();
        }

        public virtual void BulkInsert(IList<T> entities, string destinationTableName = null)
        {
            _dbContext.BulkInsert<T, TKey>(entities, destinationTableName);
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
            return _dbContext.Update(where, updateExp);
        }

        public virtual Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return _dbContext.UpdateAsync(@where, updateExp);
        }

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return where == null ? _set.Count() : _set.Count(@where);
        }

        public virtual Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return where == null ? _set.CountAsync() : _set.CountAsync(@where);
        }

        public virtual int Delete(TKey key)
        {
            var entity = _set.Find(key);
            if (entity == null) return 0;
            _set.Remove(entity);
            return _dbContext.SaveChanges();
        }

        public virtual int Delete(Expression<Func<T, bool>> @where)
        {
            return _dbContext.Delete(where);
        }

        public virtual Task<int> DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return _dbContext.DeleteAsync(where);
        }

        public virtual int Edit(T entity)
        {
            _dbContext.Edit(entity);
            return _dbContext.SaveChanges();
        }

        public virtual int EditRange(ICollection<T> entities)
        {
            _dbContext.EditRange(entities);
            return _dbContext.SaveChanges();
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return Get(where).Any();
        }

        public virtual Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.AnyAsync();
            return _set.Where(where).AnyAsync();
        }

        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return _dbContext.ExecuteSqlWithNonQuery(sql, parameters);
        }

        public virtual Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return _dbContext.ExecuteSqlWithNonQueryAsync(sql, parameters);
        }

        /// <summary>
        /// 根据主键获取实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetSingle(TKey key)
        {
            return _set.Find(key);
        }
        /// <summary>
        /// 根据主键获取实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual Task<T> GetSingleAsync(TKey key)
        {
            return _set.FindAsync(key);
        }

        /// <summary>
        /// 获取单个实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.SingleOrDefault();
            return _set.SingleOrDefault(@where);
        }

        /// <summary>
        /// 获取单个实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual Task<T> GetSingleOrDefaultAsync(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.SingleOrDefaultAsync();
            return _set.SingleOrDefaultAsync(where);
        }

        /// <summary>
        /// 获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return (@where != null ? _set.AsNoTracking().Where(@where) : _set.AsNoTracking());
        }

        /// <summary>
        /// 获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return _set.Where(where).ToListAsync();
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
            return _dbContext.SqlQuery<T, TView>(sql, parameters);
        }

        public virtual Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return _dbContext.SqlQueryAsync<T,TView>(sql, parameters);
        }

        public virtual int Update(T model, params string[] updateColumns)
        {
            _dbContext.Update(model, updateColumns);
            return _dbContext.SaveChanges();
        }

        public virtual int Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return _dbContext.Update(where, updateFactory);
        }

        public virtual Task<int> UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            return _dbContext.UpdateAsync(where, updateFactory);
        }

        public virtual void Dispose()
        {
            if (_dbContext != null)
            {
                _dbContext.Dispose();
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public abstract class BaseRepository<T, TKey>:IRepository<T, TKey> where T : class, IBaseModel<TKey>
    {
        private readonly DefaultDbContext _dbContext;
        private readonly DbSet<T> _set;
        public BaseRepository(DefaultDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbContext.Database.EnsureCreated();
            _set = dbContext.Set<T>();
        }
        public virtual void Add(T entity)
        {
            _set.Add(entity);
        }

        public Task AddAsync(T entity)
        {
            return _dbContext.AddAsync(entity);
        }

        public virtual void AddRange(ICollection<T> entities)
        {
            _set.AddRange(entities);
        }

        public Task AddRangeAsync(ICollection<T> entities)
        {
            return _dbContext.AddRangeAsync(entities);
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
        public int BatchUpdate(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return _dbContext.Update(where, updateExp);
        }

        public Task<int> BatchUpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
        {
            return _dbContext.UpdateAsync(@where, updateExp);
        }

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return where == null ? _set.Count() : _set.Count(@where);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return where == null ? _set.CountAsync() : _set.CountAsync(@where);
        }

        public virtual void Delete(TKey key)
        {
            var entity = _set.Find(key);
            if (entity == null) return;
            _set.Remove(entity);
        }

        public virtual void Delete(Expression<Func<T, bool>> @where)
        {
            _dbContext.Delete(where);
        }

        public Task DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return _dbContext.DeleteAsync(where);
        }

        public virtual void Edit(T entity)
        {
            _dbContext.Edit(entity);
        }

        public virtual void EditRange(ICollection<T> entities)
        {
            _dbContext.EditRange(entities);
        }

        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return Get(where).Any();
        }

        public Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.AnyAsync();
            return _set.Where(where).AnyAsync();
        }

        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return _dbContext.ExecuteSqlWithNonQuery(sql, parameters);
        }

        public Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return _dbContext.ExecuteSqlWithNonQueryAsync(sql, parameters);
        }

        public virtual T GetSingle(TKey key)
        {
            return _set.Find(key);
        }

        public Task<T> GetSingleAsync(TKey key)
        {
            return _set.FindAsync(key);
        }

        public virtual T GetSingle(TKey key, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingle(key);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            Expression<Func<T, bool>> filter = m => m.Id.Equal(key);
            return query.SingleOrDefault(filter.Compile());
        }

        public Task<T> GetSingleAsync(TKey key, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingleAsync(key);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            Expression<Func<T, bool>> filter = m => m.Id.Equal(key);
            return query.SingleOrDefaultAsync(filter);
        }

        public T GetSingle(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.SingleOrDefault();
            return _set.SingleOrDefault(@where);
        }

        public Task<T> GetSingleAsync(Expression<Func<T, bool>> @where = null)
        {
            if (where == null) return _set.SingleOrDefaultAsync();
            return _set.SingleOrDefaultAsync(where);
        }

        public T GetSingle(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingle(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            if (where == null) return query.SingleOrDefault();
            return query.SingleOrDefault(where);
        }

        public Task<T> GetSingleAsync(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null) return GetSingleAsync(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            if (where == null) return query.SingleOrDefaultAsync();
            return query.SingleOrDefaultAsync(where);
        }

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return (@where != null ? _set.AsNoTracking().Where(@where) : _set.AsNoTracking());
        }

        public Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return _set.Where(where).ToListAsync();
        }

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null)
                return Get(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return @where != null ? query.AsNoTracking().Where(@where) : query.AsNoTracking();
        }

        public Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null, params Expression<Func<T, object>>[] includes)
        {
            if (includes == null)
                return GetAsync(where);
            var query = _set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return @where != null ? query.AsNoTracking().Where(@where).ToListAsync() : query.AsNoTracking().ToListAsync();
        }


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

        public Task<List<TView>> SqlQueryAsync<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return _dbContext.SqlQueryAsync<T,TView>(sql, parameters);
        }

        public virtual void Update(T model, params string[] updateColumns)
        {
            _dbContext.Update(model, updateColumns);
        }

        public void Update(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
        {
            _dbContext.Update(where, updateFactory);
        }

        public Task UpdateAsync(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
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


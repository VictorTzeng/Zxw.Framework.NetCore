﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Repositories
{
    public abstract class BaseRepository<T, TKey>:IRepository<T, TKey> where T : BaseModel<TKey>, new()
    {
        protected readonly IDbContextCore DbContext;
        protected readonly ISqlOperatorUtility SqlOperatorUtility;

        protected DbSet<T> DbSet => DbContext.GetDbSet<T>();

        protected BaseRepository(IDbContextCore dbContext, ISqlOperatorUtility sqlOperator)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbContext.EnsureCreated();

            SqlOperatorUtility = sqlOperator;
        }

        #region Insert

        public virtual int Add(T entity)
        {
            return DbContext.Add(entity);
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            return await DbContext.AddAsync(entity);
        }

        public virtual int AddRange(ICollection<T> entities)
        {
            return DbContext.AddRange(entities);
        }

        public virtual async Task<int> AddRangeAsync(ICollection<T> entities)
        {
            return await DbContext.AddRangeAsync(entities);
        }

        public virtual void BulkInsert(IList<T> entities, string destinationTableName = null)
        {
            DbContext.BulkInsert<T>(entities, destinationTableName);
        }

        public int AddBySql(string sql)
        {
            return DbContext.ExecuteSqlWithNonQuery(sql);
        }

        #endregion

        #region Update

        public int DeleteBySql(string sql)
        {
            return SqlOperatorUtility.ExecuteSqlCommand(sql);
        }

        public virtual int Edit(T entity)
        {
            return DbContext.Edit<T>(entity);
        }

        public virtual int EditRange(ICollection<T> entities)
        {
            return DbContext.EditRange(entities);
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
        public virtual int Update(T model, params string[] updateColumns)
        {
            DbContext.Update(model, updateColumns);
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

        public int UpdateBySql(string sql)
        {
            return SqlOperatorUtility.ExecuteSqlCommand(sql);
        }

        #endregion

        #region Delete

        public virtual int Delete(TKey key)
        {
            return DbContext.Delete<T,TKey>(key);
        }

        public virtual int Delete(Expression<Func<T, bool>> @where)
        {
            return DbContext.Delete(where);
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> @where)
        {
            return await DbContext.DeleteAsync(where);
        }
        

        #endregion

        #region Query

        public virtual int Count(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Count(where);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.CountAsync(where);
        }


        public virtual bool Exist(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.Exist(where);
        }

        public virtual async Task<bool> ExistAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.ExistAsync(where);
        }

        /// <summary>
        /// 根据主键获取实体。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetSingle(TKey key)
        {
            return DbContext.Find<T, TKey>(key);
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
            return await DbContext.FindAsync<T, TKey>(key);
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
        public virtual IList<T> Get(Expression<Func<T, bool>> @where = null)
        {
            return DbContext.GetByCompileQuery(where);
        }

        /// <summary>
        /// 获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual async Task<List<T>> GetAsync(Expression<Func<T, bool>> @where = null)
        {
            return await DbContext.GetByCompileQueryAsync(where);
        }

        /// <summary>
        /// 分页获取实体列表。建议：如需使用Include和ThenInclude请重载此方法。
        /// </summary>
        public virtual IEnumerable<T> GetByPagination(Expression<Func<T, bool>> @where, int pageSize, int pageIndex, bool asc = true, params Func<T, object>[] @orderby)
        {
            var filter = DbContext.Get(where);
            if (orderby != null)
            {
                foreach (var func in orderby)
                {
                    filter = asc ? filter.OrderBy(func).AsQueryable() : filter.OrderByDescending(func).AsQueryable();
                }
            }
            return filter.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        public List<T> GetBySql(string sql)
        {
            return SqlOperatorUtility.SqlQuery<T>(sql).ToList();
        }

        public List<TView> GetViews<TView>(string sql)
        {
            var list = SqlOperatorUtility.SqlQuery<TView>(sql).ToList();
            return list;
        }

        public List<TView> GetViews<TView>(string viewName, Func<TView, bool> @where)
        {
            var list = SqlOperatorUtility.SqlQuery<TView>($"select * from {viewName}").ToList();
            if (where != null)
            {
                return list.Where(where).ToList();
            }

            return list;
        }

        public virtual PaginationResult SqlQueryByPagination(string sql, string[] orderBys, int pageIndex, int pageSize,
            params DbParameter[] parameters)
        {
            return DbContext.SqlQueryByPagination<T>(sql, orderBys, pageIndex, pageSize, parameters);
        }

        public virtual PaginationResult SqlQueryByPagination<TView>(string sql, string[] orderBys, int pageIndex, int pageSize) where TView : class
        {
            return DbContext.SqlQueryByPagination<T,TView>(sql, orderBys, pageIndex, pageSize);
        }

        #endregion

        public void Dispose()
        {
            DbContext?.Dispose();
        }
    }
}


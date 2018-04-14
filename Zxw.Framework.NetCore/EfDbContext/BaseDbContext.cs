using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.Triggers;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Z.EntityFramework.Plus;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.EfDbContext
{
    public abstract class BaseDbContext : DbContext, IEfDbContext
    {
        protected readonly DbContextOption _option;
        public new virtual void Add<T>(T entity) where T : class
        {
            base.Add(entity);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="option"></param>
        protected BaseDbContext(IOptions<DbContextOption> option)
        {
            if(option==null)
                throw new ArgumentNullException(nameof(option));
            if(string.IsNullOrEmpty(option.Value.ConnectionString))
                throw new ArgumentNullException(nameof(option.Value.ConnectionString));
            if (string.IsNullOrEmpty(option.Value.ModelAssemblyName))
                throw new ArgumentNullException(nameof(option.Value.ModelAssemblyName));
            _option = option.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MappingEntityTypes(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void MappingEntityTypes(ModelBuilder modelBuilder)
        {
            var assembly = Assembly.Load(_option.ModelAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract &&
                t.GetInterfaces().Any(m => m.GetGenericTypeDefinition() == typeof(IBaseModel<>))).ToList();
            if (list != null && list.Any())
            {
                list.ForEach(t =>
                {
                    if (modelBuilder.Model.FindEntityType(t) == null)
                        modelBuilder.Model.AddEntityType(t);
                });
            }
        }

        public virtual Task AddAsync<T>(T entity) where T : class
        {
            return base.AddAsync(entity);
        }

        public virtual void AddRange<T>(ICollection<T> entities) where T : class
        {
            base.AddRange(entities);
        }

        public virtual Task AddRangeAsync<T>(ICollection<T> entities) where T : class
        {
            return base.AddRangeAsync(entities);
        }

        public virtual int Count<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().Count() : Set<T>().Count(@where);
        }

        public virtual Task<int> CountAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().CountAsync() : Set<T>().CountAsync(@where);
        }

        public void Delete<T, TKey>(TKey key) where T : class
        {
            var entity = Find<T>(key);
            Remove(entity);
        }

        public virtual bool EnsureCreated()
        {
            return Database.EnsureCreated();
        }

        public virtual Task<bool> EnsureCreatedAsync()
        {
            return Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// ExecuteSqlWithNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(sql,
                CancellationToken.None,
                parameters);
        }

        public virtual Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommandAsync(sql,
                CancellationToken.None,
                parameters);
        }

        /// <summary>
        /// edit an entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual void Edit<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Modified;
        }

        /// <summary>
        /// edit entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual void EditRange<T>(ICollection<T> entities) where T : class
        {
            Set<T>().AttachRange(entities.ToArray());
        }

        public bool Exist<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return @where == null ? Set<T>().Any() : Set<T>().Any(@where);
        }

        public IQueryable<T> FilterWithInclude<T>(Func<IQueryable<T>, IQueryable<T>> include, Expression<Func<T, bool>> @where) where T : class
        {
            var result = GetDbSet<T>().AsQueryable();
            if (where != null)
                result = GetDbSet<T>().Where(where);
            if (include != null)
                result = include(result);
            return result;
        }

        public Task<bool> ExistAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return @where == null ? Set<T>().AnyAsync() : Set<T>().AnyAsync(@where);
        }

        public T Find<T, TKey>(TKey key) where T : class
        {
            return base.Find<T>(key);
        }

        public Task<T> FindAsync<T, TKey>(TKey key) where T : class
        {
            return base.FindAsync<T>(key);
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            if (where == null)
                return Set<T>().AsNoTracking();
            return Set<T>().Where(where).AsNoTracking();
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        public T GetSingleOrDefault<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().SingleOrDefault() : Set<T>().SingleOrDefault(where);
        }

        public Task<T> GetSingleOrDefaultAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().SingleOrDefaultAsync() : Set<T>().SingleOrDefaultAsync(where);
        }

        /// <summary>
        /// update data by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        public virtual void Update<T>(T model, params string[] updateColumns) where T : class
        {
            if (updateColumns != null && updateColumns.Length > 0)
            {
                if (Entry(model).State == EntityState.Added ||
                    Entry(model).State == EntityState.Detached) Set<T>().Attach(model);
                foreach (var propertyName in updateColumns)
                {
                    Entry(model).Property(propertyName).IsModified = true;
                }
            }
            else
            {
                Entry(model).State = EntityState.Modified;
            }
        }

        public virtual int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return Set<T>().Where(where).Update(updateFactory);
        }

        public virtual Task<int> UpdateAsync<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return Set<T>().Where(where).UpdateAsync(updateFactory);
        }

        /// <summary>
        /// delete by query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public virtual int Delete<T>(Expression<Func<T, bool>> @where) where T : class
        {
            return Set<T>().Where(@where).Delete();
        }

        public virtual Task<int> DeleteAsync<T>(Expression<Func<T, bool>> @where) where T : class
        {
            return Set<T>().Where(@where).DeleteAsync();
        }

        /// <summary>
        /// bulk insert by sqlbulkcopy, and with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entities"></param>
        /// <param name="destinationTableName"></param>
        public virtual void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null) where T : class, IBaseModel<TKey>
        {
            if (!Database.IsSqlServer()&&!Database.IsMySql())
             throw new NotSupportedException("This method only supports for SQL Server or MySql.");
        }

        public virtual List<TView> SqlQuery<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToList();
        }

        public virtual Task<List<TView>> SqlQueryAsync<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToListAsync();
        }

        public int SaveChangesWithTriggers()
        {
            return this.SaveChangesWithTriggers(SaveChanges);
        }

        public int SaveChangesWithTriggers(bool acceptAllChangesOnSuccess)
        {
            return this.SaveChangesWithTriggers(SaveChanges, acceptAllChangesOnSuccess);
        }

        public Task<int> SaveChangesWithTriggersAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess: true,
                cancellationToken: cancellationToken);
        }

        public Task<int> SaveChangesWithTriggersAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.SaveChangesWithTriggersAsync(base.SaveChangesAsync, acceptAllChangesOnSuccess,
                cancellationToken: cancellationToken);
        }
    }
}
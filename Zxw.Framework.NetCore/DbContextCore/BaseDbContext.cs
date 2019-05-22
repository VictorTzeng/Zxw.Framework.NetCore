using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Z.EntityFramework.Plus;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public abstract class BaseDbContext : DbContext, IDbContextCore
    {
        public DbContextOption Option { get; }
        public DatabaseFacade GetDatabase() => Database;

        public new virtual int Add<T>(T entity) where T : class
        {
            base.Add(entity);
            return  SaveChanges();
        }

        protected BaseDbContext(DbContextOption option)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
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
            //if (string.IsNullOrEmpty(option.Value.ModelAssemblyName))
            //    throw new ArgumentNullException(nameof(option.Value.ModelAssemblyName));
            Option = option.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            MappingEntityTypes(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void MappingEntityTypes(ModelBuilder modelBuilder)
        {
            if (string.IsNullOrEmpty(Option.ModelAssemblyName))
                return;
            var assembly = Assembly.Load(Option.ModelAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract &&
                t.GetInterfaces().Any(m => m.IsAssignableFrom(typeof(BaseModel<>)))).ToList();
            if (list != null && list.Any())
            {
                list.ForEach(t =>
                {
                    if (modelBuilder.Model.FindEntityType(t) == null)
                        modelBuilder.Model.AddEntityType(t);
                });
            }
        }

        public virtual async Task<int> AddAsync<T>(T entity) where T : class
        {
            await base.AddAsync(entity);
            return await SaveChangesAsync();
        }

        public virtual int AddRange<T>(ICollection<T> entities) where T : class
        {
            base.AddRange(entities);
            return  SaveChanges();
        }

        public virtual async Task<int> AddRangeAsync<T>(ICollection<T> entities) where T : class
        {
            await base.AddRangeAsync(entities);
            return await SaveChangesAsync();
        }

        public virtual int Count<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().Count() : Set<T>().Count(@where);
        }

        public virtual async Task<int> CountAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return await (where == null ? Set<T>().CountAsync() : Set<T>().CountAsync(@where));
        }

        public virtual int Delete<T, TKey>(TKey key) where T : class
        {
            var entity = Find<T>(key);
            Remove(entity);
            return  SaveChanges();
        }

        public virtual bool EnsureCreated()
        {
            return Database.EnsureCreated();
        }

        public virtual async Task<bool> EnsureCreatedAsync()
        {
            return await Database.EnsureCreatedAsync();
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

        public virtual async Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlCommandAsync(sql,
                CancellationToken.None,
                parameters);
        }

        /// <summary>
        /// edit an entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual int Edit<T,TKey>(T entity) where T : BaseModel<TKey>
        {
            var dbModel = Find<T>(entity.Id);
            if (dbModel == null) return -1;
            //Entry(model).CurrentValues.SetValues(entity);
            var properties = typeof(T).GetProperties();
            var changedProperties = new List<string>();
            foreach (var property in properties)
            {
                var reflector = property.GetReflector();

                var value = reflector.GetValue(entity);
                var dbvalue = reflector.GetValue(dbModel);
                if (value != dbvalue)
                {
                    changedProperties.Add(property.Name);
                }
            }

            return Update(entity, changedProperties.ToArray());
        }

        /// <summary>
        /// edit entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int EditRange<T>(ICollection<T> entities) where T : class
        {
            Set<T>().AttachRange(entities.ToArray());
            return  SaveChanges();
        }

        public virtual bool Exist<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return @where == null ? Set<T>().Any() : Set<T>().Any(@where);
        }

        public virtual IQueryable<T> FilterWithInclude<T>(Func<IQueryable<T>, IQueryable<T>> include, Expression<Func<T, bool>> @where) where T : class
        {
            var result = GetDbSet<T>().AsQueryable();
            if (where != null)
                result = GetDbSet<T>().Where(where);
            if (include != null)
                result = include(result);
            return result;
        }

        public virtual async Task<bool> ExistAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return await (@where == null ? Set<T>().AnyAsync() : Set<T>().AnyAsync(@where));
        }

        public virtual T Find<T, TKey>(TKey key) where T : class
        {
            return base.Find<T>(key);
        }

        public virtual async Task<T> FindAsync<T, TKey>(TKey key) where T : class
        {
            return await base.FindAsync<T>(key);
        }

        public virtual IQueryable<T> Get<T>(Expression<Func<T, bool>> @where = null, bool asNoTracking = false) where T : class
        {
            var query = Set<T>().AsQueryable();
            if (where != null)
                query = query.Where(where);
            if (asNoTracking)
                query = query.AsNoTracking();
            return query;
        }

        public virtual DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        public virtual T GetSingleOrDefault<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return where == null ? Set<T>().SingleOrDefault() : Set<T>().SingleOrDefault(where);
        }

        public virtual async Task<T> GetSingleOrDefaultAsync<T>(Expression<Func<T, bool>> @where = null) where T : class
        {
            return await (where == null ? Set<T>().SingleOrDefaultAsync() : Set<T>().SingleOrDefaultAsync(where));
        }

        /// <summary>
        /// update data by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        public virtual int Update<T>(T model, params string[] updateColumns) where T : class
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
            return  SaveChanges();
        }

        public virtual int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return Set<T>().Where(where).Update(updateFactory);
        }

        public virtual async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return await Set<T>().Where(where).UpdateAsync(updateFactory);
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

        public virtual async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> @where) where T : class
        {
            return await Set<T>().Where(@where).DeleteAsync();
        }

        /// <summary>
        /// bulk insert by sqlbulkcopy, and with transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entities"></param>
        /// <param name="destinationTableName"></param>
        public virtual void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null) where T : BaseModel<TKey>
        {
            if (!Database.IsSqlServer()&&!Database.IsMySql())
             throw new NotSupportedException("This method only supports for SQL Server or MySql.");
        }

        public virtual List<TView> SqlQuery<T,TView>(string sql, params object[] parameters) 
            where T : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToList();
        }

        public virtual PaginationResult SqlQueryByPagnation<T, TView>(string sql, string[] orderBys, int pageIndex, int pageSize,
            Action<TView> eachAction = null) where T : class where TView : class
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<TView>> SqlQueryAsync<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return await Set<T>().FromSql(sql, parameters).Cast<TView>().ToListAsync();
        }
    }
}
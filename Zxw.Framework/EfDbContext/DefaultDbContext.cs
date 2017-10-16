using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Z.EntityFramework.Plus;
using Zxw.Framework.Extensions;
using Zxw.Framework.Models;
using Zxw.Framework.Options;

namespace Zxw.Framework.EfDbContext
{
    public sealed class DefaultDbContext : DbContext
    {
        private DbContextOption _option;

        static DefaultDbContext()
        {
            //当模型属性发生改变时重建数据库
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<DefaultDbContext>());
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="option"></param>
        public DefaultDbContext(DbContextOption option) : base(option.ConnectionString)
        {
            if(option==null)
                throw new ArgumentNullException(nameof(option));
            if (string.IsNullOrEmpty(option.ModelAssemblyName))
                throw new ArgumentNullException(nameof(option.ModelAssemblyName));
            _option = option;
            this.Configuration.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var assembly = Assembly.Load(_option.ModelAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract &&
                t.GetInterfaces().Any(m => m.GetGenericTypeDefinition() == typeof(IBaseModel<>))).ToList();
            if (list != null && list.Any())
            {
                list.ToList().ForEach(modelBuilder.RegisterEntityType);
            }
            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// ExecuteSqlWithNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql,
                CancellationToken.None,
                parameters);
        }

        /// <summary>
        /// Custom SqlQuery
        /// </summary>
        /// <typeparam name="TView">ViewModel</typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IList<TView> SqlQuery<TView>(string sql, params object[] parameters) where TView : class, new()
        {
            return Database.SqlQuery<TView>(sql, parameters).ToList();
        }

        /// <summary>
        /// edit an entity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Edit<T>(T entity) where T : class
        {
            Entry(entity).State = EntityState.Modified;
            return SaveChanges();
        }

        /// <summary>
        /// edit entities.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int EditRange<T>(ICollection<T> entities) where T : class
        {
            Set<T>().AddOrUpdate(entities.ToArray());
            return SaveChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="updateExp"></param>
        /// <returns></returns>
        public int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
            where T : class
        {
            return Set<T>().Where(@where).Update(updateExp);
        }

        public int Update<T>(T model, params string[] updateColumns) where T : class
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
            return SaveChanges();
        }


        public int Delete<T>(Expression<Func<T, bool>> @where) where T : class
        {
            Set<T>().Where(@where).Delete();
            return SaveChanges();
        }

        /// <summary>
        /// bulk insert by sqlbulkcopy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="entities"></param>
        /// <param name="destinationTableName"></param>
        public void BulkInsert<T>(IList<T> entities, string destinationTableName = null) where T : class
        {
            if (entities == null || !entities.Any()) return;
            if (string.IsNullOrEmpty(destinationTableName))
            {
                var mappingTableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
                destinationTableName = string.IsNullOrEmpty(mappingTableName) ? typeof(T).Name : mappingTableName; 
            }
            using (var dt = entities.ToDataTable())
            {
                using (var conn = new SqlConnection(_option.ConnectionString))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran))
                            {
                                bulk.BatchSize = entities.Count;
                                bulk.DestinationTableName = destinationTableName;
                                bulk.EnableStreaming = true;
                                bulk.WriteToServerAsync(dt);
                                tran.Commit();
                            }
                        }
                        catch (Exception)
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                    conn.Close();
                }
            }
        }
    }
}
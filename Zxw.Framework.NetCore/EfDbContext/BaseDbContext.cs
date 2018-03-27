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
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public Task<int> AddAsync<T>(T entity) where T : class
        {
            Set<T>().AddAsync(entity);
            return SaveChangesAsync();
        }

        public Task<int> AddRangeAsync<T>(ICollection<T> entities) where T : class
        {
            Set<T>().AddRangeAsync(entities);
            return SaveChangesAsync();
        }

        public DatabaseFacade GetDatabase()
        {
            return Database;
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        /// <summary>
        /// ExecuteSqlWithNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlCommand(sql,
                CancellationToken.None,
                parameters);
        }

        public Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
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
            Set<T>().AttachRange(entities.ToArray());
            return SaveChanges();
        }

        /// <summary>
        /// update data by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
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

        public int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return Set<T>().Where(where).Update(updateFactory);
        }

        public Task<int> UpdateAsync<T>(Expression<Func<T, bool>> @where, Expression<Func<T,T>> updateFactory) where T : class
        {
            return Set<T>().Where(where).UpdateAsync(updateFactory);
        }

        /// <summary>
        /// delete by query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(Expression<Func<T, bool>> @where) where T : class
        {
            return Set<T>().Where(@where).Delete();
        }

        public Task<int> DeleteAsync<T>(Expression<Func<T, bool>> @where) where T : class
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
        public void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null) where T : class, IBaseModel<TKey>
        {
            if (entities == null || !entities.Any()) return;
            if (string.IsNullOrEmpty(destinationTableName))
            {
                var mappingTableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
                destinationTableName = string.IsNullOrEmpty(mappingTableName) ? typeof(T).Name : mappingTableName;
            }
            if (Database.IsSqlServer())
                SqlBulkInsert<T,TKey>(entities, destinationTableName);
            else if (Database.IsMySql())
                MySqlBulkInsert(entities, destinationTableName);
            else throw new NotSupportedException("This method only support for SQL Server or MySql.");

        }

        private void SqlBulkInsert<T,TKey>(IList<T> entities, string destinationTableName = null) where T : class,IBaseModel<TKey>
        {
            using (var dt = entities.ToDataTable())
            {
                dt.TableName = destinationTableName;
                using (var conn = Database.GetDbConnection() as SqlConnection ?? new SqlConnection(_option.ConnectionString))
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, tran)
                            {
                                BatchSize = entities.Count,
                                DestinationTableName = dt.TableName,
                            };
                            GenerateColumnMappings<T, TKey>(bulk.ColumnMappings);
                            bulk.WriteToServerAsync(dt);
                            tran.Commit();
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

        private void GenerateColumnMappings<T, TKey>(SqlBulkCopyColumnMappingCollection mappings)
            where T : class, IBaseModel<TKey>
        {
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                if (property.GetCustomAttributes<KeyAttribute>().Any())
                {
                    mappings.Add(new SqlBulkCopyColumnMapping(property.Name, typeof(T).Name + property.Name));
                }
                else
                {
                    mappings.Add(new SqlBulkCopyColumnMapping(property.Name, property.Name));                    
                }
            }
        }

        private void MySqlBulkInsert<T>(IList<T> entities, string destinationTableName) where T : class
        {
            var tmpDir = Path.Combine(AppContext.BaseDirectory, "Temp");
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);
            var csvFileName = Path.Combine(tmpDir, $"{DateTime.Now:yyyyMMddHHmmssfff}.csv");
            if (!File.Exists(csvFileName))
                File.Create(csvFileName);
            var separator = ",";
            entities.SaveToCsv(csvFileName, separator);
            using (var conn = Database.GetDbConnection() as MySqlConnection ?? new MySqlConnection(_option.ConnectionString))
            {
                conn.Open();
                var bulk = new MySqlBulkLoader(conn)
                {
                    NumberOfLinesToSkip = 0,
                    TableName = destinationTableName,
                    FieldTerminator = separator,
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n"
                };
                bulk.LoadAsync();
                conn.Close();
            }
            File.Delete(csvFileName);
        }

        public List<TView> SqlQuery<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToList();
        }

        public Task<List<TView>> SqlQueryAsync<T,TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class
        {
            return Set<T>().FromSql(sql, parameters).Cast<TView>().ToListAsync();
        }

        public Task<int> SaveAsync()
        {
            return this.SaveChangesAsync();
        }

        public Task<int> SaveAsync(bool acceptAllChangesOnSuccess)
        {
            return this.SaveChangesAsync(acceptAllChangesOnSuccess);
        }
    }
}
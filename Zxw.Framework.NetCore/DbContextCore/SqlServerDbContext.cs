﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class SqlServerDbContext:BaseDbContext, ISqlServerDbContext
    {
        
        public SqlServerDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public SqlServerDbContext(DbContextOption option) : base(option)
        {

        }
        //public SqlServerDbContext(DbContextOptions options) : base(options)
        //{

        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        public override void BulkInsert<T>(IList<T> entities, string destinationTableName = null)
        {
            if (entities == null || !entities.Any()) return;
            if (string.IsNullOrEmpty(destinationTableName))
            {
                var mappingTableName = typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
                destinationTableName = string.IsNullOrEmpty(mappingTableName) ? typeof(T).Name : mappingTableName;
            }
            SqlBulkInsert<T>(entities, destinationTableName);
        }

        private void SqlBulkInsert<T>(IList<T> entities, string destinationTableName = null) where T : class 
        {
            using (var dt = entities.ToDataTable())
            {
                dt.TableName = destinationTableName;
                var conn = (SqlConnection)Database.GetDbConnection();
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
                        GenerateColumnMappings<T>(bulk.ColumnMappings);
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

        private void GenerateColumnMappings<T>(SqlBulkCopyColumnMappingCollection mappings)
            where T : class 
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

        public override PaginationResult SqlQueryByPagination<T, TView>(string sql, string[] orderBys, int pageIndex, int pageSize,
            Action<TView> eachAction = null)
        {
            var total = SqlQuery<int>($"select count(1) from ({sql}) as s").FirstOrDefault();
            var jsonResults = SqlQuery<TView>(
                    $"select * from (select *,row_number() over (order by {string.Join(",", orderBys)}) as RowId from ({sql}) as s) as t where RowId between {pageSize * (pageIndex - 1) + 1} and {pageSize * pageIndex} order by {string.Join(",", orderBys)}")
                .ToList();
            if (eachAction != null)
            {
                jsonResults = jsonResults.Each(eachAction).ToList();
            }

            return new PaginationResult(true, string.Empty, jsonResults)
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                total = total
            };
        }

        public override DataTable GetDataTable(string sql, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            return GetDataTables(sql, cmdTimeout, parameters).FirstOrDefault();
        }

        public override PaginationResult SqlQueryByPagination<T>(string sql, string[] orderBys, int pageIndex, int pageSize,
            params DbParameter[] parameters)
        {
            var total = (int)this.ExecuteScalar($"select count(1) from ({sql}) as s");
            var jsonResults = GetDataTable(
                    $"select * from (select *,row_number() over (order by {string.Join(",", orderBys)}) as RowId from ({sql}) as s) as t where RowId between {pageSize * (pageIndex - 1) + 1} and {pageSize * pageIndex} order by {string.Join(",", orderBys)}")
                .ToList<T>();
            return new PaginationResult(true, string.Empty, jsonResults)
            {
                pageIndex = pageIndex,
                pageSize = pageSize,
                total = total
            };
        }

        //public override List<DataTable> GetDataTables(string sql, int cmdTimeout = 30, params DbParameter[] parameters)
        //{
        //    var dts = new List<DataTable>();
        //    //TODO： connection 不能dispose 或者 用using，否则下次获取connection会报错提示“the connectionstring property has not been initialized。”
        //    var connection = Database.GetDbConnection();
        //    if (connection.State != ConnectionState.Open)
        //        connection.Open();

        //    using (var cmd = new SqlCommand(sql, (SqlConnection) connection))
        //    {
        //        cmd.CommandTimeout = cmdTimeout;
        //        if (parameters != null && parameters.Length > 0)
        //        {
        //            cmd.Parameters.AddRange(parameters);
        //        }
                
        //        using (var da = new SqlDataAdapter(cmd))
        //        {
        //            using (var ds = new DataSet())
        //            {
        //                da.Fill(ds);
        //                foreach (DataTable table in ds.Tables)
        //                {
        //                    dts.Add(table);
        //                }
        //            }
        //        }
        //    }
        //    connection.Close();

        //    return dts;
        //}
    }
}

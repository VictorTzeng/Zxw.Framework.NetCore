using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.IDbContext;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class SqlOperatorUtility: ISqlOperatorUtility
    {
        private IDbContextCore DbContext { get; }

        public SqlOperatorUtility(IDbContextCore context)
        {
            this.DbContext = context;
        }

        private DbConnection GetDbConnection()
        {
            var connection = DbContext.GetDatabase().GetDbConnection();
            if(connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }

        private DbCommand BuildDbCommand(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30,
            params DbParameter[] parameters)
        {
            var cmd = GetDbConnection().CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = cmdTimeout;
            cmd.CommandType = cmdType;
            parameters.Each(p => { cmd.Parameters.Add(p); });
            return cmd;
        }
        private DbCommand BuildDbCommandWithTransaction(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30,
            params DbParameter[] parameters)
        {
            var connection = GetDbConnection();
            var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = cmdTimeout;
            cmd.CommandType = cmdType;
            parameters.Each(p => { cmd.Parameters.Add(p); });
            var tran = connection.BeginTransaction();
            cmd.Transaction = tran;
            return cmd;
        }

        public DataTable SqlQuery(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            using (var cmd = BuildDbCommand(sql, CommandType.Text, cmdTimeout, parameters))
            {
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                var dt = reader.Fill();
                reader.Close();
                cmd.Connection.Close();
                return dt;
            }
        }

        public DataSet SqlQuery(List<string> sqlList, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            var ds = new DataSet();
            var sql = new StringBuilder();
            sqlList.Each(s => sql.AppendLine(s.TrimEnd(';') + ";"));
            using (var cmd = BuildDbCommand(sql.ToString(), CommandType.Text, cmdTimeout, parameters))
            {
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                var dt = reader.Fill();

                ds.Tables.Add(dt);

                while (reader.NextResult())
                {
                    dt = reader.Fill();
                    ds.Tables.Add(dt);
                }

                reader.Close();
                cmd.Connection.Close();
                return ds;
            }
        }

        public IList<T> SqlQuery<T>(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            using (var cmd = BuildDbCommand(sql, cmdType, cmdTimeout, parameters))
            {
                var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                var list = reader.ToList<T>();
                reader.Close();
                cmd.Connection.Close();
                return list.ToList();
            }
        }

        public int ExecuteSqlCommand(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            using (var cmd = BuildDbCommand(sql, cmdType, cmdTimeout, parameters))
            {
                var ret = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                return ret;
            }
        }

        public int ExecuteSqlCommandWithTransaction(List<string> sqlList, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            var sql = new StringBuilder();
            sqlList.Each(s => sql.AppendLine(s.TrimEnd(';') + ";"));
            using (var cmd = BuildDbCommandWithTransaction(sql.ToString(), CommandType.Text, cmdTimeout, parameters))
            {
                var ret = -1;
                try
                {
                    ret = cmd.ExecuteNonQuery();
                    cmd.Transaction.Commit();
                }
                catch (Exception)
                {
                    cmd.Transaction.Rollback();
                    throw;
                }
                finally
                {
                    cmd.Connection.Close();
                }
                return ret;
            }
        }

        public object ExecuteScalar(string sql, CommandType cmdType = CommandType.Text, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            using (var cmd = BuildDbCommand(sql, cmdType, cmdTimeout, parameters))
            {
                var ret = cmd.ExecuteScalar();
                cmd.Connection.Close();
                return ret;
            }
        }

        /// <summary>
        /// 执行存储过程返回IEnumerable数据集
        /// </summary>
        public IList<T> ExecuteStoredProcedure<T>(string sql, int cmdTimeout = 30, DbParameter[] sqlParams = null) where T : new()
        {
            return SqlQuery<T>(sql, CommandType.StoredProcedure, cmdTimeout, sqlParams);
        }

        public void ClearDataTables(params string[] tables)
        {
            if (tables == null)
            {
                var tableList = new List<string>();
                var types = DbContext.GetAllEntityTypes();
                if (types.Any())
                {
                    foreach (var type in types)
                    {
                        var tableName = type.ClrType.GetCustomAttribute<TableAttribute>()?.Name;
                        if (tableName.IsNullOrWhiteSpace())
                            tableName = type.ClrType.Name;
                        tableList.Add(tableName);
                    }
                }
                else
                {
                    tableList.AddRange(DbContext.GetCurrentDatabaseTableList().Select(m => m.TableName));
                }

                tables = tableList.ToArray();
            }

            var sql = new List<string>();
            foreach (var table in tables)
            {
                sql.Add($"delete from {table};");
            }

            this.ExecuteSqlCommandWithTransaction(sql);
        }
    }
}

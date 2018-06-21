using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using Zxw.Framework.NetCore.DbContextCore;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class EntityFrameworkCoreExtensions
    {
        public static DataTable GetDataTable(this IDbContextCore context, string sql, params DbParameter[] parameters)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            db.EnsureCreated();
            var connection = db.GetDbConnection();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            var ds = new DataSet();
            var dt = new DataTable();
            DbCommand cmd;
            DataAdapter da;
            if (db.IsSqlServer())
            {
                cmd = new SqlCommand(sql, (SqlConnection) connection);
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                da = new SqlDataAdapter((SqlCommand) cmd);
            }
            else if (db.IsMySql())
            {
                cmd = new MySqlCommand(sql, (MySqlConnection) connection);
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                da = new MySqlDataAdapter((MySqlCommand) cmd);
            }
            else if (db.IsNpgsql())
            {
                cmd = new NpgsqlCommand(sql, (NpgsqlConnection) connection);
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                da = new NpgsqlDataAdapter((NpgsqlCommand) cmd);
            }
            else if (db.IsSqlite())
            {
                cmd = new SqliteCommand(sql, (SqliteConnection) connection);
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                dt = cmd.ExecuteReader().GetSchemaTable();
                cmd.Dispose();
                connection.Close();
                return dt;
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            da.Fill(ds);
            dt = ds.Tables[0];
            da.Dispose();
            connection.Close();
            return dt;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Npgsql;
using StackExchange.Redis.Extensions.Core.Extensions;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Models;

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
                throw new NotSupportedException("This method does not support current database yet.");
            }

            da.Fill(ds);
            dt = ds.Tables[0];
            da.Dispose();
            connection.Close();
            return dt;
        }

        public static DataTable GetCurrentDatabaseAllTables(this IDbContextCore context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            var sql = string.Empty;
            if (db.IsSqlServer())
            {
                sql = "select * from (SELECT (case when a.colorder=1 then d.name else '' end) as TableName," +
                      "(case when a.colorder=1 then isnull(f.value,'') else '' end) as TableComment" +
                      " FROM syscolumns a" +
                      " inner join sysobjects d on a.id=d.id  and d.xtype='U' and  d.name<>'dtproperties'" +
                      " left join sys.extended_properties f on d.id=f.major_id and f.minor_id=0) t" +
                      " where t.TableName!=''";
            }
            else if (db.IsMySql())
            {
                sql =
                    "SELECT TABLE_NAME as TableName," +
                    " Table_Comment as TableComment" +
                    " FROM INFORMATION_SCHEMA.TABLES" +
                    $" where TABLE_SCHEMA = '{db.GetDbConnection().Database}'";
            }
            else if (db.IsNpgsql())
            {
                sql =
                    "select relname as TableName," +
                    " cast(obj_description(relfilenode,'pg_class') as varchar) as TableComment" +
                    " from pg_class c" +
                    " where relkind = 'r' and relname not like 'pg_%' and relname not like 'sql_%'" +
                    " order by relname";
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            return context.GetDataTable(sql);
        }

        public static DataTable GetTableColumns(this IDbContextCore context, string tableName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            var sql = string.Empty;
            if (db.IsSqlServer())
            {
                sql = "SELECT a.name as ColName," +
                      "CONVERT(bit,(case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then 1 else 0 end)) as IsIdentity, " +
                      "CONVERT(bit,(case when (SELECT count(*) FROM sysobjects  WHERE (name in (SELECT name FROM sysindexes  WHERE (id = a.id) AND (indid in  (SELECT indid FROM sysindexkeys  WHERE (id = a.id) AND (colid in  (SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))  AND (xtype = 'PK'))>0 then 1 else 0 end)) as IsPrimaryKey," +
                      "b.name as ColumnType," +
                      "COLUMNPROPERTY(a.id,a.name,'PRECISION') as ColumnLength," +
                      "CONVERT(bit,(case when a.isnullable=1 then 1 else 0 end)) as IsNullable,  " +
                      "isnull(e.text,'') as DefaultValue," +
                      "isnull(g.[value], ' ') AS Comment " +
                      "FROM  syscolumns a left join systypes b on a.xtype=b.xusertype  inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties' left join syscomments e on a.cdefault=e.id  left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id left join sys.extended_properties f on d.id=f.class and f.minor_id=0 " +
                      $"where b.name is not null and d.name='{tableName}' order by a.id,a.colorder";
            }
            else if (db.IsMySql())
            {
                sql =
                    "select column_name as ColName, " +
                    " column_default as DefaultValue," +
                    " IF(extra = 'auto_increment','TRUE','FALSE') as IsIdentity," +
                    " IF(is_nullable = 'YES','TRUE','FALSE') as IsNullable," +
                    " DATA_TYPE as ColumnType," +
                    " CHARACTER_MAXIMUM_LENGTH as ColumnLength," +
                    " IF(COLUMN_KEY = 'PRI','TRUE','FALSE') as IsPrimaryKey," +
                    " COLUMN_COMMENT as Comment " +
                    $" from information_schema.columns where table_schema = '{db.GetDbConnection().Database}' and table_name = '{tableName}'";
            }
            else if (db.IsNpgsql())
            {
                sql =
                    "select column_name as ColName," +
                    "data_type as ColumnType," +
                    "coalesce(character_maximum_length, numeric_precision, -1) as ColumnLength," +
                    "CAST((case is_nullable when 'NO' then 0 else 1 end) as bool) as IsNullable," +
                    "column_default as DefaultValue," +
                    "CAST((case when position('nextval' in column_default)> 0 then 1 else 0 end) as bool) as IsIdentity, " +
                    "CAST((case when b.pk_name is null then 0 else 1 end) as bool) as IsPrimaryKey," +
                    "c.DeText as Comment" +
                    " from information_schema.columns" +
                    " left join " +
                    " (select pg_attr.attname as colname,pg_constraint.conname as pk_name from pg_constraint " +
                    " inner join pg_class on pg_constraint.conrelid = pg_class.oid" +
                    " inner join pg_attribute pg_attr on pg_attr.attrelid = pg_class.oid and  pg_attr.attnum = pg_constraint.conkey[1]" +
                    $" inner join pg_type on pg_type.oid = pg_attr.atttypid where pg_class.relname = '{tableName}' and pg_constraint.contype = 'p') b on b.colname = information_schema.columns.column_name " +
                    " left join " +
                    " (select attname, description as DeText from pg_class " +
                    " left join pg_attribute pg_attr on pg_attr.attrelid = pg_class.oid" +
                    " left join pg_description pg_desc on pg_desc.objoid = pg_attr.attrelid and pg_desc.objsubid = pg_attr.attnum " +
                    $" where pg_attr.attnum > 0 and pg_attr.attrelid = pg_class.oid and pg_class.relname = '{tableName}') c on c.attname = information_schema.columns.column_name" +
                    $" where table_schema = 'public' and table_name = '{tableName}' order by ordinal_position asc";
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            return context.GetDataTable(sql);
        }

        public static IList<DbTable> GetCurrentDatabaseTableList(this IDbContextCore context)
        {
            var tables = context.GetCurrentDatabaseAllTables().ToList<DbTable>();
            var db = context.GetDatabase();
            DatabaseType dbType;
            if (db.IsSqlServer())
                dbType = DatabaseType.MSSQL;
            else if (db.IsMySql())
                dbType = DatabaseType.MySQL;
            else if (db.IsNpgsql())
            {
                dbType = DatabaseType.PostgreSQL;
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }
            tables.ForEach(item =>
            {
                item.Columns = context.GetTableColumns(item.TableName).ToList<DbTableColumn>();
                item.Columns.ForEach(x =>
                {
                    var csharpType = DbColumnTypeCollection.DbColumnDataTypes.FirstOrDefault(t =>
                        t.DatabaseType == dbType && t.ColumnTypes.Split(',').Any(p =>
                            p.Trim().Equals(x.ColumnType, StringComparison.OrdinalIgnoreCase)))?.CSharpType;
                    if (string.IsNullOrEmpty(csharpType))
                    {
                        throw new SqlTypeException($"未从字典中找到\"{x.ColumnType}\"对应的C#数据类型，请更新DbColumnTypeCollection类型映射字典。");
                    }

                    x.CSharpType = csharpType;
                });
            });
            return tables;
        }
    }
}

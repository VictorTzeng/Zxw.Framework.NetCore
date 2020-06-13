using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis.Extensions.Core.Extensions;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class EntityFrameworkCoreExtensions
    {
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
            else if (db.IsOracle())
            {
                sql =
                    "select \"a\".TABLE_NAME as \"TableName\",\"b\".COMMENTS as \"TableComment\" from USER_TABLES \"a\" JOIN user_tab_comments \"b\" on \"b\".TABLE_NAME=\"a\".TABLE_NAME";
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }

            return context.GetDataTable(sql);
        }

        public static DataTable GetTableColumns(this IDbContextCore context, params string[] tableNames)
        {
            if(tableNames == null || tableNames.Length == 0)
                return null;
            if (context == null) throw new ArgumentNullException(nameof(context));
            var db = context.GetDatabase();
            var sql = string.Empty;
            if (db.IsSqlServer())
            {
                sql = "SELECT d.name as TableName,a.name as ColName," +
                      "CONVERT(bit,(case when COLUMNPROPERTY(a.id,a.name,'IsIdentity')=1 then 1 else 0 end)) as IsIdentity, " +
                      "CONVERT(bit,(case when (SELECT count(*) FROM sysobjects  WHERE (name in (SELECT name FROM sysindexes  WHERE (id = a.id) AND (indid in  (SELECT indid FROM sysindexkeys  WHERE (id = a.id) AND (colid in  (SELECT colid FROM syscolumns WHERE (id = a.id) AND (name = a.name)))))))  AND (xtype = 'PK'))>0 then 1 else 0 end)) as IsPrimaryKey," +
                      "b.name as ColumnType," +
                      "COLUMNPROPERTY(a.id,a.name,'PRECISION') as ColumnLength," +
                      "CONVERT(bit,(case when a.isnullable=1 then 1 else 0 end)) as IsNullable,  " +
                      "isnull(e.text,'') as DefaultValue," +
                      "isnull(g.[value], ' ') AS Comments " +
                      "FROM  syscolumns a left join systypes b on a.xtype=b.xusertype  inner join sysobjects d on a.id=d.id and d.xtype='U' and d.name<>'dtproperties' left join syscomments e on a.cdefault=e.id  left join sys.extended_properties g on a.id=g.major_id AND a.colid=g.minor_id left join sys.extended_properties f on d.id=f.class and f.minor_id=0 " +
                      $"where b.name is not null and d.name in ({tableNames.Select(m=>$"'{m}'").Join(",")}) order by a.id,a.colorder";
            }
            else if (db.IsMySql())
            {
                sql =
                    "select table_name as TableName,column_name as ColName, " +
                    " column_default as DefaultValue," +
                    " IF(extra = 'auto_increment','TRUE','FALSE') as IsIdentity," +
                    " IF(is_nullable = 'YES','TRUE','FALSE') as IsNullable," +
                    " DATA_TYPE as ColumnType," +
                    " CHARACTER_MAXIMUM_LENGTH as ColumnLength," +
                    " IF(COLUMN_KEY = 'PRI','TRUE','FALSE') as IsPrimaryKey," +
                    " COLUMN_COMMENT as Comments " +
                    $" from information_schema.columns where table_schema = '{db.GetDbConnection().Database}' and table_name in ({tableNames.Select(m=>$"'{m}'").Join(",")})";
            }
            else if (db.IsNpgsql())
            {
                sql =
                    "select table_name as TableName,column_name as ColName," +
                    "data_type as ColumnType," +
                    "coalesce(character_maximum_length, numeric_precision, -1) as ColumnLength," +
                    "CAST((case is_nullable when 'NO' then 0 else 1 end) as bool) as IsNullable," +
                    "column_default as DefaultValue," +
                    "CAST((case when position('nextval' in column_default)> 0 then 1 else 0 end) as bool) as IsIdentity, " +
                    "CAST((case when b.pk_name is null then 0 else 1 end) as bool) as IsPrimaryKey," +
                    "c.DeText as Comments" +
                    " from information_schema.columns" +
                    " left join " +
                    " (select pg_attr.attname as colname,pg_constraint.conname as pk_name from pg_constraint " +
                    " inner join pg_class on pg_constraint.conrelid = pg_class.oid" +
                    " inner join pg_attribute pg_attr on pg_attr.attrelid = pg_class.oid and  pg_attr.attnum = pg_constraint.conkey[1]" +
                    $" inner join pg_type on pg_type.oid = pg_attr.atttypid where pg_class.relname in ({tableNames.Select(m=>$"'{m}'").Join(",")}) and pg_constraint.contype = 'p') b on b.colname = information_schema.columns.column_name " +
                    " left join " +
                    " (select attname, description as DeText from pg_class " +
                    " left join pg_attribute pg_attr on pg_attr.attrelid = pg_class.oid" +
                    " left join pg_description pg_desc on pg_desc.objoid = pg_attr.attrelid and pg_desc.objsubid = pg_attr.attnum " +
                    $" where pg_attr.attnum > 0 and pg_attr.attrelid = pg_class.oid and pg_class.relname in ({tableNames.Select(m=>$"'{m}'").Join(",")})) c on c.attname = information_schema.columns.column_name" +
                    $" where table_schema = 'public' and table_name in ({tableNames.Select(m=>$"'{m}'").Join(",")}) order by ordinal_position asc";
            }
            else if (db.IsOracle())
            {
                sql = "select a.Table_Name as TableName,"
                      + "a.DATA_LENGTH as ColumnLength,"
                      + "a.COLUMN_NAME as ColName,"
                      + "a.DATA_PRECISION as DataPrecision,"
                      + "a.DATA_SCALE as DataScale,"
                      + "a.DATA_TYPE as ColumnType,"
                      + "decode(a.NULLABLE, 'Y', 'TRUE', 'N', 'FALSE') as IsNullable,"
                      + "case when d.COLUMN_NAME is null then 'FALSE' else 'TRUE' end as IsPrimaryKey,"
                      + "decode(a.IDENTITY_COLUMN, 'YES', 'TRUE', 'NO', 'FALSE') as IsIdentity,"
                      + "b.COMMENTS as Comments "
                      + "from user_tab_columns a "
                      + "left join user_tab_comments b on b.TABLE_NAME = a.COLUMN_NAME "
                      + "left join user_constraints c on c.TABLE_NAME = a.TABLE_NAME and c.CONSTRAINT_TYPE = 'P' "
                      + "left join user_cons_columns d on d.CONSTRAINT_NAME = c.CONSTRAINT_NAME and d.COLUMN_NAME = a.COLUMN_NAME "
                      + $"where a.Table_Name in ({tableNames.Select(m=>$"'{m.ToUpper()}'").Join(",")})";
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
            else if(db.IsOracle())
            {
                dbType = DatabaseType.Oracle;
            }
            else
            {
                throw new NotImplementedException("This method does not support current database yet.");
            }
            var columns = context.GetTableColumns(tables.Select(m=>m.TableName).ToArray()).ToList<DbTableColumn>();
            tables.ForEach(item =>
            {
                var dt = context.GetDataTable($"select * from [{item.TableName}] where 1 != 1");
                item.Columns = columns.Where(m=>m.TableName == item.TableName).ToList();
                item.Columns.ForEach(x =>
                {
                    x.CSharpType = dt.Columns[x.ColName].DataType.Name;
                });
            });
            return tables;
        }


        /// <summary>
        /// 执行SQL返回受影响的行数
        /// </summary>
        public static int ExecuteSqlNoQuery(this IDbContextCore context, string sql, DbParameter[] sqlParams = null)
        {
            return ExecuteNoQuery(context, sql, sqlParams);
        }
        /// <summary>
        /// 执行存储过程返回IEnumerable数据集
        /// </summary>
        public static IEnumerable<T> ExecuteStoredProcedure<T>(this IDbContextCore context, string sql, DbParameter[] sqlParams = null) where T : new()
        {
            return Execute<T>(context, sql, CommandType.StoredProcedure, sqlParams);
        }
        /// <summary>
        /// 执行sql返回IEnumerable数据集
        /// </summary>
        public static IEnumerable<T> ExecuteSqlReader<T>(this IDbContextCore context, string sql, DbParameter[] sqlParams = null) where T : new()
        {
            return Execute<T>(context, sql, CommandType.Text, sqlParams);
        }
        private static int ExecuteNoQuery(this IDbContextCore context, string sql, DbParameter[] sqlParams)
        {
            var db = context.GetDatabase();
            var connection = db.GetDbConnection();
            var cmd = connection.CreateCommand();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (sqlParams != null)
            {
                cmd.Parameters.AddRange(sqlParams);
            }
            var result = cmd.ExecuteNonQuery();
            connection.Close();
            return result;
        }
        private static IEnumerable<T> Execute<T>(this IDbContextCore context, string sql, CommandType type, params DbParameter[] sqlParams) where T : new()
        {
            var db = context.GetDatabase();
            var connection = db.GetDbConnection();
            var cmd = connection.CreateCommand();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            cmd.CommandText = sql;
            cmd.CommandType = type;
            if (sqlParams != null)
            {
                cmd.Parameters.AddRange(sqlParams);
            }

            var reader = cmd.ExecuteReader();
            var result = reader.ToList<T>();
            connection.Close();
            return result;
        }

        public static void ClearDatabase(this IDbContextCore context)
        {
            context.ClearDataTables();
        }

        public static void ClearDataTables(this IDbContextCore context, params string[] tables)
        {
            if (tables == null)
            {
                var tableList = new List<string>();
                var types = context.GetAllEntityTypes();
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
                    tableList.AddRange(context.GetCurrentDatabaseTableList().Select(m=>m.TableName));
                }

                tables = tableList.ToArray();
            }

            var sql = new StringBuilder();
            foreach (var table in tables)
            {
                sql.AppendLine($"delete from {table};");
            }
            context.ExecuteSqlWithNonQuery(sql.ToString());
        }

        public static object ExecuteScalar(this IDbContextCore context, string sql, params DbParameter[] sqlParams)
        {
            var db = context.GetDatabase();
            var connection = db.GetDbConnection();
            var cmd = connection.CreateCommand();
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            if (sqlParams != null)
            {
                cmd.Parameters.AddRange(sqlParams);
            }

            var result = cmd.ExecuteScalar();
            connection.Close();
            return result;
        }
    }
}

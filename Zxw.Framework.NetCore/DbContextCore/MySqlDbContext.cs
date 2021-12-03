using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class MySqlDbContext: BaseDbContext, IMySqlDbContext
    {
        public MySqlDbContext(DbContextOption option) : base(option)
        {

        }
        public MySqlDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public MySqlDbContext(DbContextOptions options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ServerVersion.AutoDetect(Option.ConnectionString));
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
            MySqlBulkInsert(entities, destinationTableName);
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
            var conn = (MySqlConnection) Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
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
            File.Delete(csvFileName);
        }
        public override DataTable GetDataTable(string sql, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            return GetDataTables(sql, cmdTimeout, parameters).FirstOrDefault();
        }

        public override List<DataTable> GetDataTables(string sql, int cmdTimeout = 30, params DbParameter[] parameters)
        {
            var dts = new List<DataTable>();
            //TODO： connection 不能dispose 或者 用using，否则下次获取connection会报错提示“the connectionstring property has not been initialized。”
            var connection = Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var cmd = new MySqlCommand(sql, (MySqlConnection) connection))
            {
                cmd.CommandTimeout = cmdTimeout;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                
                using (var da = new MySqlDataAdapter(cmd))
                {
                    using (var ds = new DataSet())
                    {
                        da.Fill(ds);
                        foreach (DataTable table in ds.Tables)
                        {
                            dts.Add(table);
                        }
                    }
                }
            }
            connection.Close();

            return dts;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseMySql(Option.ConnectionString);
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
            using (var conn = Database.GetDbConnection() as MySqlConnection ?? new MySqlConnection(Option.ConnectionString))
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

    }
}

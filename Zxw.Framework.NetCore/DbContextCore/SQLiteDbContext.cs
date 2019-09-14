using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class SQLiteDbContext:BaseDbContext, ISQLiteDbContext
    {
        public SQLiteDbContext(DbContextOption option) : base(option)
        {

        }
        public SQLiteDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public SQLiteDbContext(DbContextOptions options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        public override DataTable GetDataTable(string sql, params DbParameter[] parameters)
        {
            return GetDataTables(sql, parameters).FirstOrDefault();
        }

        public override List<DataTable> GetDataTables(string sql, params DbParameter[] parameters)
        {
            var dts = new List<DataTable>();
            //TODO： connection 不能dispose 或者 用using，否则下次获取connection会报错提示“the connectionstring property has not been initialized。”
            var connection = Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var cmd = new SqliteCommand(sql, (SqliteConnection) connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    dts.Add(reader.GetSchemaTable());
                }
            }
            connection.Close();

            return dts;
        }

    }
}

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class OracleDbContext:BaseDbContext, IOracleDbContext
    {
        public OracleDbContext(DbContextOption option) : base(option)
        {

        }
        public OracleDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public OracleDbContext(DbContextOptions options) : base(options){}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseOracle(Option.ConnectionString);
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

            using (var cmd = new OracleCommand(sql, (OracleConnection) connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                
                using (var da = new OracleDataAdapter(cmd))
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

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
    }
}

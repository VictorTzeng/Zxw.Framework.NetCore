using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class PostgreSQLDbContext:BaseDbContext, IPostgreSQLDbContext
    {
        public PostgreSQLDbContext(DbContextOption option) : base(option)
        {

        }
        public PostgreSQLDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public PostgreSQLDbContext(DbContextOptions options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

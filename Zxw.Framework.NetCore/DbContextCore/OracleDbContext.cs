using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseOracle(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

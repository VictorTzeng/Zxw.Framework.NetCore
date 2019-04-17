using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class PostgreSQLDbContext:BaseDbContext
    {
        public PostgreSQLDbContext(DbContextOption option) : base(option)
        {

        }
        public PostgreSQLDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseNpgsql(_option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlite(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

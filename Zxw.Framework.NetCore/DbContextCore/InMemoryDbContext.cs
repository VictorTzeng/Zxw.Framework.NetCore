using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class InMemoryDbContext:BaseDbContext,IInMemoryDbContext
    {
        public InMemoryDbContext(DbContextOption option) : base(option)
        {

        }
        public InMemoryDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseInMemoryDatabase(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

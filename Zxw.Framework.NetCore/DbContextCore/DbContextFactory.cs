using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class DbContextFactory
    {
        public static DbContextFactory Instance => new DbContextFactory();

        public IServiceCollection ServiceCollection { get; set; }

        public DbContextFactory()
        {
        }

        public void AddDbContext<TContext>(DbContextOption option)
            where TContext : BaseDbContext, IDbContextCore
        {
            ServiceCollection.AddDbContext<IDbContextCore,TContext>(option);
        }

        public void AddDbContext<ITContext, TContext>(DbContextOption option)
            where ITContext : IDbContextCore
            where TContext : BaseDbContext, ITContext
        {
            ServiceCollection.AddDbContext<ITContext, TContext>(option);
        }
    }
}

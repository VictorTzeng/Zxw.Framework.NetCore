using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class DbContextFactory
    {
        public static DbContextFactory Instance => new DbContextFactory();

        public IServiceCollection ServiceCollection { get; set; }

        public void AddDbContext<TContext>(DbContextOption option)
            where TContext : BaseDbContext, IDbContextCore
        {
            TContext context = (TContext) Activator.CreateInstance(typeof(TContext), option);
            ServiceCollection.AddSingleton<IDbContextCore>(context);
        }
    }
}

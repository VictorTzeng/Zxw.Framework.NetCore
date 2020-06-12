using System;
using System.Threading;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
using Zxw.Framework.NetCore.EventBus;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;

namespace Zxw.Framework.NetCore.Transaction
{
    /// <summary>
    /// 本地事务
    /// </summary>
    public interface IScopedTransaction:IoC.IScopedDependency
    {
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction Begin();
    }

    public class DefaultCapScopedTransaction : IScopedTransaction
    {
        IDbContextCore DbContext { get; }
        IEventPublisher EventPublisher { get; }

        public DefaultCapScopedTransaction(IDbContextCore dbContext,
            IEventPublisher capPublisher)
        {
            DbContext = dbContext ?? throw new ArgumentException(nameof(dbContext));
            EventPublisher = capPublisher;
        }
        public IDbContextTransaction Begin()
        {
            return DbContext.GetDatabase().BeginTransaction(EventPublisher.CapPublisher);
        }
    }
}

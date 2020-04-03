using System;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore.Storage;
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

    internal class DefaultCapScopedTransaction : IScopedTransaction
    {
        IDbContextCore DbContext { get; }
        ICapPublisher CapPublisher { get; }

        public DefaultCapScopedTransaction(IDbContextCore dbContext,
            ICapPublisher capPublisher)
        {
            DbContext = dbContext ?? throw new ArgumentException(nameof(dbContext));
            CapPublisher = capPublisher;
        }
        public IDbContextTransaction Begin()
        {
            return DbContext.GetDatabase().BeginTransaction(CapPublisher);
        }
    }
}

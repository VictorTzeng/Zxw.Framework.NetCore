using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Transaction;

namespace Zxw.Framework.NetCore.EventBus
{
    public abstract class BaseEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : class, IEvent
    {
        IServiceProvider serviceProvider { get; }
        Lazy<IScopedTransaction> transaction;
        Lazy<ILogger> logger;

        protected IScopedTransaction Transaction => transaction.Value;
        protected ILogger Logger => logger.Value;

        public BaseEventHandler(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            logger = new Lazy<ILogger>(() => serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(this.GetType()));
            transaction = new Lazy<IScopedTransaction>(() => serviceProvider.GetRequiredService<IScopedTransaction>());
        }

        public abstract Task Execute(TEvent @event);
    }
}

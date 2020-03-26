using System.Threading.Tasks;
using DotNetCore.CAP;

namespace Zxw.Framework.NetCore.EventBus
{
    /// <summary>
    /// 事件订阅处理
    /// </summary>
    public interface IEventHandler<TEvent> : ICapSubscribe, IoC.ITransientDependency
        where TEvent : class, IEvent
    {
        /// <summary>
        /// 执行处理逻辑
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task Execute(TEvent @event);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;

namespace Zxw.Framework.NetCore.EventBus
{
    class DefaultEventPublisher : IEventPublisher
    {
        public DefaultEventPublisher(ICapPublisher publisher)
        {
            CapPublisher = publisher;
        }

        public ICapPublisher CapPublisher { get; }

        public void Publish<T>(T eventModel, string callbackName = null)
            where T : class, IEvent
        {
            if (eventModel == null)
                throw new ArgumentNullException(nameof(eventModel));
            CapPublisher.Publish(typeof(T).FullName, eventModel, callbackName);
        }

        public Task PublishAsync<T>(T eventModel, string callbackName = null, CancellationToken cancelToken = default(CancellationToken))
            where T : class, IEvent
        {
            if (eventModel == null)
                throw new ArgumentNullException(nameof(eventModel));
            return CapPublisher.PublishAsync(typeof(T).FullName, eventModel, callbackName, cancelToken);
        }
    }
}

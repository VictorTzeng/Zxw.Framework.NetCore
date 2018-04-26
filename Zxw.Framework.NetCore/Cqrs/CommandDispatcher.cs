using System;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Cqrs
{
    public class CommandDispatcher : ICommandDispatcher
    {
        public void Dispatch<TParameter>(TParameter command) where TParameter : ICommand
        {
            var handler = AspectCoreContainer.Resolve<ICommandHandler<TParameter>>();
            handler.Execute(command);
        }
    }
}
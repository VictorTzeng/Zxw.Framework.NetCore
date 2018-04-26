using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Repositories;

namespace Zxw.Framework.NetCore.Cqrs.CommandHandler
{
    public abstract class BaseCommandHandler<TCommand, T, TKey> : ICommandHandler<TCommand, T, TKey>
        where TCommand : ICommand<T, TKey> 
        where T : BaseModel<TKey>
    {
        public IRepository<T,TKey> Repository { get; set; }

        public BaseCommandHandler(IRepository<T, TKey> repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public virtual void Execute(TCommand command)
        {
            throw new NotImplementedException();
        }
        public virtual Task ExecuteAsync(TCommand command)
        {
            throw new NotImplementedException();
        }
    }
}

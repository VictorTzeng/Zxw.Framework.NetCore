using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Repositories;

namespace Zxw.Framework.NetCore.Cqrs.CommandHandler
{
    public abstract class BaseInsertCommandHandler<TCommand, T, TKey> : BaseCommandHandler<TCommand, T, TKey>
        where TCommand : ICommand<T, TKey> 
        where T : BaseModel<TKey>
    {
        public BaseInsertCommandHandler(IRepository<T, TKey> repository) : base(repository)
        {
        }

        public override void Execute(TCommand command)
        {
            Repository.Add(command.Instance);
        }

        public override async Task ExecuteAsync(TCommand command)
        {
            await Repository.AddAsync(command.Instance);
        }
    }
}

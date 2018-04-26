using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Cqrs.Command
{
    public abstract class BaseCommand<T, TKey> : ICommand<T, TKey> 
        where T : BaseModel<TKey>
    {
        public T Instance { get; set; }
    }
}

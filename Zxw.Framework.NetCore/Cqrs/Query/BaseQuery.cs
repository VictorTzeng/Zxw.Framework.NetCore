using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Cqrs.Query
{
    public abstract class BaseQuery<T, TKey> : IQuery<T, TKey> where T : BaseModel<TKey>
    {
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Repositories;

namespace Zxw.Framework.NetCore.Cqrs.QueryHandler
{
    public abstract class BaseQueryHandler<TQuery, T, TKey, TQueryResult> : IQueryHandler<TQuery, TQueryResult>
        where TQuery : IQuery<T, TKey>
        where TQueryResult : IQueryResult
        where T : BaseModel<TKey>
    {
        public IRepository<T, TKey> Repository { get; set; }

        public BaseQueryHandler(IRepository<T, TKey> repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public abstract TQueryResult Retrieve(TQuery query);
    }
}

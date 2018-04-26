using System;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Cqrs
{
    public class QueryDispatcher : IQueryDispatcher
    {

        public TResult Dispatch<TParameter, TResult>(TParameter query)
            where TParameter : IQuery
            where TResult : IQueryResult
        {
            var handler = AspectCoreContainer.Resolve<IQueryHandler<TParameter, TResult>>();
            return handler.Retrieve(query);
        }

    }
}
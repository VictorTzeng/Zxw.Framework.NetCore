using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Cqrs
{
    /// <summary>
    /// Marker interface to mark a query
    /// </summary>
    public interface IQuery
    {
        
    }

    public interface IQuery<T, TKey> : IQuery where T : BaseModel<TKey>
    {

    }
}
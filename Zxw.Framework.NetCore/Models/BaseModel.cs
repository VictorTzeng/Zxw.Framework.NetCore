using Zxw.Framework.NetCore.Cqrs;

namespace Zxw.Framework.NetCore.Models
{
    public interface IAggregateRoot
    {
        
    }
    /// <summary>
    /// 所有数据表实体类都必须实现此接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class BaseModel<TKey>: IAggregateRoot
    {
        public virtual TKey Id { get; set; }
    }
}

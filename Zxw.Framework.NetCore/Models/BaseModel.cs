using System;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Models
{
    public interface IAggregateRoot
    {
        
    }
    /// <summary>
    /// 所有数据表实体类都必须实现此接口
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [Serializable]
    public abstract class BaseModel<TKey>: IAggregateRoot
    {
        public abstract TKey Id { get; set; }

        public virtual string UniqueId { get; set; } = CommonHelper.NewMongodbId().ToString();
    }
}

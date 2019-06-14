using System;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Models
{
    public interface IAggregateRoot
    {
        
    }
    /// <summary>
    /// 所有数据表实体类都必须继承此类
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    [Serializable]
    public abstract class BaseModel<TKey>: IAggregateRoot
    {
        public abstract TKey Id { get; set; }
    }
    /// <summary>
    /// 所有数据库视图对应实体类必须继承此类
    /// </summary>
    [Serializable]
    public abstract class BaseViewModel : IAggregateRoot
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ITutorClassTypeRepository:IRepository<TutorClassType, Int32>
    {
        [MemoryCache]//使用MemoryCache，缓存有效时间默认10分钟
        IList<TutorClassType> GetByMemoryCached(Expression<Func<TutorClassType, bool>> where = null);

        [RedisCache(Expiration = 5)]//使用Redis，缓存有效时间为5分钟
        IList<TutorClassType> GetByRedisCached(Expression<Func<TutorClassType, bool>> where = null);
    }
}
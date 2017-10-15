using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.IServices;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.IServices
{
    public interface ITutorClassTypeService:IService<TutorClassType, Int32>
    {
        [MemoryCache]
        IList<TutorClassType> GetByCache(Expression<Func<TutorClassType, bool>> where = null);
    }
}

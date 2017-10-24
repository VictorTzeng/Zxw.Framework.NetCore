using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ITutorClassTypeRepository:IRepository<TutorClassType, Int32>
    {
        [MemoryCache]
        IList<TutorClassType> GetByCached(Expression<Func<TutorClassType, bool>> where = null);
    }
}
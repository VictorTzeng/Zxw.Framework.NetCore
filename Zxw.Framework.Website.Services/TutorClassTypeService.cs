using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Services;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.IServices;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Services
{
    public class TutorClassTypeService: BaseService<TutorClassType, Int32>, ITutorClassTypeService
    {
        public TutorClassTypeService(ITutorClassTypeRepository repository) : base(repository)
        {
        }

        public IList<TutorClassType> GetByCache(Expression<Func<TutorClassType, bool>> @where = null)
        {
            return Get(where);
        }
    }
}
using System;
using Zxw.Framework.Services;
using Zxw.Framework.Sample.IRepositories;
using Zxw.Framework.Sample.IServices;
using Zxw.Framework.Sample.Models;

namespace Zxw.Framework.Sample.Services
{
    public class TutorClassTypeService: BaseService<TutorClassType, Int32>, ITutorClassTypeService
    {
        public TutorClassTypeService(ITutorClassTypeRepository repository) : base(repository)
        {
        }
    }
}
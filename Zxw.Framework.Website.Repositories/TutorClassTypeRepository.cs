using System;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class TutorClassTypeRepository : BaseRepository<TutorClassType, Int32>, ITutorClassTypeRepository
    {
        public TutorClassTypeRepository(DefaultDbContext dbContext) : base(dbContext)
        {
        }
    }
}
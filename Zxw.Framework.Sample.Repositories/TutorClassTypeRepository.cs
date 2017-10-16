using System;
using Zxw.Framework.EfDbContext;
using Zxw.Framework.Repositories;
using Zxw.Framework.Sample.IRepositories;
using Zxw.Framework.Sample.Models;

namespace Zxw.Framework.Sample.Repositories
{
    public class TutorClassTypeRepository : BaseRepository<TutorClassType, Int32>, ITutorClassTypeRepository
    {
        public TutorClassTypeRepository(DefaultDbContext dbContext) : base(dbContext)
        {
        }
    }
}
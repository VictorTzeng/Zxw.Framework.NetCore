using System;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class CacheRepository : BaseRepository<Cache, Int32>, ICacheRepository
    {
        public CacheRepository(IEfDbContext dbContext) : base(dbContext)
        {
        }
    }
}
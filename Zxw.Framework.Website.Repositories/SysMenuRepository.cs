using System;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysMenuRepository : BaseRepository<SysMenu, Int32>, ISysMenuRepository
    {
        public SysMenuRepository(IEfDbContext dbContext) : base(dbContext)
        {
        }
    }
}
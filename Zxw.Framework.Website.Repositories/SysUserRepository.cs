using System;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Repositories
{
    public class SysUserRepository : BaseRepository<SysUser, Int32>, ISysUserRepository
    {
        public SysUserRepository(IEfDbContext dbContext) : base(dbContext)
        {
        }
    }
}
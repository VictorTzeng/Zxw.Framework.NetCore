using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ISysMenuRepository:IRepository<SysMenu, Int32>
    {
        IList<SysMenuViewModel> GetHomeMenusByTreeView(Expression<Func<SysMenu, bool>> where);
        IList<SysMenuViewModel> GetMenusByTreeView(Expression<Func<SysMenu, bool>> where);

        [RedisCache(CacheKey = "Redis_Cache_SysMenu", Expiration = 5)]
        IList<SysMenu> GetMenusByCache(Expression<Func<SysMenu, bool>> where);
    }
}
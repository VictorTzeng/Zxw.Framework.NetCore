using System;
using System.Collections.Generic;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ISysMenuRepository:IRepository<SysMenu, Int32>
    {
        IList<SysMenuViewModel> GetMenusByTreeView();
    }
}
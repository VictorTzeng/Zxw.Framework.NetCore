using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Nelibur.ObjectMapper;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Repositories
{
    public class SysMenuRepository : BaseRepository<SysMenu, Int32>, ISysMenuRepository
    {
        public SysMenuRepository(IEfDbContext dbContext) : base(dbContext)
        {
            TinyMapper.Bind<SysMenu, SysMenuViewModel>();
            //插入成功后触发
            Triggers<SysMenu>.Inserted += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Context.SaveChangesWithTriggers(entry.Context.SaveChanges);
                DistributedCacheHelper.GetInstance().Remove("Redis_Cache_SysMenu");
            };
            //修改时触发
            Triggers<SysMenu>.Updating += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
            };
        }

        public IList<SysMenuViewModel> GetHomeMenusByTreeView(Expression<Func<SysMenu, bool>> where)
        {
            return GetHomeTreeMenu(where);
        }
        private IList<SysMenuViewModel> GetHomeTreeMenu(Expression<Func<SysMenu, bool>> where)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(where).OrderBy(m => m.SortIndex);
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.Children = GetHomeTreeMenu(m => m.ParentId == tmp.Id && m.Activable && m.Visiable);
                reslut.Add(tmp);
            }
            return reslut;
        }
        public IList<SysMenuViewModel> GetMenusByTreeView(Expression<Func<SysMenu, bool>> @where)
        {
            return GetTreeMenu(where);
        }

        public IList<SysMenu> GetMenusByCache(Expression<Func<SysMenu, bool>> @where)
        {
            return DbContext.Get(where).ToList();
        }

        private IList<SysMenuViewModel> GetTreeMenu(Expression<Func<SysMenu, bool>> where)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(where).OrderBy(m => m.SortIndex);
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.Children = GetTreeMenu(m => m.ParentId == tmp.Id && m.Activable);
                reslut.Add(tmp);
            }
            return reslut;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Nelibur.ObjectMapper;
using Zxw.Framework.NetCore.EfDbContext;
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
            //插入成功后触发
            Triggers<SysMenu>.Inserted += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                Edit(entry.Entity);
            };
            //修改成功后触发
            Triggers<SysMenu>.Updated += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                Edit(entry.Entity);
            };
            TinyMapper.Bind<SysMenu, SysMenuViewModel>();
        }

        public override int Add(SysMenu entity)
        {
            DbContext.Add(entity);
            return DbContext.SaveChangesWithTriggers();
        }

        public override Task<int> AddAsync(SysMenu entity)
        {
            DbContext.AddAsync(entity);
            return DbContext.SaveChangesWithTriggersAsync();
        }

        public override int AddRange(ICollection<SysMenu> entities)
        {
            DbContext.AddRange(entities);
            return DbContext.SaveChangesWithTriggers();
        }

        public override Task<int> AddRangeAsync(ICollection<SysMenu> entities)
        {
            DbContext.AddRangeAsync(entities);
            return DbContext.SaveChangesWithTriggersAsync();
        }

        public override int Edit(SysMenu entity)
        {
            DbContext.Edit(entity);
            return DbContext.SaveChangesWithTriggers();
        }

        public override int EditRange(ICollection<SysMenu> entities)
        {
            DbContext.EditRange(entities);
            return DbContext.SaveChangesWithTriggers();
        }

        public IList<SysMenuViewModel> GetMenusByTreeView(int menuId = 0)
        {
            return GetTreeMenu(menuId);
        }

        private IList<SysMenuViewModel> GetTreeMenu(int menuId = 0)
        {
            var reslut = new List<SysMenuViewModel>();
            var rootViewModel = new SysMenuViewModel();
            if (menuId != 0)
            {
                var root = GetSingle(menuId);
                if (root == null) return null;
                rootViewModel = TinyMapper.Map(root, rootViewModel);
                if (rootViewModel.ParentId == 0)
                {
                    reslut.Add(rootViewModel);
                    return reslut;
                }
            }
            var children = Get(m => m.ParentId == menuId);
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.Children = GetTreeMenu(tmp.Id);
                rootViewModel.Children.Add(tmp);
            }
            reslut.Add(rootViewModel);
            return reslut;
        }
    }
}
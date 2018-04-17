using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
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
            TinyMapper.Bind<SysMenu, SysMenuViewModel>();
            //插入成功后触发
            Triggers<SysMenu>.Inserted += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Context.SaveChangesWithTriggers(entry.Context.SaveChanges);
            };
            //修改成功后触发
            Triggers<SysMenu>.Updated += entry =>
            {
                var parentMenu = GetSingle(entry.Entity.ParentId);
                var origEntry = entry.Context.Entry(entry.Entity);
                entry.Entity.SortIndex = entry.Entity.Id;
                entry.Entity.MenuPath = (parentMenu?.MenuPath ?? "0") + "," + entry.Entity.Id;
                origEntry.CurrentValues.SetValues(entry.Entity);
                entry.Context.SaveChanges(true);
            };
        }

        public override int Add(SysMenu entity)
        {
            DbContext.Add(entity);
            return DbContext.SaveChangesWithTriggers(true);
        }

        public override Task<int> AddAsync(SysMenu entity)
        {
            DbContext.AddAsync(entity);
            return DbContext.SaveChangesWithTriggersAsync(true);
        }

        public override int AddRange(ICollection<SysMenu> entities)
        {
            DbContext.AddRange(entities);
            return DbContext.SaveChangesWithTriggers(true);
        }

        public override Task<int> AddRangeAsync(ICollection<SysMenu> entities)
        {
            DbContext.AddRangeAsync(entities);
            return DbContext.SaveChangesWithTriggersAsync(true);
        }

        public override int Edit(SysMenu entity)
        {
            DbContext.Edit(entity);
            return DbContext.SaveChangesWithTriggers(true);
        }

        public override int EditRange(ICollection<SysMenu> entities)
        {
            DbContext.EditRange(entities);
            return DbContext.SaveChangesWithTriggers(true);
        }

        public IList<SysMenuViewModel> GetMenusByTreeView(int menuId = 0)
        {
            return GetTreeMenu(menuId);
        }

        private IList<SysMenuViewModel> GetTreeMenu(int menuId = 0)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(m => m.ParentId == menuId && m.Activable && m.Visiable).OrderBy(m => m.SortIndex);
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.Children = GetTreeMenu(tmp.Id);
                reslut.Add(tmp);
            }
            return reslut;
        }
    }
}
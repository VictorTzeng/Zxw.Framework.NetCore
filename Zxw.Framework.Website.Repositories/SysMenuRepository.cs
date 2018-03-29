using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        private Action OnBeforeSaving => () =>
        {
            var entries = _dbContext.GetChangeTracker().Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is SysMenu menu)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            menu.MenuPath = (this.GetSingle(menu.ParentId)?.MenuPath ?? "0") + "," + menu.Id;
                            break;

                        case EntityState.Added:
                            menu.SortIndex = menu.Id;
                            menu.MenuPath = (this.GetSingle(menu.ParentId)?.MenuPath ?? "0") + "," + menu.Id;
                            break;
                    }
                }
            }
        };

        public override int Add(SysMenu entity)
        {
            _dbContext.GetDbSet<SysMenu>().Add(entity);
            return _dbContext.SaveChanges(OnBeforeSaving);
        }

        public override Task<int> AddAsync(SysMenu entity)
        {
            _dbContext.AddAsync(entity);
            return _dbContext.SaveChangesAsync(OnBeforeSaving);
        }

        public override int Edit(SysMenu entity)
        {
            _dbContext.Edit(entity);
            return _dbContext.SaveChanges(OnBeforeSaving);
        }

        public override int EditRange(ICollection<SysMenu> entities)
        {
            _dbContext.EditRange(entities);
            return _dbContext.SaveChanges(OnBeforeSaving);
        }

        public override int Update(SysMenu model, params string[] updateColumns)
        {
            _dbContext.Update(model, updateColumns);
            return _dbContext.SaveChanges(OnBeforeSaving);
        }
    }
}
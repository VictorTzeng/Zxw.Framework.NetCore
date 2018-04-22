using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggers;
using Microsoft.AspNetCore.Mvc;
using Nelibur.ObjectMapper;
using Zxw.Framework.NetCore.Attributes;
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
                DistributedCacheHelper.GetInstance().Remove("Redis_Cache_SysMenu");//插入成功后清除缓存以更新
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
        public IList<SysMenuViewModel> GetMenusByTreeView(Expression<Func<SysMenu, bool>> @where)
        {
            return GetTreeMenu(where);
        }

        public IList<SysMenu> GetMenusByCache(Expression<Func<SysMenu, bool>> @where)
        {
            return DbContext.Get(where).ToList();
        }
        /// <summary>
        /// 初始化系统模块
        /// </summary>
        public void InitSysMenus(string controllerAssemblyName)
        {
            var assembly = Assembly.Load(controllerAssemblyName);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>t.Name.Contains("Controller") && !t.IsAbstract).ToList();
            var menus = new List<SysMenu>();
            if (list != null)
            {
                foreach (var type in list)
                {
                    var controllerName = type.Name.Replace("Controller", "");
                    var methods = type.GetMethods().Where(m =>
                        m.IsPublic && (m.ReturnType == typeof(IActionResult) ||
                                       m.ReturnType == typeof(Task<IActionResult>)));
                    foreach (var method in methods)
                    {
                        var identity = $"{controllerName}/{method.Name}";
                        if (Count(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)) == 0 &&
                            !menus.Any(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase)))
                        {
                            menus.Add(new SysMenu()
                            {
                                MenuName = method.Name,
                                Activable = true,
                                Visiable = method.GetCustomAttribute<AjaxRequestOnlyAttribute>() == null,
                                Identity = identity,
                                RouteUrl = identity,
                                ParentId = 0
                            });
                        }
                    }
                }
            }

            if (menus.Any())
            {
                AddRange(menus, true);
            }
        }

        private IList<SysMenuViewModel> GetHomeTreeMenu(Expression<Func<SysMenu, bool>> where)
        {
            var reslut = new List<SysMenuViewModel>();
            var children = Get(where).OrderBy(m => m.SortIndex);
            foreach (var child in children)
            {
                var tmp = new SysMenuViewModel();
                tmp = TinyMapper.Map(child, tmp);
                tmp.RouteUrl = ConfigHelper.GetConfigurationValue("appSettings:AdminDomain") + tmp.RouteUrl;
                tmp.Children = GetHomeTreeMenu(m => m.ParentId == tmp.Id && m.Activable && m.Visiable);
                reslut.Add(tmp);
            }
            return reslut;
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
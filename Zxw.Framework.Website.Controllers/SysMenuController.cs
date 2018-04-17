using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    public class SysMenuController : Controller
    {
        private ISysMenuRepository menuRepository;
        
        public SysMenuController(ISysMenuRepository menuRepository)
        {
            this.menuRepository = menuRepository ?? throw new ArgumentNullException(nameof(menuRepository));
        }

        #region Views

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View(menuRepository.GetSingle(id));
        }

        #endregion

        #region Methods

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                    #region 添加模块，注释掉了

                    //repository.AddRange(new List<SysMenu>
                    //{
                    //    new SysMenu
                    //    {
                    //        Identity = "SysMenu/Add",
                    //        MenuName = "增加",
                    //        ParentId = 1,
                    //        RouteUrl = "~/SysMenu/Add",
                    //        Visiable = false
                    //    },
                    //    new SysMenu
                    //    {
                    //        Identity = "SysMenu/Edit",
                    //        MenuName = "编辑",
                    //        ParentId = 1,
                    //        RouteUrl = "~/SysMenu/Edit",
                    //        Visiable = false
                    //    },
                    //    new SysMenu
                    //    {
                    //        Identity = "SysMenu/Delete",
                    //        MenuName = "删除",
                    //        ParentId = 1,
                    //        RouteUrl = "~/SysMenu/Delete",
                    //        Visiable = false
                    //    },
                    //    new SysMenu
                    //    {
                    //        Identity = "SysMenu/Active",
                    //        MenuName = "启停用",
                    //        ParentId = 1,
                    //        RouteUrl = "~/SysMenu/Active",
                    //        Visiable = false
                    //    },
                    //    new SysMenu
                    //    {
                    //        Identity = "SysMenu/Visualize",
                    //        MenuName = "显示/隐藏",
                    //        ParentId = 1,
                    //        RouteUrl = "~/SysMenu/Visualize",
                    //        Visiable = false
                    //    }
                    //});

                    #endregion
                    var rows = menuRepository.GetMenusByTreeView().OrderBy(m=>m.SortIndex).ToList();
                    return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var total = menuRepository.Count(m => true);
                var rows = menuRepository.GetByPagination(m => true, pageSize, pageIndex, true,
                    m => m.Id).ToList();
                return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
            });
        }
        /// <summary>
        /// 新建
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost,ValidateAntiForgeryToken]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if(!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                menuRepository.AddAsync(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                if (!ModelState.IsValid)
                    return Json(ExcutedResult.FailedResult("数据验证失败"));
                menuRepository.Edit(menu);
                return Json(ExcutedResult.SuccessResult());
            });
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Delete(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                menuRepository.Delete(id);
                return Json(ExcutedResult.SuccessResult("成功删除一条数据。"));
            });
        }

        /// <summary>
        /// 启停用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Active(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Activable = !entity.Activable;
                menuRepository.Update(entity, "Activable");
                return Json(ExcutedResult.SuccessResult(entity.Activable?"OK，已成功启用。":"OK，已成功停用"));
            });
        }
        /// <summary>
        /// 是否在左侧菜单栏显示
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Visualize(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var entity = menuRepository.GetSingle(id);
                entity.Visiable = !entity.Visiable;
                menuRepository.Update(entity, "Visiable");
                return Json(ExcutedResult.SuccessResult("操作成功，请刷新当前网页或者重新进入系统。"));
            });
        }

        #endregion
	}
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

namespace Zxw.Framework.Website.Controllers
{
    public class SysMenuController : BaseController
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
                var rows = menuRepository.GetHomeMenusByTreeView(m=>m.Activable && m.Visiable && m.ParentId == 0).OrderBy(m=>m.SortIndex).ToList();
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetTreeMenus(int parentId = 0)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var nodes = menuRepository.GetMenusByTreeView(m=>m.Activable && m.ParentId == 0).OrderBy(m => m.SortIndex).Select(m=>GetTreeMenus(m,parentId)).ToList();
                var rows = new[]
                {
                    new
                    {
                        text = " 根节点",
                        icon = "fas fa-boxes",
                        tags = "0",
                        nodes,
                        state = new
                        {
                            selected = 0 == parentId
                        }
                    }
                };
                return Json(ExcutedResult.SuccessResult(rows));
            });
        }

        private object GetTreeMenus(SysMenuViewModel viewModel, int parentId = 0)
        {
            if (viewModel.Children.Any())
            {
                return new
                {
                    text = " "+viewModel.MenuName,
                    icon = viewModel.MenuIcon,
                    tags = viewModel.Id.ToString(),
                    nodes = viewModel.Children.Select(GetTreeMenus),
                    state = new
                    {
                        expanded = false,
                        selected = viewModel.Id == parentId
                    }
                };
            }
            return new 
            {
                text = " "+viewModel.MenuName,
                icon = viewModel.MenuIcon,
                tags = viewModel.Id.ToString(),
                state = new
                {
                    selected = viewModel.Id == parentId
                }
            };
        }

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                var total = menuRepository.CountAsync(m => true).Result;
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
                menuRepository.AddAsync(menu, true);
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
                menuRepository.Edit(menu, true);
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
                menuRepository.Update(entity, false, "Activable");
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
                menuRepository.Update(entity, false, "Visiable");
                return Json(ExcutedResult.SuccessResult("操作成功，请刷新当前网页或者重新进入系统。"));
            });
        }

        #endregion
	}
}
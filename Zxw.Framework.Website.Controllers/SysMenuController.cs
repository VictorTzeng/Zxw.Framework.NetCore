using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.UnitOfWork;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    public class SysMenuController : Controller
    {
        private IUnitOfWork _unitOfWork;
        
        public SysMenuController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public IActionResult Index()
        {
            return View();
        }

        [AjaxRequestOnly, HttpGet]
        public Task<IActionResult> GetMenus()
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    var rows = repository.Get(m => m.Activable && m.Visiable);
                    return Json(ExcutedResult.SuccessResult(rows));
                }
            });
        }

        [AjaxRequestOnly]
        public Task<IActionResult> GetMenusByPaged(int pageSize, int pageIndex)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    var total = repository.Count(m => m.Activable && m.Visiable);
                    var rows = repository.GetByPagination(m => m.Activable && m.Visiable, pageSize, pageIndex, true,
                        m => m.SortIndex).ToList();
                    return Json(PaginationResult.PagedResult(rows, total, pageSize, pageIndex));
                }
            });
        }
        /// <summary>
        /// ÐÂ½¨
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly,HttpPost]
        public Task<IActionResult> Add(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    repository.AddAsync(menu);
                    return Json(ExcutedResult.SuccessResult());
                }
            });
        }
        /// <summary>
        /// ±à¼­
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        [AjaxRequestOnly, HttpPost]
        public Task<IActionResult> Edit(SysMenu menu)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    repository.Edit(menu);
                    return Json(ExcutedResult.SuccessResult());
                }
            });
        }
        /// <summary>
        /// É¾³ý
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Delete(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    repository.Delete(id);
                    return Json(ExcutedResult.SuccessResult());
                }
            });
        }

        /// <summary>
        /// ÆôÍ£ÓÃ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Active(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    var entity = repository.GetSingle(id);
                    entity.Activable = !entity.Activable;
                    repository.Update(entity, "Activable");
                    return Json(ExcutedResult.SuccessResult());
                }
            });
        }
        /// <summary>
        /// ÊÇ·ñÔÚ×ó²à²Ëµ¥À¸ÏÔÊ¾
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AjaxRequestOnly]
        public Task<IActionResult> Visualize(int id)
        {
            return Task.Factory.StartNew<IActionResult>(() =>
            {
                using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
                {
                    var entity = repository.GetSingle(id);
                    entity.Visiable = !entity.Visiable;
                    repository.Update(entity, "Visiable");
                    return Json(ExcutedResult.SuccessResult());
                }
            });
        }

		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
	}
}
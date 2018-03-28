using System;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.UnitOfWork;
using Zxw.Framework.Website.IRepositories;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.ViewModels;

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

        [AjaxRequestOnly]
        public ActionResult GetMenus()
        {
            using (var repository = _unitOfWork.GetRepository<ISysMenuRepository>())
            {
                var rows = repository.GetAsync(m => m.Activable && m.Visiable).Result;
                return Json(ExcutedResult.SuccessResult(rows));
            }
        }

        [AjaxRequestOnly]
        public IActionResult Add()
        {
            return Json(new { });
        }

        [AjaxRequestOnly]
        public IActionResult Edit()
        {
            return Json(new { });
        }

        [AjaxRequestOnly]
        public IActionResult Delete()
        {
            return Json(new { });
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
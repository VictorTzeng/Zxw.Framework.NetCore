using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;

namespace Zxw.Framework.Website.Controllers
{
    [Ignore]
    public class LoginController:BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LockScreen()
        {
            return View();
        }

        [HttpPost, AjaxRequestOnly]
        public Task<IActionResult> Login()
        {
            return Task.Factory.StartNew<IActionResult>(() => { return Json(new { }); });
        }
    }
}

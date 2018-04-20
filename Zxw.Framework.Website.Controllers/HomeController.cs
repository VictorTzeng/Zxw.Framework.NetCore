using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    [Ignore]
    public class HomeController : BaseController
    {
        public HomeController(ISysMenuRepository menuRepository)
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            menuRepository.InitSysMenus();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "Readme";
            ViewBag.PageHeader = "README.md";
            ViewBag.PageDescription = "项目简介";
            return View();
        }
    }
}

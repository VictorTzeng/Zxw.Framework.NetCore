using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.CodeGenerator;

namespace Zxw.Framework.Website.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController()
        {
            CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
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

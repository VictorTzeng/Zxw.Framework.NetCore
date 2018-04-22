using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    [Ignore]
    public class HomeController : BaseController
    {
        public HomeController(ISysMenuRepository menuRepository, IOptions<CodeGenerateOption> options)
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            menuRepository.InitSysMenus(options.Value.ControllersNamespace);
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

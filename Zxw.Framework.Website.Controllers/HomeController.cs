using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.Website.IServices;
using Zxw.Framework.Website.ViewModels;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.Controllers
{
    public class HomeController : Controller
    {
        private ITutorClassTypeService iTutorClassTypeService;

        public HomeController(ITutorClassTypeService tutorClassTypeService)
        {
            if(tutorClassTypeService==null)
                throw new ArgumentNullException(nameof(tutorClassTypeService));
            iTutorClassTypeService = tutorClassTypeService;
        }
        public IActionResult Index()
        {
            // CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            // CodeGenerator.GenerateSingle<TutorClassType, int>();//生成单个实体类对应的Repository和Service层代码文件
            var list = iTutorClassTypeService.Get(m => true, m => m.Id, m => m.Active);
            return View(list);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                iTutorClassTypeService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

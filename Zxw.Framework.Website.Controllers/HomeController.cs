using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.IUnitOfWork;
//using Zxw.Framework.Website.IServices;
using Zxw.Framework.Website.ViewModels;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    public class HomeController : Controller
    {
        //private ITutorClassTypeService iTutorClassTypeService;
        private IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            if(unitOfWork == null)
                throw new ArgumentNullException(nameof(unitOfWork));
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            //CodeGenerator.GenerateSingle<TutorClassType, int>();//生成单个实体类对应的Repository和Service层代码文件
            var list = _unitOfWork.Repository<ITutorClassTypeRepository>().Get();
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
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

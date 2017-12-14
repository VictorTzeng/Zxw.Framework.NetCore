using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.UnitOfWork;
using Zxw.Framework.Pay.AliPay;
using Zxw.Framework.Pay.UnionPay;
using Zxw.Framework.Website.ViewModels;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    public class HomeController : Controller
    {
        private IUnitOfWork _unitOfWork;
        

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        public IActionResult Index()
        {
            //CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
            //CodeGenerator.GenerateSingle<TutorClassType, int>();//生成单个实体类对应的Repository和Service层代码文件
            using (var repository = _unitOfWork.GetRepository<ITutorClassTypeRepository>())
            {
                var list = repository.GetByPagination(t=>true, 10, 1, false, m=>m.Id).ToList();
                return View(list);
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            using (var repository = _unitOfWork.GetRepository<ITutorClassTypeRepository>())
            {
                repository.Add(new TutorClassType()
                {
                    Active = true,
                    TutorClassTypeName = "小学",
                    TutorClassCount = 5
                });
                repository.Add(new TutorClassType()
                {
                    Active = true,
                    TutorClassTypeName = "初中",
                    TutorClassCount = 15
                });
                repository.Add(new TutorClassType()
                {
                    Active = true,
                    TutorClassTypeName = "高中",
                    TutorClassCount = 25
                });
                _unitOfWork.SaveChanges();
            }
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            _unitOfWork.BatchUpdate<TutorClassType>(m => m.Active, m => new TutorClassType() {TutorClassCount = 100});
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

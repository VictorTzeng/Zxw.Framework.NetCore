using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.UnitOfWork;
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
                var list1 = repository.GetByMemoryCached(t=>true);
                return View(list1);
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
                var list2 = repository.GetByRedisCached(t => true);
                return View(list2);
            }
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            //_unitOfWork.BatchUpdate<TutorClassType>(m => m.Active, m => new TutorClassType() {TutorClassCount = 100});
            var list = new List<TutorClassType>();
            for (int i = 0; i < 1000000; i++)
            {
                list.Add(new TutorClassType()
                {
                    Active = true,
                    TutorClassTypeName = "高中" + (i + 1),
                    TutorClassCount = i + 1,
                    Remark = "test"
                });
            }
            _unitOfWork.GetRepository<ITutorClassTypeRepository>().BulkInsert(list, "TutorClassType");
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

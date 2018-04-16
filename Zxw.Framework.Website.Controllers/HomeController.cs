using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.ViewModels;
using Zxw.Framework.Website.Models;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            CodeGenerator.Generate();//生成所有实体类对应的Repository和Service层代码文件
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Default()
        {
            ViewData["Title"] = "Readme";
            ViewBag.PageHeader = "README.md";
            ViewBag.PageDescription = "项目简介";
            return View();
        }
    }
}

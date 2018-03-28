using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.NetCore.UnitOfWork;
using Zxw.Framework.Website.Models;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Repositories
{
    public class SysUserController : Controller
    {
        private IUnitOfWork _unitOfWork;
        
        public SysUserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
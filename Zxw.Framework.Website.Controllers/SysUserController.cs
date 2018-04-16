using System;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    public class SysUserController : Controller
    {
        private ISysUserRepository userRepository;
        
        public SysUserController(ISysUserRepository user)
        {
            userRepository = user ?? throw new ArgumentNullException(nameof(user));
        }
	}
}
using System;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers
{
    public class SysUserController : BaseController
    {
        private ISysUserRepository userRepository;
        
        public SysUserController(ISysUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }
	}
}
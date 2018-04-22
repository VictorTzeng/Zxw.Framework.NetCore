using System.Collections.Generic;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.ViewModels
{
    public class SysMenuViewModel : SysMenu
    {
        public IList<SysMenuViewModel> Children { get; set; } = new List<SysMenuViewModel>();
    }
}

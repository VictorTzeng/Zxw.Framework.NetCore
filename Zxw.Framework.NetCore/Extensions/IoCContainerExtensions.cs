using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class IoCContainerExtensions
    {
        public static void AddAspect(this AutofacContainer container)
        {
            if(container==null)
                throw new ArgumentNullException(nameof(container));
        }
    }
}

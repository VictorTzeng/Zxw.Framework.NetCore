using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Helpers
{
    public class DistributedCacheHelper
    {
        private  static IDistributedCache Instance { get; }

        static DistributedCacheHelper()
        {
            Instance = AutofacContainer.Resolve<IDistributedCache>();
        }

        public static IDistributedCache GetInstance() => Instance;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Helpers
{
    public class MemoryCacheHelper
    {
        private static IMemoryCache Instance { get; }

        static MemoryCacheHelper()
        {
            Instance = AutofacContainer.Resolve<IMemoryCache>();
        }

        public static IMemoryCache GetInstance() => Instance;
    }
}

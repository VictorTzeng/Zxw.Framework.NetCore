using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.UnitTest.TestModels;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;
using System.Diagnostics;

namespace Zxw.Framework.UnitTest
{
    [TestClass]
    public class DbContextUnitTest
    {
        [TestMethod]
        public void TestCompileQuery()
        {
            BuildServiceForSqlServer();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var watch = new Stopwatch();
            watch.Start();
            dbContext.Get<SysMenu>(m => m.Active);
            watch.Stop();
            Console.WriteLine($"Get --> {watch.ElapsedMilliseconds} ms.");
            watch.Restart();
            dbContext.GetByCompileQuery<SysMenu>(m => m.Active);
            watch.Stop();
            Console.WriteLine($"GetByCompileQuery --> {watch.ElapsedMilliseconds} ms.");
        }

        #region public methods

        public void BuildServiceForSqlServer()
        {
            IServiceCollection services = new ServiceCollection();

            //在这里注册EF上下文
            services = RegisterSqlServerContext(services);

            services.AddOptions();
            services.BuildAspectCoreServiceProvider();
        }

        /// <summary>
        /// 注册SQLServer上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterSqlServerContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString =
                    "initial catalog=NetCoreDemo;data source=.;password=admin123!@#;User id=sa;MultipleActiveResultSets=True;";
                options.ModelAssemblyName = "Zxw.Framework.UnitTest";
                options.IsOutputSql = false;
            });
            services.AddTransient<IDbContextCore, SqlServerDbContext>(); //注入EF上下文
            return services;
        }
        #endregion

    }
}

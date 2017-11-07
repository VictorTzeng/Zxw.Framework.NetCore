using log4net;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text;
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Filters;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.NetCore.UnitOfWork;

namespace Zxw.Framework.Website
{
    public class Startup
    {
        public static ILoggerRepository Repository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //初始化log4net
            Repository = LogManager.CreateRepository("NETCoreRepository");
            Log4NetHelper.SetConfig(Repository, "log4net.config");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return InitIoC(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        /// <summary>
        /// IoC初始化
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private IServiceProvider InitIoC(IServiceCollection services)
        {
            //database connectionstring
            var connectionString = Configuration.GetConnectionString("MsSqlServer");
            var dbContextOption = new DbContextOption
            {
                ConnectionString = connectionString,
                ModelAssemblyName = "Zxw.Framework.Website.Models",
                DbType = DbType.MSSQLSERVER
            };
            var codeGenerateOption = new CodeGenerateOption
            {
                ModelsNamespace = "Zxw.Framework.Website.Models",
                IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories",
                RepositoriesNamespace = "Zxw.Framework.Website.Repositories"
            };
            //启用Redis
            //services.AddDistributedRedisCache(option =>
            //{
            //    option.Configuration = "localhost";//redis连接字符串
            //    option.InstanceName = "";//Redis实例名称
            //});
            services.Configure<MemoryCacheEntryOptions>(
                options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)); //设置MemoryCache缓存有效时间为5分钟。
            //.Configure<DistributedCacheEntryOptions>(option =>
            //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));//设置Redis缓存有效时间为5分钟。

            services.AddMemoryCache();//启用MemoryCache
            services.AddSingleton(Configuration)
                .AddSingleton(dbContextOption)
                .AddSingleton(codeGenerateOption)
                .AddDbContext<DefaultDbContext>()
                .RegisterAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories")
                .AddTransient<IUnitOfWork, EfUnitOfWork>();
            services.AddMvc(option =>
                {
                    option.Filters.Add(new GlobalExceptionFilter());
                })
                .AddControllersAsServices();
            services.AddDynamicProxy();
            //services = ServiceLocator.CreateServiceBuilder(services);

            return AutofacContainer.Build(services);//接入Autofac
            //return services.BuildAspectCoreServiceProvider(); 
        }
    }
}

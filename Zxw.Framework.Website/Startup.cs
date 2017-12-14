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
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Filters;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.NetCore.UnitOfWork;
using Zxw.Framework.Website.Repositories;

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
            var connectionString = Configuration.GetConnectionString("PostgreSQL");

            //启用Redis
            //services.AddDistributedRedisCache(option =>
            //{
            //    option.Configuration = "localhost";//redis连接字符串
            //    option.InstanceName = "";//Redis实例名称
            //});
            //设置Redis缓存有效时间为5分钟。
            //services.Configure<DistributedCacheEntryOptions>(option =>
            //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));

            //启用MemoryCache
            services.AddMemoryCache();
            //设置MemoryCache缓存有效时间为5分钟。
            services.Configure<MemoryCacheEntryOptions>(
                options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)); 

            //配置DbContextOption
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = connectionString;
                options.ModelAssemblyName = "Zxw.Framework.Website.Models";
                options.DbType = DbType.NPGSQL;
            });
            //配置CodeGenerateOption
            services.Configure<CodeGenerateOption>(options =>
            {
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
            });

            services.AddSingleton(Configuration)//注入Configuration，ConfigHelper要用
                .AddDbContext<DefaultDbContext>()//注入EF上下文
                .RegisterAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories")//注入仓储
                .AddTransient<IUnitOfWork, EfUnitOfWork>();//注入工作单元
            services.AddMvc(option =>
                {
                    option.Filters.Add(new GlobalExceptionFilter());
                })
                .AddControllersAsServices();

            return services.BuildAspectCoreWithAutofacServiceProvider();//接入Autofac和AspectCore
        }
    }
}

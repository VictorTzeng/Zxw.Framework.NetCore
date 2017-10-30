using System;
using System.Text;
using Dora.Interception;
using log4net;
using log4net.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public static ILoggerRepository repository { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //初始化log4net
            repository = LogManager.CreateRepository("NETCoreRepository");
            Log4NetHelper.SetConfig(repository, "log4net.config");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();//启用MemoryCache
            //services.AddDistributedRedisCache(option =>
            //{
            //    option.Configuration = "localhost";//redis连接字符串
            //    option.InstanceName = "";//Redis实例名称
            //});//启用Redis
            services.Configure<MemoryCacheEntryOptions>(
                options => options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)); //设置MemoryCache缓存有效时间为5分钟。
                                                                                               //.Configure<DistributedCacheEntryOptions>(option =>
                                                                                               //    option.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5));//设置Redis缓存有效时间为5分钟。

            InitIoC(services);
            services.AddDbContext<DefaultDbContext>();
            services.AddInterception();
            services.AddMvc(option=>
            {
                option.Filters.Add(new GlobalExceptionFilter());
            });
            return IoCContainer.Build(services);
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
        private void InitIoC(IServiceCollection services)
        {
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
            //IoCContainer.Register(Configuration);//注册配置
            //IoCContainer.Register(dbContextOption);//注册数据库配置信息
            //IoCContainer.Register(codeGenerateOption);//注册代码生成器相关配置信息
            //IoCContainer.Register(typeof(DefaultDbContext));//注册EF上下文
            //IoCContainer.Register("Zxw.Framework.Website.Repositories", "Zxw.Framework.Website.IRepositories");//注册仓储
            //IoCContainer.Register<IUnitOfWork, EfUnitOfWork>();//注册EF工作单元
            //return IoCContainer.Build(services);
            services.AddSingleton(Configuration);
            services.AddSingleton(dbContextOption);
            services.AddSingleton(codeGenerateOption);
            services.RegisterAssembly("Zxw.Framework.Website.IRepositories", "Zxw.Framework.Website.Repositories");
            services.AddTransient<IUnitOfWork, EfUnitOfWork>();
        }
    }
}

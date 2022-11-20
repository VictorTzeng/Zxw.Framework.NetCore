using System;
using System.Data;
using System.Threading;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Cache;
using Zxw.Framework.NetCore.CodeGenerator;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using Zxw.Framework.UnitTest.TestModels;

namespace Zxw.Framework.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        #region Test methods for Oracle

        [TestMethod]
        public void TestGetDataTableForOracle()
        {
            BuildServiceForOracle();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var dt1 = dbContext.GetCurrentDatabaseAllTables();
            Assert.IsNotNull(dt1);
            foreach (DataRow row in dt1.Rows)
            {
                var dt2 = dbContext.GetTableColumns(row["TableName"].ToString());
                Assert.IsNotNull(dt2);
            }
        }

        [TestMethod]
        public void TestGetDataTableListForOracle()
        {
            BuildServiceForOracle();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var tables = dbContext.GetCurrentDatabaseTableList();
            Assert.IsNotNull(tables);
        }

        [TestMethod]
        public void TestGenerateEntitiesForOracle()
        {
            BuildServiceForOracle();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            dbContext.GenerateAllCodesFromDatabase(true);
        }

        #endregion

        #region Test methods for PostgreSQL

        [TestMethod]
        public void TestGetDataTableForPostgreSql()
        {
            BuildServiceForPostgreSql();

            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var dt1 = dbContext.GetCurrentDatabaseAllTables();
            Assert.IsNotNull(dt1);
            foreach (DataRow row in dt1.Rows)
            {
                var dt2 = dbContext.GetTableColumns(row["TableName"].ToString());
                Assert.IsNotNull(dt2);
            }
        }

        [TestMethod]
        public void TestGetDataTableListForPostgreSql()
        {
            BuildServiceForPostgreSql();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var tables = dbContext.GetCurrentDatabaseTableList();
            Assert.IsNotNull(tables);
        }

        [TestMethod]
        public void TestGenerateEntitiesForPostgreSql()
        {
            BuildServiceForPostgreSql();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            dbContext.GenerateAllCodesFromDatabase(true);
        }

        #endregion

        #region Test methods for SQL Server

        [TestMethod]
        public void TestGetDataTableForSqlServer()
        {
            BuildServiceForSqlServer();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var dt1 = dbContext.GetCurrentDatabaseAllTables();
            Assert.IsNotNull(dt1);
            foreach (DataRow row in dt1.Rows)
            {
                var dt2 = dbContext.GetTableColumns(row["TableName"].ToString());
                Assert.IsNotNull(dt2);
            }

            var sqlOperator = ServiceLocator.Resolve<ISqlOperatorUtility>();
            var ds = sqlOperator.SqlQuery("select * from SysMenu");
            Assert.IsNotNull(ds);
        }

        [TestMethod]
        public void TestGetDataTableListForSqlServer()
        {
            BuildServiceForSqlServer();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var tables = dbContext.GetCurrentDatabaseTableList();
            Assert.IsNotNull(tables);
        }

        [TestMethod]
        public void TestGenerateEntitiesForSqlServer()
        {
            BuildServiceForSqlServer();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            dbContext.GenerateAllCodesFromDatabase(true);
        }

        #endregion

        #region Test methods for MySQL

        [TestMethod]
        public void TestGetDataTableForMySql()
        {
            BuildServiceFoMySql();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var dt1 = dbContext.GetCurrentDatabaseAllTables();
            Assert.IsNotNull(dt1);
            foreach (DataRow row in dt1.Rows)
            {
                var dt2 = dbContext.GetTableColumns(row["TableName"].ToString());
                Assert.IsNotNull(dt2);
            }
        }

        [TestMethod]
        public void TestGetDataTableListForMySql()
        {
            BuildServiceFoMySql();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            var tables = dbContext.GetCurrentDatabaseTableList();
            Assert.IsNotNull(tables);
        }

        [TestMethod]
        public void TestGenerateEntitiesForMySql()
        {
            BuildServiceFoMySql();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            dbContext.GenerateAllCodesFromDatabase(true);
        }

        #endregion

        #region Test methods for Redis

        [TestMethod]
        public void TestCsRedisClient()
        {
            BuildServiceForSqlServer();
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            RedisHelper.Set("test_cache_key", JsonConvertor.Serialize(dbContext.GetCurrentDatabaseTableList()),
                10 * 60);
            Thread.Sleep(2000);
            var content = RedisCacheManager.Get("test_cache_key");
            Assert.IsNotNull(content);
        }
        
        #endregion

        #region public methods

        public void BuildServiceForPostgreSql()
        {
            IServiceCollection services = new ServiceCollection();
            //在这里注册EF上下文
            services = RegisterPostgreSqlContext(services);
            services.Configure<CodeGenerateOption>(options =>
            {
                options.OutputPath = "F:\\Test\\PostgreSQL";
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
                options.ControllersNamespace = "Zxw.Framework.Website.Controllers";
            });
            services.AddOptions();
            
            services.BuildAspectCoreServiceProvider(); 
        }

        public void BuildServiceForSqlServer()
        {
            IServiceCollection services = new ServiceCollection();

            //在这里注册EF上下文
            services = RegisterSqlServerContext(services);

            services.AddOptions();
            services.UseCodeGenerator(new CodeGenerateOption()
            {
                ModelsNamespace = "AeroIotPlatform.Models.Bridge",
                IRepositoriesNamespace = "AeroIotPlatform.IRepositories.Bridge",
                RepositoriesNamespace = "AeroIotPlatform.Repositories.Bridge",
                ControllersNamespace = "AeroIotPlatform.WebApi.Controllers",
                IServicesNamespace = "AeroIotPlatform.IServices.Bridge",
                ServicesNamespace = "AeroIotPlatform.Services.Bridge",
                OutputPath = "D:\\CodeGenerator\\AeroIotPlatform\\Bridge"
            });
            services.BuildAspectCoreServiceProvider(); 
        }

        public void BuildServiceFoMySql()
        {
            IServiceCollection services = new ServiceCollection();

            //在这里注册EF上下文
            services = RegisterMySqlContext(services);
            services.Configure<CodeGenerateOption>(options =>
            {
                options.OutputPath = "F:\\Test\\MySQL";
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
                options.ControllersNamespace = "Zxw.Framework.Website.Controllers";
            });
            services.AddOptions();
            services.BuildAspectCoreServiceProvider(); 
        }

        public void BuildServiceForSqLite()
        {
            IServiceCollection services = new ServiceCollection();

            //在这里注册EF上下文
            services = RegisterSqLiteContext(services);
            services.Configure<CodeGenerateOption>(options =>
            {
                options.OutputPath = "F:\\Test\\SQLite";
                options.ModelsNamespace = "Zxw.Framework.Website.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.Website.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.Website.Repositories";
                options.ControllersNamespace = "Zxw.Framework.Website.Controllers";
            });
            services.AddOptions();
            services.BuildAspectCoreServiceProvider(); 
        }

        public void BuildServiceForOracle()
        {
            IServiceCollection services = new ServiceCollection();

            services.Configure<CodeGenerateOption>(options =>
            {
                options.OutputPath = "F:\\Test\\Oracle";
                options.ModelsNamespace = "Zxw.Framework.UnitTest.Models";
                options.IRepositoriesNamespace = "Zxw.Framework.UnitTest.IRepositories";
                options.RepositoriesNamespace = "Zxw.Framework.UnitTest.Repositories";
                options.ControllersNamespace = "Zxw.Framework.UnitTest.Controllers";
            });
            //在这里注册EF上下文
            services = RegisterOracleDbContext(services);
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
                    "initial catalog=NetCoreDemo;data source=127.0.0.1;password=admin123!@#;User id=sa;MultipleActiveResultSets=True;";
                //options.ModelAssemblyName = "Zxw.Framework.Website.Models";
            });
            services.AddScoped<IDbContextCore, SqlServerDbContext>(); //注入EF上下文
            services.AddScoped<ISqlOperatorUtility, SqlOperatorUtility>();
            return services;
        }

        /// <summary>
        /// 注册MySQL上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterMySqlContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString =
                    "Server=127.0.0.1;Database=test; User ID=root;Password=123456;port=3306;CharSet=utf8;pooling=true;";
                //options.ModelAssemblyName = "Zxw.Framework.Website.Models";
            });
            services.AddScoped<IDbContextCore, MySqlDbContext>(); //注入EF上下文
            return services;
        }

        /// <summary>
        /// 注册PostgreSQL上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterPostgreSqlContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.TagName = "db0";
                options.ConnectionString =
                    "User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=ZxwPgDemo;Pooling=true;";
                //options.ModelAssemblyName = "Zxw.Framework.Website.Models";
            });

            services.AddDbContextFactory(factory =>
            {
                factory.AddDbContext<PostgreSQLDbContext>(new DbContextOption(){ TagName = "db1", ConnectionString = "User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=ZxwPgDemo;Pooling=true;" });
                factory.AddDbContext<SqlServerDbContext>(new DbContextOption() { TagName = "db2", ConnectionString = "Data Source=127.0.0.1;Initial Catalog=HardwarePlatform;User ID=sa;password=123456;" });
            });

            services.AddScoped<IDbContextCore, PostgreSQLDbContext>(); //注入EF上下文
            return services;
        }

        /// <summary>
        /// 注册SQLite上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterSqLiteContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = "Data Source=F:\\EF6.db;Version=3;";
                //options.ModelAssemblyName = "Zxw.Framework.Website.Models";
            });
            services.AddScoped<IDbContextCore, SQLiteDbContext>(); //注入EF上下文
            return services;
        }

        /// <summary>
        /// 注册Oracle上下文
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceCollection RegisterOracleDbContext(IServiceCollection services)
        {
            services.Configure<DbContextOption>(options =>
            {
                options.ConnectionString = "DATA SOURCE=127.0.0.1:1234/testdb;USER ID=test;PASSWORD=123456;PERSIST SECURITY INFO=True;Pooling=True;Max Pool Size=100;Incr Pool Size=2;";
            });
            services.AddScoped<IDbContextCore, OracleDbContext>(); //注入EF上下文
            return services;
        }
        #endregion
    }
}

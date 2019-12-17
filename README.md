# Zxw.Framework.NetCore
[![Build Status](https://dev.azure.com/v-xiaze0473/v-xiaze/_apis/build/status/VictorTzeng.Zxw.Framework.NetCore?branchName=master)](https://dev.azure.com/v-xiaze0473/v-xiaze/_build/latest?definitionId=1&branchName=master)

基于EF Core的Code First模式的DotNetCore快速开发框架

**Nuget [最新版本：3.1.2]**

[Zxw.Framework.NetCore](https://www.nuget.org/packages/Zxw.Framework.NetCore/3.1.2) 
* Install-Package Zxw.Framework.NetCore -Version 3.1.2
* dotnet add package Zxw.Framework.NetCore --version 3.1.2

**开发环境**
* VS2019 / VS Code
* .net core 3.1.100

**支持的数据库**
* SQL Server
* MySQL
* Sqlite
* InMemory
* PostgreSQL
* Oracle
* MongoDB (Beta)(第三方驱动[Blueshift.EntityFrameworkCore.MongoDB](https://github.com/BlueshiftSoftware/EntityFrameworkCore))

**日志组件**
* log4net

**DI组件**
* Autofac
* [Aspectcore.Injector](https://github.com/dotnetcore/AspectCore-Framework/blob/master/docs/injector.md)

**AOP缓存组件使用**

本项目采用的AOP中间件 ：[AspectCore.Extensions.Cache](https://github.com/VictorTzeng/AspectCore.Extensions.Cache)

# 示例
* [Zxw.Framework.NetCore.Demo](https://github.com/VictorTzeng/Zxw.Framework.NetCore.Demo)


# .net framework版本地址
* [Zxw.Framework.Nfx](https://github.com/VictorTzeng/Zxw.Framework.Nfx)

# 项目说明
* 请参考我的博客：[http://www.cnblogs.com/zengxw/p/7673952.html](http://www.cnblogs.com/zengxw/p/7673952.html)

# 更新日志

**2019/12/17**
* 1. 添加框架同一入口扩展方法 [AddCoreX](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/66ce81a3ffa3eb9379631ba11a3fd36c4c369e60/Zxw.Framework.NetCore/Extensions/ServiceExtension.cs#L419)
```
services.AddCoreX(config=> { })
```

* 2. AddCoreX方法里面默认开启注入实现了ISingletonDependency、IScopedDependency、ITransientDependency三种不同生命周期的类，以及AddHttpContextAccessor和AddDataProtection。如需要自动注入，只需要按需实现ISingletonDependency、IScopedDependency、ITransientDependency这三种生命周期接口即可。

* 3. 添加会话上下文 [WebContext](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/master/Zxw.Framework.NetCore/Web/WebContext.cs)

**2019/09/16**
* 1.更换Oracle for efcore驱动，使用Oracle官方驱动

**2019/09/15**
* 1.重构AOP缓存，统一用CachedAttribute

**2019/08/11**
* 1.重构代码生成器，分CodeFirst和DbFirst

 a.启用代码生成器
```

//启用代码生成器
services.UseCodeGenerator(new CodeGeneratorOption());

```

 b.使用代码生成器
 
```

//CodeFirst---根据model生成其他各层的代码
dbContext.CodeFirst().GenerateAll(ifExsitedCovered:true);

//DbFirst---根据现有数据表生成各层代码
dbCOntext.DbFirst().GenerateAll(ifExsitedCovered:true);

```
* 2.添加对APIController的代码生成

**2019/04/25**
* 1.修改缓存拦截器默认key格式为：{namespace}{class}{method}{参数值hashcode}
* 2.缓存拦截器添加对Task<>类型的支持


**2019/04/18**
* 1.删除触发器功能...
* 2.实现多数据库上下文。用法：

```
    //注入数据库上下文
    services.AddDbContextFactory(factory =>
    {
        factory.AddDbContext<PostgreSQLDbContext>("db1", new DbContextOption(){ConnectionString = "User ID=postgres;Password=123456;Host=localhost;Port=5432;Database=ZxwPgDemo;Pooling=true;" });
        factory.AddDbContext<SqlServerDbContext>("db2", new DbContextOption() { ConnectionString = "" });
        factory.AddDbContext<MongoDbContext>("db3", new DbContextOption() { ConnectionString = "" });
    });


    //获取
    public class TestController
    {
        public IDbContextCore DbContext1 { get; set; }
        public IDbContextCore DbContext2 { get; set; }
        public IDbContextCore DbContext3 { get; set; }

        public TestController(DbContextFactory factory)
        {
            DbContext1 = factory.GetDbContext("db1");
            DbContext2 = factory.GetDbContext("db2");
            DbContext3 = factory.GetDbContext("db3");
        }

        public void Run()
        {
            var db = DbContext1.GetDatabase();
            Console.WriteLine();
        }
    }
```

* 3.多数据库上下文支持属性注入,用法如下：（具体请参考单元测试）
```    
public class TestRepository: BaseRepository<MongoModel, ObjectId>, IMongoRepository
    {
        [FromDbContextFactory("db1")]
        public IDbContextCore DbContext1 { get; set; }
        [FromDbContextFactory("db2")]
        public IDbContextCore DbContext2 { get; set; }
        [FromDbContextFactory("db3")]
        public IDbContextCore DbContext3 { get; set; }



        public void Run()
        {
            Console.WriteLine("Over!");
        }

        public TestRepository(IDbContextCore dbContext) : base(dbContext)
        {
        }
    }
```

**2018/09/24**
* 1.实现Oracle for EfCore，引用第三方驱动[Citms.EntityFrameworkCore.Oracle](https://github.com/CrazyJson/Citms.EntityFrameworkCore.Oracle)
* 2.实现MongoDB for EfCore，引用第三方驱动[Blueshift.EntityFrameworkCore.MongoDB](https://github.com/BlueshiftSoftware/EntityFrameworkCore)

**2018/08/26**
* 1.添加自定义视图分页查询，数据库分页，目前只支持sqlserver
* 2.update packages

**2018/07/06 合并dev分支到master**
* 1.添加EFCore直接返回[DataTable](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/d99b321006ad7ee12883e92742d3ef1fe44968f7/Zxw.Framework.NetCore/Extensions/EntityFrameworkCoreExtensions.cs#L20)功能
* 2.DBFirst功能，目前仅支持SQL Server、MySQL、NpgSQL三种数据库。根据已存在的数据表直接生成实体代码，详见[CodeGenerator](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/b07589d550a9f757b8da75e4fc685b917be29f34/Zxw.Framework.NetCore/CodeGenerator/CodeGenerator.cs#L197)
* 3.添加单元测试项目，并完成对以上两点新功能的测试
* 4.引入IOC容器[Aspectcore.Injector](https://github.com/dotnetcore/AspectCore-Framework/blob/master/docs/injector.md)，详见[AspectCoreContainer.cs](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/master/Zxw.Framework.NetCore/IoC/AspectCoreContainer.cs)

# 开源协议
* 本开源项目遵守[MIT](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/master/LICENSE)开源协议，请保留原作者出处。

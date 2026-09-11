# Zxw.Framework.NetCore
[![Build Status](https://dev.azure.com/v-xiaze0473/v-xiaze/_apis/build/status/VictorTzeng.Zxw.Framework.NetCore?branchName=master)](https://dev.azure.com/v-xiaze0473/v-xiaze/_build/latest?definitionId=1&branchName=master)

基于 EF Core 的 Code First 模式 .NET 快速开发框架

**NuGet [最新版本：7.0.0]**

[Zxw.Framework.NetCore](https://www.nuget.org/packages/Zxw.Framework.NetCore)
* `Install-Package Zxw.Framework.NetCore -Version 7.0.0`
* `dotnet add package Zxw.Framework.NetCore --version 7.0.0`

[Zxw.Framework.AI](https://www.nuget.org/packages/Zxw.Framework.AI)（可选）
* `Install-Package Zxw.Framework.AI -Version 1.0.0`
* `dotnet add package Zxw.Framework.AI --version 1.0.0`
* 内置 LLM 网关选项：[OrcaRouter](https://www.orcarouter.ai/ref/ref_4efd338f7db91cf2aa1d)（OpenAI 兼容，默认 `orcarouter/auto`）

**开发环境**
* Visual Studio 2022 / VS Code / Cursor
* .NET SDK 8 / 9 / 10

**目标框架**
* `net8.0` / `net9.0` / `net10.0`（已移除 net6 / net7）

**支持的数据库**
* SQL Server
* MySQL（net8/net9：Pomelo；net10：MySql.EntityFrameworkCore，待 Pomelo 正式支持 EF10 后切回）
* Sqlite
* InMemory
* PostgreSQL
* Oracle
* MongoDB（已移除）

**日志组件**
* log4net（建议业务侧优先使用 `ILogger<T>`）

**DI 组件**
* Microsoft.Extensions.DependencyInjection（默认）
* Autofac（可选）
* [AspectCore](https://github.com/dotnetcore/AspectCore-Framework)（可选 AOP）

**AOP 缓存组件使用**

本项目可采用 AOP 中间件：[AspectCore.Extensions.Cache](https://github.com/VictorTzeng/AspectCore.Extensions.Cache)

# 示例
* [Zxw.Framework.NetCore.Demo](https://github.com/VictorTzeng/Zxw.Framework.NetCore.Demo)


# .net framework版本地址
* [Zxw.Framework.Nfx](https://github.com/VictorTzeng/Zxw.Framework.Nfx)

# 项目说明
* 请参考我的博客：[http://www.cnblogs.com/zengxw/p/7673952.html](http://www.cnblogs.com/zengxw/p/7673952.html)

# 更新日志

**2026/08/19 — 7.0.0（破坏性升级）**

*Breaking*
* 1. 目标框架改为 `net8.0` / `net9.0` / `net10.0`，移除对 `net6.0` / `net7.0` 的支持
* 2. `IRepository` / `IService` / `IWebContext` 生命周期改为 **Scoped**（不再使用 Transient / Singleton 承载请求态依赖）
* 3. ASP.NET Core 改为 `FrameworkReference`，移除 `Microsoft.AspNetCore.* 2.2` 兼容包
* 4. 淘汰 **Jil**，`JsonConvertor` 统一为 `System.Text.Json`（原 `Jil.Options` 参数改为 `JsonSerializerOptions`；已移除 `DeserializeDynamic`）

*数据访问 / 工作单元*
* 5. 新增 `IUnitOfWork` / `EfUnitOfWork`；`DbContextOption.AutoSaveChanges`（默认 `true` 兼容旧行为，设为 `false` 后需显式提交以支持跨仓储事务）
* 6. 仓储构造函数不再调用 `EnsureCreated()`；生产环境请使用迁移
* 7. 仓储 `Dispose` 不再释放共享 `DbContext`（由 DI 作用域管理）
* 8. 修复 `EditRange` 未标记 `Modified`；分页 `GetByPagination` 多字段排序改为 `OrderBy` + `ThenBy`
* 9. 修复假异步（如 `ExistAsync`）；异步 API 补充 `CancellationToken`
* 10. SQL Server / MySQL `BulkInsert` 改为同步等待完成，避免未 await 导致数据不确定

*安全 / Web*
* 11. 新增 `SqlIdentifier`，校验分页 `ORDER BY`、视图名、表名等动态 SQL 标识符
* 12. `WebContext` 改为 Scoped，每次从 `IHttpContextAccessor` 读取当前 `HttpContext`
* 13. `GlobalExceptionFilter` 返回 `ProblemDetails`（500），不再静默吞掉异常
* 14. 修正 `RegisterControllers` 中 `IsAssignableFrom` 判断写反的问题

*依赖*
* 15. 按 TFM 对齐 EF Core / Npgsql / Oracle / Z.EntityFramework.Plus 主版本
* 16. net10 MySQL 暂用 `MySql.EntityFrameworkCore`（Pomelo 10 未发布）；`BulkInsert` 在 net10 退化为 `AddRange`
* 17. 移除重复的 `DotNetCore.NPOI` 引用与停更的 Jil / StackExchange.Redis.Extensions.JilCore

*测试*
* 18. 补充 `SqlIdentifier`、`IUnitOfWork` / `AutoSaveChanges`、分页 ThenBy、`WebContext` 作用域等单元测试（net8/9/10）

*可选 AI*
* 19. 新增独立包 **`Zxw.Framework.AI`**：内置 [OrcaRouter](https://www.orcarouter.ai/ref/ref_4efd338f7db91cf2aa1d) 作为一等 LLM Provider（`AddOrcaRouter` / `IChatClient`，含非流式与 SSE）；主包不引用，按需安装

*迁移提示*
```csharp
// 跨仓储事务：关闭自动提交
var option = new DbContextOption
{
    ConnectionString = "...",
    AutoSaveChanges = false
};
// ...
await repoA.AddAsync(a);
await repoB.AddAsync(b);
await uow.SaveChangesAsync(); // IUnitOfWork
```

*后续规划（包拆分）*
* 计划拆为可选包：`Abstractions` / `EFCore`（及各数据库 Provider）/ `Caching` / `AspNetCore` / `CodeGenerator`，并保留 `Zxw.Framework.NetCore` 元包兼容旧引用方式
* 已先行提供可选包 **`Zxw.Framework.AI`**：内置 [OrcaRouter](https://www.orcarouter.ai/ref/ref_4efd338f7db91cf2aa1d) 作为一等 LLM Provider（不进入主包，按需引用）

# LLM 网关（可选）— OrcaRouter

[![Powered by OrcaRouter](https://img.shields.io/badge/Powered_by-OrcaRouter-2563eb)](https://www.orcarouter.ai/ref/ref_4efd338f7db91cf2aa1d)

本框架将 **OrcaRouter** 注册为内置 LLM **provider**（见 [`providers/orcarouter.json`](providers/orcarouter.json)）。EF / DI 主包不绑定大模型；需要 Chat Completions 时引用 **`Zxw.Framework.AI`**，通过 `AddOrcaRouter()` 启用。

```csharp
services.AddOrcaRouter(o =>
{
    o.ApiKey = Configuration["OrcaRouter:ApiKey"]; // 或 ORCAROUTER_API_KEY；申请 Key：https://www.orcarouter.ai/ref/ref_4efd338f7db91cf2aa1d
    o.DefaultModel = "orcarouter/auto";
    o.FallbackModels = new[] { "openai/gpt-4o-mini", "deepseek/deepseek-chat" };
});

// 注入使用
public class MyService(IChatClient chat)
{
    public async Task<string> AskAsync(string prompt)
    {
        var resp = await chat.CompleteAsync(new ChatCompletionRequest
        {
            Messages = { ChatMessage.User(prompt) }
        });
        return resp.GetContent();
    }
}
```
* Provider 清单：[providers/orcarouter.json](providers/orcarouter.json)（id: orcarouter）
* 环境变量示例：[.env.example](.env.example)
* Base URL 默认：`https://api.orcarouter.ai/v1`
* 协议：OpenAI 兼容 `/chat/completions`（含 SSE 流式）
* 文档：[docs.orcarouter.ai](https://docs.orcarouter.ai/introduction)

## 两种接入方式（API Key / 账号授权）

两种方式最终都只产生同一种普通 OrcaRouter API Key（`sk-orca-…`），下游 provider、模型目录与各入口使用同一个凭据 seam
（`IOrcaCredentialStore`），不需要各自复制认证逻辑。

| 方式 | Provider ID | 标签 | 凭据来源 |
| --- | --- | --- | --- |
| 已有 Key | `orcarouter` | `OrcaRouter - API` | 用户粘贴或配置 `sk-orca-…` |
| 账号登录 | `orcarouter-oauth` | `OrcaRouter - Auth` | 浏览器授权（OAuth 2.0 + PKCE，Flow B out-of-band） |

**方式一：API Key** — 沿用本框架既有的配置/环境变量习惯，支持更新与清除，鉴权失败时给出明确提示：

```csharp
services.AddOrcaRouter(o => o.ApiKey = Configuration["OrcaRouter:ApiKey"]); // 或 ORCAROUTER_API_KEY
```

**方式二：Connect with OrcaRouter（OAuth 2.0 + PKCE）** — 无需 client secret，无需预注册 redirect URI。
采用 Flow B（out-of-band code）：本库由 ASP.NET Core 应用承载，部署地址各不相同且库不应接管宿主的 socket 生命周期，
因此没有可预注册的回环地址。

```csharp
var pkce = services.BuildServiceProvider().GetRequiredService<OrcaPkceCredentialProvider>();
var session = pkce.Start(appName: "My App");
Console.WriteLine(session.AuthorizeUrl);          // 在浏览器打开并同意
var result = await session.CompleteAsync(codeFromScreen);  // 输入屏幕上显示的 code
```

* 授权 origin：`https://www.orcarouter.ai`，authorize 固定 `/auth`，exchange 固定 `/api/v1/auth/keys`
* 推理与模型目录 origin：`https://api.orcarouter.ai/v1`（两个 origin 相互独立，不会互相推导）
* 覆盖顺序：显式 `AuthBaseUrl` / `ApiBaseUrl` > 共享 `SharedBaseUrl` / `ORCA_BASE_URL` > 公开默认值
* 始终使用 S256；verifier 每次尝试由加密 RNG 新建，且不进入 URL / 日志 / 遥测
* PKCE 换回的是**持久 API Key，不是 refresh token**：重启后直接复用直到被撤销，不会主动刷新，
  也不会每次启动重新授权（每用户 24 小时最多签发 10 个 PKCE key）
* 中继返回 `401` 时按 terminal reauthentication 处理：只标记发出被拒请求的那个账号与凭据 generation

## 模型目录与能力下拉

模型清单的唯一事实源是当前配置 origin 下的 `GET /v1/models`，模型 ID 原样保留 `vendor/model` 命名空间。
`OrcaModelSelector` 是唯一向上层提供下拉选项的地方，按入口能力过滤：

| 入口 | 过滤规则 |
| --- | --- |
| 文本 chat / agent | `?capability=chat`，且 `supported_endpoint_types` 命中 openai / anthropic / gemini / openai-response，排除 image-generation / openai-video / jina-rerank / embeddings |
| 多模态理解 | 先满足 chat，再要求 `architecture.input_modalities` 明确包含实际上传的模态（image / audio / video）；未声明的模型 fail closed |
| Embedding | `?capability=embedding` 或 `embeddings` endpoint |
| 图片生成 | `?capability=image` 或 `image-generation` endpoint |
| 视频生成 | 严格匹配 `openai-video` endpoint |
| Rerank | 严格匹配 `jina-rerank` endpoint |

```csharp
var catalog = await selector.GetOptionsAsync(OrcaModelCapability.Multimodal, new[] { "image" });
// catalog.Status: Live / SeedFallback / LastKnownGood / Empty，配合 catalog.IsDegraded 显示降级状态
```

* 实时发现成功时其结果为权威目录，**不会**混入 seed
* 发现失败时回退到小型已验证 seed（`openai/gpt-5.5`、`anthropic/claude-opus-4.8`、
  `google/gemini-3.5-flash`、`deepseek/deepseek-v4-pro`、`orcarouter/auto`），保留已验证的
  context / input-modality / reasoning 元数据，并明确标注为降级，不会退回自由文本输入
* 多模态开关 / 附件类型变化时重新计算选项；已选模型不再兼容时清空并提示用户重新选择

## 示例宿主

[`samples/Zxw.Framework.AI.SampleHost`](samples/Zxw.Framework.AI.SampleHost) 同时演示两种接入方式、模型下拉，
以及 OAuth 登录锁在成功 / 拒绝 / 交换失败 / 超时 / 显式取消 / 切换方式 / 关闭弹窗 / `pagehide` 下的释放：

```bash
dotnet run --project samples/Zxw.Framework.AI.SampleHost -- --live-check   # 真实目录 + 真实推理
dotnet run --project samples/Zxw.Framework.AI.SampleHost -- --urls http://127.0.0.1:5080
```

API Key 只保留在服务端，浏览器只拿到模型元数据。

**2022/11/20**
* 1.兼容.net7.0，
* 2.移除对MongoDB的支持
* 3.HttpRequestHelper标识为已过时，请用HttpClient

**2019/12/17**
* 1. 添加框架同一入口扩展方法 [AddCoreX](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/66ce81a3ffa3eb9379631ba11a3fd36c4c369e60/Zxw.Framework.NetCore/Extensions/ServiceExtension.cs#L419)
```
services.AddCoreX(config=> { })
```

* 2. AddCoreX方法里面默认开启注入实现了ISingletonDependency、IScopedDependency、ITransientDependency三种不同生命周期的类，以及AddHttpContextAccessor和AddDataProtection。如需要自动注入，只需要按需实现ISingletonDependency、IScopedDependency、ITransientDependency这三种生命周期接口即可。

* 3. 添加会话上下文 [WebContext](https://github.com/VictorTzeng/Zxw.Framework.NetCore/blob/master/Zxw.Framework.NetCore/Web/WebContext.cs)

* 4. 升级 [AspectCore](https://github.com/dotnetcore/AspectCore-Framework) 至 2.0.0

* 5. 示例 [Zxw.Framework.NetCore.Demo](https://github.com/VictorTzeng/Zxw.Framework.NetCore.Demo) 已同步更新。

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

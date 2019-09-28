using System;
using System.Data;
using Zxw.Framework.NetCore.CodeGenerator.CodeFirst;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.CodeGenerator.DbFirst
{
    public interface IDbFirst
    {
        IDbContextCore DbContext { get; set; }
        void GenerateAll(bool ifExistCovered = false);
        IDbFirst Generate(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateEntity(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateSingle(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateIRepository(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateRepository(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateIService(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateService(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateController(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateApiController(Func<DbTable, bool> selector, bool ifExistCovered = false);
        IDbFirst GenerateViewModel(string viewName, bool ifExistCovered = false);
        IDbFirst GenerateViewModel(DataTable dt, string className, bool ifExistCovered = false);
        IDbFirst GenerateViewModel(DataSet ds, bool ifExistCovered = false);
    }

    public static class DbFirstExtensions
    {
        public static IDbFirst DbFirst(this IDbContextCore dbContext)
        {
            var dbFirst = ServiceLocator.Resolve<IDbFirst>();
            if(dbFirst == null)
                throw new Exception("请先在Startup.cs文件的ConfigureServices方法中调用UseCodeGenerator方法以注册。");
            dbFirst.DbContext = dbContext;
            return dbFirst;
        }
    }
}
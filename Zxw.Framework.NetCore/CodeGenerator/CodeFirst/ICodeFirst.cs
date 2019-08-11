using System;
using Zxw.Framework.NetCore.CodeGenerator.DbFirst;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.CodeGenerator.CodeFirst
{
    public interface ICodeFirst
    {
        void GenerateAll(bool ifExsitedCovered = false);
        ICodeFirst GenerateSingle<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateIRepository<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateRepository<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateIService<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateService<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateController<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
        ICodeFirst GenerateApiController<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>;
    }

    public static class CodeFirstExtensions
    {
        public static ICodeFirst CodeFirst(this IDbContextCore dbContext)
        {
            var codeFirst = AspectCoreContainer.Resolve<ICodeFirst>();
            if (codeFirst == null)
                throw new Exception("请先在Startup.cs文件的ConfigureServices方法中调用UseCodeGenerator方法以注册。");
            return codeFirst;
        }
    }

}
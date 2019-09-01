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
        void GenerateAll(bool ifExistCovered = false);
        ICodeFirst GenerateSingle<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateIRepository<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateRepository<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateIService<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateService<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateController<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
        ICodeFirst GenerateApiController<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>;
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
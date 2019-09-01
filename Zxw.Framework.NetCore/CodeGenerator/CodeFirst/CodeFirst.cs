using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.CodeGenerator.CodeFirst
{
    internal sealed class CodeFirst:ICodeFirst
    {
        private CodeGenerator Instance { get; set; }
        public CodeFirst(IOptions<CodeGenerateOption> option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            Instance = new CodeGenerator(option.Value);
        }

        public void GenerateAll(bool ifExistCovered = false)
        {
            Instance.Generate(ifExistCovered);
        }

        public ICodeFirst GenerateSingle<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateSingle<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateIRepository<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateIRepository<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateRepository<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateRepository<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateIService<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateIService<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateService<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateService<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateController<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateController<T, TKey>(ifExistCovered);
            return this;
        }

        public ICodeFirst GenerateApiController<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            Instance.GenerateApiController<T, TKey>(ifExistCovered);
            return this;
        }
    }
}

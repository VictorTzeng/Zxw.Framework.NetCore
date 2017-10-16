using Zxw.Framework.EfDbContext;
using Zxw.Framework.Helpers;
using Zxw.Framework.IoC;
using Zxw.Framework.Options;

namespace Zxw.Framework.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitIoC();

            CodeGenerator.CodeGenerator.Generate();
        }

        static void InitIoC()
        {
            var dbContextOption = new DbContextOption
            {
                ConnectionString = ConfigHelper.GetConnectionString("mssqlserver"),
                ModelAssemblyName = "Zxw.Framework.Sample.Models"
            };
            var codeGenerateOption = new CodeGenerateOption
            {
                ModelsNamespace = "Zxw.Framework.Sample.Models",
                IRepositoriesNamespace = "Zxw.Framework.Sample.IRepositories",
                RepositoriesNamespace = "Zxw.Framework.Sample.Repositories",
                IServicsNamespace = "Zxw.Framework.Sample.IServices",
                ServicesNamespace = "Zxw.Framework.Sample.Services"
            };
            IoCContainer.Register(dbContextOption);
            IoCContainer.Register(codeGenerateOption);
            IoCContainer.Register<DefaultDbContext>();

            #region 此段代码在Repository和Service层代码生成之后再启用

            //IoCContainer.RegisterImplementationAssemblyAsInterface("Zxw.Framework.Sample.Repositories", "Zxw.Framework.Sample.IRepositories");
            //IoCContainer.RegisterImplementationAssemblyAsInterface("Zxw.Framework.Sample.Services", "Zxw.Framework.Sample.IServices");

            #endregion

            IoCContainer.Build();
        }
    }
}

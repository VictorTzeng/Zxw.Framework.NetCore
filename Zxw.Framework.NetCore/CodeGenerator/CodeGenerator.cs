using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.CodeGenerator
{
    /// <summary>
    /// 代码生成器。
    /// <remarks>
    /// 根据指定的实体域名空间生成Repositories和Services层的基础代码文件。
    /// </remarks>
    /// </summary>
    public class CodeGenerator
    {
        private static readonly string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符
        private static readonly string ParentPath;//

        private static readonly CodeGenerateOption Option;
        /// <summary>
        /// 静态构造函数：从IoC容器读取配置参数，如果读取失败则会抛出ArgumentNullException异常
        /// </summary>
        static CodeGenerator()
        {
            Option = AutofacContainer.Resolve<IOptions<CodeGenerateOption>>()?.Value;
            if (Option == null)
            {
                throw new ArgumentNullException(nameof(Option));
            }
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var flag = path.IndexOf("/bin");
            if (flag > 0)
                Delimiter = "/";//如果可以取到值，修改分割符
            path = path.Substring(0, path.IndexOf(Delimiter + "bin"));
            ParentPath = path.Substring(0, path.LastIndexOf(Delimiter));
        }

        /// <summary>
        /// 生成指定的实体域名空间下各实体对应Repositories和Services层的基础代码文件
        /// </summary>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public static void Generate(bool ifExsitedCovered = false)
        {
            var assembly = Assembly.Load(Option.ModelsNamespace);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract && !t.IsNested && t.GetInterfaces()
                    .Any(m => m.GetGenericTypeDefinition() == typeof(IBaseModel<>)));
            if (list != null)
            {
                foreach (var type in list)
                {
                    GenerateSingle(type, ifExsitedCovered);
                }
            }
        }

        /// <summary>
        /// 生成指定的实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <typeparam name="T">实体类型（必须实现IBaseModel接口）</typeparam>
        /// <typeparam name="TKey">实体主键类型</typeparam>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public static void GenerateSingle<T, TKey>(bool ifExsitedCovered = false) where T : class, IBaseModel<TKey>
        {
            GenerateSingle(typeof(T), ifExsitedCovered);
        }

        /// <summary>
        /// 生成指定的实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <param name="modelType">实体类型（必须实现IBaseModel接口）</param>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        private static void GenerateSingle(Type modelType, bool ifExsitedCovered = false)
        {
            var modelTypeName = modelType.Name;
            var keyTypeName = modelType.GetProperty("Id")?.PropertyType.Name;
            GenerateIRepository(modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateRepository(modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateController(modelTypeName, keyTypeName, ifExsitedCovered);
        }

        /// <summary>
        /// 从代码模板中读取内容
        /// </summary>
        /// <param name="templateName">模板名称，应包括文件扩展名称。比如：template.txt</param>
        /// <returns></returns>
        private static string ReadTemplate(string templateName)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var content = string.Empty;
            using (var stream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.CodeTemplate.{templateName}"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = reader.ReadToEnd();
                    }
                }
            }
            return content;
        }

        /// <summary>
        /// 生成IRepository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateIRepository(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var iRepositoryPath = ParentPath + Delimiter + Option.IRepositoriesNamespace;
            if (!Directory.Exists(iRepositoryPath))
            {
                iRepositoryPath = ParentPath + Delimiter + "IRepositories";
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        /// <summary>
        /// 生成Repository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateRepository(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var repositoryPath = ParentPath + Delimiter + Option.RepositoriesNamespace;
            if (!Directory.Exists(repositoryPath))
            {
                repositoryPath = ParentPath + Delimiter + "Repositories";
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{RepositoriesNamespace}", Option.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        /// <summary>
        /// 生成Controller层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateController(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var controllerPath = ParentPath + Delimiter + Option.ControllersNamespace;
            if (!Directory.Exists(controllerPath))
            {
                controllerPath = ParentPath + Delimiter + "Controllers";
                Directory.CreateDirectory(controllerPath);
            }
            var fullPath = controllerPath + Delimiter + modelTypeName + "Controller.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("ControllerTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{ControllersNamespace}", Option.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        private static void WriteAndSave(string fileName, string content)
        {
            //实例化一个文件流--->与写入文件相关联
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                //实例化一个StreamWriter-->与fs相关联
                using (var sw = new StreamWriter(fs))
                {
                    //开始写入
                    sw.Write(content);
                    //清空缓冲区
                    sw.Flush();
                    //关闭流
                    sw.Close();
                    fs.Close();
                }
            }
        }
    }
}

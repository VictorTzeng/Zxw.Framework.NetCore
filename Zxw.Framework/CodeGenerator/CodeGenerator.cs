using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Zxw.Framework.IoC;
using Zxw.Framework.Models;
using Zxw.Framework.Options;

namespace Zxw.Framework.CodeGenerator
{
    /// <summary>
    /// 代码生成器。
    /// <remarks>
    /// 根据指定的实体域名空间生成IServices和Services层的基础代码文件。
    /// </remarks>
    /// </summary>
    public class CodeGenerator
    {
        private static CodeGenerateOption _option;
        /// <summary>
        /// 静态构造函数：从IoC容器读取配置参数，如果读取失败则会抛出ArgumentNullException异常
        /// </summary>
        static CodeGenerator()
        {
            _option = IoCContainer.Resolve<CodeGenerateOption>();
            if (_option == null)
            {
                throw new ArgumentNullException(nameof(_option));
            }
        }

        /// <summary>
        /// 生成指定的实体域名空间下各实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public static void Generate(bool ifExsitedCovered = false)
        {
            var assembly = Assembly.Load(_option.ModelsNamespace);
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
        public static void GenerateSingle<T, TKey>(bool ifExsitedCovered = false) where T:class, IBaseModel<TKey>
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
            var modelsNamespace = modelType.Namespace;
            var modelTypeName = modelType.Name;
            var keyTypeName = modelType.GetProperty("Id")?.PropertyType.Name;
            GenerateIRepository(modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateRepository(modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateIService(modelsNamespace, modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateService(modelsNamespace, modelTypeName, keyTypeName, ifExsitedCovered);
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
            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", _option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", _option.IRepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Substring(0, path.IndexOf("\\bin"));
            var parentPath = path.Substring(0, path.LastIndexOf("\\"));
            var iServicesPath = parentPath + "\\" + _option.IRepositoriesNamespace;
            if (!Directory.Exists(iServicesPath))
            {
                iServicesPath = parentPath + "\\IRepositories";
                Directory.CreateDirectory(iServicesPath);
            }
            var fullPath = iServicesPath + "\\I" + modelTypeName + "Repository.cs";
            if (!File.Exists(fullPath) || ifExsitedCovered)
            {
                WriteAndSave(fullPath, content);
            }
        }
        /// <summary>
        /// 生成Repository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateRepository(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", _option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", _option.IRepositoriesNamespace)
                .Replace("{RepositoriesNamespace}", _option.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Substring(0, path.IndexOf("\\bin"));
            var parentPath = path.Substring(0, path.LastIndexOf("\\"));
            var iServicesPath = parentPath + "\\" + _option.RepositoriesNamespace;
            if (!Directory.Exists(iServicesPath))
            {
                iServicesPath = parentPath + "\\Repositories";
                Directory.CreateDirectory(iServicesPath);
            }
            var fullPath = iServicesPath + "\\" + modelTypeName + "Repository.cs";
            if (!File.Exists(fullPath) || ifExsitedCovered)
            {
                WriteAndSave(fullPath, content);
            }
        }

        /// <summary>
        /// 生成IService文件
        /// </summary>
        /// <param name="modelsNamespace"></param>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateIService(string modelsNamespace, string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var iServicsNamespace = _option.IServicsNamespace;
            var content = ReadTemplate("IServiceTemplate.txt");
            content = content.Replace("{ModelsNamespace}", modelsNamespace)
                .Replace("{IServicsNamespace}", iServicsNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Substring(0, path.IndexOf("\\bin"));
            var parentPath = path.Substring(0, path.LastIndexOf("\\"));
            var iServicesPath = parentPath + "\\" + iServicsNamespace;
            if (!Directory.Exists(iServicesPath))
            {
                iServicesPath = parentPath + "\\IServices";
                Directory.CreateDirectory(iServicesPath);
            }
            var fullPath = iServicesPath + "\\I" + modelTypeName + "Service.cs";
            if (!File.Exists(fullPath)|| ifExsitedCovered)
            {
                WriteAndSave(fullPath, content);
            }
        }

        /// <summary>
        /// 生成Service文件
        /// </summary>
        /// <param name="modelsNamespace"></param>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateService(string modelsNamespace, string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var iServicsNamespace = _option.IServicsNamespace;
            var servicesNamespace = _option.ServicesNamespace;
            var content = ReadTemplate("ServiceTemplate.txt");
            content = content
                .Replace("{IRepositoriesNamespace}", _option.IRepositoriesNamespace)
                .Replace("{IServicsNamespace}", iServicsNamespace)
                .Replace("{ModelsNamespace}", modelsNamespace)
                .Replace("{ServicesNamespace}", servicesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Substring(0, path.IndexOf("\\bin"));
            var parentPath = path.Substring(0, path.LastIndexOf("\\"));
            var servicesPath = parentPath + "\\" + servicesNamespace;
            if (!Directory.Exists(servicesPath))
            {
                servicesPath = parentPath + "\\Services";
                Directory.CreateDirectory(servicesPath);
            }
            var fullPath = servicesPath + "\\" + modelTypeName + "Service.cs";
            if (!File.Exists(fullPath) || ifExsitedCovered)
            {
                WriteAndSave(fullPath, content);
            }
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

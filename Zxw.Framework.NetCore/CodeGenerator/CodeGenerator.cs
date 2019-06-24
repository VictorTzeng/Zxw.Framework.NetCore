using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IDbContext;
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
    public static class CodeGenerator
    {
        private static readonly string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符

        private static IOptions<CodeGenerateOption> options;
        /// <summary>
        /// 静态构造函数：从IoC容器读取配置参数，如果读取失败则会抛出ArgumentNullException异常
        /// </summary>
        static CodeGenerator()
        {
            options = AspectCoreContainer.Resolve<IOptions<CodeGenerateOption>>();
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var flag = path.IndexOf("/bin");
            if (flag > 0)
                Delimiter = "/";//如果可以取到值，修改分割符
        }

        /// <summary>
        /// 生成指定的实体域名空间下各实体对应Repositories和Services层的基础代码文件
        /// </summary>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public static void Generate(bool ifExsitedCovered = false)
        {
            var assembly = Assembly.Load(options.Value.ModelsNamespace);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsClass && !t.IsGenericType && !t.IsAbstract && !t.IsNested);
            if (list != null)
            {
                foreach (var type in list)
                {
                    var baseType = typeof(BaseModel<>).MakeGenericType(new[]{ type.BaseType?.GenericTypeArguments[0] });
                    if (type.IsSubclassOf(baseType))
                    {
                        GenerateSingle(type, ifExsitedCovered);
                    }
                }
            }
        }

        /// <summary>
        /// 生成指定的实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <typeparam name="T">实体类型（必须实现IBaseModel接口）</typeparam>
        /// <typeparam name="TKey">实体主键类型</typeparam>
        /// <param name="ifExsitedCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public static void GenerateSingle<T, TKey>(bool ifExsitedCovered = false) where T : BaseModel<TKey>
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
            GenerateIService(modelTypeName, keyTypeName, ifExsitedCovered);
            GenerateService(modelTypeName, keyTypeName, ifExsitedCovered);
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
            var iRepositoryPath = options.Value.OutputPath + Delimiter + "IRepositories";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName.ToPascalCase() + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
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
            var repositoryPath = options.Value.OutputPath + Delimiter + "Repositories";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName.ToPascalCase() + "Repository.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
                .Replace("{RepositoriesNamespace}", options.Value.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        /// <summary>
        /// 生成IRepository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExsitedCovered"></param>
        private static void GenerateIService(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var iRepositoryPath = options.Value.OutputPath + Delimiter + "IServices";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName.ToPascalCase() + "Service.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("IServiceTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
                .Replace("{IServicesNamespace}", options.Value.IServicesNamespace)
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
        private static void GenerateService(string modelTypeName, string keyTypeName, bool ifExsitedCovered = false)
        {
            var repositoryPath = options.Value.OutputPath + Delimiter + "Services";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName.ToPascalCase() + "Service.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("ServiceTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", options.Value.IRepositoriesNamespace)
                .Replace("{IServicesNamespace}", options.Value.IServicesNamespace)
                .Replace("{ServicesNamespace}", options.Value.ServicesNamespace)
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
            var controllerPath = options.Value.OutputPath + Delimiter + "Controllers";
            if (!Directory.Exists(controllerPath))
            {
                Directory.CreateDirectory(controllerPath);
            }
            var fullPath = controllerPath + Delimiter + modelTypeName.ToPascalCase() + "Controller.cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;
            var content = ReadTemplate("ControllerTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{IServicesNamespace}", options.Value.IServicesNamespace)
                .Replace("{ControllersNamespace}", options.Value.ControllersNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        /// <summary>
        /// 根据数据表生成Model层、Controller层、IRepository层和Repository层代码
        /// </summary>
        /// <param name="ifExsitedCovered">是否覆盖已存在的同名文件</param>
        /// <param name="selector"></param>
        public static void GenerateAllCodesFromDatabase(bool ifExsitedCovered = false, Func<DbTable, bool> selector = null)
        {
            var dbContext = AspectCoreContainer.Resolve<IDbContextCore>();
            if(dbContext == null)
                throw new Exception("未能获取到数据库上下文，请先注册数据库上下文。");
            var tables = dbContext.GetCurrentDatabaseTableList();
            if (tables != null && tables.Any())
            {
                if (selector == null)
                    selector = m => true;
                tables = tables.Where(selector).ToList();
                foreach (var table in tables)
                {
                    if (table.Columns.Any(c => c.IsPrimaryKey))
                    {
                        var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                        GenerateEntity(table, ifExsitedCovered);
                        GenerateIRepository(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                        GenerateRepository(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                        GenerateIService(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                        GenerateService(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                        GenerateController(table.TableName.ToPascalCase(), pkTypeName, ifExsitedCovered);
                    }
                }
            }
        }

        private static void GenerateEntity(DbTable table, bool ifExsitedCovered = false)
        {
            var modelPath = options.Value.OutputPath + Delimiter + "Models";
            if (!Directory.Exists(modelPath))
            {
                Directory.CreateDirectory(modelPath);
            }

            var fullPath = modelPath + Delimiter + table.TableName + ".cs";
            if (File.Exists(fullPath) && !ifExsitedCovered)
                return;

            var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
            var sb = new StringBuilder();
            foreach (var column in table.Columns)
            {
                var tmp = GenerateEntityProperty(column);
                sb.AppendLine(tmp);
                sb.AppendLine();
            }
            var content = ReadTemplate("ModelTemplate.txt");
            content = content.Replace("{ModelsNamespace}", options.Value.ModelsNamespace)
                .Replace("{Comment}", table.TableComment)
                .Replace("{TableName}", table.TableName)
                .Replace("{ModelName}", table.TableName.ToPascalCase())
                .Replace("{KeyTypeName}", pkTypeName)
                .Replace("{ModelProperties}", sb.ToString());
            WriteAndSave(fullPath, content);
        }

        private static string GenerateEntityProperty(DbTableColumn column)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(column.Comments))
            {
                sb.AppendLine("\t\t/// <summary>");
                sb.AppendLine("\t\t/// " + column.Comments);
                sb.AppendLine("\t\t/// </summary>");
            }
            if (column.IsPrimaryKey)
            {
                sb.AppendLine("\t\t[Key]");
                sb.AppendLine($"\t\t[Column(\"{column.ColName}\")]");
                if (column.IsIdentity)
                {
                    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }
                sb.AppendLine($"\t\tpublic override {column.CSharpType} Id " + "{get;set;}");
            }
            else
            {
                sb.AppendLine($"\t\t[Column(\"{column.ColName}\")]");
                if (!column.IsNullable)
                {
                    sb.AppendLine("\t\t[Required]");
                }

                if (column.ColumnLength.HasValue && column.ColumnLength.Value>0)
                {
                    sb.AppendLine($"\t\t[MaxLength({column.ColumnLength.Value})]");
                }
                if (column.IsIdentity)
                {
                    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }

                var colType = column.CSharpType;
                if (colType.ToLower() != "string" && colType.ToLower() != "byte[]" && colType.ToLower() != "object" &&
                    column.IsNullable)
                {
                    colType = colType + "?";
                }

                sb.AppendLine($"\t\tpublic {colType} {column.ColName.ToPascalCase()} " + "{get;set;}");
            }

            return sb.ToString();
        }

        public static void ToGenerateViewModelFile(this DataTable dt, string className, string outputDir)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));
            var template = ReadTemplate("ViewModelTemplate.txt");
            //var columnTemplate = "[SugarColumn(ColumnName = \"{0}\")]\r\npublic {1} {2} { get; set; }";
            var columnBuilder = new StringBuilder();
            foreach (DataColumn column in dt.Columns)
            {
                columnBuilder.AppendLine($"[Column(\"{column.ColumnName}\")]")
                    .AppendLine(
                    $"public {column.DataType.Name} {column.ColumnName.ToPascalCase()}" +
                    "{ get; set; }");
                columnBuilder.AppendLine();
            }

            var content = template.Replace("{0}", options.Value.ViewModelsNamespace)
                .Replace("{1}", dt.TableName)
                .Replace("{2}", className)
                .Replace("{3}", columnBuilder.ToString());

            WriteAndSave($"{outputDir}\\{className}.cs", content);
        }

        public static void ToGenerateViewModelFile(this DataSet ds, string outputDir)
        {
            if (ds == null) throw new ArgumentNullException(nameof(ds));
            foreach (DataTable table in ds.Tables)
            {
                table.ToGenerateViewModelFile(table.TableName, outputDir);
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

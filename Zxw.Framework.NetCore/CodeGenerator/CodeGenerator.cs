using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.Core.Internal;
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
    public class CodeGenerator
    {
        private string Delimiter = "\\";//分隔符，默认为windows下的\\分隔符
        public  CodeGenerateOption Option { get; set; }
        /// <summary>
        /// 静态构造函数：从IoC容器读取配置参数，如果读取失败则会抛出ArgumentNullException异常
        /// </summary>
        public CodeGenerator(CodeGenerateOption option)
        {
            Option = option;
            //Options = ServiceLocator.Resolve<IOptions<CodeGenerateOption>>().Value;
            //if (Options == null)
            //{
            //    throw new ArgumentNullException(nameof(Options));
            //}
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var flag = path.IndexOf("/bin");
            if (flag > 0)
                Delimiter = "/";//如果可以取到值，修改分割符
        }

        /// <summary>
        /// 生成指定的实体域名空间下各实体对应Repositories和Services层的基础代码文件
        /// </summary>
        /// <param name="ifExistCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public  void Generate(bool ifExistCovered = false)
        {
            var assembly = Assembly.Load(Option.ModelsNamespace);
            var types = assembly?.GetTypes();
            var list = types?.Where(t =>
                t.IsImplement(typeof(IBaseModel<>)));
            if (list != null)
            {
                foreach (var type in list)
                {
                    GenerateSingle(type, ifExistCovered);
                }
            }
        }

        /// <summary>
        /// 生成指定的实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <typeparam name="T">实体类型（必须实现IBaseModel接口）</typeparam>
        /// <typeparam name="TKey">实体主键类型</typeparam>
        /// <param name="ifExistCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public  void GenerateSingle<T, TKey>(bool ifExistCovered = false) where T : IBaseModel<TKey>
        {
            GenerateSingle(typeof(T), ifExistCovered);
        }

        /// <summary>
        /// 生成指定的实体对应IServices和Services层的基础代码文件
        /// </summary>
        /// <param name="modelType">实体类型（必须实现IBaseModel接口）</param>
        /// <param name="ifExistCovered">如果目标文件存在，是否覆盖。默认为false</param>
        public  void GenerateSingle(Type modelType, bool ifExistCovered = false)
        {
            var modelTypeName = modelType.Name;
            var keyTypeName = modelType.GetProperty("Id")?.PropertyType.Name;
            GenerateIRepository(modelTypeName, keyTypeName, ifExistCovered);
            GenerateRepository(modelTypeName, keyTypeName, ifExistCovered);
            GenerateIService(modelTypeName, keyTypeName, ifExistCovered);
            GenerateService(modelTypeName, keyTypeName, ifExistCovered);
            GenerateController(modelTypeName, keyTypeName, ifExistCovered);
            GenerateApiController(modelTypeName, keyTypeName, ifExistCovered);
        }

        /// <summary>
        /// 从代码模板中读取内容
        /// </summary>
        /// <param name="templateName">模板名称，应包括文件扩展名称。比如：template.txt</param>
        /// <returns></returns>
        public  string ReadTemplate(string templateName)
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


        public string ReadFile(string fileFullName)
        {
            return File.ReadAllText(fileFullName);
        }

        public string ReadFile(string fileType, string fileName)
        {
            var fileFullName = Option.OutputPath + Delimiter + fileType + Delimiter + fileName;
            return ReadFile(fileFullName);
        }
        /// <summary>
        /// 生成IRepository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateIRepository(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var iRepositoryPath = Option.OutputPath + Delimiter + "IRepositories";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("IRepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        public  void GenerateIRepository<TEntity, TKey>(bool ifExistCovered = false)
            => GenerateIRepository(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);
        /// <summary>
        /// 生成Repository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateRepository(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var repositoryPath = Option.OutputPath + Delimiter + "Repositories";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName + "Repository.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("RepositoryTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{RepositoriesNamespace}", Option.RepositoriesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        public  void GenerateRepository<TEntity, TKey>(bool ifExistCovered = false)
        => GenerateRepository(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);
        /// <summary>
        /// 生成IRepository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateIService(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var iRepositoryPath = Option.OutputPath + Delimiter + "IServices";
            if (!Directory.Exists(iRepositoryPath))
            {
                Directory.CreateDirectory(iRepositoryPath);
            }
            var fullPath = iRepositoryPath + Delimiter + "I" + modelTypeName + "Service.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("IServiceTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{IServicesNamespace}", Option.IServicesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }

        public  void GenerateIService<TEntity, TKey>(bool ifExistCovered = false)
            => GenerateIService(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);
        /// <summary>
        /// 生成Repository层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateService(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var repositoryPath = Option.OutputPath + Delimiter + "Services";
            if (!Directory.Exists(repositoryPath))
            {
                Directory.CreateDirectory(repositoryPath);
            }
            var fullPath = repositoryPath + Delimiter + modelTypeName + "Service.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("ServiceTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IRepositoriesNamespace}", Option.IRepositoriesNamespace)
                .Replace("{IServicesNamespace}", Option.IServicesNamespace)
                .Replace("{ServicesNamespace}", Option.ServicesNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        public  void GenerateService<TEntity, TKey>(bool ifExistCovered = false)
            => GenerateService(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);

        /// <summary>
        /// 生成Controller层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateController(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var controllerPath = Option.OutputPath + Delimiter + "Controllers";
            if (!Directory.Exists(controllerPath))
            {
                Directory.CreateDirectory(controllerPath);
            }
            var fullPath = controllerPath + Delimiter + modelTypeName + "Controller.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("ControllerTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IServicesNamespace}", Option.IServicesNamespace)
                .Replace("{ControllersNamespace}", Option.ControllersNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        public  void GenerateController<TEntity, TKey>(bool ifExistCovered = false)
            => GenerateController(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);
        /// <summary>
        /// 生成Controller层代码文件
        /// </summary>
        /// <param name="modelTypeName"></param>
        /// <param name="keyTypeName"></param>
        /// <param name="ifExistCovered"></param>
        public  void GenerateApiController(string modelTypeName, string keyTypeName, bool ifExistCovered = false)
        {
            var controllerPath = Option.OutputPath + Delimiter + "Controllers";
            if (!Directory.Exists(controllerPath))
            {
                Directory.CreateDirectory(controllerPath);
            }
            var fullPath = controllerPath + Delimiter + modelTypeName + "Controller.cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;
            var content = ReadTemplate("ApiControllerTemplate.txt");
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{IServicesNamespace}", Option.IServicesNamespace)
                .Replace("{ControllersNamespace}", Option.ControllersNamespace)
                .Replace("{ModelTypeName}", modelTypeName)
                .Replace("{KeyTypeName}", keyTypeName);
            WriteAndSave(fullPath, content);
        }
        public  void GenerateApiController<TEntity, TKey>(bool ifExistCovered = false)
            => GenerateApiController(typeof(TEntity).Name, typeof(TKey).Name, ifExistCovered);

        /// <summary>
        /// 根据数据表生成Model层、Controller层、IRepository层和Repository层代码
        /// </summary>
        /// <param name="ifExistCovered">是否覆盖已存在的同名文件</param>
        /// <param name="selector"></param>
        public  void GenerateAllCodesFromDatabase(bool ifExistCovered = false, Func<DbTable, bool> selector = null)
        {
            var dbContext = ServiceLocator.Resolve<IDbContextCore>();
            if(dbContext == null)
                throw new Exception("未能获取到数据库上下文，请先注册数据库上下文。");
            GenerateAllCodesFromDatabase(dbContext, ifExistCovered, selector);
        }
        public void GenerateAllCodesFromDatabase(IDbContextCore dbContext,bool ifExistCovered = false, Func<DbTable, bool> selector = null)
        {
            var tables = dbContext.GetCurrentDatabaseTableList();
            if (tables != null && tables.Any())
            {
                if (selector != null)
                    tables = tables.Where(selector).ToList();
                Generate(tables.ToList(), ifExistCovered);
            }
        }

        public void Generate(List<DbTable> tables,bool ifExistCovered = false)
        {
            foreach (var table in tables)
            {
                if (table.Columns.Any(c => c.IsPrimaryKey))
                {
                    var pkTypeName = table.Columns.First(m => m.IsPrimaryKey).CSharpType;
                    var tableName = table.TableName;
                    var className = table.Alias.IsNullOrEmpty() ? tableName : table.Alias;
                    GenerateEntity(table, ifExistCovered);
                    GenerateIRepository(className, pkTypeName, ifExistCovered);
                    GenerateRepository(className, pkTypeName, ifExistCovered);
                    GenerateIService(className, pkTypeName, ifExistCovered);
                    GenerateService(className, pkTypeName, ifExistCovered);
                    if (!Option.GenerateApiController)
                    {
                        GenerateController(className, pkTypeName, ifExistCovered);
                    }
                    else
                    {
                        GenerateApiController(className, pkTypeName, ifExistCovered);
                    }
                }
            }
        }

        public void GenerateEntity(DbTable table, bool ifExistCovered = false)
        {
            var modelPath = Option.OutputPath + Delimiter + "Models";
            if (!Directory.Exists(modelPath))
            {
                Directory.CreateDirectory(modelPath);
            }

            var tableName = table.TableName;
            var className = table.Alias.IsNullOrEmpty() ? tableName : table.Alias;

            var fullPath = modelPath + Delimiter + tableName + ".cs";
            if (File.Exists(fullPath) && !ifExistCovered)
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
            content = content.Replace("{ModelsNamespace}", Option.ModelsNamespace)
                .Replace("{Comment}", table.TableComment)
                .Replace("{TableName}", tableName)
                .Replace("{ModelName}", className)
                .Replace("{KeyTypeName}", pkTypeName)
                .Replace("{ModelProperties}", sb.ToString());
            WriteAndSave(fullPath, content);
        }

        private  string GenerateEntityProperty(DbTableColumn column)
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
                if (Option.IsPascalCase)
                {
                    sb.AppendLine($"\t\t[Column(\"{column.ColName}\")]");
                }
                if (!column.IsNullable)
                {
                    sb.AppendLine("\t\t[Required]");
                }

                var colType = column.CSharpType;
                if (colType.ToLower() == "string" && column.ColumnLength.HasValue && column.ColumnLength.Value>0)
                {
                    sb.AppendLine($"\t\t[MaxLength({column.ColumnLength.Value})]");
                }
                if (column.IsIdentity)
                {
                    sb.AppendLine("\t\t[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }

                if (colType.ToLower() != "string" && colType.ToLower() != "byte[]" && colType.ToLower() != "object" &&
                    column.IsNullable)
                {
                    colType = colType + "?";
                }

                var colName = column.ColName;
                if (!column.Alias.IsNullOrEmpty()) colName = column.Alias;
                if (Option.IsPascalCase) colName = colName.ToPascalCase();
                sb.AppendLine($"\t\tpublic {colType} {colName} " + "{get;set;}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public  void WriteAndSave(string fileName, string content)
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

        public void GenerateViewModel(DataTable dt, string className, bool ifExistCovered = true)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));
            var template = ReadTemplate("ViewModelTemplate.txt");

            var columnBuilder = new StringBuilder();
            foreach (DataColumn column in dt.Columns)
            {
                if (Option.IsPascalCase)
                {
                    columnBuilder.AppendLine($"[Column(\"{column.ColumnName}\")]");
                }
                columnBuilder.AppendLine(
                        $"public {column.DataType.Name} {(Option.IsPascalCase?column.ColumnName.ToPascalCase():column.ColumnName)}" +
                        "{ get; set; }");
                columnBuilder.AppendLine();
            }

            var content = template.Replace("{0}", Option.ViewModelsNamespace)
                .Replace("{1}", dt.TableName)
                .Replace("{2}", className)
                .Replace("{3}", columnBuilder.ToString());
            var modelPath = Option.OutputPath + Delimiter + "ViewModels";
            if (!Directory.Exists(modelPath))
                Directory.CreateDirectory(modelPath);
            var fullPath = modelPath + Delimiter + className + ".cs";
            if (File.Exists(fullPath) && !ifExistCovered)
                return;

            WriteAndSave(fullPath, content);
        }

        public void GenerateViewModel(DataSet ds, bool ifExistCovered = false)
        {
            if (ds == null) throw new ArgumentNullException(nameof(ds));
            foreach (DataTable table in ds.Tables)
            {
                GenerateViewModel(table, table.TableName, ifExistCovered);
            }
        }
    }
}

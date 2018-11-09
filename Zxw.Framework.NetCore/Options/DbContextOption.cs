using System.Collections.Generic;

namespace Zxw.Framework.NetCore.Options
{
    public class DbContextOption
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 实体程序集名称
        /// </summary>
        public string ModelAssemblyName { get; set; }
        /// <summary>
        /// 程序集映射字典（Key：interface程序集名称，Value:implement程序集名称。例如：Key-Zxw.Framework.Website.IRepositories,Value-Zxw.Framework.Website.Repositories）
        /// </summary>
        public Dictionary<string, string> AssemblyDictionary { get; set; }
        /// <summary>
        /// 是否开启触发器功能
        /// </summary>
        public bool OpenDbTrigger { get; set; } = false;
    }
}

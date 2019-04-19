namespace Zxw.Framework.NetCore.Options
{
    public class DbContextOption
    {
        public string TagName { get; set; }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 实体程序集名称
        /// </summary>
        public string ModelAssemblyName { get; set; }
    }
}

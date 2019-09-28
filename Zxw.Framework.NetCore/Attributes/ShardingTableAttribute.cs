using System.ComponentModel.DataAnnotations.Schema;

namespace Zxw.Framework.NetCore.Attributes
{
    public class ShardingTableAttribute:TableAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string Splitter { get; set; } = "_";
        /// <summary>
        /// 分表后缀格式。默认值：_yyyyMMdd
        /// </summary>
        public string Suffix { get; set; } = "yyyyMMdd";
        public ShardingTableAttribute(string name) : base(name)
        {
            this.Suffix = "yyyyMMdd";
            this.Splitter = "_";
        }

        public ShardingTableAttribute(string name, string splitter = "_", string suffix = "yyyyMMdd") : base(name)
        {
            this.Suffix = suffix;
        }
    }
}

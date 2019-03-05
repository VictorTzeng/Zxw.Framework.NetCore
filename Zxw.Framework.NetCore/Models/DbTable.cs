using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zxw.Framework.NetCore.Models
{
    [Serializable]
    public class DbTable
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 表说明
        /// </summary>
        public string TableComment { get; set; }
        /// <summary>
        /// 字段集合
        /// </summary>
        public virtual ICollection<DbTableColumn> Columns { get; set; } = new List<DbTableColumn>();
    }
    [Serializable]
    public class DbTableColumn
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string ColName { get; set; }
        /// <summary>
        /// 是否自增
        /// </summary>
        public bool IsIdentity { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 字段数据类型
        /// </summary>
        public string ColumnType { get; set; }
        /// <summary>
        /// 字段数据长度
        /// </summary>
        public long? ColumnLength { get; set; }
        /// <summary>
        /// 是否允许为空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 字段说明
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// C#数据类型
        /// </summary>
        public string CSharpType { get; set; }
    }
}

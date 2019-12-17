namespace Zxw.Framework.NetCore.Models
{
    public class ExcutedResult
    {
        public bool success { get; set; }

        public string msg { get; set; }

        public object rows { get; set; }

        public int? code { get; set; }

        public ExcutedResult(bool success, string msg, object rows, int? code = null)
        {
            this.success = success;
            this.msg = msg;
            this.rows = rows;
            this.code = code;
        }
        public static ExcutedResult SuccessResult(string msg = null)
        {
            return new ExcutedResult(true, msg, null);
        }
        public static ExcutedResult SuccessResult(object rows)
        {
            return new ExcutedResult(true, null, rows);
        }

        public static ExcutedResult FailedResult(string msg, int? code = null)
        {
            return new ExcutedResult(false, msg, null, code);
        }
    }

    public class PaginationResult : ExcutedResult
    {
        /// <summary>
        /// 总条数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 每页条数
        /// </summary>
        public int pageSize { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int pageIndex { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int pageCount => total % pageSize == 0 ? total / pageSize : total / pageSize + 1;

        public PaginationResult(bool success, string msg, object rows) : base(success, msg, rows)
        {
        }

        public static PaginationResult PagedResult(object rows, int total, int size, int index)
        {
            return new PaginationResult(true, null, rows)
            {
                total = total,
                pageSize = size,
                pageIndex = index
            };
        }
    }
}

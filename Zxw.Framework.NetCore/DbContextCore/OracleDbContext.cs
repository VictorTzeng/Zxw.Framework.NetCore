using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class OracleDbContext:BaseDbContext
    {
        public OracleDbContext(DbContextOption option) : base(option)
        {

        }
        public OracleDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseOracle(_option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        public override void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null)
        {
            base.BulkInsert<T, TKey>(entities, destinationTableName);
        }
    }
}

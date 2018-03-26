using System;
using System.Collections.Generic;
using System.Text;
using IBM.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.EfDbContext
{
    public class DB2DbContext:BaseDbContext
    {
        public DB2DbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseDb2(_option.ConnectionString, builder => { });
            base.OnConfiguring(optionsBuilder);
        }
    }
}

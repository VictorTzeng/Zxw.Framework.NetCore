using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class ShardDbContextFactory<T>:IDesignTimeDbContextFactory<T> where T:DbContext,new()
    {
        public T CreateDbContext(string[] args)
        {
            throw new NotImplementedException();
        }

        public T CreateDbContext(string connectionString)
        {
            var newContext =
                (T) Activator.CreateInstance(typeof(T),
                    new DbContextOption()
                    {
                        ConnectionString = connectionString, 
                        TagName = typeof(T).Name
                    });
            return newContext;
        }

        public T CreateDbContext(DbContextOption option) => (T) Activator.CreateInstance(typeof(T), option);
    }
}

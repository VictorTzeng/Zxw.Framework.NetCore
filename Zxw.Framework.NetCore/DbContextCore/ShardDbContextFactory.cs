using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public class ShardDbContextFactory<T>:IDesignTimeDbContextFactory<T> where T:DbContext,new()
    {
        public T CreateDbContext(string[] args)
        {
            return ServiceLocator.Resolve<T>();
        }        
    }
}

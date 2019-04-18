using System;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.DbContextCore;

namespace Zxw.Framework.UnitTest
{
    public class TestController
    {
        public IDbContextCore DbContext1 { get; set; }
        public IDbContextCore DbContext2 { get; set; }
        public IDbContextCore DbContext3 { get; set; }

        public TestController(DbContextFactory factory)
        {
            DbContext1 = factory.GetDbContext("db1");
            DbContext2 = factory.GetDbContext("db2");
            DbContext3 = factory.GetDbContext("db3");
        }

        public void Run()
        {
            var db = DbContext1.GetDatabase();
            Console.WriteLine();
        }
    }
}
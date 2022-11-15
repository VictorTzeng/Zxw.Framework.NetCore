using System;
using MongoDB.Bson;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.UnitTest.TestModels;

namespace Zxw.Framework.UnitTest
{
    public class TestRepository: BaseRepository<MongoModel, ObjectId>, IMongoRepository
    {
        [FromDbContextFactory("db1")]
        public IDbContextCore DbContext1 { get; set; }
        [FromDbContextFactory("db2")]
        public IDbContextCore DbContext2 { get; set; }

        public void Run()
        {
            Console.WriteLine("Over!");
        }

        public TestRepository(IDbContextCore dbContext, ISqlOperatorUtility sqlOperator) : base(dbContext, sqlOperator)
        {
        }
    }
}
using MongoDB.Bson;
using Zxw.Framework.NetCore.Attributes;
using Zxw.Framework.NetCore.Repositories;

namespace Zxw.Framework.UnitTest.TestModels
{
    [FromDbContextFactoryInterceptor]
    public interface IMongoRepository:IRepository<MongoModel, ObjectId>
    {
        void Run();
    }
}
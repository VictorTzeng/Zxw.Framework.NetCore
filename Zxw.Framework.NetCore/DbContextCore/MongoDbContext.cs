using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    [MongoDatabase("ZxwMongoDb")]
    public class MongoDbContext:BaseDbContext, IMongoDbContext
    {
        public MongoDbContext(DbContextOption option) : base(option)
        {

        }
        public MongoDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        public MongoDbContext(DbContextOptions options) : base(options){}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //var mongoUrl = new MongoUrl(Option.ConnectionString);
            //var settings = MongoClientSettings.FromUrl(mongoUrl);
            //var mongoClient = new MongoClient(settings);
            optionsBuilder.UseMongoDb(Option.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}

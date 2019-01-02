using System;
using System.Collections.Generic;
using System.Text;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.DbContextCore
{
    [MongoDatabase("ZxwMongoDb")]
    public class MongoDbContext:BaseDbContext
    {
        public MongoDbContext(IOptions<DbContextOption> option) : base(option)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var mongoUrl = new MongoUrl(_option.ConnectionString);
            var settings = MongoClientSettings.FromUrl(mongoUrl);
            var mongoClient = new MongoClient(settings);
            optionsBuilder.UseMongoDb(mongoClient);
        }
    }
}

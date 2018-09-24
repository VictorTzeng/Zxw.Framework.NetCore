using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using MongoDB.Bson;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.UnitTest.TestModels
{
    public class MongoModel:BaseModel<ObjectId>
    {
        [Key]
        public override ObjectId Id { get; set; }
        public string UserName { get; set; }
        public int Age { get; set; }

        public double Wage { get; set; }
        public bool IsBitch { get; set; }
        public DateTime Birthday { get; set; }
    }
}

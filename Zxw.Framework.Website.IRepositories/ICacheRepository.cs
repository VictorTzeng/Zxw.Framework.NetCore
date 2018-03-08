using System;
using Zxw.Framework.NetCore.Repositories;
using Zxw.Framework.Website.Models;

namespace Zxw.Framework.Website.IRepositories
{
    public interface ICacheRepository:IRepository<Cache, Int32>
    {
    }
}
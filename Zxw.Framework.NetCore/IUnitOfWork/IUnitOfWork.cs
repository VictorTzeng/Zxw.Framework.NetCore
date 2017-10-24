using System;
using System.Collections.Generic;
using System.Text;
using Zxw.Framework.NetCore.IRepositories;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.IUnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        IRepo Repository<IRepo>();

        int SaveChanges();
    }
}

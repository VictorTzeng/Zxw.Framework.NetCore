using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        IRepo GetRepository<IRepo>();
    }
}

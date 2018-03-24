using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Z.EntityFramework.Plus;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private IEfDbContext _context;
        public EfUnitOfWork(IEfDbContext context)
        {
            _context = context;
        }

        public IRepo GetRepository<IRepo>()
        {
            return AutofacContainer.Resolve<IRepo>(new TypedParameter(typeof(IEfDbContext), _context));
        }

        public int Commit()
        {
            return _context.SaveChanges(true);
        }

        public Task<int> CommitAsync()
        {
            return _context.SaveAsync(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

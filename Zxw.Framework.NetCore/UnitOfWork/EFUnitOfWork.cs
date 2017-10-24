using System;
using Autofac;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork.IUnitOfWork
    {
        private bool disposed = false;
        private DefaultDbContext _context;
        public EfUnitOfWork(DefaultDbContext context)
        {
            _context = context;
        }

        public IRepo Repository<IRepo>()
        {
            return IoCContainer.Resolve<IRepo>(new TypedParameter(typeof(DefaultDbContext), _context));
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
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

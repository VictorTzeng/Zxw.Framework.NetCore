using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Zxw.Framework.NetCore.EfDbContext;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private bool disposed = false;
        private DefaultDbContext _context;
        public EfUnitOfWork(DefaultDbContext context)
        {
            _context = context;
        }

        public void ChangeDatabase(string database)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State.HasFlag(ConnectionState.Open))
            {
                connection.ChangeDatabase(database);
            }
            else
            {
                var connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
                connection.ConnectionString = connectionString;
            }

            // Following code only working for mysql.
            var items = _context.Model.GetEntityTypes();
            foreach (var item in items)
            {
                if (item.Relational() is RelationalEntityTypeAnnotations extensions)
                {
                    extensions.Schema = database;
                }
            }
        }

        public IRepo GetRepository<IRepo>()
        {
            return AutofacContainer.Resolve<IRepo>(new TypedParameter(typeof(DefaultDbContext), _context));
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected void UseTransaction(IDbContextTransaction transaction)
        {
            _context.Database.UseTransaction(transaction.GetDbTransaction());
        }

        public async Task<int> SaveChangesAsync(params IUnitOfWork[] unitOfWorks)
        {
            // TransactionScope will be included in .NET Core v2.0
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var count = 0;
                    foreach (var unitOfWork in unitOfWorks)
                    {
                        var uow = unitOfWork as EfUnitOfWork;
                        uow.UseTransaction(transaction);
                        count += await uow.SaveChangesAsync();
                    }

                    count += await SaveChangesAsync();

                    transaction.Commit();

                    return count;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();

                    throw ex;
                }
            }
        }

        /// <summary>
        /// ExecuteSqlWithNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return _context.ExecuteSqlWithNonQuery(sql, parameters);
        }


        public IQueryable<TView> SqlQuery<T, TView>(string sql, params object[] parameters)
            where T : class
            where TView : class
        {
            return _context.SqlQuery<T, TView>(sql, parameters).AsQueryable();
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

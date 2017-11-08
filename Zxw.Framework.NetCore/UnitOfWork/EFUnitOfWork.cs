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

        /// <summary>
        /// update query datas by columns.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="updateExp"></param>
        /// <returns></returns>
        public int BatchUpdate<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp)
            where T : class
        {
            return _context.Set<T>().Where(@where).Update(updateExp);
        }

        public int SaveChanges()
        {
            using (var tran = _context.Database.CurrentTransaction ?? _context.Database.BeginTransaction())
            {
                try
                {
                    var result = _context.SaveChanges();
                    tran.Commit();
                    return result;
                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            using (var tran = _context.Database.CurrentTransaction ?? await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await _context.SaveChangesAsync();
                    tran.Commit();
                    return result;

                }
                catch (Exception)
                {
                    tran.Rollback();
                    throw;
                }
            }
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
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
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

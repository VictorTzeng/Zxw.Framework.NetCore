using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        int BatchUpdate<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateExp) where T : class;
        void ChangeDatabase(string database);
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        IRepo GetRepository<IRepo>();
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(params IUnitOfWork[] unitOfWorks);
        IQueryable<TView> SqlQuery<T, TView>(string sql, params object[] parameters)
            where T : class
            where TView : class;
    }
}

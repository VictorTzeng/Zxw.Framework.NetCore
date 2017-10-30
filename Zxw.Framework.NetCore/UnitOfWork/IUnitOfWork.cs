using System;
using System.Linq;
using System.Threading.Tasks;

namespace Zxw.Framework.NetCore.UnitOfWork
{
    public interface IUnitOfWork:IDisposable
    {
        void ChangeDatabase(string database);

        IRepo GetRepository<IRepo>();

        int SaveChanges();

        Task<int> SaveChangesAsync();

        Task<int> SaveChangesAsync(params IUnitOfWork[] unitOfWorks);

        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);

        IQueryable<TView> SqlQuery<T, TView>(string sql, params object[] parameters)
            where T : class
            where TView : class;
    }
}

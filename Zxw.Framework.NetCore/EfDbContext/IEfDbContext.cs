using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.EfDbContext
{
    public interface IEfDbContext:IDisposable
    {
        Task<int> AddAsync<T>(T entity) where T : class;
        Task<int> AddRangeAsync<T>(ICollection<T> entities) where T : class;
        DatabaseFacade GetDatabase();
        DbSet<T> GetDbSet<T>() where T : class;
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters);
        int Edit<T>(T entity) where T : class;
        int EditRange<T>(ICollection<T> entities) where T : class;
        int Update<T>(T model, params string[] updateColumns) where T : class;
        int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory) where T : class;
        Task<int> UpdateAsync<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
            where T : class;
        int Delete<T>(Expression<Func<T, bool>> @where) where T : class;
        Task<int> DeleteAsync<T>(Expression<Func<T, bool>> @where) where T : class;
        void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null)
            where T : class, IBaseModel<TKey>;
        List<TView> SqlQuery<T, TView>(string sql, params object[] parameters) 
            where T : class
            where TView : class;
        Task<List<TView>> SqlQueryAsync<T, TView>(string sql, params object[] parameters)
            where T : class
            where TView : class;

        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);

        Task<int> SaveAsync();
        Task<int> SaveAsync(bool acceptAllChangesOnSuccess);
    }
}
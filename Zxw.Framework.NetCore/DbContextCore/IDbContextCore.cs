using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.DbContextCore
{
    public interface IDbContextCore:IDisposable
    {
        DatabaseFacade GetDatabase();
        int Add<T>(T entity) where T : class;
        Task<int> AddAsync<T>(T entity) where T : class;
        int AddRange<T>(ICollection<T> entities) where T : class;
        Task<int> AddRangeAsync<T>(ICollection<T> entities) where T : class;
        int Count<T>(Expression<Func<T, bool>> @where = null) where T : class;
        Task<int> CountAsync<T>(Expression<Func<T, bool>> @where = null) where T : class;
        int Delete<T,TKey>(TKey key) where T : class;
        bool EnsureCreated();
        Task<bool> EnsureCreatedAsync();
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters);
        int Edit<T, TKey>(T entity) where T : BaseModel<TKey>;
        int EditRange<T>(ICollection<T> entities) where T : class;
        bool Exist<T>(Expression<Func<T, bool>> @where = null) where T : class;
        IQueryable<T> FilterWithInclude<T>(Func<IQueryable<T>, IQueryable<T>> include, Expression<Func<T, bool>> where) where T : class;
        Task<bool> ExistAsync<T>(Expression<Func<T, bool>> @where = null) where T : class;
        T Find<T,TKey>(TKey key) where T : class;
        Task<T> FindAsync<T,TKey>(TKey key) where T : class;
        IQueryable<T> Get<T>(Expression<Func<T, bool>> @where = null, bool asNoTracking = false) where T : class;
        DbSet<T> GetDbSet<T>() where T : class;
        T GetSingleOrDefault<T>(Expression<Func<T, bool>> @where = null) where T : class;
        Task<T> GetSingleOrDefaultAsync<T>(Expression<Func<T, bool>> @where = null) where T : class;
        int Update<T>(T model, params string[] updateColumns) where T : class;
        int Update<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory) where T : class;
        Task<int> UpdateAsync<T>(Expression<Func<T, bool>> @where, Expression<Func<T, T>> updateFactory)
            where T : class;
        int Delete<T>(Expression<Func<T, bool>> @where) where T : class;
        Task<int> DeleteAsync<T>(Expression<Func<T, bool>> @where) where T : class;
        void BulkInsert<T, TKey>(IList<T> entities, string destinationTableName = null)
            where T : BaseModel<TKey>;
        List<TView> SqlQuery<T, TView>(string sql, params object[] parameters) 
            where T : class;
        PaginationResult SqlQueryByPagnation<T, TView>(string sql, string[] orderBys, int pageIndex, int pageSize, Action<TView> eachAction = null)
            where T : class
            where TView : class;
        Task<List<TView>> SqlQueryAsync<T, TView>(string sql, params object[] parameters)
            where T : class
            where TView : class;
        int SaveChanges();
        int SaveChanges(bool acceptAllChangesOnSuccess);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,CancellationToken cancellationToken = default(CancellationToken));
    }
}
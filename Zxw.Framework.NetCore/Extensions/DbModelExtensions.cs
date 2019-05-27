using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Models;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class DbModelExtensions
    {
        public static int Add<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            using (var db = AspectCoreContainer.Resolve<IDbContextCore>())
            {
                return db.Add(model);
            }
        }

        public static async Task<int> AddAsync<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            return await Task.Run(model.Add<T, TKey>);
        }
        public static int AddRange<T, TKey>(this ICollection<T> models) where T : BaseModel<TKey>
        {
            using (var db = AspectCoreContainer.Resolve<IDbContextCore>())
            {
                return db.AddRange(models);
            }
        }

        public static async Task<int> AddRangeAsync<T, TKey>(this ICollection<T> models) where T : BaseModel<TKey>
        {
            return await Task.Run(models.AddRange<T, TKey>);
        }

        public static int Edit<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            using (var db = AspectCoreContainer.Resolve<IDbContextCore>())
            {
                return db.Edit<T,TKey>(model);
            }
        }

        public static async Task<int> EditAsync<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            return await Task.Run(model.Edit<T, TKey>);
        }

        public static int Update<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            using (var db = AspectCoreContainer.Resolve<IDbContextCore>())
            {
                return db.Update<T,TKey>(model);
            }
        }

        public static async Task<int> UpdateAsync<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            return await Task.Run(model.Update<T, TKey>);
        }

        public static int Delete<T, TKey>(this T model) where T : BaseModel<TKey>
        {
            using (var db = AspectCoreContainer.Resolve<IDbContextCore>())
            {
                return db.Delete<T, TKey>(model.Id);
            }
        }
    }
}

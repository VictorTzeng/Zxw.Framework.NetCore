using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Cache
{
    public class DistributedCacheManager
    {
        private static IDistributedCache Instance => AspectCoreContainer.Resolve<IDistributedCache>();

        public static object Get(string key) => Instance.Get(key).ToObject();

        public static async Task<object> GetAsync(string key)
        {
            var content = await Instance.GetAsync(key);
            return content.ToObject();
        }
        public static T Get<T>(string key) => (T) Get(key);

        public static async Task<T> GetAsync<T>(string key) => (T) await GetAsync(key);

        public static void Set(string key, object data, int expiredSeconds) => Instance.Set(key, data.ToBytes(),
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiredSeconds)
            });

        public static async Task SetAsync(string key, object data, int expiredSeconds) => await Instance.SetAsync(
            key,
            data.ToBytes(),
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiredSeconds)
            });

        public static void Remove(string key) => Instance.Remove(key);

        public static async Task RemoveAsync(string key) => await Instance.RemoveAsync(key);

        public static void Refresh(string key) => Instance.Refresh(key);

        public static async Task RefreshAsync(string key) => await Instance.RefreshAsync(key);

        public static void Clear()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Zxw.Framework.NetCore.Extensions;

namespace Zxw.Framework.NetCore.Cache
{
    public class DistributedCacheManager:IDistributedCacheManager
    {
        private IDistributedCache _cache;
        public DistributedCacheManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        public void Set(string key, object value)
        {
            if (value == null) return;
            _cache.Set(key, value.ToBytes());
        }

        public async Task SetAsync(string key, object value)
        {
            if (value == null) return;
            await _cache.SetAsync(key, value.ToBytes());
        }
        public void Set(string key, object value, int expiredSeconds)
        {
            if (value == null) return;
            _cache.Set(key, value.ToBytes(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiredSeconds)
            });
        }

        public async Task SetAsync(string key, object value, int expiredSeconds)
        {
            if (value == null) return;
            await _cache.SetAsync(key, value.ToBytes(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(expiredSeconds)
            });
        }

        public object Get(string key)
        {
            var data = _cache.Get(key);
            return data?.ToObject();
        }

        public async Task<object> GetAsync(string key)
        {
            var data = await _cache.GetAsync(key);
            return data?.ToObject();
        }

        public object Get(string key, Type type)
        {
            return Convert.ChangeType(Get(key), type);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            return Convert.ChangeType(await GetAsync(key), type);
        }

        public T Get<T>(string key)
        {
            var data = _cache.Get(key);
            return (T) data?.ToObject();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var data = await _cache.GetAsync(key);
            return (T) data?.ToObject();
        }

        public object Get(string key, Func<object> func)
        {
            var result = Get(key);
            if (result == null)
            {
                result = func.Invoke();
                Set(key, result);
            }

            return result;
        }
        public async Task<object> GetAsync(string key, Func<object> func)
        {
            var result = await GetAsync(key);
            if (result == null)
            {
                result = func.Invoke();
                await SetAsync(key, result);
            }

            return result;
        }
        public T Get<T>(string key, Func<T> func)
        {
            var result = Get<T>(key);
            if (result == null)
            {
                result = func.Invoke();
                Set(key, result);
            }

            return result;
        }
        public async Task<T> GetAsync<T>(string key, Func<T> func)
        {
            var result = await GetAsync<T>(key);
            if (result == null)
            {
                result = func.Invoke();
                await SetAsync(key, result);
            }

            return result;
        }

        public object Get(string key, Func<object> func, int expiredSeconds)
        {
            var result = Get(key);
            if (result == null)
            {
                result = func.Invoke();
                Set(key, result, expiredSeconds);
            }

            return result;
        }
        public async Task<object> GetAsync(string key, Func<object> func, int expiredSeconds)
        {
            var result = await GetAsync(key);
            if (result == null)
            {
                result = func.Invoke();
                await SetAsync(key, result, expiredSeconds);
            }

            return result;
        }
        public T Get<T>(string key, Func<T> func, int expiredSeconds)
        {
            var result = Get<T>(key);
            if (result == null)
            {
                result = func.Invoke();
                Set(key, result, expiredSeconds);
            }

            return result;
        }
        public async Task<T> GetAsync<T>(string key, Func<T> func, int expiredSeconds)
        {
            var result = await GetAsync<T>(key);
            if (result == null)
            {
                if (func.Method.IsDefined(typeof(AsyncStateMachineAttribute), false))
                {
                    dynamic invoked = func.Invoke();
                    if (invoked is Task<T> task)
                    {
                        result = (T)task.Result;
                    }
                }
                else
                {
                    result = func.Invoke();
                }
                await SetAsync(key, result, expiredSeconds);
            }

            return result;
        }

    }
}

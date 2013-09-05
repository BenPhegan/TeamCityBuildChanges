using System;
using ServiceStack.CacheAccess;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    static public class ICacheClientExtensions
    {
        public static T GetFromCacheOrFunc<T>(this ICacheClient cache, string key, Func<string, T> func) where T : class
        {
            var typedKey = String.Format("{0}:{1}", typeof (T).FullName, key);
            var cacheHit = cache.Get<T>(typedKey);
            if (cacheHit != null)
            {
                return cacheHit;
            }
            var result = func(key);
            //TODO concurrency issue here?
            cache.Add(typedKey, result);
            return result;
        }

        public static T GetFromCacheOrFunc<T>(this ICacheClient cache, string key1, string key2, Func<string, string, T> func) where T : class
        {
            var typedKey = String.Format("{0}:{1}:{2}", typeof(T).FullName, key1 ?? String.Empty, key2 ?? String.Empty);

            var cacheHit = cache.Get<T>(typedKey);
            if (cacheHit != null)
            {
                return cacheHit;
            }
            var result = func(key1,key2);
            //TODO concurrency issue here?
            cache.Add(typedKey, result);
            return result;
        }

        public static T GetFromCacheOrFunc<T>(this ICacheClient cache, Func<string> keyGenerator, Func<T> fallbackFunction) where T : class
        {
            var typedKey = keyGenerator();

            var cacheHit = cache.Get<T>(typedKey);
            if (cacheHit != null)
            {
                return cacheHit;
            }
            var result = fallbackFunction();
            //TODO concurrency issue here?
            cache.Add(typedKey, result);
            return result;
        }
    }
}
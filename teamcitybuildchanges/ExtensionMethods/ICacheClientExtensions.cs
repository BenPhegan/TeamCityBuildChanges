using System;
using ServiceStack.CacheAccess;

namespace TeamCityBuildChanges.ExtensionMethods
{
    static public class ICacheClientExtensions
    {
        public static T GetFromCacheOrFunc<T>(this ICacheClient cache, string key, Func<string, T> func) where T : class
        {
            return cache.GetFromCacheOrFunc(
                () => String.Format("{0}:{1}", typeof (T).FullName, key),
                () => func(key));
        }

        public static T GetFromCacheOrFunc<T>(this ICacheClient cache, string key1, string key2, Func<string, string, T> func) where T : class
        {
            return cache.GetFromCacheOrFunc(
                () => String.Format("{0}:{1}:{2}", typeof (T).FullName, key1 ?? String.Empty, key2 ?? String.Empty),
                () => func(key1, key2));
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
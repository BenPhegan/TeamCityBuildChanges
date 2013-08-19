using System.Runtime.Caching;

namespace TeamCityBuildChanges.ExternalApi.TeamCity
{
    public class MemoryBasedBuildCache : ICache
    {
        public T GetItem<T>(string key) where T : class
        {
            return CacheHelper<T>.Cache[key] as T;
        }

        public bool TryGetItem<T>(string key, out T value) where T : class
        {
            var val = CacheHelper<T>.Cache.Get(key);

            if (val != null)
            {
                value = (T)val;
                return true;
            }
            value = null;
            return false;
        }

        public bool SetItem<T>(string key, T value) where T : class
        {
            return CacheHelper<T>.Cache.Add(key, value, new CacheItemPolicy());
        }

        private static class CacheHelper<T> where T : class
        {
// ReSharper disable StaticFieldInGenericType
            internal static readonly MemoryCache Cache = new MemoryCache(typeof(T).Name);
// ReSharper restore StaticFieldInGenericType
        }
    }


}

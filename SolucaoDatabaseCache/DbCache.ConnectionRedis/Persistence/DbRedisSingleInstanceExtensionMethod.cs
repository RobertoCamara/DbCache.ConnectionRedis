using Newtonsoft.Json;
using StackExchange.Redis;

namespace DbCache.ConnectionRedis.Persistence
{
    public static class DbRedisSingleInstanceExtensionMethod
    {
        /// <summary>
        /// Extension Method. Save or update object - Using JsonConvert to serialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <param name="value">Generic object value</param>
        public static void SaveOrUpdate<T>(this IDatabase db, string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            db.StringSet(key, json);
        }

        /// <summary>
        /// Extension Method. Delete key
        /// </summary>
        /// <param name="key"></param>
        public static void Delete(this IDatabase db, string key)
        {
            if (db.KeyExists(key))
                db.KeyDelete(key);
        }

        /// <summary>
        /// Extension Method. Get by key - Using JsonConvert to deserialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <returns>Object T</returns>
        public static T GetByDeserializeObject<T>(this IDatabase db, string key)
        {
            if (db.KeyExists(key))
                return JsonConvert.DeserializeObject<T>(db.StringGet(key));

            return default(T);
        }
    }
}

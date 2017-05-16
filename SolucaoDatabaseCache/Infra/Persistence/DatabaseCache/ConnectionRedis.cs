using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infra.Persistence.DatabaseCache
{
    /// <summary>
    /// Manage the connection to the redis database
    /// </summary>
    public class ConnectionRedis
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _idDb;
        
        private static Lazy<ConnectionMultiplexer> _redis;
        private readonly ConfigurationOptions _configOptions;
        private static IDatabase _database { get; set; }

        public ConnectionRedis(string host, int port, bool allowAdmin = true, int idDb = 0)
        {
            _host = host;
            _port = port;
            _idDb = idDb;

            _configOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                AllowAdmin = allowAdmin
            };
            
        }

        /// <summary>
        /// Connect lazy
        /// </summary>
        private void Connect()
        {
            if (_redis == null)
            {
                _redis = new Lazy<ConnectionMultiplexer>(() =>
                {
                    return ConnectionMultiplexer.Connect(configuration: _configOptions);
                });

                _database = _redis.Value.GetDatabase(db: _idDb);
            }
        }

        /// <summary>
        /// Get server connected
        /// </summary>
        /// <returns></returns>
        private IServer GetServer()
        {
            Connect();
            return _redis.Value.GetServer(hostAndPort: string.Concat(_host, ":", _port.ToString()));
        }

        /// <summary>
        /// Get all object RedisKey
        /// </summary>
        /// <returns>IEnumerable<RedisKey></returns>
        private IEnumerable<RedisKey> GetAllRedisKey()
        {
            Connect();
            return GetServer().Keys(database: _idDb, pattern: default(RedisValue), pageSize: 10, flags: CommandFlags.None);
        }

        /// <summary>
        /// Save or update object
        /// </summary>
        /// <param name="key">String key</param>
        /// <param name="value">Object value</param>
        public void SaveOrUpdate(string key, object value)
        {
            Connect();
            _database.SetAdd(key, (RedisValue)value);
        }

        /// <summary>
        /// Save or update object - Using JsonConvert to serialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <param name="value">Generic object value</param>
        /// <param name="isSerialize">Flag serialize object</param>
        public void SaveOrUpdate<T>(string key, T value, bool isSerialize = true)
        {
            Connect();
            string json = isSerialize ? JsonConvert.SerializeObject(value) : Convert.ToString(value);
            _database.StringSet(key, json);
        }

        /// <summary>
        /// Get by key - Using JsonConvert to deserialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <returns>Object T</returns>
        public T GetBy<T>(string key)
        {
            Connect();
            if (_database.KeyExists(key))
                return JsonConvert.DeserializeObject<T>(_database.StringGet(key));

            return default(T);
        }

        /// <summary>
        /// Get by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetBy(string key)
        {
            Connect();
            return _database.StringGet(key);
        }

        /// <summary>
        /// Delete key
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            Connect();
            if (_database.KeyExists(key))
                _database.KeyDelete(key);
        }

        /// <summary>
        /// Get all database keys
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllDatabaseKeys()
        {
            Connect();
            return GetAllRedisKey().Select(s => s.ToString());
        }

        /// <summary>
        /// Delete all the keys of the database.
        /// </summary>
        public void DeleteAllDatabaseKeys()
        {
            GetServer().FlushDatabase(database: _idDb);
        }

        /// <summary>
        /// Delete all the keys of all databases on the server.
        /// </summary>
        public void DeleteAllKeysFromAllDatabases()
        {
            GetServer().FlushAllDatabases();
        }
    }
}

using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DbCache.ConnectionRedis.Standard.Persistence
{
    /// <summary>
    /// Manage the connection to the redis database
    /// </summary>
    public class DbRedis
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _idDb;

        private Lazy<ConnectionMultiplexer> _redis;
        private readonly ConfigurationOptions _configOptions;
        public IDatabase DatabaseContext { get; private set; }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="host">Host to create endpoint</param>
        /// <param name="port">Port to create endpoint</param>
        /// <param name="allowAdmin">Indicates whether admin operations should be allowed</param>
        /// <param name="idDb">Database identifier</param>
        public DbRedis(string host, int port, bool allowAdmin = true, int idDb = 0)
        {
            _host = host;
            _port = port;
            _idDb = idDb;

            _configOptions = new ConfigurationOptions
            {
                EndPoints = { { host, port } },
                AllowAdmin = allowAdmin
            };

            _redis = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(configuration: _configOptions);
            });

            DatabaseContext = _redis.Value.GetDatabase(db: _idDb);
        }

        /// <summary>
        /// Get server connected
        /// </summary>
        /// <returns></returns>
        private IServer GetServer()
        {
            return _redis.Value.GetServer(hostAndPort: string.Concat(_host, ":", _port.ToString()));
        }

        /// <summary>
        /// Get all object RedisKey
        /// </summary>
        /// <returns>IEnumerable<RedisKey></returns>
        private IEnumerable<RedisKey> GetAllRedisKey()
        {
            return GetServer().Keys(database: _idDb, pattern: default(RedisValue), pageSize: 10, flags: CommandFlags.None);
        }

        /// <summary>
        /// Save or update object - Using JsonConvert to serialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <param name="value">Generic object value</param>
        public void SaveOrUpdate<T>(string key, T value)
        {
            string json = JsonConvert.SerializeObject(value);
            DatabaseContext.StringSet(key, json);
        }

        /// <summary>
        /// Get by key - Using JsonConvert to deserialize object
        /// </summary>
        /// <typeparam name="T">Generic object</typeparam>
        /// <param name="key">String key</param>
        /// <returns>Object T</returns>
        public T GetByDeserializeObject<T>(string key)
        {
            if (DatabaseContext.KeyExists(key))
                return JsonConvert.DeserializeObject<T>(DatabaseContext.StringGet(key));

            return default(T);
        }

        /// <summary>
        /// Delete key
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            if (DatabaseContext.KeyExists(key))
                DatabaseContext.KeyDelete(key);
        }

        /// <summary>
        /// Get all database keys
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllDatabaseKeys()
        {
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

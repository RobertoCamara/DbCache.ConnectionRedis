using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DbCache.ConnectionRedis.Persistence
{
    public class DbRedisSingleInstance
    {
        private static string _redisHost;
        private static int _redisPort;
        private static int _redisIdDatabase;

        private static readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        
        private static void ValidateSettings()
        {
            List<string> erros = new List<string>();
            SetRedisHost(erros);
            SetRedisPort(erros);
            SetRedisIdDatabase(erros);

            if (erros.Count > 0)
            {
                string msg = string.Join("\r\n", erros.ToArray());
                throw new Exception(msg);
            }
        }

        private static void SetRedisHost(List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["REDIS_HOST"]))
                erros.Add("A variável [REDIS_HOST] não foi definida no arquivo de configuração.");
            
            _redisHost = ConfigurationManager.AppSettings["REDIS_HOST"];
        }

        private static void SetRedisPort(List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["REDIS_PORT"]))
                erros.Add("A variável [REDIS_PORT] não foi definida no arquivo de configuração.");

            _redisPort = Convert.ToInt16(ConfigurationManager.AppSettings["REDIS_PORT"]);
        }

        private static void SetRedisIdDatabase(List<string> erros)
        {
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["REDIS_ID_DATABASE"]))
                erros.Add("A variável [REDIS_ID_DATABASE] não foi definida no arquivo de configuração.");

            _redisIdDatabase = Convert.ToInt16(ConfigurationManager.AppSettings["REDIS_ID_DATABASE"]);
        }

        static DbRedisSingleInstance()
        {
            ValidateSettings();

            var configOptions = new ConfigurationOptions
            {
                EndPoints = { { _redisHost, _redisPort } }
            };

            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(configuration: configOptions);
            });
        }

        /// <summary>
        /// Get server connected
        /// </summary>
        /// <returns></returns>
        private static IServer GetServer()
        {
            return _lazyConnection.Value.GetServer(hostAndPort: string.Concat(_redisHost, ":", _redisPort.ToString()));
        }

        /// <summary>
        /// Get all object RedisKey
        /// </summary>
        /// <returns>IEnumerable<RedisKey></returns>
        private static IEnumerable<RedisKey> GetAllRedisKey()
        {
            return GetServer().Keys(database: _redisIdDatabase, pattern: default(RedisValue), pageSize: 10, flags: CommandFlags.None);
        }

        /// <summary>
        /// 
        /// </summary>
        public static IDatabase DatabaseContext => _lazyConnection.Value.GetDatabase(db: _redisIdDatabase);

        /// <summary>
        /// Get all database keys
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllDatabaseKeys()
        {
            return GetAllRedisKey().Select(s => s.ToString());
        }

        /// <summary>
        /// Delete all the keys of the database.
        /// </summary>
        public static void DeleteAllDatabaseKeys()
        {
            GetServer().FlushDatabase(database: _redisIdDatabase);
        }

        /// <summary>
        /// Delete all the keys of all databases on the server.
        /// </summary>
        public static void DeleteAllKeysFromAllDatabases()
        {
            GetServer().FlushAllDatabases();
        }
    }
}

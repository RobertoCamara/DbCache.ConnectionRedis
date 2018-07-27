using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DbCache.ConnectionRedis.Standard.Persistence
{
    public class DbRedisConfig
    {
        public string RedisHost { get; set; }
        public int? RedisPort { get; set; }
        public string RedisPassword { get; set; }
        public bool AllowAdmin { get; set; }
        public int? RedisIdDatabaseDefault { get; set; }
    }

    public sealed class DbRedisMultiServer
    {
        private static Lazy<DbRedisMultiServer> _instance = null;
        private static object syncLock = new object();
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection = null;

        private DbRedisConfig _dbRedisConfig = null;

        private DbRedisMultiServer(DbRedisConfig redisDto)
        {
            var configOptions = new ConfigurationOptions
            {
                EndPoints = { { redisDto.RedisHost, redisDto.RedisPort.Value } },
                Password = redisDto.RedisPassword,
                AbortOnConnectFail = false,
                AllowAdmin = redisDto.AllowAdmin
            };

            _dbRedisConfig = redisDto;

            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(configuration: configOptions);
            });
        }

        #region Private Methods

        private static DbRedisMultiServer CreateInstance(DbRedisConfig redisDto)
        {
            if (_instance == null || !_instance.Value._dbRedisConfig.Equals(redisDto))
            {
                lock (syncLock)
                {
                    if (_instance == null || !_instance.Value._dbRedisConfig.Equals(redisDto))
                    {
                        _instance = new Lazy<DbRedisMultiServer>(() =>
                        {
                            return new DbRedisMultiServer(redisDto);
                        });
                    }
                }
            }


            return _instance.Value;
        }

        private static void ValidateParameters(DbRedisConfig redisDto)
        {
            List<string> erros = new List<string>();

            foreach (var item in redisDto.GetType().GetProperties())
            {
                if (item.Name == "RedisIdDatabaseDefault") continue;

                var value = item.GetValue(redisDto);

                if (value == null)
                    erros.Add($"\r\nO parâmetro [{item.Name}] é obrigatório.");
            }

            if (erros.Count > 0)
            {
                string msg = string.Join("\r\n", erros.ToArray());
                throw new Exception(msg);
            }
        }

        private int SetRedisIdDatabase(int? redisIdDatabase)
        {
            int? id = redisIdDatabase != null ? redisIdDatabase.Value : _dbRedisConfig.RedisIdDatabaseDefault;

            if (id == null)
                throw new Exception("Informe um valor para RedisIdDatabase.");

            return id.Value;
        }

        /// <summary>
        /// Get server connected
        /// </summary>
        /// <returns></returns>
        private IServer GetServer()
        {
            return this._lazyConnection.Value.GetServer(hostAndPort: string.Concat(this._dbRedisConfig.RedisHost, ":", this._dbRedisConfig.RedisPort.ToString()));
        }

        /// <summary>
        /// Get all object RedisKey
        /// </summary>
        /// <returns>IEnumerable<RedisKey></returns>
        private IEnumerable<RedisKey> GetAllRedisKey(int redisIdDatabase)
        {
            return GetServer().Keys(database: redisIdDatabase, pattern: default(RedisValue), pageSize: 10, flags: CommandFlags.None);
        }

        #endregion

        #region Public Methods

        public static DbRedisMultiServer GetInstance(DbRedisConfig redisDto)
        {
            ValidateParameters(redisDto);

            return CreateInstance(redisDto);
        }

        public IDatabase DatabaseContext(int? redisIdDatabase = null)
        {
            try
            {
                if (!RedisIsAvailable)
                    throw new Exception("Serviço indisponível.");

                return this._lazyConnection.Value.GetDatabase(db: SetRedisIdDatabase(redisIdDatabase));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// Check if the redis service is available
        /// </summary>
        public bool RedisIsAvailable => this._lazyConnection.Value.IsConnected;

        /// <summary>
        /// Get all database keys
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllDatabaseKeys(int redisIdDatabase = -1)
        {
            return GetAllRedisKey(SetRedisIdDatabase(redisIdDatabase)).Select(s => s.ToString());
        }

        /// <summary>
        /// Delete all the keys of the database.
        /// </summary>
        public void DeleteAllDatabaseKeys(int redisIdDatabase = -1)
        {
            GetServer().FlushDatabase(database: SetRedisIdDatabase(redisIdDatabase));
        }

        /// <summary>
        /// Delete all the keys of all databases on the server.
        /// </summary>
        public void DeleteAllKeysFromAllDatabases()
        {
            GetServer().FlushAllDatabases();
        }

        #endregion
    }
}

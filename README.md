# ConnectionRedis
- Exemplo de projeto utilizando o database in-memory Redis 
- Encapsula a conexão e estende métodos do Redis.
  
### Instalação
```
Install-Package DbCache.ConnectionRedis

Install-Package DbCache.ConnectionRedis.Standard
```
  
### Exemplo 
- DbRedis
```csharp
    class Cliente
    {
        public string Cnpj { get; set; }
        public Telefone Telefone { get; set; }
    }

    class Telefone
    {
        public string Numero { get; set; }
    }

    class Program
    {
        private static DbCache.ConnectionRedis.Persistence.DbRedis _redis = new DbCache.ConnectionRedis.Persistence.DbRedis("localhost", 6379, idDb: 1);

        static void Main(string[] args)
        {
            Cliente cliente = new Cliente { Cnpj = "1234567989", Telefone = new Telefone { Numero = "2199998878" } };
            //Salvando ou atualizando um registro
            //1ª opção
            _redis.SaveOrUpdate("key", cliente);
            //2ª opção
            _redis.SaveOrUpdate<Cliente>("key", new Cliente { Cnpj = "1234567989" });

            //Consultando registros
            //Objetos complexos. 
            Cliente result = _redis.GetByDeserializeObject<Cliente>("1234567989");

            //Para acessar demais métodos do Redis, utilize a propriedade DatabaseContext
            string ex1 = _redis.DatabaseContext.StringGet("teste");
            long ex2 = _redis.DatabaseContext.ListLength("key");
        }
    }
```

### To use single instance:
- DbRedisSingleInstance

### Configuration
- Adicione as chaves no arquivo de configuração da sua aplicação
```xml
<appSettings>
    <add key ="REDIS_HOST" value="localhost"/>
    <add key="REDIS_PORT" value="6379"/>
    <add key="REDIS_ID_DATABASE" value="1"/>
   <add key="REDIS_PASSWORD" value="SuaSenha"/>
  </appSettings>
```

### Exemplo 
- DbRedisSingleInstance
```csharp
class Cliente
    {
        public string Cnpj { get; set; }
        public Telefone Telefone { get; set; }
    }

    class Telefone
    {
        public string Numero { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Cliente cliente = new Cliente { Cnpj = "1234567989", Telefone = new Telefone { Numero = "2199998878" } };
                //Salvando ou atualizando um objeto complexo
                //1ª opção
                DbRedisSingleInstance.DatabaseContext.SaveOrUpdate("1234567989", cliente);
                //2ª opção
                DbRedisSingleInstance.DatabaseContext.SaveOrUpdate<Cliente>("010101010", new Cliente { Cnpj = "010101010", Telefone = new Telefone { Numero = "2122212123" } });

                //Consultando Objetos complexos
                Cliente result = DbRedisSingleInstance.DatabaseContext.GetByDeserializeObject<Cliente>("1234567989");
                Console.WriteLine($"***Consultando Objetos complexos*** \r\n Cnpj: {result.Cnpj} - Telefone: {result.Telefone.Numero}");
                
                //Utilizando demais métodos do Redis
                DbRedisSingleInstance.DatabaseContext.StringSet("dataHora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ffff"));
                string ex2 = DbRedisSingleInstance.DatabaseContext.StringGet("dataHora");

                Console.WriteLine($"\r\n***Consultando Objetos simples*** \r\n dataHora: {ex2}");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                Console.ReadKey();
            }

        }
    }
```
### Exemplo 
- DbRedisMultiServer
```csharp

class Cliente
    {
        public string Cnpj { get; set; }
        public Telefone Telefone { get; set; }
    }

    class Telefone
    {
        public string Numero { get; set; }
    }


    class Program
    {
        static DbRedisConfig dtoServer1 = new DbRedisConfig
        {
            RedisHost = "Ip_Host1",
            RedisPort = 6379,
            RedisPassword = "SenhaRedis",
            AllowAdmin = true,
            RedisIdDatabaseDefault = 4
        };

        static DbRedisConfig dtoServer2 = new DbRedisConfig
        {
            RedisHost = "Ip_Host2",
            RedisPort = 6379,
            RedisPassword = "SenhaRedis",
            AllowAdmin = true
        };
               

        private static DbRedisMultiServer _redisServer1 = DbRedisMultiServer.GetInstance(dtoServer1);
        private static DbRedisMultiServer _redisServer2 = DbRedisMultiServer.GetInstance(dtoServer2);

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("1 - Gerar Cache(Server 1) \r\n2 - Gerar Cache Other Database(Server 1) \r\n3 - Gerar Cache(Server 2) \r\n4 - Obter Todas as Keys(Server 1) \r\n5 - Deletar todas as keys(Server 2) \r\nQ - Quit");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        GenerateCacheServer1();
                        break;

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        GenerateCacheServer1_OtherDb();
                        break;

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        GenerateCacheServer2();
                        break;

                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        GetAllDatabaseKeysServer1();
                        break;

                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        DeleteAllDatabaseKeysServer2();
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        Console.WriteLine("Unknown input. Please try again.");
                        break;
                }
            }
        }
        
        private static void GenerateCacheServer1()
        {
            try
            {
                Console.WriteLine("Serviço disponível: " + _redisServer1.RedisIsAvailable);

                string cnpj = Guid.NewGuid().ToString();

                Cliente cliente = new Cliente { Cnpj = cnpj, Telefone = new Telefone { Numero = "2199998878" } };
                //Salvando ou atualizando um objeto complexo
                //1ª opção
                _redisServer1.DatabaseContext().SaveOrUpdate(cnpj, cliente);

                //Consultando Objetos complexos
                Cliente result = _redisServer1.DatabaseContext().GetByDeserializeObject<Cliente>(cnpj);
                Console.WriteLine($"***Consultando Objetos complexos*** \r\n Cnpj: {result?.Cnpj} - Telefone: {result?.Telefone.Numero}");

                //Utilizando demais métodos do Redis
                string key = Guid.NewGuid().ToString();
                _redisServer1.DatabaseContext(3).StringSet(key, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ffff"));
                string ex2 = _redisServer1.DatabaseContext(3).StringGet(key);

                Console.WriteLine($"\r\n***Consultando Objetos simples*** \r\n dataHora: {ex2}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void GenerateCacheServer1_OtherDb()
        {
            try
            {
                Console.WriteLine("Serviço disponível: " + _redisServer1.RedisIsAvailable);

                string cnpj = Guid.NewGuid().ToString();

                Console.WriteLine("Utilizando outro Database do Server 1");

                Cliente cliente = new Cliente { Cnpj = cnpj, Telefone = new Telefone { Numero = "2199844452" } };
                _redisServer1.DatabaseContext(4).SaveOrUpdate<Cliente>(key: cnpj, value: cliente);

                Cliente result = _redisServer1.DatabaseContext(4).GetByDeserializeObject<Cliente>(cnpj);
                Console.WriteLine($"***Consultando Objetos complexos*** \r\n Cnpj: {result.Cnpj} - Telefone: {result.Telefone.Numero}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void GetAllDatabaseKeysServer1()
        {
            var keys = _redisServer1.GetAllDatabaseKeys();
            Console.WriteLine("\r\nObtendo todas as chaves do banco 3 - Server 1\r\n");
            foreach (var item in keys)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("*********************************************************");
        }

        private static void DeleteAllDatabaseKeysServer2()
        {
            Console.WriteLine("Deletando os registros.");
            _redisServer2.DeleteAllDatabaseKeys(2);
        }

        private static void GenerateCacheServer2()
        {
            try
            {
                Console.WriteLine("Serviço disponível: " + _redisServer2.RedisIsAvailable);

                string cnpj = Guid.NewGuid().ToString();

                Cliente cliente = new Cliente { Cnpj = cnpj, Telefone = new Telefone { Numero = "2199844452" } };
                _redisServer2.DatabaseContext().SaveOrUpdate<Cliente>(key: cnpj, value: cliente);

                Cliente result = _redisServer2.DatabaseContext(2).GetByDeserializeObject<Cliente>(cnpj);
                Console.WriteLine("Utilizando outro Servidor Redis");
                Console.WriteLine($"***Consultando Objetos complexos*** \r\n Cnpj: {result.Cnpj} - Telefone: {result.Telefone.Numero}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }

```
  
### Referências
- Download Redis para windows (https://github.com/MSOpenTech/redis/releases)
- MsOpenTech (https://github.com/MSOpenTech/redis)
- StackExchange.Redis (https://github.com/StackExchange/StackExchange.Redis)
- Redis (https://redis.io/)
- Manager Redis (https://redisdesktop.com/)

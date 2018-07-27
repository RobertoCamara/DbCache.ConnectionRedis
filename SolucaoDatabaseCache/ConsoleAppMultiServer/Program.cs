using DbCache.ConnectionRedis.Persistence;
using System;

namespace ConsoleAppSingleInstanceDynamicNode
{
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
            RedisHost = "IpHost1",
            RedisPort = 6379,
            RedisPassword = "SuaSenha",
            AllowAdmin = true,
            RedisIdDatabaseDefault = 4
        };

        static DbRedisConfig dtoServer2 = new DbRedisConfig
        {
            RedisHost = "IpHost2",
            RedisPort = 6379,
            RedisPassword = "SuaSenha",
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


}

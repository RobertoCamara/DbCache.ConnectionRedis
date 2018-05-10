using DbCache.ConnectionRedis.Standard.Persistence;
using System;

namespace ConsoleAppSingleInstance
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
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("1 - Gerar Cache | Esc - Cancelar | Q - Quit");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        GenerateCache();
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        Console.WriteLine("Unknown input. Please try again.");
                        break;
                }
            }
        }

        private static void DeleteAllDatabaseKeys()
        {
            DbRedisSingleInstance.DeleteAllDatabaseKeys();
        }

        private static void GenerateCache()
        {
            try
            {
                Console.WriteLine("Serviço disponível: " + DbRedisSingleInstance.RedisIsAvailable);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }

}

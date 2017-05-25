using DbCache.ConnectionRedis.Persistence;
using prmToolkit.Notification;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

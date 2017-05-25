# ConnectionRedis
- Exemplo de projeto utilizando o database in-memory Redis 
- Encapsula a conexão e estende métodos do Redis.
  
### Instalação - DbRedis
```
Install-Package DbCache.ConnectionRedis
```
  
### Exemplos - DbRedis
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
    <add key="REDIS_ID_DATABASE" value="0"/>
  </appSettings>
```

### Exemplos - DbRedisSingleInstance
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

  
### Referências
- Download Redis para windows (https://github.com/MSOpenTech/redis/releases)
- MsOpenTech (https://github.com/MSOpenTech/redis)
- StackExchange.Redis (https://github.com/StackExchange/StackExchange.Redis)
- Redis (https://redis.io/)
- Manager Redis (https://redisdesktop.com/)

# ConnectionRedis
- Exemplo de projeto utilizando o database in-memory Redis 
- Encapsula a conexão e adiciona novos métodos ao Redis

### Console Application
  UI para manipular os métodos do Redis
  
### Instalação
```th
    Install-Package DbCache.ConnectionRedis
```
  
### Exemplos
```th
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
  
### Referências
- Download Redis para windows (https://github.com/MSOpenTech/redis/releases)
- MsOpenTech (https://github.com/MSOpenTech/redis)
- StackExchange.Redis (https://github.com/StackExchange/StackExchange.Redis)
- Redis (https://redis.io/)
- Manager Redis (https://redisdesktop.com/)

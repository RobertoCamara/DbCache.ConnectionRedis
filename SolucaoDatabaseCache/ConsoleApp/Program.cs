using DbCache.ConnectionRedis.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ConsoleApp
{

    class Program
    {
        //somente para simular autenticação na aplicação e não com REDIS
        const string LOGIN_ADM = "admin";
        const string PASSWORD_ADM = "Redis@dmin";

        private static bool _isAuthenticated = false;

        private static DbRedis conn = new DbRedis("192.168.1.44", 6379, idDb: 0, password: PASSWORD_ADM);

        static void Main(string[] args)
        {
            Console.Title = "=============== CRUD COM REDIS ===============";
            Menu();
        }

        private static void Menu()
        {
            Console.Clear();
            Console.WriteLine("[1]- Obter Usuário | [2]- Cadastrar Usuário | [3]- Obter Todos | [4]- Administrador | [5]- Sair");
            string opcao = Console.ReadLine();

            if (opcao == "4")
                AutenticarAdministrator();

            if (opcao != "1" && opcao != "2" && opcao != "3" && opcao != "5")
                Menu();

            SelecionarMenuPrincipal(opcao);
        }

        private static void AutenticarAdministrator()
        {
            Console.WriteLine("# ========== PAINEL ADMINISTRADOR ========== #");
            Usuario adm = new Usuario();
            Console.Write("Login: "); adm.Login = Console.ReadLine();
            Console.Write("Senha: "); adm.Senha = Console.ReadLine();
            GerenciarMenuAdministrator(adm);
        }

        private static void GerenciarMenuAdministrator(Usuario adm)
        {
            _isAuthenticated = AutenticarAdm(adm);
            if (_isAuthenticated)
                MenuAdministratorAutenticado();
            else
            {
                Console.WriteLine("Login/Senha não encontrado.");
                Console.ReadKey();
                Menu();
            }
        }

        private static void MenuAdministratorAutenticado()
        {
            bool opcaoValida = true;
            do
            {
                Console.Clear();
                Console.WriteLine("[1]- Delete All Database Keys | [2]- Delete All Keys From All Databases | [3]- Menu Principal");
                string opcao = Console.ReadLine();
                switch (opcao)
                {
                    case "1":
                        DeleteAllKeysDatabase();
                        break;
                    case "2":
                        DeleteAllKeysFromAllDatabases();
                        break;
                    case "3":
                        Menu();
                        break;
                }
                opcaoValida = (opcao == "1" || opcao == "2");
            } while (opcaoValida);
        }

        private static void DeleteAllKeysDatabase()
        {
            try
            {
                conn.DeleteAllDatabaseKeys();
                Console.WriteLine("All keys deleted");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            MenuAdministratorAutenticado();
        }

        private static void DeleteAllKeysFromAllDatabases()
        {
            try
            {
                conn.DeleteAllKeysFromAllDatabases();
                Console.WriteLine("All the keys deleted from the server");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            MenuAdministratorAutenticado();
        }

        private static bool AutenticarAdm(Usuario usuario)
        {
            return (usuario.Login == LOGIN_ADM && usuario.Senha == PASSWORD_ADM);
        }

        private static void SelecionarMenuPrincipal(string opcao)
        {
            switch (opcao)
            {
                case "1":
                    ObterUsuario();
                    break;
                case "2":
                    Cadastrar();
                    break;
                case "3":
                    ObterTodosUsuarios();
                    break;
                case "5":
                    Environment.Exit(0);
                    break;
            }
        }

        private static void ObterTodosUsuarios()
        {
            Console.Clear();
            Console.WriteLine("# ========== TODOS USUÁRIOS ========== #");

            var keys = conn.GetAllDatabaseKeys();
            foreach (var key in keys)
            {
                Usuario usuario = conn.GetByDeserializeObject<Usuario>(key);
                if (usuario != null)
                {
                    Console.WriteLine("=================================================");
                    Console.WriteLine($"| Login: {usuario.Login}");
                    Console.WriteLine("-------------------------------------------------");
                    Console.WriteLine($"| CPF: {usuario.Cpf}");
                    Console.WriteLine("-------------------------------------------------");
                    Console.WriteLine("| Empresas: ");
                    foreach (var item in usuario.Empresas)
                    {
                        Console.WriteLine("| ---------> " + item);
                    }
                    Console.WriteLine("=================================================");
                    Console.WriteLine("\r\n");
                }
            }
            if (keys.Count() == 0)
                Console.WriteLine("Nenhum usuário encontrado.");

            Console.ReadKey();
            Menu();

        }

        private static void ObterUsuario()
        {
            Console.Clear();
            Console.WriteLine("# ========== CONSULTAR USUÁRIO ========== #");
            Console.Write("Pesquisar por Login: ");
            string opcao = Console.ReadLine();
            Usuario usuario = conn.GetByDeserializeObject<Usuario>(opcao);
            if (usuario != null)
            {
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine($"| Login: {usuario.Login}");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine($"| CPF: {usuario.Cpf}");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("| Empresas: ");
                foreach (var item in usuario.Empresas)
                {
                    Console.WriteLine("| ---------> " + item);
                }
                Console.WriteLine("-------------------------------------------------");
                SubMenuConsulta(usuario);
            }
            else
            {
                Console.WriteLine("Usuário não encontrado.");
                Console.WriteLine("[1]- Menu");
                opcao = Console.ReadLine();
                if (opcao == "1")
                    Menu();

                ObterUsuario();
            }
        }

        private static string SubMenuConsulta(Usuario usuario)
        {
            string opcao;
            Console.WriteLine("[1]- Menu | [2]- Deletar");
            opcao = Console.ReadLine();
            if (opcao != "1" && opcao != "2")
            {
                Console.WriteLine("Selecione uma opção.");
                SubMenuConsulta(usuario);
            }

            if (opcao == "1")
                Menu();
            else
                DeletarUsuario(usuario);

            Console.ReadKey();
            return opcao;
        }

        private static void DeletarUsuario(Usuario usuario)
        {
            string opcao;
            Console.WriteLine("Tem certeza que deseja deletar o registro? [S]- sim | [N]- não");
            opcao = Console.ReadLine();
            if (opcao.ToUpper() != "S" && opcao.ToUpper() != "N")
            {
                Console.WriteLine("Opção inválida. Informe apenas [S] ou [N]");
                DeletarUsuario(usuario);
            }

            if (opcao.ToUpper() == "S")
            {
                conn.Delete(usuario.Login);
                Console.WriteLine("Registro deletado com sucesso.");
                Console.ReadKey();
                Menu();
            }
            else
                Menu();
        }

        private static void Cadastrar()
        {
            Console.Clear();
            Usuario usuario = new Usuario();
            Console.WriteLine("########### CADASTRAR USUÁRIO ##########");
            Console.Write("Login: ");
            usuario.Login = Console.ReadLine();
            Console.Write("Senha: ");
            usuario.Senha = Console.ReadLine();
            Console.Write("CPF: ");
            usuario.Cpf = Console.ReadLine();

            for (int i = 0; i < 5; i++)
            {
                usuario.AddEmpresa();
            }

            conn.SaveOrUpdate<Usuario>(usuario.Login, usuario);
            Console.WriteLine("Usuário cadastrado com sucesso.");
            Console.ReadKey();
            Menu();
        }
    }

    public class Usuario
    {
        public string Login { get; set; }
        public string Senha { get; set; }
        public string Cpf { get; set; }
        public List<string> Empresas { get; set; }

        private Random _rnd;

        public Usuario()
        {
            this.Empresas = new List<string>();
            _rnd = new Random();
        }


        public void AddEmpresa()
        {
            string[] emp = { "Alfa Ltda", "Beta S/A", "Gama Ltda", "Delta Ltda", "Zeta S/A", "Lambda Ltda", "Omega S/A" };
            int ix = _rnd.Next(emp.Length);
            string e = emp[ix];
            if (!this.Empresas.Any(a => a == e))
                this.Empresas.Add(e);
        }
    }
}

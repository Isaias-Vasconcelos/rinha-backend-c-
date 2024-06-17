using MySql.Data.MySqlClient;
using ProvadorDeRoupas.Request;
using ProvadorDeRoupas.Response;

namespace ProvadorDeRoupas.Database
{
    public class DB
    {
        const string stringConnection = "Server=127.0.0.1;Database=rinha_backend;Uid=root;Pwd=jjkeys61;";
        public static async Task<IEnumerable<ClienteResponse>> ListarClientes()
        {
            using var connection = new MySqlConnection(stringConnection);

            ICollection<ClienteResponse> clientes = [];
            var clientesDict = new Dictionary<string, ClienteResponse>();

            try
            {
                connection.Open();

                string sqlSelect = @"SELECT clientes.id,
                                            clientes.name,
                                            clientes.lastname,
                                            roupas.name AS roupa
                                     FROM clientes
                                     INNER JOIN roupas ON clientes.id = roupas.clienteId";


                using (var command = new MySqlCommand(sqlSelect, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var idCliente = reader["id"].ToString();
                            var nome = reader["name"].ToString();
                            var sobrenome = reader["lastname"].ToString();
                            string? descricaoRoupa = reader["roupa"] as string;

                            if (!clientesDict.ContainsKey(idCliente))
                            {
                                var cliente = new ClienteResponse
                                {
                                    Id = idCliente,
                                    Name = nome,
                                    Lastname = sobrenome,
                                    Clothes = []
                                };

                                clientesDict[idCliente] = cliente;

                                clientes.Add(cliente);
                            }

                            if (descricaoRoupa != null)
                            {
                                var cliente = clientesDict[idCliente];
                                var roupasList = new List<string>(cliente.Clothes)
                                {
                                    descricaoRoupa
                                };
                                cliente.Clothes = [.. roupasList];
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return clientes;
        }

        public static async Task<IEnumerable<ClienteResponse>> ListarClientesPorTermo(string termo)
        {
            using var connection = new MySqlConnection(stringConnection);

            ICollection<ClienteResponse> clientes = [];

            var clientesDict = new Dictionary<string, ClienteResponse>();

            try
            {
                connection.Open();

                string sqlSelect = @"SELECT clientes.id,
                                            clientes.name,
                                            clientes.lastname,
                                            roupas.name AS roupa
                                     FROM clientes
                                     INNER JOIN roupas ON clientes.id = roupas.clienteId
                                     WHERE clientes.id LIKE @Termo
                                     OR clientes.name LIKE @Termo
                                     OR clientes.lastname LIKE @Termo
                                     OR roupas.name LIKE @Termo";



                using (var command = new MySqlCommand(sqlSelect, connection))
                {
                    command.Parameters.AddWithValue("@Termo", $"%{termo}%");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var idCliente = reader["id"].ToString();
                            var nome = reader["name"].ToString();
                            var sobrenome = reader["lastname"].ToString();
                            string? descricaoRoupa = reader["roupa"] as string;

                            if (!clientesDict.ContainsKey(idCliente))
                            {
                                var cliente = new ClienteResponse
                                {
                                    Id = idCliente,
                                    Name = nome,
                                    Lastname = sobrenome,
                                    Clothes = []
                                };

                                clientesDict[idCliente] = cliente;

                                clientes.Add(cliente);
                            }

                            if (descricaoRoupa != null)
                            {
                                var cliente = clientesDict[idCliente];
                                var roupasList = new List<string>(cliente.Clothes)
                                {
                                    descricaoRoupa
                                };
                                cliente.Clothes = [.. roupasList];
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return clientes;
        }
    }
}

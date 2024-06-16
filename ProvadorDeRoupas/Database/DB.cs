using MySql.Data.MySqlClient;
using ProvadorDeRoupas.Request;
using ProvadorDeRoupas.Response;

namespace ProvadorDeRoupas.Database
{
    public class DB
    {
        const string stringConnection = "Server=127.0.0.1;Database=rinha_backend;Uid=root;Pwd=jjkeys61;";
        public static async Task ProcessData(List<ClienteRequest> clienteRequests, int maxConcurrency = 500)
        {
            using var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = clienteRequests.Select(async item =>
            {
                await semaphore.WaitAsync();

                try
                {
                    await Insert(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

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
        static async Task Insert(ClienteRequest clienteRequest)
        {
            using var connection = new MySqlConnection(stringConnection);

            try
            {
                connection.Open();

                string clienteId = Guid.NewGuid().ToString();

                string insertCliente = "INSERT INTO clientes (id,name,lastname) VALUES (@Id,@Name,@LastName)";

                string insertClothes = "INSERT INTO roupas (clienteId,name) VALUES (@ClienteId,@Name1),(@ClienteId,@Name2),(@ClienteId,@Name3);";

                using MySqlCommand commandInsertCliente = new(insertCliente, connection);

                commandInsertCliente.Parameters.AddWithValue("Id", clienteId);
                commandInsertCliente.Parameters.AddWithValue("Name", clienteRequest.Name);
                commandInsertCliente.Parameters.AddWithValue("LastName", clienteRequest.Lastname);

                using MySqlCommand commandInsertClothes = new(insertClothes, connection);

                commandInsertClothes.Parameters.AddWithValue("@ClienteId", clienteId);
                commandInsertClothes.Parameters.AddWithValue("@Name1", clienteRequest.Clothes[0]);
                commandInsertClothes.Parameters.AddWithValue("@Name2", clienteRequest.Clothes[1]);
                commandInsertClothes.Parameters.AddWithValue("@Name3", clienteRequest.Clothes[2]);

                await commandInsertCliente.ExecuteNonQueryAsync();
                await commandInsertClothes.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                connection.Dispose();
            }
        }
    }
}

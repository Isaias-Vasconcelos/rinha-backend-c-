using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProvadorDeRoupas.Database;
using ProvadorDeRoupas.Request;
using System.Collections.Specialized;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options
  => options.AddPolicy(name: "_police", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

List<ClienteRequest> clienteRequests = [];

const string stringConnection = "Server=127.0.0.1;Database=rinha_backend;Uid=root;Pwd=jjkeys61;";

app.MapPost("/api/clientes", async ([FromBody] ClienteRequest clienteRequest) =>
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
        Console.WriteLine(ex.Message);
    }
    finally
    {
        connection.Dispose();
    }

    return Results.Ok();
});

app.MapGet("/api/clientes", async () =>
{
    var clientes = await DB.ListarClientes();

    return Results.Ok(clientes);
});

app.MapGet("/api/clientes/termo", async (string? name) =>
{
    var clientes = await DB.ListarClientesPorTermo(name);

    return Results.Ok(clientes);
});


app.UseCors("_police");

app.UseHttpsRedirection();

app.Run();


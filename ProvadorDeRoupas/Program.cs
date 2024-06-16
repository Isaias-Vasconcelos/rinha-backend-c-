using Microsoft.AspNetCore.Mvc;
using ProvadorDeRoupas.Database;
using ProvadorDeRoupas.Request;

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


var clientesRequest = new List<ClienteRequest>();

app.MapPost("/api/clientes", async ([FromBody] ClienteRequest clienteRequest) =>
{
    clientesRequest.Add(clienteRequest);

    if (clientesRequest!.Count == 5000)
    {
        await DB.ProcessData(clientesRequest);
        clientesRequest.Clear();
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


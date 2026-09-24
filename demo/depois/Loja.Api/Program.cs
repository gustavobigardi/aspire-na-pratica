using System.Text.Json;
using Loja.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// ServiceDefaults: OpenTelemetry (logs, traces, métricas), health checks,
// service discovery e resiliência padrão no HttpClient. Uma linha.
builder.AddServiceDefaults();

// As connection strings "lojadb" e "cache" chegam pelo AppHost (WithReference).
builder.AddNpgsqlDbContext<LojaDb>("lojadb");
builder.AddRedisDistributedCache("cache");

// "recomendacoes" é o nome lógico do serviço Python no AppHost.
// O ServiceDefaults já dá retry, timeout e circuit breaker a todo HttpClient.
// Recomendação é "nice to have": aqui a gente prefere falhar rápido a segurar o pedido.
builder.Services.AddHttpClient<RecomendacoesClient>(client =>
        client.BaseAddress = new Uri("https+http://recomendacoes"))
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 1;
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);
    });

var app = builder.Build();

app.MapDefaultEndpoints();

// Cria o schema e carrega o catálogo na primeira execução.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LojaDb>();
    await db.Database.EnsureCreatedAsync();
}

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.MapGet("/produtos", async (LojaDb db, IDistributedCache cache, ILogger<Program> log) =>
{
    const string chave = "produtos";
    var emCache = await cache.GetStringAsync(chave);
    if (emCache is not null)
    {
        log.LogInformation("Catálogo servido do cache");
        return Results.Content(emCache, "application/json");
    }

    var produtos = await db.Produtos.OrderBy(p => p.Id).ToListAsync();
    var json = JsonSerializer.Serialize(produtos, jsonOptions);
    await cache.SetStringAsync(chave, json, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });
    log.LogInformation("Catálogo lido do banco e guardado no cache ({Quantidade} produtos)", produtos.Count);
    return Results.Content(json, "application/json");
});

app.MapGet("/pedidos", async (LojaDb db) =>
    await db.Pedidos.OrderByDescending(p => p.Id).Take(20).ToListAsync());

app.MapPost("/pedidos", async (NovoPedido novo, LojaDb db, RecomendacoesClient recomendacoes, ILogger<Program> log) =>
{
    var produto = await db.Produtos.FindAsync(novo.ProdutoId);
    if (produto is null)
    {
        return Results.NotFound(new { erro = $"Produto {novo.ProdutoId} não existe" });
    }

    var pedido = new Pedido { ProdutoId = produto.Id, Quantidade = novo.Quantidade };
    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync();
    log.LogInformation("Pedido {PedidoId} criado: {Quantidade}x {Produto}", pedido.Id, pedido.Quantidade, produto.Nome);

    // Chamada para o serviço em Python. Se ele falhar, o pedido já foi salvo,
    // mas o cliente não recebe recomendações: a gente devolve 502 para ficar visível.
    Recomendacao[] sugestoes;
    try
    {
        sugestoes = await recomendacoes.ParaProdutoAsync(produto.Id);
    }
    catch (HttpRequestException ex)
    {
        log.LogError(ex, "Serviço de recomendações falhou para o produto {ProdutoId}", produto.Id);
        return Results.Problem(
            title: "Pedido salvo, mas o serviço de recomendações falhou",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }

    return Results.Created($"/pedidos/{pedido.Id}", new
    {
        pedido.Id,
        Produto = produto.Nome,
        pedido.Quantidade,
        Total = produto.Preco * pedido.Quantidade,
        Recomendacoes = sugestoes
    });
});

app.MapPost("/cache/limpar", async (IDistributedCache cache, ILogger<Program> log) =>
{
    await cache.RemoveAsync("produtos");
    log.LogInformation("Cache do catálogo limpo");
    return Results.NoContent();
});

app.Run();

record NovoPedido(int ProdutoId, int Quantidade = 1);

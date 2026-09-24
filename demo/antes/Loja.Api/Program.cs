using System.Text.Json;
using Loja.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Cada dependência é configurada na mão, com endereço fixo no appsettings / .env.
builder.Services.AddDbContext<LojaDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("lojadb")));

builder.Services.AddStackExchangeRedisCache(options =>
    options.Configuration = builder.Configuration["Redis"]);

builder.Services.AddHttpClient<RecomendacoesClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Recomendacoes:Url"]!));

var app = builder.Build();

// Se o Postgres ainda não subiu, isso estoura e o processo morre.
// Solução do time: "roda de novo".
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LojaDb>();
    await db.Database.EnsureCreatedAsync();
}

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.MapGet("/health", () => Results.Ok("Healthy"));

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

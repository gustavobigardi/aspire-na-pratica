using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// "api" é o nome lógico no AppHost. Local vira localhost:porta; na nuvem, o endereço interno.
// Retry padrão do ServiceDefaults é ótimo para GET; para POST criaria pedidos duplicados.
builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("https+http://api"))
    .RemoveAllResilienceHandlers()
    .AddStandardResilienceHandler(options =>
        options.Retry.ShouldHandle = args => ValueTask.FromResult(
            args.Outcome.Result?.RequestMessage?.Method == HttpMethod.Get
            && HttpClientResiliencePredicates.IsTransient(args.Outcome)));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", async (IHttpClientFactory factory, string? msg, string? erro) =>
{
    var api = factory.CreateClient("api");
    var produtos = await api.GetFromJsonAsync<List<Produto>>("/produtos") ?? [];
    var pedidos = await api.GetFromJsonAsync<List<Pedido>>("/pedidos") ?? [];
    var nomes = produtos.ToDictionary(p => p.Id, p => p.Nome);

    var html = new StringBuilder();
    html.Append("""
        <!doctype html><html lang="pt-br"><head><meta charset="utf-8"><title>Loja</title>
        <style>
          body{font-family:system-ui,sans-serif;max-width:900px;margin:2rem auto;padding:0 1rem;color:#1b2a41}
          h1{color:#008cfd} table{border-collapse:collapse;width:100%} td,th{padding:.5rem;border-bottom:1px solid #dde}
          form{display:flex;gap:.5rem;align-items:center;margin:1rem 0} button{background:#008cfd;color:#fff;border:0;padding:.5rem 1rem;border-radius:.5rem;font-size:1rem}
          .ok{background:#e7f7ee;padding:.75rem;border-radius:.5rem} .erro{background:#fdecea;padding:.75rem;border-radius:.5rem}
          small{color:#8a94a6}
        </style></head><body>
        <h1>Loja</h1>
        """);

    if (msg is not null) html.Append($"<p class=\"ok\">{WebUtility.HtmlEncode(msg)}</p>");
    if (erro is not null) html.Append($"<p class=\"erro\">{WebUtility.HtmlEncode(erro)}</p>");

    html.Append("<form method=\"post\" action=\"/pedidos\"><label>Produto <select name=\"produtoId\">");
    foreach (var p in produtos)
    {
        html.Append($"<option value=\"{p.Id}\">{WebUtility.HtmlEncode(p.Nome)} · R$ {p.Preco:N2}</option>");
    }
    html.Append("</select></label><label>Qtd <input type=\"number\" name=\"quantidade\" value=\"1\" min=\"1\" style=\"width:4rem\"></label>");
    html.Append("<button type=\"submit\">Fazer pedido</button></form>");

    html.Append("<h2>Últimos pedidos</h2><table><tr><th>#</th><th>Produto</th><th>Qtd</th><th>Quando (UTC)</th></tr>");
    foreach (var p in pedidos)
    {
        var nome = nomes.TryGetValue(p.ProdutoId, out var n) ? n : $"produto {p.ProdutoId}";
        html.Append($"<tr><td>{p.Id}</td><td>{WebUtility.HtmlEncode(nome)}</td><td>{p.Quantidade}</td><td>{p.CriadoEm:dd/MM HH:mm:ss}</td></tr>");
    }
    html.Append("</table><p><small>Loja.Web → Loja.Api → PostgreSQL, Redis e recomendacoes (Python)</small></p></body></html>");

    return Results.Content(html.ToString(), "text/html; charset=utf-8");
});

app.MapPost("/pedidos", async (HttpRequest request, IHttpClientFactory factory) =>
{
    var form = await request.ReadFormAsync();
    var produtoId = int.Parse(form["produtoId"]!);
    var quantidade = int.TryParse(form["quantidade"], out var q) ? q : 1;

    var api = factory.CreateClient("api");
    var resposta = await api.PostAsJsonAsync("/pedidos", new { produtoId, quantidade });

    if (!resposta.IsSuccessStatusCode)
    {
        var problema = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        var titulo = problema.TryGetProperty("title", out var t) ? t.GetString() : resposta.ReasonPhrase;
        return Results.Redirect($"/?erro={Uri.EscapeDataString($"{(int)resposta.StatusCode}: {titulo}")}");
    }

    var criado = await resposta.Content.ReadFromJsonAsync<JsonElement>();
    var recomendacoes = criado.GetProperty("recomendacoes").EnumerateArray()
        .Select(r => r.GetProperty("nome").GetString()).ToArray();
    var msg = $"Pedido #{criado.GetProperty("id").GetInt32()} criado. Quem comprou isso também levou: {string.Join(", ", recomendacoes)}";
    return Results.Redirect($"/?msg={Uri.EscapeDataString(msg)}");
});

app.Run();

record Produto(int Id, string Nome, decimal Preco);
record Pedido(int Id, int ProdutoId, int Quantidade, DateTime CriadoEm);

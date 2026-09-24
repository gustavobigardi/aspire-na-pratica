namespace Loja.Api;

public record Recomendacao(int ProdutoId, string Nome, string Motivo);

// Cliente tipado: o endereço base vem do service discovery ("https+http://recomendacoes").
public class RecomendacoesClient(HttpClient http)
{
    public async Task<Recomendacao[]> ParaProdutoAsync(int produtoId, CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<Recomendacao[]>($"/recomendacoes/{produtoId}", ct) ?? [];
    }
}

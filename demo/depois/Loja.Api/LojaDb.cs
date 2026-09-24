using Microsoft.EntityFrameworkCore;

namespace Loja.Api;

public class Produto
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public decimal Preco { get; set; }
}

public class Pedido
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

public class LojaDb(DbContextOptions<LojaDb> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public static readonly Produto[] Catalogo =
    [
        new() { Id = 1, Nome = "Teclado mecânico", Preco = 449.90m },
        new() { Id = 2, Nome = "Mouse sem fio", Preco = 189.90m },
        new() { Id = 3, Nome = "Monitor 27\" 4K", Preco = 2_499.00m },
        new() { Id = 4, Nome = "Headset com cancelamento de ruído", Preco = 899.00m },
        new() { Id = 5, Nome = "Webcam 1080p", Preco = 349.90m },
        new() { Id = 6, Nome = "Hub USB-C", Preco = 259.90m },
        new() { Id = 7, Nome = "Cadeira ergonômica", Preco = 1_899.00m },
        new() { Id = 8, Nome = "Suporte para notebook", Preco = 129.90m },
        new() { Id = 13, Nome = "Caneca MVP Conf 2026", Preco = 49.90m },
    ];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>().HasData(Catalogo);
    }
}

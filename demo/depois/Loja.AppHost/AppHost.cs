// AppHost: o "mapa" da aplicação. Aqui a gente declara quem existe,
// quem depende de quem e como os serviços se encontram.
var builder = DistributedApplication.CreateBuilder(args);

// Onde publicar. Padrão: Docker Compose (local). Com Deploy__Target=aca, Azure Container Apps.
if (builder.ExecutionContext.IsPublishMode)
{
    if (string.Equals(builder.Configuration["Deploy:Target"], "aca", StringComparison.OrdinalIgnoreCase))
    {
        builder.AddAzureContainerAppEnvironment("aca-env");
    }
    else
    {
        builder.AddDockerComposeEnvironment("compose");
    }
}

// Infra: containers gerenciados pelo Aspire, com volume para não perder dados entre execuções.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb();

var lojadb = postgres.AddDatabase("lojadb");

var cache = builder.AddRedis("cache")
    .WithRedisInsight();

// Serviço em Python (FastAPI): mesmo AppHost, mesmo dashboard, mesmo trace.
var recomendacoes = builder.AddUvicornApp("recomendacoes", "../recomendacoes", "main:app")
    .WithHttpEndpoint(env: "PORT")
    .WithHttpHealthCheck("/health");

// API em .NET: recebe a connection string do banco e do cache por WithReference,
// e só sobe depois que o banco, o cache e o serviço Python estiverem saudáveis.
var api = builder.AddProject<Projects.Loja_Api>("api")
    .WithReference(lojadb)
    .WithReference(cache)
    .WithReference(recomendacoes)
    .WaitFor(lojadb)
    .WaitFor(cache)
    .WaitFor(recomendacoes)
    .WithHttpHealthCheck("/health");

// Front em .NET: fala com a API pelo nome lógico "api" (service discovery).
builder.AddProject<Projects.Loja_Web>("web")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();

# .NET Aspire na prática: o jeito moderno de construir, executar e observar aplicações distribuídas

Materiais da palestra apresentada no **MVP Conf 2026** (São Paulo, setembro de 2026) por Gustavo Bigardi.

> Uma visão prática de como o Aspire simplifica o desenvolvimento cloud-native com orquestração local, observabilidade integrada, service discovery e melhor experiência para times que trabalham com múltiplos serviços. A mesma aplicação aparece duas vezes: do jeito que a maioria dos times roda hoje e com um AppHost.

## A mensagem em uma frase

Orquestração local não é luxo: é onde o time passa o dia, e o AppHost é o contrato entre o laptop e a produção.

## Conteúdo

| Pasta/arquivo | O que é |
|---|---|
| [`slides.pdf`](slides.pdf) | Slides da palestra |
| [`demo/antes/`](demo/antes) | A Loja sem Aspire: `docker-compose`, `.env`, portas fixas e três terminais |
| [`demo/depois/`](demo/depois) | A mesma Loja com AppHost, ServiceDefaults, service discovery e OpenTelemetry |
| [`.agents/skills/`](.agents/skills) | Skills do Aspire instaladas por `aspire agent init`, usadas na demo com agente |
| [`referencias.md`](referencias.md) | Fontes, todas conferidas contra o Aspire 13.5.4 |

## A Loja

Cinco recursos, duas linguagens:

- `Loja.Web` (.NET 10): página com formulário de pedido; fala com a API pelo nome lógico `api`.
- `Loja.Api` (.NET 10): catálogo com cache no Redis, pedidos no PostgreSQL, sugestões vindas do serviço Python.
- `recomendacoes` (Python 3, FastAPI): "quem comprou X também levou Y". O produto 13 não tem regra de propósito: é o bug que a palestra caça no dashboard e com um agente.
- PostgreSQL e Redis em containers.

O diff entre `demo/antes` e `demo/depois` é a palestra:

```bash
diff -r --exclude=bin --exclude=obj --exclude=.venv demo/antes demo/depois
```

## Como rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (só o AppHost precisa dele; os serviços poderiam continuar em .NET 8 ou 9).
- [Aspire CLI](https://aspire.dev/get-started/install-cli/) 13.5 ou mais recente.
- Docker (ou Podman) em execução.
- Python 3.10+ (o Aspire cria o `.venv` e instala o `requirements.txt` sozinho).

### Depois (com Aspire)

```bash
cd demo/depois && aspire run
```

O dashboard abre sozinho. Faça um pedido de qualquer produto e depois um da "Caneca MVP Conf 2026"; siga o erro pelo trace.

Para gerar os artefatos de Docker Compose:

```bash
cd demo/depois && aspire publish -o ../../aspire-output
```

Para Azure Container Apps (pede subscription e região na primeira vez):

```bash
cd demo/depois && Deploy__Target=aca aspire deploy
```

### Antes (sem Aspire)

Veja [`demo/antes/README.md`](demo/antes/README.md). São sete passos e três terminais, de propósito.

### Agente de IA

O AppHost expõe recursos, logs e traces por MCP. A configuração para VS Code está em `demo/depois/.vscode/mcp.json`; para outros agentes, rode `aspire agent init` na pasta. Pergunte ao agente por que o pedido da caneca falha e deixe-o ler o trace.

## Avisos

- PostgreSQL e Redis em container no Azure Container Apps servem para a demo. Em produção, prefira os serviços gerenciados (`AddAzurePostgresFlexibleServer(...).RunAsContainer()` mantém o container só no ambiente local).
- O dashboard do Aspire, local ou no Azure, é ferramenta de desenvolvimento e diagnóstico. Em produção, exporte a telemetria por OTLP para o seu backend (Application Insights, Grafana etc.).
- O código foi escrito para o Aspire 13.5.4. Versões novas podem mudar APIs; `aspire update` ajuda na migração.

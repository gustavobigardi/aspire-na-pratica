# Referências

Fontes usadas na palestra. Tudo foi conferido contra o Aspire **13.5.4**, a versão instalada na máquina da demo em 22/09/2026. Os comandos do CLI citados nos slides saem de `aspire --help` dessa versão.

## Aspire: o que é e para onde vai

- [O que é o Aspire](https://aspire.dev/get-started/what-is-aspire/): "a code-first orchestration and observability layer for distributed applications".
- [Bem-vindo ao novo Aspire](https://devblogs.microsoft.com/aspire/aspirepolyglot/): o ".NET" sai do nome e o projeto assume o rumo poliglota.
- [What's new in Aspire 13](https://aspire.dev/whats-new/aspire-13/) (nov/2025): Python e JavaScript como cidadãos de primeira classe, AppHost de arquivo único, `aspire do`, novo pipeline de publish/deploy, exige .NET 10 SDK no AppHost.
- [Aspire 13.2](https://devblogs.microsoft.com/aspire/aspire-13-2-announcement/) (23/03/2026): CLI pensado para agentes, `aspire start`, `--isolated`, exportar traces e logs, AppHost em TypeScript (preview).
- [Aspire 13.3](https://devblogs.microsoft.com/aspire/whats-new-aspire-13-3/): skill *aspireify*, comandos com resultado, browser logs, Kubernetes e AKS.
- [Aspire 13.4](https://devblogs.microsoft.com/aspire/whats-new-aspire-13-4/) (01/06/2026): AppHost TypeScript GA, `aspire logs --search`, Go e Bun, Foundry.
- [Aspire 13.5](https://aspire.dev/whats-new/aspire-13-5/) (18/08/2026): dashboard com visual novo, terminal interativo, CLI bundle por padrão, AI Assistant do dashboard removido, 12 breaking changes. Ressalva: misturar pacotes 13.4 e 13.5 falha em runtime.
- [Roadmap 2026 → 2027](https://github.com/microsoft/aspire/discussions/18581) (01/07/2026): "agent-ready", "scales with your team", "built to ship"; AppHost em Python, Java, Go e Rust; testes; AWS como alvo de deploy.
- [Upgrade to Aspire 13](https://learn.microsoft.com/dotnet/aspire/get-started/upgrade-to-aspire-13): o AppHost exige .NET 10 SDK; `aspire update` migra 9.x → 13.
- [Pré-requisitos](https://aspire.dev/get-started/prerequisites/): serviços podem continuar em .NET 8 ou 9; só o AppHost em C# precisa do .NET 10.

## Conceitos usados nos slides

- [AppHost](https://aspire.dev/get-started/app-host/) e o [glossário](https://aspire.dev/reference/glossary/): recursos, `WithReference`, `WaitFor`, `WaitForCompletion`.
- [Service discovery](https://aspire.dev/fundamentals/service-discovery/): variáveis `services__<nome>__<protocolo>__<índice>` e o esquema `https+http://`.
- [Health checks](https://aspire.dev/fundamentals/health-checks/): `/health` e `/alive`, diferença entre health check do AppHost e do serviço.
- [Telemetria](https://aspire.dev/fundamentals/telemetry/): o que o ServiceDefaults configura; `OTEL_EXPORTER_OTLP_ENDPOINT` e companhia injetados pelo AppHost.
- [Explorar o dashboard](https://aspire.dev/dashboard/explore/): Resources, Console logs, Structured logs, Traces, Metrics, Graph, GenAI visualizer.
- [Migrar do Docker Compose](https://aspire.dev/get-started/docker-compose-migration/): tabela comparativa e padrões de migração usados no bloco "antes e depois".
- [Adicionar Aspire a um app existente](https://aspire.dev/get-started/add-aspire-existing-app/): `aspire init` e a skill *aspireify*.
- [Python no Aspire](https://aspire.dev/integrations/frameworks/python/): `AddUvicornApp`, detecção de `uv`/`pip`, Dockerfile gerado.
- [OpenTelemetry em Python com o dashboard](https://aspire.dev/dashboard/python-opentelemetry/): base do `telemetry.py` da demo.
- [Resiliência HTTP padrão](https://learn.microsoft.com/dotnet/core/resilience/http-resilience): o `AddStandardResilienceHandler` que o ServiceDefaults aplica a todo `HttpClient` (retry, timeout, circuit breaker) e que retenta POST se você não pedir o contrário.

## Publicar e implantar

- [Docker Compose](https://aspire.dev/deployment/docker-compose/): `AddDockerComposeEnvironment`, `aspire publish`, artefatos gerados (`docker-compose.yaml`, `.env`, Dockerfiles).
- [Azure Container Apps](https://aspire.dev/deployment/azure/container-apps/): `AddAzureContainerAppEnvironment`, o que `aspire deploy` cria (ACR, ambiente, identidade gerenciada, dashboard) e `aspire destroy`.
- [Azure App Service](https://learn.microsoft.com/azure/app-service/quickstart-aspire) (preview) e [Kubernetes/AKS](https://devblogs.microsoft.com/aspire/whats-new-aspire-13-3/).
- [Dashboard do Aspire no Azure Container Apps](https://learn.microsoft.com/azure/container-apps/aspire-dashboard): visualização de telemetria, exige Contributor ou Owner no ambiente.
- [Application Insights com Aspire](https://learn.microsoft.com/dotnet/aspire/deployment/aspire-deploy/application-insights): para onde a telemetria vai em produção.

## Agentes de IA

- [Aspire MCP server](https://aspire.dev/get-started/aspire-mcp-server/): `aspire agent mcp`, lista de ferramentas (`list_resources`, `list_structured_logs`, `list_traces`, `execute_resource_command`...). O MCP embutido no dashboard foi removido.
- [Aspire skills](https://aspire.dev/get-started/aspire-skills/): `aspire agent init`, skills `aspire`, `aspire-init`, `aspire-orchestration`, `aspire-monitoring`, `aspire-deployment` e `aspireify`.
- [Depurar falhas com agentes](https://aspire.dev/dashboard/ai-coding-agents/): como o agente usa logs, traces e recursos do dashboard.
- Referência do CLI: [`aspire agent mcp`](https://aspire.dev/reference/cli/commands/aspire-agent-mcp/), [`aspire logs`](https://aspire.dev/reference/cli/commands/aspire-logs/), [`aspire otel traces`](https://aspire.dev/reference/cli/commands/aspire-otel-traces/), [`aspire describe`](https://aspire.dev/reference/cli/commands/aspire-describe/).

## Comunidade e leitura complementar

- [Microsoft steers Aspire to a polyglot future](https://www.infoworld.com/article/4085051/aspires-polyglot-future.html), InfoWorld.
- [Aspire Has Changed: A Catch-Up for the 2025 Crowd](https://blog.nimblepros.com/blogs/aspire-revisit/), NimblePros.
- [Repositório microsoft/aspire](https://github.com/microsoft/aspire) e [microsoft/aspire-skills](https://github.com/microsoft/aspire-skills).

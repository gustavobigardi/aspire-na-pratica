# Loja, antes do Aspire

Três serviços (Web e API em .NET, recomendações em Python), um banco e um cache. Cada um sobe do seu jeito.

## Para rodar localmente

1. Copie `.env.example` para `.env`.
2. Suba a infra:

   ```bash
   docker-compose up -d
   ```

3. Espere o Postgres ficar pronto (uns 5 segundos; se a API cair na subida, rode de novo).
4. Terminal 1, o serviço Python:

   ```bash
   cd recomendacoes && python3 -m venv .venv && .venv/bin/pip install -r requirements.txt && .venv/bin/uvicorn main:app --port 8000
   ```

5. Terminal 2, a API (porta 5001, fixa no `launchSettings.json`):

   ```bash
   cd Loja.Api && dotnet run
   ```

6. Terminal 3, o front (porta 5010, aponta para `http://localhost:5001` no `appsettings.json`):

   ```bash
   cd Loja.Web && dotnet run
   ```

7. Abra http://localhost:5010.

Ou rode `./subir.sh`, que faz isso com `sleep` no meio.

## Quando dá errado

- Front na 5010 porque a 5000 no macOS é do AirPlay Receiver. Mudou a porta da API? Mude também no `appsettings.json` do front.
- A API subiu antes do Postgres? Ela morre no `EnsureCreated`. Rode de novo.
- Mudou a senha no `.env`? Mude também no `appsettings.json` da API.
- Pedido deu erro 502? Olhe o terminal do Python. Depois o da API. Depois o do front. Nada liga um ao outro.

#!/usr/bin/env bash
# "Script do time": sobe tudo em ordem. Funciona na maioria das vezes.
set -e
cd "$(dirname "$0")"

[ -f .env ] || cp .env.example .env
docker-compose up -d
echo "esperando o postgres..."; sleep 5

(cd recomendacoes && python3 -m venv .venv >/dev/null && .venv/bin/pip install -q -r requirements.txt && .venv/bin/uvicorn main:app --port 8000) &
(cd Loja.Api && dotnet run) &
sleep 8
(cd Loja.Web && dotnet run) &

echo "Web: http://localhost:5010  API: http://localhost:5001  Python: http://localhost:8000"
echo "Logs misturados aqui. Ctrl+C mata só o que der."
wait

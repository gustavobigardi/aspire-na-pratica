"""Serviço de recomendações: "quem comprou X também levou Y".

Propositalmente simples. A regra vive num dicionário em memória e o produto 13
não está nele. Sem trace distribuído, o erro aparece só no terminal deste processo.
"""
import logging

from fastapi import FastAPI

logging.basicConfig(level=logging.INFO)
app = FastAPI(title="recomendacoes")
log = logging.getLogger("recomendacoes")

CATALOGO = {
    1: "Teclado mecânico",
    2: "Mouse sem fio",
    3: 'Monitor 27" 4K',
    4: "Headset com cancelamento de ruído",
    5: "Webcam 1080p",
    6: "Hub USB-C",
    7: "Cadeira ergonômica",
    8: "Suporte para notebook",
    13: "Caneca MVP Conf 2026",
}

# Quem comprou a chave também levou os valores.
REGRAS = {
    1: [2, 8],
    2: [1, 6],
    3: [6, 8],
    4: [5, 2],
    5: [4, 6],
    6: [3, 5],
    7: [8, 3],
    8: [7, 1],
    # 13 ficou de fora quando a caneca entrou no catálogo.
}


@app.get("/health")
async def health():
    return {"status": "Healthy"}


@app.get("/recomendacoes/{produto_id}")
async def recomendacoes(produto_id: int):
    log.info("Calculando recomendações para o produto %s", produto_id)
    relacionados = REGRAS[produto_id]  # KeyError para produtos sem regra
    return [
        {
            "produtoId": rid,
            "nome": CATALOGO[rid],
            "motivo": f"Quem levou {CATALOGO[produto_id]} também levou",
        }
        for rid in relacionados
    ]

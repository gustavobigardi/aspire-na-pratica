"""OpenTelemetry para o dashboard do Aspire.

O AppHost injeta OTEL_EXPORTER_OTLP_ENDPOINT, OTEL_EXPORTER_OTLP_PROTOCOL,
OTEL_EXPORTER_OTLP_HEADERS e OTEL_SERVICE_NAME. Os exporters leem essas
variáveis sozinhos, então aqui só ligamos os provedores.
"""
import logging
import os

from opentelemetry import metrics, trace
from opentelemetry._logs import set_logger_provider
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter
from opentelemetry.exporter.otlp.proto.grpc.metric_exporter import OTLPMetricExporter
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.instrumentation.fastapi import FastAPIInstrumentor
from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry.sdk.metrics import MeterProvider
from opentelemetry.sdk.metrics.export import PeriodicExportingMetricReader
from opentelemetry.sdk.resources import Resource
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor


def configurar(app) -> None:
    if not os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT"):
        logging.basicConfig(level=logging.INFO)
        return  # rodando sem Aspire: só log no console

    resource = Resource.create({"service.name": os.getenv("OTEL_SERVICE_NAME", "recomendacoes")})

    tracer_provider = TracerProvider(resource=resource)
    tracer_provider.add_span_processor(BatchSpanProcessor(OTLPSpanExporter()))
    trace.set_tracer_provider(tracer_provider)

    meter_provider = MeterProvider(
        resource=resource,
        metric_readers=[PeriodicExportingMetricReader(OTLPMetricExporter())],
    )
    metrics.set_meter_provider(meter_provider)

    logger_provider = LoggerProvider(resource=resource)
    logger_provider.add_log_record_processor(BatchLogRecordProcessor(OTLPLogExporter()))
    set_logger_provider(logger_provider)
    logging.basicConfig(
        level=logging.INFO,
        handlers=[LoggingHandler(logger_provider=logger_provider), logging.StreamHandler()],
    )

    FastAPIInstrumentor.instrument_app(app)

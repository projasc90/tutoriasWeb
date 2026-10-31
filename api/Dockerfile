# ── AuraLearn API — imagen de producción (Railway / cualquier host de contenedores) ──
# Multi-stage: build con SDK 10, runtime con aspnet 10 (imagen más pequeña).
# Secretos: nunca en la imagen; se inyectan por variables de entorno del host
# (ConnectionStrings__Default, Jwt__Key, Cors__Origins, Storage__DocumentsRoot).

# ── Stage 1: build ─────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj primero para aprovechar la caché de capas de Docker
COPY AuraLearn.Api/AuraLearn.Api.csproj AuraLearn.Api/
COPY AuraLearn.Application/AuraLearn.Application.csproj AuraLearn.Application/
COPY AuraLearn.Domain/AuraLearn.Domain.csproj AuraLearn.Domain/
COPY AuraLearn.Infrastructure/AuraLearn.Infrastructure.csproj AuraLearn.Infrastructure/
RUN dotnet restore AuraLearn.Api/AuraLearn.Api.csproj

# Copiar el resto del código y publicar
COPY AuraLearn.Api/ AuraLearn.Api/
COPY AuraLearn.Application/ AuraLearn.Application/
COPY AuraLearn.Domain/ AuraLearn.Domain/
COPY AuraLearn.Infrastructure/ AuraLearn.Infrastructure/
RUN dotnet publish AuraLearn.Api/AuraLearn.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 2: runtime ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Usuario no-root: puerto 80 es privilegiado, por eso el servicio escucha en
# 8080 y el proyecto de Vercel debe definir PORT=8080 (ADR-009).
RUN addgroup --system --gid 1001 auralearn \
    && adduser --system --uid 1001 --ingroup auralearn auralearn

WORKDIR /app
COPY --from=build /app/publish .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

USER auralearn

# El entrypoint fija ASPNETCORE_URLS=http://0.0.0.0:$PORT (default 8080).
# Los archivos NO se guardan en disco: producción usa Vercel Blob (ADR-009).
# Puerto: Vercel enruta a 80 por defecto y la base ASP.NET escucha en 8080
# (ASPNETCORE_HTTP_PORTS). Por eso el proyecto de Vercel debe definir
# PORT=8080 — sin eso todas las rutas devuelven 502. EXPOSE es solo
# documental: no influye en el routing del host.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    PORT=8080
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]

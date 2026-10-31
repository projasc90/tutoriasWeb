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

# Usuario no-root (requisito de seguridad; Railway lo soporta)
RUN addgroup --system --gid 1001 auralearn \
    && adduser --system --uid 1001 --ingroup auralearn auralearn

WORKDIR /app
COPY --from=build /app/publish .
COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

# Carpeta de documentos (montar volumen en el host: /data/documents)
RUN mkdir -p /data/documents && chown -R auralearn:auralearn /data /app

USER auralearn

# Railway inyecta PORT; el entrypoint lo respeta vía ASPNETCORE_URLS.
# El script de entrada permite aplicar migraciones de forma explícita
# (APPLY_MIGRATIONS=true) antes de arrancar el servidor.
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]

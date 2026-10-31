#!/bin/sh
# Entrypoint del contenedor de la API AuraLearn.
# - APPLY_MIGRATIONS=true → aplica migraciones EF (one-shot) y termina.
#   Lo usa el job `migrate` de .github/workflows/api-ci.yml, no el servicio
#   desplegado: Vercel no permite sobrescribir el comando de arranque, así que
#   el contenedor de producción NUNCA define esta variable (ADR-009).
# - En caso contrario arranca la API escuchando en $PORT (default 8080;
#   el proyecto de Vercel debe definir PORT=8080).
set -e

if [ "$APPLY_MIGRATIONS" = "true" ]; then
  echo "[auralearn] Aplicando migraciones..."
  export ApplyMigrations=true
  exec dotnet AuraLearn.Api.dll
fi

export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"
exec dotnet AuraLearn.Api.dll

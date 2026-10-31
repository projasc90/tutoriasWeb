#!/bin/sh
# Entrypoint del contenedor de la API AuraLearn.
# - APPLY_MIGRATIONS=true → aplica migraciones EF (one-shot) y termina.
#   El orquestador (Railway/CI) lanza un contenedor efímero con esta variable
#   antes del deploy; el contenedor del servidor NUNCA la define.
# - En caso contrario arranca la API escuchando en $PORT (default 8080).
set -e

if [ "$APPLY_MIGRATIONS" = "true" ]; then
  echo "[auralearn] Aplicando migraciones..."
  export ApplyMigrations=true
  exec dotnet AuraLearn.Api.dll
fi

export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"
exec dotnet AuraLearn.Api.dll

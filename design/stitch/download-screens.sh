#!/usr/bin/env bash
# Descarga las 4 pantallas del proyecto Stitch "Tutor Marketplace Booking Platform"
# (ID: 12227662788794145381) a design/stitch/screens/.
#
# Requiere: export STITCH_API_KEY="<clave>"
# Uso:      bash design/stitch/download-screens.sh
set -euo pipefail

PROJECT_ID="12227662788794145381"
OUT_DIR="$(cd "$(dirname "$0")" && pwd)/screens"
mkdir -p "$OUT_DIR"

# nombre-de-archivo:id-de-pantalla
SCREENS=(
  "01-directorio:a5733c072d9542b2a484294d22114756"
  "02-perfil-reserva:fb98f0bbbd49460eaa0756d68237c2b3"
  "03-checkout:999e7d49c050445f820c11818baba797"
  "04-mis-tutorias:acc1944b62a44b5aa1e86bc8abb0742e"
)

if [[ -z "${STITCH_API_KEY:-}" ]]; then
  echo "ERROR: STITCH_API_KEY no está definida. Exporta la clave antes de ejecutar." >&2
  exit 1
fi

for entry in "${SCREENS[@]}"; do
  name="${entry%%:*}"
  screen_id="${entry##*:}"
  echo "→ Pantalla ${name} (${screen_id})"

  payload=$(printf '{"jsonrpc":"2.0","id":1,"method":"tools/call","params":{"name":"get_screen","arguments":{"name":"projects/%s/screens/%s"}}}' "$PROJECT_ID" "$screen_id")

  resp=$(curl -sS -X POST "https://stitch.googleapis.com/mcp" \
    -H "x-goog-api-key: ${STITCH_API_KEY}" \
    -H "Content-Type: application/json" \
    -d "$payload")

  html_url=$(echo "$resp" | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d["result"]["content"][0]["text"])' | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d["htmlCode"]["downloadUrl"])')
  png_url=$(echo "$resp" | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d["result"]["content"][0]["text"])' | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d["screenshot"]["downloadUrl"])')
  width=$(echo "$resp" | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d["result"]["content"][0]["text"])' | python3 -c 'import json,sys; d=json.load(sys.stdin); print(d.get("width") or 2560)')

  curl -sSL "$html_url" -o "${OUT_DIR}/${name}.html"
  curl -sSL "${png_url}=w${width}" -o "${OUT_DIR}/${name}.png"

  h=$(wc -c < "${OUT_DIR}/${name}.html" | tr -d ' ')
  p=$(wc -c < "${OUT_DIR}/${name}.png" | tr -d ' ')
  echo "  ${name}.html (${h} bytes) · ${name}.png (${p} bytes)"
  if [[ "$h" -lt 500 || "$p" -lt 5000 ]]; then
    echo "ERROR: archivo demasiado pequeño para ${name}" >&2
    exit 1
  fi
done

echo "DONE: 4 pantallas en ${OUT_DIR}"

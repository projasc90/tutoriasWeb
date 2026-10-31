# Verificación — Portal del tutor fase 2

**Fecha:** 2026-10-11 · **Base:** 0.8.0 → **0.9.0**

## Comandos ejecutados

| Gate | Comando | Resultado |
|------|---------|-----------|
| Backend tests | `cd api && dotnet test` | **112/112** (106 previos + 6 nuevos: 3 de reglas de documentos reescritas + 2 de normalización + 1 de acumulación de títulos) |
| Frontend lint | `cd web && npm run lint` | 0 errores (4 warnings preexistentes) |
| Frontend tipos | `cd web && npx tsc --noEmit` | OK |
| Frontend build | `cd web && npm run build` | **17 rutas** (nueva `/tutor/documentos`) |
| Migración | `dotnet ef database update` | `20261011185628_TutorDocumentsAccumulateTitles` aplicada (índice único parcial ahora `kind = 3` solo) |

## Smoke E2E API (curl)

1. `POST /api/tutor-applications` con `subjects: ["cálculo i","estadística y probabilidad"]` → 202; BD: `{"Cálculo I","Estadística y Probabilidad"}` ✓ (normalización backend)
2. Uploads iniciales: Titulo 201, Certificacion 201, Identificacion 201 ✓
3. Approve → 204 ✓
4. **Post-aprobación (Verified):** 2º título → **201** (acumula) ✓ · 2ª certificación → **201** ✓ · identificación → **409** `{"error":"IDENTIFICATION_LOCKED"}` ✓
5. BD tras post-aprobación: Titulo=2, Certificacion=2, Identificacion=1 ✓

## Smoke E2E navegador (Playwright)

1. Login como tutor Verified → `/tutor/documentos`: subnav con 4 pestañas (Perfil·Disponibilidad·Sesiones·**Documentos**), aviso "Tu perfil está verificado…", secciones Títulos 2/5 y Certificaciones 2/5, identificación con botón **"Verificada — bloqueada"** (disabled, candado) ✓
2. Subida de un 3er título desde la UI → aviso "Documento agregado. Queda disponible para el equipo de verificación." y contador **3/5** ✓
3. Sugerencia de materias: implementada en wizard y perfil (verificación visual pendiente en uso real; la lógica está cubierta por tests del backend y es espejo exacto en `lib/subjects.ts`).

## Limpieza

Usuarios/postulaciones/documentos del smoke eliminados (DELETE 6/1/2); `~/.auralearn/uploads/documents` y fixtures de `/tmp` borrados; servidores detenidos.

## Riesgos residuales

1. La sugerencia de materias usa el catálogo estático de 10 materias (gap P2.7: catálogo maestro en BD).
2. Los documentos agregados post-aprobación no disparan re-revisión (decisión de producto pendiente, gap P3.11).
3. El índice único parcial anterior (kind IN (1,3)) se reemplazó; el rollback manual de la migración está comentado en el archivo.

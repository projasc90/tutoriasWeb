# Verificación — Foto de perfil del tutor + X en franjas

**Fecha:** 2026-10-11 · **Base:** 0.9.0 → **0.10.0**

## Comandos ejecutados

| Gate | Comando | Resultado |
|------|---------|-----------|
| Backend tests | `cd api && dotnet test` | **121/121** (112 previos + 9 nuevos de `TutorPhotoServiceTests`) |
| Frontend lint | `cd web && npm run lint` | 0 errores (4 warnings preexistentes) |
| Frontend tipos | `cd web && npx tsc --noEmit` | OK (tras añadir `photoUrl` a `lib/types/tutor.ts` del directorio) |
| Frontend build | `cd web && npm run build` | 17 rutas |
| Migración | `dotnet ef database update` | `20261011210258_AddTutorPhoto` aplicada (`tutors.photo_path` varchar(500) nullable, rollback manual) |

## Smoke E2E API (curl)

1. Register + login + postular → 201/202 ✓
2. `POST /api/tutors/me/photo` (multipart PNG) → **200** `{"photoUrl":"/api/tutors/{id}/photo"}` ✓
3. `GET /api/tutors/{id}/photo` **sin autenticación** → **200** (PNG servido) ✓
4. `GET /api/tutors/{id}` → DTO con `photoUrl` poblado ✓

## Smoke E2E navegador (Playwright)

1. `/tutor/perfil`: tarjeta "Foto de perfil" con preview de la imagen subida y botón "Reemplazar foto" ✓
2. `/tutor/disponibilidad` (tutor promovido a Verified, solo dev): franja agregada con **X en la esquina superior derecha** — geometría verificada: botón (right 327, top 98) dentro de la tarjeta (right 333, top 92) ✓
3. `/tutores/{id}` (perfil público): `<img>` de la foto presente, sin fallback de iniciales ✓
4. El tutor `PendingReview` recibe 403 en disponibilidad (correcto por diseño; el rol del JWT se fija al login).

## Limpieza

Usuario/postulación del smoke eliminados (DELETE 1/1); archivos de foto borrados; fixtures de `/tmp` eliminados.

## Riesgos residuales

1. La foto es pública por diseño (no PII de verificación); sin moderación de contenido (fuera de alcance).
2. Sin recorte/edición de imagen; el navegador escala con `object-cover`.
3. La foto no se pide en el wizard `/postular` (decisión: solo desde el portal).

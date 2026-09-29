# Verificación — Onboarding del Profesor

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-28-tutor-onboarding-plan.md)

## Testing gate

| Gate | Comando | Resultado |
|------|---------|-----------|
| Backend build | `cd api && dotnet build` | ✅ 0 errores, 0 advertencias |
| Backend test | `cd api && dotnet test` | ✅ **25/25** (18 nuevos de `TutorApplicationServiceTests` + 7 de auth + validadores) |
| Frontend lint | `cd web && npm run lint` | ✅ 0 errores (3 warnings preexistentes) |
| Frontend tsc | `cd web && npx tsc --noEmit` | ✅ limpio |
| Frontend build | `cd web && npm run build` | ✅ ruta `/postular` generada (static) |

## TDD

- `TutorApplicationServiceTests.cs` (18 tests) escrito **antes** de `TutorApplicationService`.
- Cobertura: submit válido (PendingReview), conflicto con postulación activa, re-postulación tras rechazo, request inválido, usuario inexistente, status found/not-found, listado pendiente, approve→Verified, reject sin motivo→Invalid, reject con motivo→Rejected+motivo, id inexistente, decisión inválida.

## Migración

- `dotnet ef database update` aplicado contra `auralearn_dev`: columnas `user_id`/`rejection_reason`, índice unique `ix_tutors_user_id`, FK `fk_tutors_users_user_id` (cascade). Historia `20260929070000_AddTutorUserId` registrada.
- Escrita a mano (sin scaffold) por falta de terminal en la fase de código; Designer + Snapshot actualizados a mano y validados por el build + migración real.

## Smoke E2E (19 verificaciones, todas verdes)

| # | Verificación | Esperado | Obtenido |
|---|--------------|----------|----------|
| 1 | Registro profesor | 201 | ✅ 201 |
| 2 | Status sin postulación | 404 | ✅ 404 |
| 3 | Enviar postulación | 202 PendingReview | ✅ 202 |
| 4 | Status con postulación | 200 PendingReview | ✅ 200 |
| 5 | Reenvío duplicado | 409 | ✅ 409 |
| 6 | Catálogo no muestra pendiente | totalCount 0 | ✅ 0 |
| 7 | Admin sin token | 401 | ✅ 401 |
| 8 | Login tras promover rol en BD | 200 | ✅ 200 |
| 9 | Cola de pendientes (Admin) | 1 item | ✅ 1 |
| 10 | Aprobar | 204 | ✅ 204 |
| 11 | Catálogo muestra aprobado | totalCount 1 | ✅ 1 |
| 12 | Decisión inválida | 400 | ✅ 400 |
| 13 | Id inexistente | 404 | ✅ 404 |
| 14 | 2º profesor registra y postula | 201/202 | ✅ |
| 15 | Token viejo (rol pre-promoción) en admin | 403 | ✅ 403 (claim inmutable, correcto) |
| 16 | Rechazo sin motivo | 400 | ✅ 400 |
| 17 | Rechazo con motivo | 204 + motivo visible al profesor | ✅ |
| 18 | Re-postulación tras rechazo | 202 | ✅ 202 |
| 19 | Estado vuelve a PendingReview | 200 | ✅ 200 |

## Limpieza

- Datos del smoke eliminados de `auralearn_dev` (2 usuarios + 2 postulaciones). Catálogo de vuelta a 12 tutores seed.
- API de smoke detenida; scripts temporales en `/tmp` (fuera del repo).

## Riesgos residuales

- `AuraLearn.Api.http` sigue obsoleto (template weatherforecast) — deuda menor.
- Sin tests de integración HTTP (Testcontainers) — la capa Api/Controller no está cubierta por tests automatizados, solo smoke manual.
- El wizard usa `localStorage` para la sesión (sin expiración ni refresh) — deuda de seguridad ya registrada en el plan de auth.

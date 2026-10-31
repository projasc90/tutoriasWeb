---
id: current-state
priority: 1
loadWhen: always
maxLines: 60
---

# Estado Actual — AuraLearn

**Versión backend:** solución `AuraLearn.slnx` .NET 10 · **Versión frontend:** `0.12.1` (SEMVER en `web/package.json`)

## Completado recientemente

- **Contenedores + despliegue** (`docs/plans/2026-10-16-deploy-containers/`, 0.12.1, ADR-008): `api/Dockerfile` multi-stage (SDK 10 → aspnet 10, no-root, `entrypoint.sh` con PORT dinámico y migraciones opt-in `APPLY_MIGRATIONS=true`), `api/.dockerignore`, `docker-compose.yml` local (api+postgres 16, healthcheck, volúmenes); `Program.cs`: CORS por entorno (`Cors:Origins` con `;`) + PORT dinámico + modo migraciones one-shot; CI/CD: `.github/workflows/api-deploy.yml` (build+tests→GHCR→migraciones→redeploy Railway) y `web-ci.yml` (gate lint/tsc/build; deploy nativo Vercel); `web/vercel.json` + `.env.example` actualizado. **Topología: API→Railway (contenedor), BD→Neon, web→Vercel** (Vercel no soporta .NET/contenedores — aclarado al usuario). Gates verdes (149 tests, lint 0 errores, tsc, YAML válidos); PORT dinámico verificado localmente (200 en /health con PORT=5099). Docker Desktop no instalado en la Mac (sin brew; usuario optó por saltar la verificación local del build — Railway lo validará al conectar el repo). Guía paso a paso en `deploy-guide.md`.

- **Panel estudiante + login en reserva + admin de estudiantes** (`docs/plans/2026-10-16-student-panel/`, 0.12.0): panel del estudiante en `/estudiante/{reservas,monedero}` con subnav (contenido migrado de `/mis-tutorias`, hoy redirect); login/registro con `?redirect=` de retorno (anti open-redirect) y CTA "Inicia sesión para reservar" en `/tutores/[id]` que preserva el slot elegido (`&slot=`) y lo preselecciona al volver; reservas restringidas a rol Estudiante (`Create/SubmitComprobante/Cancel` 403 para otros); `GET /api/admin/users` `[Admin]` (filtro rol + búsqueda + paginado) + página `/admin/estudiantes`; landing Mentors con API real (client component, fetch en runtime; mock `MENTORS` archivado en `.ai/archive/`); nav corregido ("Para Universidades" eliminado, Logo → `/`). Tests **149/149** (5 nuevos); gates verdes (21 rutas). Smoke E2E en navegador verde (flujo completo sin login → login → retorno con slot → reserva → cancelación; admin lista/filtra/busca).
- **Reglas de reserva del Admin** (`docs/plans/2026-10-12-booking-rules/`, 0.11.0): tabla `booking_rules` (fila única, 4 reglas **opcionales** + auditoría; seed 12/60/60/14), `GET /api/booking-rules` público + `PUT /api/admin/booking-rules` `[Admin]` (422 `BOOKING_RULES_INVALID_*`), **nueva página `/admin/configuracion`** (input vacío = sin definir). Las reglas **gobiernan la lógica**: cancelación (fallback 12 h), ventana de pago SINPE (fallback 60 min), horizonte ensure-ahead (fallback 14 días); `slotDurationMinutes` persistida pero sin efecto (deuda). `/tutor/disponibilidad` muestra las tarjetas desde BD (regla sin definir → no se muestra, nota "solo lectura"); checkout lee la regla de BD. Smoke curl + navegador verdes.
- **Foto de perfil + X en franjas** (`2026-10-11-tutor-photo-ui/`, 0.10.0): `tutors.photo_path`, `TutorPhotoService`, `POST me/photo` + `GET {id}/photo` público, `ProfilePhotoCard`, X en franjas. Tests 121/121.
- **Portal del tutor fase 2** (`2026-10-11-tutor-portal-phase2/`, 0.9.0): títulos acumulables (máx 5), post-aprobación agrega títulos/certs (ID bloqueada 409), materias normalizadas, `/tutor/documentos`.
- **Sección del tutor completa** (0.7.0) · **Cierre de reservas** (0.6.0) · **Motor de slots + SINPE** (0.5.0) · **Panel admin** (0.4.2) · **Onboarding tutor** (0.4.0) · **Auth TDD** (0.3.0) · **Backend .NET 10** (ADR-003) · conexión frontend · mocks extraídos · gobernanza (ADR-001/002).

## Pendientes inmediatos

- **P1 del análisis de gaps** (`docs/plans/2026-10-11-tutor-portal-phase2/gaps-analysis.md`): canal de sesión (link videollamada/sala), pizarra digital realtime, notificaciones email, vista de ganancias del tutor (`TutorPayout` existe, sin UI).
- Efecto real de `slotDurationMinutes` (ADR: granularidad de franjas + índices + UI); caché de reglas; FAQ landing estática (marketing).
- Catálogo maestro de materias en BD; re-verificación de docs post-aprobación; "completar documentos" desde "en revisión" (deuda 0.8.0).
- Eliminar usuarios Admin de prueba `admin.test@auralearn.dev` y `admin-smoke-2@test.dev` (creados para smokes).
- Historial de movimientos del monedero (`GET /api/wallet/movements`) — fuera de alcance en 0.12.0 por decisión del usuario.
- **Despliegue real pendiente (manual del usuario):** crear proyecto Neon, conectar repo en Railway (root `api/`) con variables de `deploy-guide.md`, conectar repo en Vercel (root `web/`) con `NEXT_PUBLIC_API_URL`, añadir dominio Vercel a `Cors__Origins`, secrets `RAILWAY_TOKEN`/`RAILWAY_SERVICE` en GitHub. Verificar build Docker (no verificado localmente: sin Docker en la Mac).
- Migrar `IFileStorage` a object storage (S3/R2/Blob) si se necesita durabilidad multi-instancia (volumen Railway es local al servicio).
- Firma digital SINPE real (requiere API del banco; ADR-004 manual vigente).
- Test de concurrencia contra Postgres real (Testcontainers sin configurar) para débito y doble-booking.
- Refresh tokens + rate limiting en login; ADR cookies httpOnly vs localStorage.
- Pizarra digital realtime; notificaciones email; dashboard "Resumen" del panel tutor; reseñas reales; Playwright E2E permanente; Docker/compose; portal público `portal/`.

## Contexto rápido de arquitectura

- **Flujo de reserva:** `/tutores/[id]` (público; sin sesión → CTA `/login?redirect=...&slot=` con retorno y preselección) → slots (ensure-ahead con horizonte de `booking_rules`) → `useReservation.create` (solo rol Estudiante) → `/checkout/{id}` (SINPE con ventana de `booking_rules`, polling 3 s) → comprobante → `/admin/pagos` → approve/reject → `Confirmed` → tutor completa tras `EndAt` (liquidación `TutorPayout`); cancelar ≥ anticipación de `booking_rules` acredita `CancellationCredit`.
- **Reglas de reserva:** `booking_rules` fila única (id `00000000-...-0001`), 4 reglas nullable; el Admin las edita en `/admin/configuracion`; fallback `BookingRulesDefaults` en Application; el tutor las ve en `/tutor/disponibilidad` (solo lectura).
- **Ciclo de vida del tutor:** `/postular` (wizard 5 pasos) → `PendingReview` + docs → admin decide en `/admin/tutores` (gate 409) → approve promueve rol + `Verified` → tutor edita `/tutor/perfil` y `/tutor/disponibilidad`; rechazo → motivo + re-postulación.
- **Documentos:** PII — solo Admin descarga; tutor ve metadatos; identificación única (upsert) y bloqueada tras `Verified` (409); archivos nunca en BD ni repo; materias normalizadas (`SubjectNormalizer`).
- **Exclusividad del slot:** índice único parcial `(slot_id) WHERE status IN (1,2)`; expiración perezosa (`TryExpireIfDueAsync`).
- **Disponibilidad:** franjas en `tutor_availability` (zona del tutor, recurrentes/ancla, excepciones); sin reglas → default CR 08-20 con comida 12-13.
- **Tiempo:** UTC en BD (`timestamptz`); `America/Costa_Rica` solo en generación y presentación (`lib/time.ts`). Alinear rangos por fecha local CR (GOTCHA-005).
- **Roles y auth:** JWT claims `sub/email/role/name`, exp 8 h; `[Authorize(Roles=…)]`; el rol del JWT se fija al login (promoción exige re-login). El menú del header usa el rol de la sesión (`useAuth`).
- **Backend (`api/`):** `Api` → `Application` → `Domain`; `Infrastructure` (DbContext Npgsql, repos, migraciones con rollback manual). BD dev `auralearn_dev` (Postgres local 5432) / prod **Neon** por `ConnectionStrings__Default`. Contenedor: `api/Dockerfile` + `entrypoint.sh` (PORT dinámico, migraciones opt-in); compose local en la raíz. Storage local: `~/.auralearn/uploads/documents` (dev) / `/data/documents` (Railway con volumen).
- **Frontend (`web/`):** Next.js 16.3.6 App Router + React 19 + TS 5 + Tailwind 4 + base-nova. 21 rutas. Tokens MD3 en `globals.css`; fuente Plus Jakarta Sans. Panel estudiante en `/estudiante/*` (layout + `StudentSubnav`); `/mis-tutorias` es redirect de compatibilidad; landing Mentors consume `GET /api/tutors` (client, runtime).

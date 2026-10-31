---
id: version-changes
priority: 4
loadWhen: bump-de-version-o-cierre-de-tarea
maxLines: 70
---

# Version Changes — AuraLearn

Changelog por versión. Clasificación permitida: `feat`, `fix`, `refactor`, `db`, `test`, `docs`.
> Histórico antiguo NO es carga automática (ver `CONTEXT_BUDGET.md`). Mantener detallada solo la versión actual.

## v0.13.0 — 2026-10-30

- **fix(docker):** el primer build en Vercel Container fallaba con `addgroup: not found` (exit 127). `adduser`/`addgroup` son del paquete Debian `adduser`, ausente en las imágenes slim. Ahora se usa `USER app` (UID/GID 1654, incluido en las imágenes oficiales de .NET 8+, creado con `groupadd`/`useradd` en el template oficial) y `COPY --chown=app:app`; se elimina la creación manual del usuario. GOTCHA-013.

- **feat(infra):** API .NET desplegada en **Vercel Container Runtime** y archivos en **Vercel Blob** (ADR-009, supersede ADR-007/008). Plan en `docs/plans/2026-10-30-vercel-container/`.
  - **Corrección de premisa:** ADR-008 afirmaba que Vercel no soporta contenedores ni .NET. Vercel lanzó Container Runtime (imágenes OCI desde `Dockerfile`, Beta en todos los planes) y publicó guía oficial para ASP.NET Core. Railway deja de ser necesario.
  - **Puerto:** `ENV PORT=8080` en `api/Dockerfile` + variable `PORT` en el proyecto Vercel. Vercel enruta a 80 y la base ASP.NET escucha en 8080 → sin esto, **502 en todas las rutas**. `EXPOSE` no influye en el routing.
  - **Binding único:** `Program.cs` solo hace `app.Urls.Add` si `ASPNETCORE_URLS` no está definido (antes podía registrar el binding dos veces junto al `entrypoint.sh`).
  - **`VercelBlobFileStorage`** (`Infrastructure/Persistence`): implementa `IFileStorage` contra la REST API de Blob (v13) con `HttpClient`. Sin SDK oficial .NET, sin dependencias nuevas. Store **privado**: la lectura pasa por el API, así la autorización de PII sigue en .NET.
  - **`IFileStorage.GetInfoAsync`** (nuevo): metadatos reales del archivo en el store; también implementado en `LocalFileStorage` para mantener paridad dev/prod.
  - **Selección en DI:** `CreateFileStorage` usa Blob si hay `BLOB_READ_WRITE_TOKEN` o `Storage:Provider=VercelBlob`; falla al arrancar si se fuerza Blob sin token (nunca degrada a disco efímero).
  - **Subida directa (rompe el límite de 4.5 MB):** nueva ruta `web/app/api/blob/upload` con `@vercel/blob/client` que emite tokens de cliente. `onBeforeGenerateToken` verifica el JWT contra `GET /api/auth/me` y exige que el pathname cuelgue del usuario autenticado. Nuevos endpoints `POST /api/tutors/me/documents/attach` y `POST /api/tutors/me/photo/attach` que validan tamaño/tipo contra los metadatos reales del store. `uploadTutorPhoto` cae al multipart clásico si no hay store (dev local).
  - **Migraciones:** `api-deploy.yml` deja de publicar imágenes a GHCR y de desplegar a Railway; ahora es `api-ci` (build+tests en PR; build+tests+job `migrate` one-shot contra Neon en `main`). Secret `NEON_CONNECTION_STRING` reemplaza a `RAILWAY_TOKEN`/`RAILWAY_SERVICE`.
  - **Docs:** `infra.instructions.md` reescrito para la nueva topología; nueva guía `docs/plans/2026-10-30-vercel-container/deploy-guide.md`; la guía de Railway queda marcada como superada.
  - **Health checks separados:** `/health` pasó a ser *readiness* real (proceso + PostgreSQL vía `DatabaseHealthCheck` con `CanConnectAsync`) y `/health/live` es *liveness* sin dependencias. Antes `AddHealthChecks()` no registraba ningún check: respondía `Healthy` aunque Neon estuviera caído. Sin paquete nuevo (no se añadió `AspNetCore.HealthChecks.NpgSql`). Verificado local: con BD → `Healthy` 200 en ambas; con BD caída → `/health` `Unhealthy` 503 y `/health/live` sigue `Healthy` 200.
  - **Tests:** **158/158** (9 nuevos del camino `attach`: pathname propio, pathname ajeno rechazado, tamaño declarado ≠ real, content-type no permitido, blob inexistente, identificación bloqueada). Gates verdes: `dotnet build` sin warnings, `lint` 0 errores, `tsc --noEmit` limpio, `npm run build` con 22 rutas.
  - **Validación contra el store real** (la parte sin tests unitarios): contrato REST verificado con el store privado — `PUT /?pathname=` 200 con pathname exacto, `GET /?url=` 200 con `size`/`contentType`, inexistente → 404, `POST /delete` 200. Flujo E2E con `upload()` del SDK: sonda `enabled:true`; sin JWT → 400 `No autenticado`; JWT propio → 200 con `clientToken`; pathname ajeno → 400; categoría inválida → 400; **subida de 4.8 MB (5033165 B) completa** y `head()` confirma tamaño y tipo.
  - **Corrección de un supuesto:** un archivo de 6 MB **no** debe subir (tope de negocio 5 MB aplicado por `maximumSizeInBytes` del token de cliente). El rango que se desbloquea es **4.5–5 MB**, no "cualquier tamaño". Registrado como GOTCHA-012.
  - **Alineación de categorías:** la ruta de tokens esperaba `documents`/`photo`, pero el .NET genera `titulo`/`certificacion`/`identificacion`/`photo`. Corregido en `ALLOWED_CONTENT_TYPES` y en `buildBlobPathname`, para que ambos lados produzcan el mismo pathname.

## v0.12.2 — 2026-10-30

- **fix:** navegación del `SiteHeader` (menú de la landing) no routeaba correctamente.
  - **`#catalogo` frágil:** el ancla vivía en el grid de tutores de `Mentors`, que solo se renderiza con `state === "success"` y con tutores; con API caída o catálogo vacío el enlace no hacía nada. Ahora el `id` está en la `<section>` de los tres estados.
  - **`#como-funciona` mal ubicado:** estaba en la `SearchCard` del hero (arriba), no en `Steps`. Se movió a la sección de pasos; la tarjeta quedó como `id="buscador"`.
  - **Header reutilizado fuera de la portada:** `SiteHeader` se usa en todo el sitio (`/tutores`, paneles, checkout, `/postular`), donde los hash de la landing no existen. `NAV_ITEMS` separa `href` (ruta real) de `hash`, y los hash se resuelven a `/#seccion` cuando `usePathname() !== "/"`.
  - **"Buscar Tutores" y CTA "Reservar un Tutor" → `/tutores`** (directorio real) en lugar del grid de destacados; lo mismo en el CTA del hero, el CTA final y "Ver todas las 140+ materias" (`/tutores`).
  - **Navegación cliente:** `<a>` interno → `Link` en header (nav, login, CTA, `PanelMenu`), `disciplines`, `final-cta`, `tutor-cta` y footer (elimina recargas completas).
  - **`scroll-mt-24`** en las secciones ancla para que el header sticky no tape el título (verificado: top ≈ 96 px).
  - **Footer sin enlaces muertos:** los ítems con `href="#"` (que saltaban al inicio) se renderizan como texto; solo "Buscar Tutores", "Materias Populares" y "Publicar tu Perfil" navegan.
  - **Gates:** lint 0 errores (10 warnings preexistentes), `tsc --noEmit` limpio. Smoke Playwright en `:3001`: anclas (#como-funciona/#disciplinas → top 95.75), `/tutores`, `/postular`, y hash desde página interna (`/#como-funciona`) verdes.

## v0.12.1 — 2026-10-16

- **feat:** contenedores para desplegar la API + frontend en Vercel. Plan `docs/plans/2026-10-16-deploy-containers/`, ADR-008.
  - **Topología (decisión del usuario):** API .NET en contenedor → **Railway** (Vercel no soporta .NET/contenedores) · BD → **Neon** · frontend → **Vercel** · CI/CD → GitHub Actions.
  - **Backend:** `api/Dockerfile` multi-stage (SDK 10 build → aspnet 10 runtime, usuario no-root, imagen sin secretos), `api/entrypoint.sh` (PORT dinámico de Railway; migraciones opt-in `APPLY_MIGRATIONS=true` one-shot), `api/.dockerignore`; `Program.cs`: CORS por entorno (`Cors:Origins` separado por `;`, fallback localhost en dev), PORT dinámico (verificado: 200 en `/health` con `PORT=5099`), modo migraciones one-shot (`ApplyMigrations=true`).
  - **Local:** `docker-compose.yml` (api + postgres 16-alpine, healthcheck, volúmenes pgdata/documents, puerto API 8080).
  - **CI/CD:** `.github/workflows/api-deploy.yml` (PR: build+tests; main: build+tests → imagen a GHCR → migraciones one-shot → redeploy Railway; secrets `RAILWAY_TOKEN`/`RAILWAY_SERVICE`) y `.github/workflows/web-ci.yml` (lint+tsc+build; deploy nativo de Vercel).
  - **Frontend:** `web/vercel.json` (framework nextjs, install `npm ci`), `.env.example` documentado para Vercel (`NEXT_PUBLIC_API_URL`).
  - **Docs:** guía paso a paso Railway+Neon+Vercel en `deploy-guide.md`; `infra.instructions.md` actualizado a la topología real; `.ai/STACK.md` actualizado.
  - **Gates:** 149/149 tests, lint 0 errores, tsc limpio, 3 YAML válidos. **Deuda explícita:** build Docker no verificado localmente (sin Docker/brew en la Mac; Railway lo validará al conectar el repo); despliegue real pendiente (manual, pasos en `deploy-guide.md`); `IFileStorage` → object storage si se requiere durabilidad multi-instancia.

## v0.12.0 — 2026-10-16

- **feat:** panel del estudiante + login exigido al reservar (con retorno al flujo) + gestión de estudiantes en el panel Admin. Plan `docs/plans/2026-10-16-student-panel/`.
  - **Backend:** `[Authorize(Roles = "Estudiante")]` en `Create`/`SubmitComprobante`/`Cancel` (solo estudiantes reservan); `GET /api/admin/users` `[Admin]` (filtro `role`, búsqueda `query` sobre Email/FullName, paginado; 400 rol inválido) con `AdminUserDto`, `AdminUserSearchCriteria` + validador, `AdminUserService`, `IUserRepository.SearchAsync` + EF. Sin migración. DI en `Program.cs`.
  - **Frontend:** `lib/redirect.ts` (redirect interno seguro); `/login` y `/registro` con `?redirect=` de retorno; CTA "Inicia sesión para reservar" en `/tutores/[id]` (banner + Link con `&slot=` preservado, preselección al volver); **panel estudiante** `/estudiante/{reservas,monedero}` (layout + `StudentSubnav`, guard por rol); `/mis-tutorias` → redirect; `/panel` y `PanelMenu` actualizados; **`/admin/estudiantes`** (búsqueda con debounce, filtro rol, paginación); landing Mentors con `GET /api/tutors` real (client, runtime; "Ver Perfil" navega; mock archivado); nav: "Para Universidades" eliminado, Logo → `/`.
  - **Tests:** 149/149 (5 nuevos). **Gates:** verdes (21 rutas). **Smoke E2E navegador verde** (flujo completo sin login → login → retorno con slot → reserva → cancelación; admin filtra/busca).
  - **Deuda explícita:** historial de movimientos del monedero (`GET /api/wallet/movements`, fuera de alcance); claim `email` sin mapear en `/api/auth/me` (preexistente).

## v0.11.0 — 2026-10-12

- **feat:** reglas de reserva definidas por el Admin (booking rules en BD). Plan `docs/plans/2026-10-12-booking-rules/`.
  - **Backend:** entidad `BookingRules` (fila única, 4 reglas nullable + auditoría `updated_at`/`updated_by_user_id` sin FK), migración `AddBookingRules` (seed 12/60/60/14, rollback manual), `IBookingRulesRepository`/`BookingRulesRepository` (upsert), `BookingRulesService` + `UpdateBookingRulesValidator` (rangos por campo), `GET /api/booking-rules` público y `PUT /api/admin/booking-rules` `[Admin]` con errores `BOOKING_RULES_INVALID_*`.
  - **Lógica gobernada por BD:** cancelación usa la anticipación de BD (fallback 12 h), ventana de pago SINPE en `CreateAsync` (fallback 60 min), horizonte ensure-ahead en `TutorService`/`SlotsController` (fallback 14 días). `slotDurationMinutes` persistida pero sin efecto (deuda: granularidad de franjas + índices).
  - **Frontend:** `BookingRules`/`BOOKING_RULES_DEFAULTS`/`fetchBookingRules`/`updateBookingRules` en `api.ts`, hook `use-booking-rules`, tarjetas dinámicas en `/tutor/disponibilidad` (regla sin definir → no se muestra; nota "solo lectura"), **nueva página `/admin/configuracion`** + `BookingRulesForm` (vacío = sin definir), enlace en menú Admin, `mis-tutorias`/checkout leen la regla de BD (FAQ queda estática).
  - **Tests:** 144/144 (23 nuevos TDD). **Gates:** verdes (18 rutas). Smoke curl (GET 200, PUT Admin 200, PUT Estudiante 403, 422 con código) y navegador verdes.
  - **Deuda explícita:** efecto real de duración de slot (ADR futuro), caché de reglas, FAQ estática; usuario Admin de prueba `admin.test@auralearn.dev` creado para el smoke (eliminar).

## v0.10.0 — 2026-10-11

- **feat:** foto de perfil del tutor + X en franjas de disponibilidad. Plan `docs/plans/2026-10-11-tutor-photo-ui/`.
  - **Backend:** `tutors.photo_path` (migración `AddTutorPhoto` + rollback manual), `IFileStorage.SavePhotoAsync` (carpeta `{tutorId}/photo/`, nombre generado por el servidor), `TutorPhotoService` (JPEG/PNG/WebP ≤5 MB, upsert borra previo), `POST /api/tutors/me/photo` (`[Authorize]`) y `GET /api/tutors/{id}/photo` **público** (la foto no es PII de verificación); `photoUrl` en `TutorDto` y `TutorProfileResponse`.
  - **Frontend:** `ProfilePhotoCard` en `/tutor/perfil` (preview + reemplazar + invalidación de caché), foto en perfil público y tarjetas del directorio (fallback a iniciales); **X en la esquina superior derecha** de cada franja de disponibilidad (reemplaza el botón delete inline).
  - **Tests:** 121/121 (9 nuevos TDD). **Gates:** verdes (17 rutas). Smoke API y navegador verdes (upload, GET público, DTO, preview, X por geometría).
  - **Deuda explícita:** moderación de fotos (opcional), recorte de imagen (opcional); deuda histórica vigente (gaps-analysis).

## v0.9.0 — 2026-10-11

- **feat:** portal del tutor fase 2 — títulos acumulables, documentos post-aprobación y materias normalizadas. Plan `docs/plans/2026-10-11-tutor-portal-phase2/`.
  - **Backend:** migración `TutorDocumentsAccumulateTitles` (índice único parcial solo `kind=3`; Título/Certificacion acumulan máx 5 c/u, rollback manual); `TutorDocumentService` — con `Verified` se pueden AGREGAR títulos/certificaciones (sin cambiar el estado) y la identificación responde 409 `IDENTIFICATION_LOCKED`; `SubjectNormalizer` (primera letra mayúscula por palabra, conectores en minúscula) aplicado en submit y update de perfil.
  - **Frontend:** `lib/subjects.ts` (`normalizeSubject` + `suggestSubjectCorrection` con sugerencia inline "¿Quisiste decir X? [Usar]" en wizard y perfil); wizard pide 1..5 títulos acumulables; **nueva pestaña `/tutor/documentos`** (subnav + PanelMenu) con `DocumentsManager` (contadores n/5, agregar en cualquier estado, ID con candado si Verified); `lib/documents.ts` (validación compartida).
  - **Tests:** 112/112 (6 nuevos). **Gates:** `dotnet build`/`test` verdes; `lint`/`tsc`/`build` verdes (17 rutas). Smoke API y navegador verdes (normalización en BD, acumulación post-aprobación, ID bloqueada, subida desde UI 3/5).
  - **Entregable:** `gaps-analysis.md` — 16 gaps priorizados de la fase tutores (críticos: canal de sesión, pizarra realtime, notificaciones email, vista de ganancias).
  - **Deuda explícita:** catálogo maestro de materias en BD; re-verificación de docs agregados post-aprobación (decisión de producto); deuda 0.8.0 vigente.

## v0.8.0 — 2026-10-11

- **feat:** documentos de verificación del tutor + menú condicionado por rol. Plan `docs/plans/2026-10-11-tutor-documents/`.
  - **Backend:** tabla `tutor_documents` (migración `AddTutorDocuments` + rollback manual; índice único parcial `(tutor_id, kind) WHERE kind IN (1,3)`), enum `TutorDocumentKind`, puerto `IFileStorage` + `LocalFileStorage` (disco local fuera del repo; root `Storage:DocumentsRoot`, default `~/.auralearn/uploads/documents`; nombre físico generado por el servidor, sin path traversal), `ITutorDocumentRepository`, `TutorDocumentService` (PDF/JPEG/PNG ≤5 MB, upsert Titulo/Identificacion borra archivo previo, máx 5 certificaciones, `Verified` → 409 `DOCUMENTS_LOCKED`), `POST/GET /api/tutors/me/documents` (multipart) y `GET /api/admin/tutor-documents/{id}/file` (rol Admin, streaming). **Gate de aprobación:** `DecideAsync(approve)` exige los 3 kinds (tutores seed exentos) → 409 `MISSING_DOCUMENTS` con detalle; `AdminTutorApplicationDto.Documents`.
  - **Frontend:** wizard `/postular` a 5 pasos con `StepDocuments` (3 tarjetas, validación espejo, precarga en re-postulación), `useTutorDocuments` (uploads secuenciales + reintento en `Confirmation`), `StepReview` resume archivos; cola admin con sección Documentos + botón "Ver" (blob URL con Authorization); **"Conviértete en Tutor" oculto del header para Tutor y Admin** (`SiteHeader` filtra `NAV_ITEMS` por rol de sesión).
  - **Tests:** 106/106 (16 nuevos TDD: 13 de servicio + 3 de gate). **Gates:** `dotnet build`/`test` verdes; `lint`/`tsc`/`build` verdes (16 rutas). Smoke E2E API y navegador verdes (gate 409 → uploads → approve → descarga 200/403 → DOCUMENTS_LOCKED → rol promovido; wizard con archivos reales; menú condicionado).
  - **Deuda explícita:** "completar documentos" desde la fase "en revisión" (postulaciones previas a 0.8.0 → 409 al aprobar); migración a blob/S3 (puerto `IFileStorage` listo, ADR-007); ver visualmente el botón "Ver" del admin.
  - **ADR nuevo:** 007 (storage local de documentos tras `IFileStorage`).

## v0.7.0 — 2026-10-11

- **feat:** sección del tutor completa — promoción de rol, perfil editable y especialización. Plan `docs/plans/2026-10-11-tutor-profile-complete/`.
  - **Fix crítico de rol:** `TutorApplicationService.DecideAsync(approve)` promueve `users.role` a `Tutor` (nuevo `IUserRepository.UpdateAsync` + `UserRepository.UpdateAsync`); 3 tests TDD. Los endpoints `[Authorize(Roles="Tutor")]` dejan de dar 403 al tutor real; el JWT fija el rol al login → re-login necesario (copy en la UI de estado).
  - **Perfil editable:** `GET/PUT /api/tutors/me/profile` — `TutorProfileController` (`[Authorize]`; 404 sin fila de tutor), `TutorProfileService` + `UpdateTutorProfileValidator` (reglas espejo de la postulación), `TutorProfileResult<T>` tipado; `Rejected` → 409 `PROFILE_REJECTED_USE_REAPPLY`; PUT devuelve el estado canónico (200). Editables: título, especialización, universidad, bio, materias (1–8), tarifas; no editables: Rating/Reviews/Featured/estado/Nombre. 6 tests + 1 de submit con especialización.
  - **Especialización:** migración `AddTutorSpecialization` (columna nullable `tutors.specialization` varchar(100), rollback manual); fluye por `SubmitTutorApplicationRequest`, `AdminTutorApplicationDto`, `TutorDto` (mapeos en `TutorService`).
  - **Frontend:** `/tutor/perfil` (`ProfileForm` con tag-editor + barra sticky, `ProfileStatusCard` con progreso de verificación, motivo de rechazo + CTA re-postular, nota de re-login), layout `/tutor` con `TutorSubnav` (Perfil·Disponibilidad·Sesiones; `SiteHeader` retirado de sesiones/disponibilidad), `/tutor` → redirect a perfil, "Mi perfil de tutor" en `PanelMenu`, especialización en `TutorProfileHeader` y `application-card`; hook `use-tutor-profile`; `fetchTutorProfile`/`updateTutorProfile` en `lib/api.ts`; tipos en `lib/types/tutor-profile.ts`.
  - **Wizard `/postular`:** `StepProfile` pide Especialización (opcional ≤100); payload, `StepReview` y `Confirmation` (ítem "Configura tus horarios") actualizados.
  - **Fix colateral:** test flaky `GenerateSlots_con_reglas_solo_genera_dentro_de_las_franjas` era dependiente de la fecha (ventana de 8 días sin 2 jueves garantizados); ahora alinea el inicio por fecha local CR (GOTCHA-005).
  - **Tests:** 90/90 (11 nuevos). **Gates:** `dotnet build`/`test` verdes; `lint`/`tsc`/`build` verdes (16 rutas, 3 nuevas). Smoke E2E API: postular → GET/PUT perfil → approve → rol Tutor en BD → availability 200.
  - **Deuda explícita:** upload de atestados/documentos del título (diferido por decisión); catálogo maestro de materias; dashboard "Resumen" del panel; reseñas reales; refresh tokens (lag de rol mitigado con re-login + copy).

## v0.6.0 — 2026-10-01

- **feat:** cierre de deuda de reservas (plan `2026-10-01-reservations-closeout/`, 6 fases): migración regenerada con tooling + ensure-ahead cableado (ADR-005); smoke E2E API 15 escenarios → bug arreglado `TryExpireIfDueAsync` (TDD); pago con monedero ADR-006 (`TryDebitAsync` atómico FOR UPDATE, fallback SINPE); `Completed` con liquidación (`PATCH complete` + `TutorPayout` + `tutor-sessions`); disponibilidad semanal (`TutorAvailability`, honoring + purga, `GET/PUT me/availability`, `/tutor/disponibilidad`); reprogramación cancelar+crear. Tests 79/79; gates verdes (13 rutas). Detalle completo en `docs/plans/2026-10-01-reservations-closeout/`.

## v0.5.0 — 2026-09-29

- **feat:** motor de slots y reservas + pago SINPE end-to-end (reemplaza el `nextSlot` demo).
  - **Backend:** entidades `Slot`/`Reservation`/`WalletEntry` + enums; `ReservationStateMachine` (Domain) con tests por transición; `SlotGenerationService` (idempotente, UTC, zona CR via `TimeZoneInfo`); `SinpeComprobanteValidator` (monto exacto, nº confirmación único, receptor, ventana); `ReservationService` (crear idempotente, comprobante, decidir, cancelar con regla 12 h + monedero); **índice único parcial `(slot_id) WHERE status IN (1,2)`** como guardián de exclusividad; migración `AddSlotsAndReservations` con bloque ROLLBACK MANUAL. Endpoints: `GET /api/tutors/{id}/slots`, `POST /api/reservations` (idempotent), `GET /api/reservations`, `GET /api/reservations/{id}`, `POST /api/reservations/{id}/comprobante`, `PATCH /api/reservations/{id}/cancel`, `GET /api/admin/payments`, `PATCH /api/admin/payments/{reservationId}` (rol Admin, motivo ≥10 chars), `GET /api/wallet`. Config `Sinpe:ReceptorPhone` (default `88888888`).
  - **Frontend:** `nextSlotAt` reemplaza `nextSlot`; tipos `Slot`/`Reservation`/`PaymentInfo`/`AdminPayment`/`WalletBalance`/`ReservationStatus`; 10 funciones nuevas en `lib/api.ts`; 4 hooks (`use-tutor-slots`, `use-reservation` + `useReservationPoll`, `use-my-reservations`, `use-admin-payments`); helpers `lib/time.ts` (`Intl` + `America/Costa_Rica`); 4 pantallas nuevas (`/tutores/[id]`, `/checkout/[id]`, `/mis-tutorias`, `/admin/pagos`); `/tutores` actualizado con formato es-CR + Link al perfil.
  - **Tests:** 34 tests nuevos en backend (Reservas/Slots/SINPE/StateMachine/NextSlot) — suite 60/60 verde. TDD obligatorio: cada invariante = test (incluida la máquina de estados).
  - **Gates:** `dotnet build`/`dotnet test` verdes; `npm run lint`/`tsc --noEmit`/`npm run build` verdes (11 rutas generadas).
  - **Deuda explícita:** reprogramación (PATCH reschedule) excluida; CRUD de disponibilidad del tutor (el concepto está en Stitch 02, no implementado); firma digital real del comprobante sustituida por confirmación manual admin (ADR-004); expiración de `PendingPayment` perezosa (60 min, sin workers); aplicar saldo del monedero al pagar (consumir con monto negativo); estado `Completed` con liquidación al tutor; notificaciones por email.
  - **ADRs nuevos:** 004 (confirmación manual SINPE por Admin) y 005 (UTC en BD, zona CR en presentación, slots ensure-ahead). Plan `docs/plans/2026-09-29-slots-reservations/`.

## v0.4.2 — 2026-09-29

- **feat:** panel admin de aprobación de tutores: página `/admin/tutores` protegida por rol Admin (guard client-side), cola de postulaciones (`fetchAdminApplications`), rechazo con motivo obligatorio (modal) y aprobación contra `PATCH {id}/verify`, recarga automática de la cola. Smoke E2E: login admin → cola visible → rechazo validado → `Rejected` + `rejection_reason` en BD. Plan `docs/plans/2026-09-29-admin-panel/`.

## v0.4.1 — 2026-09-29

- **feat:** auth UI en el frontend conectada al backend real: páginas `/login` y `/registro` (validación client-side espejo, estados loading/error), contexto global de sesión `AuthProvider`/`useAuth` (login/register/logout, restauración con validación de expiración JWT), `fetchMe` en `lib/api.ts`, `SiteHeader` con sesión ("Hola, {nombre}" + cerrar sesión). Smoke E2E: login, registro, logout y persistencia en recarga; usuario creado desde la UI en BD. Plan `docs/plans/2026-09-28-auth-ui/`.

## v0.4.0 — 2026-09-29

- **feat:** onboarding del profesor full-stack. Frontend: wizard `/postular` de 4 pasos (cuenta → perfil académico → cursos/tarifa → revisión) + confirmación ≤48 h (`components/onboarding/`, `hooks/use-tutor-application.ts`, `lib/auth.ts`); CTAs "Postular como Docente"/"Conviértete en Tutor" enlazados. Backend: `POST /api/tutor-applications` (202), `GET /api/tutor-applications/status`, `GET /api/admin/tutor-applications` + `PATCH {id}/verify` (rol Admin, rechazo con motivo obligatorio); `Tutor.UserId` (FK cascade + unique) y `RejectionReason`; migración `AddTutorUserId` con rollback manual. 18 tests TDD (25/25); smoke E2E 19 verificaciones. Plan `docs/plans/2026-09-28-tutor-onboarding/`.

## v0.3.0 — 2026-09-28

- **feat:** autenticación en el backend (TDD obligatorio, tests first): `POST /api/auth/register` (hash PBKDF2 con PasswordHasher, rol Estudiante), `POST /api/auth/login` (JWT con claims sub/email/role/name, exp 8h), `GET /api/auth/me` (protegido). Entidad `User` con email único; migración `AddUsersTable` con rollback manual. 7 tests de auth (12/12 total). Paquetes: Microsoft.Extensions.Identity.Core, System.IdentityModel.Tokens.Jwt. Plan `docs/plans/2026-09-28-auth-endpoints/`.

## v0.2.1 — 2026-09-28

- **feat:** primera integración real frontend↔backend: el directorio `/tutores` consume `GET /api/tutors` vía nuevo `web/lib/api.ts` (cliente HTTP tipado) y `web/hooks/use-tutors.ts` (estados, AbortController, debounce). Mock `TUTORS` eliminado; tipo `Tutor` alineado al DTO real; sort client-side en `lib/filters.ts`; `LoadingGrid`/`ErrorState` nuevos; `.env.example` + `.env.local` para `NEXT_PUBLIC_API_URL`. Plan `docs/plans/2026-09-28-connect-frontend-api/`.

## v0.2.0 — 2026-09-28

- **feat:** backend .NET 10 implementado en `api/` (ADR-003): solución Clean Architecture 4 proyectos, EF Core + PostgreSQL con migración `InitialCreate` (rollback manual incluido), seed de 12 tutores, JWT configurado, `GET /api/tutors` con filtros/paginación, healthcheck `/health`, Swagger dev, CORS localhost:3000. 5 tests xUnit. Gate backend verde. BD local `auralearn_dev` creada.

## v0.1.1 — 2026-09-28

- **refactor:** extraídos los mocks de datos fuera de los componentes: `TUTORS` + constantes de filtro a `web/lib/data/tutors.ts`, `MENTORS` a `web/lib/data/mentors.ts`, tipos (`Tutor`, `FilterState`, `SortOption`, `Mentor`) a `web/lib/types/`, y lógica de filtrado/orden a funciones puras en `web/lib/filters.ts` (`applyFilters`/`applySort`). Comportamiento idéntico; plan cerrado en `docs/plans/2026-09-28-extract-mock-data/`.

## v0.1.0 — 2026-09-27

- **feat:** landing pública en `web/` con secciones (hero, trust-strip, rigor, disciplinas, mentores, pasos, testimonios, tutor-cta, FAQ, final-cta, footer) y header con toggle CRC/USD.
- **feat:** directorio `/tutores` con búsqueda, filtros (universidad, nivel, rating, precio, disponibilidad) y orden sobre datos mock.
- **feat:** sistema de diseño base-nova sobre `@base-ui/react` con tokens Material 3 en `app/globals.css` y Material Symbols.
- **docs:** sistema de memoria externa y gobernanza del agente: `.ai/`, `.github/copilot-instructions.md`, `.github/instructions/`, `.github/skills/`, `.github/agents/`, `.github/prompts/`, `.github/errors/`, `.github/templates/`, `docs/plans/`.
- **docs:** ADR-001 (gobernanza) y ADR-002 (sistema de componentes).

## Histórico previo

(vacío — este es el registro inicial)

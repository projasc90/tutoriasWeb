---
id: current-state
priority: 1
loadWhen: always
maxLines: 60
---

# Estado Actual — AuraLearn

**Versión backend:** solución `AuraLearn.slnx` .NET 10 · **Versión frontend:** `0.13.0` (SEMVER en `web/package.json`)

## Completado recientemente

- **API en Vercel Container + Vercel Blob** (`docs/plans/2026-10-30-vercel-container/`, 0.13.0, ADR-009): la API .NET se despliega como **contenedor en Vercel** (Root `api`, Framework Preset `Container`) y los archivos viven en **Vercel Blob** (store privado). Supersede ADR-007 (disco) y ADR-008 (Railway). Claves: `PORT=8080` obligatorio (Vercel enruta a 80 → sin esto 502); `VercelBlobFileStorage` habla la REST de Blob v13 con `HttpClient`; **subida directa** desde el navegador (`web/app/api/blob/upload` emite tokens) + `POST .../attach` valida metadatos reales y propiedad del pathname — evita el **límite de 4.5 MB** (nuestros documentos son 5 MB). Migraciones fuera del contenedor (job `migrate` en CI). Tests **158/158**; lint/tsc/build verdes (22 rutas).

- **Fix de navegación del menú** (0.12.2, 8 archivos): `#catalogo` estaba en el grid de tutores de `Mentors` (solo existe en estado success con resultados) y `#como-funciona` en la `SearchCard` del hero en vez de `Steps`; `SiteHeader` se reutiliza en todo el sitio pero usaba hash de la portada. Ahora `NAV_ITEMS` separa `href`/`hash` y resuelve a `/#seccion` fuera de `/`, "Buscar Tutores" y CTA → `/tutores`, `<a>` interno → `Link`, y `scroll-mt-24` en las secciones ancla. Footer sin `href="#"`. Gates verdes + smoke Playwright.

- **Contenedores + despliegue** (`2026-10-16-deploy-containers/`, 0.12.1, ADR-008 — **superado**): `api/Dockerfile` multi-stage, compose local, CI/CD a Railway + guide. La imagen y el `entrypoint.sh` siguen vigentes; la topología cambió a Vercel.

- **Panel estudiante + login en reserva + admin de estudiantes** (`2026-10-16-student-panel/`, 0.12.0): panel en `/estudiante/{reservas,monedero}`, login con `?redirect=` anti open-redirect, reservas solo rol Estudiante, `/admin/estudiantes`, landing con API real. Tests 149/149.


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
- **Despliegue real pendiente (manual del usuario):** crear los dos proyectos Vercel (`web`, `api` con Root `api` + preset `Container`), crear el Blob store privado, definir variables (`PORT=8080`, `JWT__Key`, `ConnectionStrings__Default`, `CORS__Origins`, `BLOB_READ_WRITE_TOKEN`), añadir `NEON_CONNECTION_STRING` a GitHub Secrets y correr el job `migrate`. Guía en `docs/plans/2026-10-30-vercel-container/deploy-guide.md`.
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
- **Backend (`api/`):** `Api` → `Application` → `Domain`; `Infrastructure` (DbContext Npgsql, repos, migraciones con rollback manual). BD dev `auralearn_dev` (Postgres local 5432) / prod **Neon** por `ConnectionStrings__Default`. Contenedor: `api/Dockerfile` + `entrypoint.sh` (`PORT` dinámico; `ASPNETCORE_URLS` es la única fuente de binding). **Archivos:** `IFileStorage` con dos implementaciones — `LocalFileStorage` (dev, `~/.auralearn/uploads/documents`) y `VercelBlobFileStorage` (prod, REST v13, store privado); se elige por `Storage:Provider` o por la presencia de `BLOB_READ_WRITE_TOKEN`.
- **Frontend (`web/`):** Next.js 16.3.6 App Router + React 19 + TS 5 + Tailwind 4 + base-nova. 22 rutas. Tokens MD3 en `globals.css`; fuente Plus Jakarta Sans. Panel estudiante en `/estudiante/*` (layout + `StudentSubnav`); `/mis-tutorias` es redirect de compatibilidad; landing Mentors consume `GET /api/tutors` (client, runtime). **Subidas:** `app/api/blob/upload` emite tokens de Vercel Blob (verifica JWT contra el API); el navegador sube directo y el .NET confirma con `.../attach`.
- **Límite duro:** Vercel Functions rechaza cuerpos > **4.5 MB**. No implementar subidas multipart contra el API para archivos grandes.

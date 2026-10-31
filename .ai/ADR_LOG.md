---
id: adr-log
priority: 3
loadWhen: decisiones-arquitectura
maxLines: 80
---

# ADR Log — AuraLearn

## Índice rápido

| ADR | Título | Estado | Fecha |
|-----|--------|--------|-------|
| [ADR-001](#adr-001) | Configuración del sistema de memoria y gobernanza | Vigente | 2026-09-27 |
| [ADR-002](#adr-002) | shadcn/ui base-nova sobre @base-ui/react como sistema de componentes | Vigente | 2026-09-27 |
| [ADR-003](#adr-003) | Solución .NET Clean Architecture 4 proyectos + EF Core PostgreSQL | Vigente | 2026-09-28 |
| [ADR-006](#adr-006) | Pago de reservas con saldo del monedero (solo pago total) | Vigente | 2026-10-01 |
| [ADR-007](#adr-007) | Storage de documentos de verificación en disco local tras IFileStorage | Superado por ADR-009 | 2026-10-11 |
| [ADR-008](#adr-008) | Despliegue: API en contenedor (Railway) + Neon + frontend en Vercel | Superado por ADR-009 | 2026-10-16 |
| [ADR-009](#adr-009) | API .NET en Vercel Container Runtime + Vercel Blob para archivos | Vigente | 2026-10-30 |

## ADRs supersedidas

Ninguna todavía.

---

## ADR-001

**Título:** Configuración del sistema de memoria y gobernanza
**Estado:** Vigente
**Fecha:** 2026-09-27

**Contexto:** El monorepo AuraLearn es multi-stack (web activo, api y portal reservados) y requiere continuidad de contexto entre sesiones del agente, reglas de ejecución verificables y conocimiento acumulado.

**Decisión:**
- Memoria externa versionada en `.ai/` (carga base: `CONTEXT_INDEX.md` + `CURRENT_STATE.md` + `AI_CONTEXT.md`).
- Punto de entrada del agente en `.github/copilot-instructions.md`, con instrucciones por `applyTo`, skills por dominio, agentes de revisión, prompts operativos, catálogo de errores y templates del stack.
- Metodología ligera por fases en `docs/plans/` para cambios grandes/transversales/riesgo medio-alto.
- Testing gate con comandos reales del repo y TDD escalado por riesgo.

**Consecuencias:** todo trabajo significativo termina con cierre de `.ai/`; los ADRs nuevos se agregan aquí y el índice se mantiene al día.

---

## ADR-002

**Título:** shadcn/ui base-nova sobre @base-ui/react como sistema de componentes
**Estado:** Vigente
**Fecha:** 2026-09-27

**Contexto:** `web/` usa componentes generados por `shadcn` (v4.21) con estilo `base-nova`, montados sobre primitivas de `@base-ui/react` (v1.8) y tokens Material 3 en `app/globals.css`.

**Decisión:** los componentes de `web/components/ui/` son la fuente única de primitivas. No se introduce otra librería UI (MUI, Chakra, etc.). El copy de producto usa Material Symbols; las primitivas usan lucide-react.

**Consecuencias:** para nuevos widgets usar `npx shadcn add <componente>`; extensiones por composición, nunca reescribiendo la primitiva.

---

## ADR-003

**Título:** Solución .NET Clean Architecture 4 proyectos + EF Core PostgreSQL
**Estado:** Vigente
**Fecha:** 2026-09-28

**Contexto:** `api/` estaba reservada. Se requiere un backend para reglas de negocio (SINPE, slots, verificación de tutores), persistencia PostgreSQL y emisión de JWT. El SDK disponible es .NET 10.0.401.

**Decisión:**
- Solución `api/AuraLearn.sln` con capas: `AuraLearn.Api` (webapi) → `AuraLearn.Application` → `AuraLearn.Domain`; `AuraLearn.Infrastructure` implementa puertos de Application.
- EF Core 10 (Npgsql 10.0.3) con migraciones en `AuraLearn.Infrastructure/Migrations/`; columnas snake_case; índices por filtros reales del catálogo.
- Seed del catálogo vía `HasData` estático (12 tutores); warning `PendingModelChangesWarning` suprimido explícitamente (falso-positivo de EF 10 con seed, ver `.github/errors/backend.md`).
- JWT bearer configurado desde `Jwt:Key` (appsettings, sin secretos commiteados); endpoints de auth llegan en tarea posterior.
- Tests xUnit en `api/tests/AuraLearn.Tests/`.

**Consecuencias:** todo nuevo código backend respeta la dirección de dependencias hacia Domain; los repositorios van en Infrastructure, los casos de uso en Application; el rollback manual es obligatorio en cada migración nueva.

---

## ADR-006

**Título:** Pago de reservas con saldo del monedero (solo pago total)
**Estado:** Vigente
**Fecha:** 2026-10-01

**Contexto:** el monedero acumula créditos por cancelaciones a tiempo (₡ completo con ≥12 h). Hasta v0.5.0 ese saldo era inerte: no se podía usar para pagar una reserva.

**Decisión:**
- Solo pago total: si `saldo >= precio` y el estudiante marca "pagar con monedero", la reserva pasa directamente a `Confirmed` con un débito negativo (`WalletEntryReason.WalletPayment`), sin comprobante SINPE ni revisión del Admin (el saldo ya fue acreditado por una decisión admin previa).
- Si el saldo es insuficiente, cae automáticamente al flujo SINPE normal (PaymentInfo + cola de pagos).
- El débito es atómico y race-safe: transacción con `SELECT id FROM users WHERE id = @u FOR UPDATE` + `SUM(AmountCrc)` dentro del lock; saldo insuficiente → rollback sin efectos.
- La reserva se persiste ANTES del débito (el `WalletEntry` lleva FK a la reserva; persistir después causaría FK 23503).
- Sin pago parcial (SINPE por el remanente): complica la validación de monto exacto; queda excluido por decisión del usuario.

**Consecuencias:** el índice único parcial sigue siendo el guardián de exclusividad (una reserva wallet-pagada es `Confirmed` y bloquea el slot); `GET /api/wallet` sirve a estudiantes y tutores; el ledger del tutor (liquidación) usará la misma tabla `wallet_entries` con nuevos reasons.

---

## ADR-007

**Título:** Storage de documentos de verificación en disco local tras IFileStorage
**Estado:** Vigente
**Fecha:** 2026-10-11

**Contexto:** La verificación de tutores exige subir documentos (título, certificaciones, identificación). Son PII: nunca en la BD ni en el repo; acceso de descarga restringido a rol Admin. No hay cuenta cloud configurada todavía (Azure/AWS).

**Decisión:**
- Puerto `IFileStorage` en Application (`SaveAsync/OpenAsync/DeleteAsync`); implementación `LocalFileStorage` en Infrastructure con root configurable `Storage:DocumentsRoot` (default dev `~/.auralearn/uploads/documents`, fuera del repo).
- El nombre físico en disco lo genera el servidor (`{tutorId}/{kind}/{Guid}.{ext}`); el nombre original del usuario solo es metadato en BD. Verificación anti path-traversal al resolver rutas.
- La BD guarda solo metadatos (`tutor_documents`: kind, file_name, content_type, size_bytes, storage_path, uploaded_at) con índice único parcial para los tipos upsert.
- Descarga exclusiva por `GET /api/admin/tutor-documents/{id}/file` con `[Authorize(Roles="Admin")]` (streaming `File()`); el tutor consulta metadatos, no contenido.

**Consecuencias:** migrar a Azure Blob/S3 en el futuro solo requiere otra implementación de `IFileStorage` (las rutas en BD son relativas al root); en prod definir `Storage:DocumentsRoot` (p. ej. `/var/lib/auralearn/uploads/documents`) con backup; los archivos huérfanos tras rollback manual de migración se limpian a mano.

---

## ADR-008

**Título:** Despliegue: API en contenedor (Railway) + Neon + frontend en Vercel
**Estado:** Vigente
**Fecha:** 2026-10-16

**Contexto:** el usuario pidió "contenedores para desplegar la API en Vercel". Vercel no soporta contenedores Docker ni .NET (solo runtimes serverless Node/Python/Go/Ruby). El usuario eligió explícitamente: BD Neon, frontend en Vercel, CI/CD con GitHub Actions, y Railway como host del contenedor de la API tras ver las alternativas.

**Decisión:**
- **API:** imagen Docker multi-stage (`api/Dockerfile`: SDK 10 build → aspnet 10 runtime, usuario no-root, `entrypoint.sh`). Despliegue en **Railway** (contenedor gestionado, HTTPS incluido).
- **BD:** **Neon** (Postgres gestionado); connection string pooled por `ConnectionStrings__Default` en Railway. Npgsql es Postgres estándar: sin cambios de código.
- **Frontend:** **Vercel** (Next.js nativo, `web/vercel.json`, env `NEXT_PUBLIC_API_URL`).
- **CORS:** configurable por entorno (`Cors:Origins` separado por `;`) con fallback localhost en Development.
- **PORT dinámico:** Railway inyecta `PORT`; el entrypoint fija `ASPNETCORE_URLS=http://0.0.0.0:$PORT` (verificado localmente).
- **Migraciones:** opt-in explícito (`ApplyMigrations=true` / `APPLY_MIGRATIONS=true` → `Database.Migrate()` one-shot y termina). El servidor nunca las aplica en silencio en cada arranque.
- **CI/CD:** `api-deploy.yml` (build+tests → GHCR → migraciones → redeploy Railway) y `web-ci.yml` (gate lint/tsc/build; deploy nativo de Vercel). Secretos solo en GitHub Secrets/Railway/Vercel.
- **Local:** `docker-compose.yml` (api + postgres 16) espeja el despliegue.

**Consecuencias:** `LocalFileStorage` requiere volumen en Railway (`/data`) — filesystem efímero sin él; para durabilidad fuerte migrar `IFileStorage` a object storage (ADR futuro). Neon free tier auto-suspende la BD tras inactividad (~1 s de cold start). Rotar `Jwt__Key` invalida sesiones (8 h). Guía operativa en `docs/plans/2026-10-16-deploy-containers/deploy-guide.md`. `infra.instructions.md` queda actualizado con esta topología (VPS/Azure/AWS siguen siendo alternativas válidas).

> **Superado por ADR-009 (2026-10-30).** La premisa de este ADR —"Vercel no
> soporta contenedores Docker ni .NET"— dejó de ser cierta: Vercel lanzó
> **Container Runtime** (OCI/Dockerfile, Beta en todos los planes) y publicó
> una guía oficial para ASP.NET Core. Railway deja de ser necesario.

---

## ADR-009

**Título:** API .NET en Vercel Container Runtime + Vercel Blob para archivos
**Estado:** Vigente (supersede ADR-007 y ADR-008)
**Fecha:** 2026-10-30

**Contexto:** el usuario quiere la API .NET y el frontend en un único proveedor (Vercel), con un repo personal en GitHub. Verificado contra la documentación oficial: `Container Runtime` ejecuta imágenes OCI construidas desde un `Dockerfile` (`x-api-version 13` en la API de Blob; `/docs/functions/runtimes/container`) y existe una guía oficial *Deploy ASP.NET Core on Vercel with Docker*. Dos restricciones de la plataforma chocan con el código previo: **4.5 MB de cuerpo máximo por request** (el límite de documentos y fotos era 5 MB) y **filesystem efímero** con solo `/tmp` escribible (ADR-007 persistía en disco).

**Decisión:**
- **Dos proyectos Vercel sobre el mismo repo:** `web` (Root Directory `web`, framework Next.js, deploy nativo) y `api` (Root Directory `api`, Framework Preset **Container**, construye `api/Dockerfile`). Sin `vercel.json` con `framework: container`: el `Dockerfile` en el root del proyecto basta.
- **Puerto:** el proyecto `api` define `PORT=8080`. Vercel enruta a 80 por defecto y la base ASP.NET escucha en 8080: sin esa variable todas las rutas devuelven 502. `EXPOSE` no influye en el routing.
- **Storage:** `VercelBlobFileStorage` implementa `IFileStorage` contra la **REST API de Blob** con `HttpClient` (no hay SDK oficial .NET; se replica el contrato del SDK JS para no añadir dependencias y permitir migrar a S3/R2 sin tocar Application). Store **privado**: la lectura siempre pasa por el API, así la autorización de PII sigue en .NET.
- **Selección en DI:** `Program.cs` usa Blob si hay `BLOB_READ_WRITE_TOKEN` (o `Storage:Provider=VercelBlob`); en caso contrario `LocalFileStorage`. Faltando el token en producción el arranque **falla explícitamente** en vez de degradar a un disco efímero.
- **Subida directa (rompe el límite de 4.5 MB):** `web/app/api/blob/upload/route.ts` emite tokens de cliente con `@vercel/blob/client`; el navegador sube el archivo directo al store y luego confirma con `POST /api/tutors/me/{documents,photo}/attach`. El API .NET **no confía en el cliente**: lee los metadatos reales (`GetInfoAsync`, GET `/?url=`), compara tamaño y tipo contra la whitelist y exige que el pathname cuelgue del `{userId}` autenticado.
- **Autenticación del endpoint de tokens:** `onBeforeGenerateToken` verifica el JWT contra `GET /api/auth/me` del API (fuente única de verdad, sin duplicar la firma ni añadir `jose`); el token viaja en `clientPayload` porque el SDK no reenvía cabeceras propias.
- **Degradación:** el frontend sondea `GET /api/blob/upload`; si no hay store (dev local) cae al multipart clásico contra `LocalFileStorage`.
- **Sin webhook `onUploadCompleted`:** no se dispara en localhost y añadiría un segundo camino de escritura en BD; la confirmación explícita lo reemplaza.
- **Migraciones:** Vercel no permite sobreescribir el comando de arranque, así que el modo one-shot (`ApplyMigrations=true`) corre en el job `migrate` de GitHub Actions contra Neon; el workflow `api-ci.yml` deja de publicar imágenes y de desplegar (Vercel construye desde Git).

**Consecuencias:** Container Runtime está en **Beta**. Scale-to-zero (5 min en producción): cold start .NET de ~1-3 s tras inactividad. Flujo con dos proyectos Vercel sobre el mismo repo. Hobby no permite conectar repos de organizaciones Git (el repo es personal: no aplica). Sin Secure Compute ni Static IPs con imágenes propias. Los documentos siguen fuera de la BD y del repo; `LocalFileStorage` queda solo para desarrollo. Railway/Azure Container Apps/VPS siguen siendo alternativas documentadas en `infra.instructions.md`.


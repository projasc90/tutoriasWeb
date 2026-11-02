---
id: task-patterns
priority: 3
loadWhen: debug-o-bug-recurrente
maxLines: 125
---

# Task Patterns — AuraLearn

Patrones, bugs recurrentes y gotchas permanentes. Máximo: patrones realmente útiles; archivar los que dejen de aplicar.

Formato: **Contexto → Error observado → Causa → Solución → Aprendido en (fecha)**

---

## GOTCHA-001 — Convenciones de `web/`: Next 16, base-nova, tokens MD3, iconos

- **Contexto:** cualquier tarea de código o estilos en `web/` (App Router, `components/ui/`, secciones nuevas).
- **Error observado:** (a) APIs de Next 14/15 que ya no aplican; (b) importar primitivas Radix o `asChild`; (c) clases tipo `bg-[#3525cd]`/`text-slate-500`; (d) mezclar Material Symbols con lucide.
- **Causa:** Next.js 16.3.6 tiene cambios rompientes (`web/AGENTS.md` lo advierte); el design system usa `@base-ui/react` (estilo `base-nova`, no Radix); el tema son tokens Material 3 en `app/globals.css` (`@theme inline`, Tailwind 4 CSS-first, sin `tailwind.config`); los iconos son duales por contexto.
- **Solución:** revisar `web/node_modules/next/dist/docs/` antes de escribir código y copiar patrones del repo (`LayoutProps<"/">`); en `components/ui` copiar `button.tsx`/`sheet.tsx` o usar `npx shadcn add` con lucide; en producto usar tokens (`bg-primary`, `text-on-surface-variant`, `text-title-md`) y `<MaterialIcon name="verified" />` (nombres del set de Google, en inglés).
- **Aprendido en:** 2026-09-27 (configuración inicial; consolidado 2026-11-01).

## GOTCHA-005 — Alinear rangos de slots por fecha local CR, no UTC

- **Contexto:** tests o lógica que construyen rangos de fechas para `SlotGenerationService` (o cualquier cálculo con zona CR).
- **Error observado:** `GenerateSlots_con_reglas_solo_genera_dentro_de_las_franjas` fallaba algunos días (Expected 8, Actual 4): una ventana de 8 días desde `hoy+7` no siempre contiene 2 jueves.
- **Causa:** el servicio itera por días LOCALES CR (`ConvertTimeFromUtc(fromUtc).Date`); medianoche UTC = 18:00 del día anterior en CR, así que alinear el inicio "en UTC" desplaza el día local uno hacia atrás.
- **Solución:** calcular la fecha local CR primero (`ConvertTimeFromUtc(DateTime.UtcNow, crZone).Date`), alinear ahí el día de la semana y convertir de vuelta con `ConvertTimeToUtc(fechaMedianoche, crZone)`. Test determinista en `SlotGenerationServiceTests`.
- **Aprendido en:** 2026-10-11 (plan tutor-profile-complete).

## GOTCHA-006 — Particularidades de Next 16 + lint react-hooks en web/

- **Contexto:** crear layouts anidados o componentes con estado que se re-sincroniza con props.
- **Error observado:** (a) `LayoutProps<"/tutor">` no compila (solo satisface `"/"`); (b) `react-hooks/set-state-in-effect` marca como error `setState` síncrono dentro de `useEffect`.
- **Causa:** los tipos generados de Next 16 en este repo solo tipan `LayoutProps<"/">`; el lint de react-hooks prohíbe el patrón effect-para-sincronizar-props.
- **Solución:** layouts anidados con `{ children: ReactNode }`; para re-sincronizar estado con props usar el patrón "adjust state during render" (guardar el valor previo en state y hacer `setState` condicional durante el render), como en `ProfileForm`.
- **Aprendido en:** 2026-10-11 (plan tutor-profile-complete).

## GOTCHA-007 — DbSet con el mismo nombre que la entidad oculta el tipo en el DbContext

- **Contexto:** agregar un DbSet cuyo nombre coincide exactamente con la entidad (p. ej. `public DbSet<BookingRules> BookingRules`).
- **Error observado:** `CS1061: "DbSet<BookingRules>" no contiene una definición para "SingletonId"` al usar `BookingRules.SingletonId` dentro de `OnModelCreating`.
- **Causa:** dentro de la clase del DbContext, el identificador `BookingRules` resuelve primero a la **propiedad** (DbSet), no al tipo.
- **Solución:** usar el nombre cualificado en ese scope (`AuraLearn.Domain.Entities.BookingRules.SingletonId`) o un local. Fuera del DbContext no hay problema.
- **Aprendido en:** 2026-10-12 (booking rules).

## GOTCHA-008 — Hook de datos: setState en effect solo con IIFE async inline

- **Contexto:** hooks de datos en `web/hooks/` (patrón `ApiState`).
- **Error observado:** `error Calling setState synchronously within an effect` al hacer `void load()` en el effect cuando `load` es un `useCallback` que llama `setState` (aunque sea tras un `await`).
- **Causa:** la regla de react-hooks v6 no traza el límite async de funciones referenciadas; sí acepta la IIFE async inline con `await` antes del `setState` (patrón de `use-availability.ts`). Complementa GOTCHA-006 (variante "adjust state during render").
- **Solución:** copiar el patrón de `use-availability.ts`: IIFE async inline en el `useEffect` (con flag `cancelled`) y exponer `reload` como callback aparte para eventos del usuario. Evitar estados `idle` que nunca se producen (rompen el narrowing de TS en las páginas).
- **Aprendido en:** 2026-10-12 (hook use-booking-rules).

## GOTCHA-009 — Servidores dev desactualizados tras cambios de código

- **Contexto:** smoke E2E contra `localhost:5037` (API) y `localhost:3000` (web) con procesos ya corriendo.
- **Error observado:** endpoint nuevo devuelve 404 (API) o la página muestra contenido viejo/500 (web) aunque el build local esté en verde.
- **Causa:** el proceso en el puerto es un binario/servidor viejo (`AuraLearn.Api` compilado antes, o `next start` con un build anterior). `dotnet run --no-build` y `next start` no recogen cambios.
- **Solución:** identificar el PID (`lsof -nP -iTCP:<puerto> -sTCP:LISTEN`), matarlo y relanzar: API `cd api/AuraLearn.Api && dotnet run --no-build` (tras `dotnet build`), web `npm run dev` (o `npm run build && npm start`). Verificar siempre el endpoint nuevo con curl antes del smoke de navegador.
- **Aprendido en:** 2026-10-16 (smoke de `GET /api/admin/users` → 404 con binario viejo).

## GOTCHA-010 — DI: servicio nuevo sin registrar en Program.cs

- **Contexto:** crear un servicio de Application nuevo (p. ej. `AdminUserService`) con controller que lo inyecta.
- **Error observado:** `InvalidOperationException: Unable to resolve service for type 'X' while attempting to activate 'YController'` → 500 en runtime, aunque `dotnet build` y los tests pasen (los tests construyen el servicio a mano).
- **Causa:** `Program.cs` registra servicios explícitamente (`AddScoped`); el compilador no valida el grafo de DI.
- **Solución:** al crear un servicio/validador nuevo, añadirlo a la sección "Application (servicios + validadores)" de `Program.cs` en el mismo cambio; verificar con curl el endpoint antes de cerrar.
- **Aprendido en:** 2026-10-16 (`AdminUserService` olvidado en DI → 500 en smoke).

## GOTCHA-011 — Anclas de la landing en un header compartido

- **Contexto:** tocar `SiteHeader` (`components/landing/currency-toggle.tsx`) o agregar secciones con `id` en `components/landing/`.
- **Error observado:** los ítems del menú principal "no hacían nada" o llevaban a un lugar inesperado: `#catalogo` no existía (el `id` vivía en el grid de `Mentors`, que solo se renderiza en estado success con tutores) y `#como-funciona` apuntaba a la `SearchCard` del hero, no a `Steps`.
- **Causa:** el header es global (se usa en `/tutores`, paneles, checkout, `/postular`), pero sus enlaces eran hash puros de la portada; además los `id` se colocaron en ramas condicionales del render.
- **Solución:** (a) `NAV_ITEMS` separa `href` (ruta real) de `hash`, resuelto con `usePathname()` → `#seccion` en `/` y `/#seccion` en el resto; (b) las rutas que existen como página usan `Link` a esa página (`/tutores`, `/postular`), no a un ancla; (c) el `id` de sección va en la `<section>` de todos los estados de carga/error/vacío; (d) `scroll-mt-24` en cada sección ancla por el header sticky (top ≈ 96 px); (e) sin `href="#"` (renderizar texto si la página no existe).
- **Aprendido en:** 2026-10-30 (fix de navegación del menú, 0.12.2).

## GOTCHA-012 — El token de Blob impone su propio `maximumSizeInBytes`

- **Contexto:** validar la subida directa a Vercel Blob (ADR-009) con archivos grandes.
- **Error observado:** `BlobFileTooLargeError: File is too large, the file length cannot be greater than 5242880 bytes` al intentar subir un PDF de 6 MB.
- **Causa:** el tope de **5 MB** (5242880 B) no es de Vercel: es el `maximumSizeInBytes` que devuelve `onBeforeGenerateToken` en `web/app/api/blob/upload/route.ts`, espejo de `TutorDocumentService.MaxSizeBytes`. El SDK lo aplica en el cliente **antes** de subir.
- **Solución:** no confundir el límite de la plataforma (**4.5 MB** por request en Vercel Functions) con el tope de negocio (**5 MB**). El rango que la subida directa desbloquea es **4.5–5 MB**; por encima de 5 MB la app rechaza por regla propia. Al verificar E2E usar un archivo de ~4.8 MB.
- **Aprendido en:** 2026-10-30 (E2E de subida directa contra el store real).

## GOTCHA-014 — Npgsql no acepta URIs y las variables PG* vienen incompletas

- **Contexto:** conectar el API a Neon (o a cualquier Postgres gestionado) usando las variables que inyecta la plataforma (`DATABASE_URL`, `PGHOST`, `PGUSER`, …).
- **Error observado:** (a) `ArgumentException: Format of the initialization string does not conform…` al pasar `postgresql://user:pass@host/db?sslmode=require` a `UseNpgsql`; (b) con solo `PGHOST`/`PGUSER`/`PGPASSWORD` la conexión cae a `localhost` o al usuario del SO.
- **Causa:** Npgsql **solo** entiende `keyword=value;`; el formato URI es de libpq/psql. Y del entorno solo respeta `PGUSER`, `PGPASSWORD`, `PGSSLCERT`…: **no** lee `PGHOST`, `PGPORT` ni `PGDATABASE`.
- **Solución:** pasar siempre por `DatabaseConnectionString.Resolve(...)` (prioridad `ConnectionStrings__Default` → `DATABASE_URL` → `POSTGRES_URL` → `PG*`), que normaliza URIs con `NpgsqlConnectionStringBuilder`, decodifica la contraseña (`Uri.UnescapeDataString`) e ignora parámetros del proveedor. En modo migraciones usar la URL directa (`DATABASE_URL_UNPOOLED`).
- **Aprendido en:** 2026-11-01 (ADR-010, plan `2026-11-01-env-vars-reuse`).

## GOTCHA-015 — `BLOB_STORE_ID` llega como `store_<id>`; el token puede traerlo también

- **Contexto:** construir la URL de un blob privado de Vercel (`https://<id>.private.blob.vercel-storage.com/...`) desde el API .NET.
- **Error observado:** con `token.Split('_')[3]` el store id resuelto era `store` y todas las lecturas daban 403/404; además `BLOB_STORE_ID` se ignoraba por completo.
- **Causa:** Vercel inyecta el id como `store_<id>` (el prefijo **no** forma parte del subdominio del store) y el token puede venir como `vercel_blob_rw_store_<id>_<secreto>`.
- **Solución:** `VercelBlobConfiguration.ResolveStoreId(token, storeId)` normaliza quitando `store_`, corta el token por el **último** `_` y falla si ambas fuentes discrepan. La validación se ejecuta en el arranque (la factory de DI es perezosa).
- **Aprendido en:** 2026-11-01 (ADR-010).

## GOTCHA-016 — `dotnet run` aplica launchSettings y migra la BD local

- **Contexto:** correr migraciones contra Neon desde la máquina de desarrollo.
- **Error observado:** el log dice `Conexión a PostgreSQL desde ConnectionStrings:Default (localhost/auralearn_dev)` y en producción aparece `42P01: relation "tutors" does not exist` aunque `/health` responde `Healthy`.
- **Causa:** `dotnet run` carga `launchSettings.json` → `ASPNETCORE_ENVIRONMENT=Development` → `appsettings.Development.json`, cuyo `ConnectionStrings:Default` apunta a `localhost` y **gana** sobre `DATABASE_URL_UNPOOLED` (prioridad #1 en ADR-010). Las migraciones se aplican al Postgres local y Neon queda vacío. `/health` solo hace `SELECT 1`, así que no detecta el esquema ausente.
- **Solución:** migrar con `ASPNETCORE_ENVIRONMENT=Production` + `DATABASE_URL_UNPOOLED`, y comprobar que el log diga `desde DATABASE_URL_UNPOOLED (ep-xxx.neon.tech/neondb)`. Desde 0.13.1 el arranque registra host/base y avisa `N migraciones pendientes`. Ver ERR-BE-007.
- **Aprendido en:** 2026-11-01 (primer deploy real; ERR-FE-005 en el catálogo de frontend).

## GOTCHA-013 — `addgroup`/`adduser` no existen en las imágenes slim de .NET

- **Contexto:** crear un usuario no-root en el stage runtime de `api/Dockerfile` (build en Vercel Container Runtime con buildah).
- **Error observado:** `/bin/sh: 1: addgroup: not found` → `exit status 127` y `[BUILD_FAILED]`. El stage de build pasa; falla solo el `RUN addgroup ... && adduser ...` del stage final.
- **Causa:** `adduser`/`addgroup` son del paquete Debian **`adduser`**, que no viene en las imágenes *slim* (y no existe en Ubuntu noble ni en Mariner). Las imágenes oficiales de .NET no los usan.
- **Solución:** **no crear usuario**. Desde .NET 8 las imágenes `runtime-deps`/`runtime`/`aspnet` ya incluyen `app` (UID/GID **1654**, `APP_UID`), creado con `groupadd`/`useradd` en el template oficial. Basta `USER app` y `COPY --chown=app:app`. Si hiciera falta crear uno, usar `groupadd`/`useradd` (paquete `passwd`/`shadow-utils`), nunca `adduser`.
- **Aprendido en:** 2026-10-31 (primer deploy a Vercel Container: `addgroup: not found`).

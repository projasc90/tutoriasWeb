# Summary — API .NET en Vercel Container Runtime + Vercel Blob

**Fecha:** 2026-10-30 · **Versión:** 0.13.0 · **ADR:** 009

## Qué se hizo

1. **Verificación de la premisa.** Se confirmó contra la documentación oficial que Vercel tiene *Container Runtime* (OCI desde `Dockerfile`, Beta en todos los planes) y guía oficial de ASP.NET Core. ADR-008 partía de la premisa contraria y quedó superado.

2. **Imagen compatible con Vercel (`api/`).**
   - `ENV PORT=8080` en `Dockerfile` + nota de que el proyecto Vercel debe definirlo (Vercel enruta a 80; la base ASP.NET escucha en 8080).
   - `Program.cs`: `app.Urls.Add` solo si `ASPNETCORE_URLS` no existe → evita doble registro con `entrypoint.sh`.
   - `api/.vercelignore`.

3. **Storage en Blob (`Infrastructure/Persistence/VercelBlobFileStorage.cs`).**
   - Implementa `IFileStorage` sobre la REST API de Blob v13 (`PUT /?pathname=`, `GET /?url=`, `POST /delete`) con `HttpClient`; sin SDK oficial .NET ni dependencias nuevas.
   - Nuevo `GetInfoAsync` en la interfaz, implementado también en `LocalFileStorage` para mantener paridad dev/prod.
   - Selección en DI: `Storage:Provider` o presencia de `BLOB_READ_WRITE_TOKEN`; si se fuerza Blob sin token, el arranque falla en vez de degradar a disco efímero.

4. **Subida directa (rompe el límite de 4.5 MB).**
   - `web/app/api/blob/upload/route.ts`: `handleUpload` de `@vercel/blob/client`; autentica contra `GET /api/auth/me` y exige que el pathname cuelgue del usuario. `GET` devuelve `{enabled}` para que el cliente degrade.
   - `POST /api/tutors/me/documents/attach` y `POST /api/tutors/me/photo/attach`: validan tamaño y content-type **reales** del store y la propiedad del pathname; nunca confían en la metadata del cliente.
   - `uploadTutorPhoto` intenta directo y cae al multipart si no hay store.
   - Sin webhook `onUploadCompleted` (no corre en localhost).

5. **Migraciones fuera del contenedor.** `api-deploy.yml` → `api-ci`: build+tests en PR; en `main` además job `migrate` one-shot contra Neon. Se elimina la publicación a GHCR y el redeploy a Railway.

6. **Documentación.** ADR-009; `infra.instructions.md` reescrito; guía nueva `deploy-guide.md`; guía de Railway marcada como superada; `.ai/` actualizado.

## Resultado

- Backend: build sin warnings, **158/158 tests**.
- Frontend: lint 0 errores, `tsc` limpio, build con **22 rutas**.

## Qué falta (usuario, en Vercel)

Crear los dos proyectos (Root `web` y Root `api` con preset `Container`), el Blob store privado, definir las variables de entorno, añadir `NEON_CONNECTION_STRING` a GitHub Secrets y correr el job `migrate`. Detalle en `deploy-guide.md`.

## Deuda registrada

- Container Runtime en Beta; cold start de 1-3 s tras scale-to-zero.
- Contrato REST de Blob replicado a mano: subir `x-api-version` obliga a tocar `VercelBlobFileStorage` (no hay tests de integración del cliente HTTP).
- Blobs huérfanos si `attach` falla después de subir.

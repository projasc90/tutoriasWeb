# Plan — API .NET en Vercel Container Runtime + Vercel Blob

**Fecha:** 2026-10-30 · **ADR:** 009 (supersede 007 y 008)
**Versión objetivo:** web `0.13.0`

## Contexto

El usuario solicitó desplegar la API .NET y el frontend en Vercel con un único
proveedor ligado a GitHub (repo personal). La nota previa en el repo
(ADR-008 / `infra.instructions.md`) afirmaba que Vercel no soporta contenedores
ni .NET. **Esa premisa era incorrecta**: verificado contra la documentación
oficial, Vercel tiene *Container Runtime* (imágenes OCI desde `Dockerfile`,
Beta en todos los planes) y una guía oficial para ASP.NET Core.

Decisiones del usuario: repo personal, storage **Vercel Blob**, ejecutar todo.

## Restricciones de plataforma que condicionan el diseño

| Restricción | Impacto | Mitigación |
|---|---|---|
| 4.5 MB por request en Vercel Functions | El límite de documentos/fotos es 5 MB → 413 antes de llegar a Kestrel; `[RequestSizeLimit]` no sirve | Subida directa navegador → Blob + `POST .../attach` |
| Filesystem efímero (solo `/tmp`, 500 MB) | `LocalFileStorage` (ADR-007) pierde archivos en cada scale-in | `VercelBlobFileStorage` |
| Puerto por defecto 80, base ASP.NET en 8080 | 502 en todas las rutas | `PORT=8080` en el proyecto `api` |
| `access` del store es inmutable | Corregir después obliga a recrear el store | Store **privado** único; fotos servidas por el API |
| `handleUpload` exige token read-write | El proyecto contenedor no puede firmar tokens | Lo firma el proyecto Next.js |
| `onUploadCompleted` no corre en localhost | Webhook poco fiable | Confirmación explícita `.../attach` |
| Sin SDK .NET de Blob | Riesgo de dependencia atada a Vercel | REST v13 con `HttpClient`, sin dependencias nuevas |

## Fases

1. **Imagen compatible** — `PORT` documentado y `ENV PORT=8080`, `ASPNETCORE_URLS`
   como única fuente de binding, `api/.vercelignore`.
2. **`VercelBlobFileStorage`** — implementación REST de `IFileStorage`, selección
   en `Program.cs` por `Storage:Provider` o presencia del token.
3. **Subida directa** — ruta Next `/api/blob/upload` (emite tokens, verifica JWT
   contra el API) + endpoints `.../attach` en .NET con validación de metadatos
   reales y de propiedad del pathname; frontend con degradación a multipart.
4. **Migraciones** — job `migrate` en `api-ci.yml`; Vercel ya no ejecuta el
   modo one-shot al arrancar.
5. **Proyectos Vercel** — pasos manuales del usuario (ver `deploy-guide.md`).
6. **Documentación** — ADR-009, `infra.instructions.md`, guía nueva, `.ai/`.

## Fuera de alcance

Vercel Daemons/Services, Vercel Postgres (se mantiene Neon), portal `portal/`,
object storage distinto de Blob.

## Verificación

Ver `verification.md` y el checklist de `deploy-guide.md`.

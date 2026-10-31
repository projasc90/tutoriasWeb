# Contexto — API .NET en Vercel Container Runtime + Vercel Blob

**Fecha:** 2026-10-30

## Solicitud

Desplegar la API .NET en un contenedor en Vercel ligado a GitHub, con `web` como
página principal. El usuario aportó una guía de "API .NET en Vercel" y pidió
verificarla. Decisiones posteriores: repo personal, storage **Vercel Blob**,
ejecutar todo.

## Verificación de la premisa aportada

| Afirmación del usuario | Veredicto |
|---|---|
| Vercel ejecuta imágenes OCI/Docker como Functions | ✅ Container Runtime, **Beta** en todos los planes |
| Vercel inyecta `PORT` (default 80) | ✅ default **80** |
| `Dockerfile` con `EXPOSE 8080` | ⚠️ `EXPOSE` no influye en el routing |
| `vercel.json` con `framework: container` | ⚠️ opcional: basta el `Dockerfile` en el root del proyecto |
| `ConfigureKestrel` + `ListenAnyIP` | ⚠️ innecesario; la guía oficial usa `UseUrls` |

Corrección importante: la nota previa del repo (ADR-008, `infra.instructions.md`)
afirmaba lo contrario. Se actualizó con ADR-009.

## Estado del código antes del cambio

- `IFileStorage` con una sola implementación: `LocalFileStorage` (disco).
- `TutorDocumentService.MaxSizeBytes` y `TutorPhotoService.MaxSizeBytes` = **5 MB**.
- `Program.cs`: `app.Urls.Add` incondicional si había `PORT`; `entrypoint.sh` exportaba `ASPNETCORE_URLS` → doble binding.
- `api-deploy.yml` publicaba a GHCR y desplegaba en Railway.

## Restricciones encontradas

1. Vercel Functions: **4.5 MB** de cuerpo por request → 413 con archivos de 5 MB, antes de llegar a Kestrel. `[RequestSizeLimit]` no ayuda.
2. Filesystem de Vercel: solo `/tmp` (500 MB), efímero por instancia.
3. `access` del Blob store (private/public) es **inmutable** tras crearlo.
4. `handleUpload` requiere `BLOB_READ_WRITE_TOKEN`; OIDC no sirve para firmar tokens de cliente.
5. `onUploadCompleted` no se dispara en localhost.
6. No hay SDK oficial de Vercel Blob para .NET.
7. Hobby no permite conectar repos de organizaciones Git (repo del usuario es personal: no aplica).

## Archivos leídos para el diseño

`api/Dockerfile`, `api/entrypoint.sh`, `api/AuraLearn.Api/Program.cs`,
`Controllers/{TutorDocuments,TutorPhotos}Controller.cs`,
`Application/Services/{TutorDocument,TutorPhoto}Service.cs`,
`Application/Interfaces/IFileStorage.cs`,
`Infrastructure/Persistence/LocalFileStorage.cs`,
`web/lib/api.ts`, `web/lib/auth.ts`, `web/package.json`,
`node_modules/@vercel/blob/dist/*` (contrato REST: v13, `x-vercel-blob-access`,
`PUT /?pathname=`, `GET /?url=`, `POST /delete`).

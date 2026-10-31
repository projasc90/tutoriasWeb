# Verificación — API en Vercel Container + Vercel Blob

**Fecha:** 2026-10-30

## Gates ejecutados (local)

| Gate | Comando | Resultado |
|---|---|---|
| Build backend | `cd api && dotnet build` | ✅ 0 errores, **0 warnings** |
| Tests backend | `cd api && dotnet test` | ✅ **158/158** (149 previos + 9 nuevos) |
| Type-check | `cd web && npx tsc --noEmit` | ✅ sin salida = limpio |
| Lint | `cd web && npm run lint` | ✅ 0 errores (10 warnings preexistentes) |
| Build frontend | `cd web && npm run build` | ✅ **22 rutas** (nueva: `ƒ /api/blob/upload`) |

## Tests nuevos (9)

Camino `attach` de subida directa (ADR-009):

**`TutorDocumentServiceTests`**
1. `Attach_WithOwnedPathAndMatchingMetadata_CreatesDocument` — pathname propio + metadatos coincidentes → crea; verifica que **no** se escribe por el API.
2. `Attach_WithPathFromAnotherUser_IsInvalidAndDoesNotPersist` — pathname de otro usuario → 400 sin consultar el store ni persistir (bypass de autorización cerrado).
3. `Attach_WhenStoredSizeDiffersFromDeclared_IsInvalid` — el store dice 9 MB y el cliente declara 1 KB → rechazado.
4. `Attach_WhenStorageContentTypeIsNotAllowed_IsInvalid` — `.exe`/`application/x-msdownload` → rechazado.
5. `Attach_WhenBlobMissingInStore_IsInvalid` — el blob no existe → rechazado.
6. `Attach_WhenIdentificationIsLockedInVerified_ReturnsConflict` — 409 `IDENTIFICATION_LOCKED`.

**`TutorPhotoServiceTests`**
7. `Attach_WithOwnedPathAndMatchingMetadata_UpdatesPhoto` — upsert correcto.
8. `Attach_WithPathFromAnotherUser_IsInvalid`.
9. `Attach_WhenStoredContentTypeIsNotAnImage_IsInvalid` — el cliente dice `image/jpeg`, el store tiene un PDF.

## Verificado contra el store real (2026-10-30)

Contrato REST de Blob validado con el token del store `auralearn-docs` (privado),
que es la parte sin cobertura de tests unitarios:

| Operación | Endpoint | Resultado |
|---|---|---|
| Subida | `PUT /?pathname=` con `x-add-random-suffix: 0` | ✅ 200, pathname exacto, `contentType` aplicado |
| Metadatos | `GET /?url=<pathname>` | ✅ 200 con `size` y `contentType` |
| Metadatos inexistente | `GET /?url=<pathname>` | ✅ 404 → `GetInfoAsync` devuelve `null` |
| Borrado | `POST /delete` | ✅ 200 |

Flujo completo con el SDK real (`upload()` de `@vercel/blob/client`) contra el
dev server:

| Caso | Resultado |
|---|---|
| Sonda `GET /api/blob/upload` | ✅ `{"enabled":true}` |
| Sin JWT | ✅ 400 `No autenticado` |
| JWT válido + pathname propio | ✅ 200 con `clientToken` |
| JWT válido + pathname de otro usuario | ✅ 400 `Ruta de subida no autorizada` |
| JWT válido + categoría desconocida | ✅ 400 `Categoría de archivo inválida` |
| Subida de **4.8 MB** (5033165 bytes) de extremo a extremo | ✅ subida + `head()` confirma `size` y `application/pdf` |

> **Corrección importante:** un archivo de 6 MB **no** debe subir — el tope de
> negocio es 5 MB (`TutorDocumentService.MaxSizeBytes`), y el token de cliente lo
> aplica vía `maximumSizeInBytes`. El rango que realmente se desbloquea es
> **4.5–5 MB**, que antes fallaba con 413 en Vercel Functions.

## Pendiente de verificación real (requiere Vercel)

No ejecutables en local (sin cuenta/proyecto):

- [ ] `GET /health` en el proyecto `api` → 200 `Healthy`
- [ ] `PORT=8080` aplicado; sin la variable el síntoma esperado es 502 en todas las rutas
- [ ] Subida de un PDF de **4.8 MB** completa (el rango 4.5–5 MB es el que antes fallaba con multipart)
- [ ] Descarga de documento: Admin 200, tutor 403
- [ ] Foto de perfil visible en `/tutores`
- [ ] Cold start medido tras 5 min sin tráfico
- [ ] Job `migrate` de `api-ci` en verde contra Neon

## Riesgos abiertos

1. **Container Runtime está en Beta.**
2. **Contrato REST de Blob replicado a mano** (no hay SDK .NET): si Vercel sube `x-api-version` de 13, hay que ajustar `VercelBlobFileStorage.ApiVersion`. Mitigación: los tests cubren la lógica de negocio, pero el cliente HTTP no tiene tests de integración.
3. **Blobs huérfanos** si `attach` falla después de la subida (hay que limpiarlos a mano).
4. **`BLOB_READ_WRITE_TOKEN` es estático de larga vida**; su rotación rompe subidas en curso.

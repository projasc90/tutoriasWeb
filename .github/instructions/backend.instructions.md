---
applyTo: "api/**/*.cs, api/**/*.csproj, api/**"
description: Reglas para el backend .NET de AuraLearn (reservado, aplican desde el primer archivo creado)
---

# Backend — AuraLearn (.NET Core)

## Arquitectura de capas

```
AuraLearn.Api/            # Controllers, middlewares, configuración; sin lógica de negocio
AuraLearn.Application/    # Casos de uso, servicios, FluentValidation, contratos
AuraLearn.Domain/         # Entidades, value objects, reglas invariantes; sin dependencias
AuraLearn.Infrastructure/ # EF Core, repositorios, integraciones externas (SINPE, correo)
```

- Dependencias apuntan hacia `Domain`. `Api` → `Application` → `Domain`; `Infrastructure` implementa contratos de `Application`.
- Controllers finos: mapean DTO ↔ comando/consulta; toda validación de negocio en `Application` con FluentValidation.

## Reglas async

- `async/await` hasta el final de la cadena; nunca `.Result`/`.Wait()`/`.GetAwaiter().GetResult()`.
- Métodos I/O con `CancellationToken` propagado.
- Endpoints async de extremo a extremo; I/O-bound en repositorios, no bloquear el request en CPU-bound pesado.

## Workers / colas

No existen aún. Cuando se agreguen (ej. verificación de atestados async): idempotencia obligatoria, reintentos con backoff, dead-letter, y el flujo de pagos nunca depende solo del worker (confirmación sinpe síncrona primero).

## Auditoría funcional

- Log estructurado con correlation-id por request; eventos de negocio como `PaymentVerified`, `SlotReserved`, `TutorVerified`.
- Auditoría funcional separada del log técnico: qué decidió el sistema (aprobó/rechazó y por qué), no solo stack traces.

## Testing gate backend

```bash
cd api && dotnet build && dotnet test
```

- Unit tests de dominio y Application; integración con EF InMemory/SQLite o Testcontainers contra PostgreSQL para flujos críticos (pagos, slots).
- TDD obligatorio (test-first) en: auth/JWT, pagos SINPE + firma digital, motor de slots, migraciones críticas.

## Seguridad

- JWT emitido aquí; roles `ESTUDIANTE`, `TUTOR`, `ADMIN` como claims; `[Authorize]` por política, no chequeos ad-hoc.
- FluentValidation en todo input; nunca confiar en validación del frontend.
- Secretos en `appsettings.Development.json` (dev, ignorado por git) y variables de entorno/Key Vault en prod. **Nunca en `appsettings.json`.**
- CORS restringido a dominios reales (Vercel/Netlify del frontend).

## Observabilidad

- Healthcheck endpoint; request logging con nivel estructurado; excepciones con stack trace correlacionado; métricas mínimas (latencia, tasa 5xx).

## Migraciones (EF Core)

- Ubicación: `api/Migrations/`. Comandos: `dotnet ef migrations add <Nombre>`, `dotnet ef database update`.
- **Toda migración incluye al final del archivo una sección de comentarios `// ROLLBACK MANUAL`** con los pasos inversos (SQL o comandos) además del método `Down()`. No asumir rollback automático.
- Cambios destructivos (drop de columnas, migración de datos) requieren plan en `docs/plans/` y ADR si son irreversibles.

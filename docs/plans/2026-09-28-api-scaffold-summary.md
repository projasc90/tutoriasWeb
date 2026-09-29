# Summary — Scaffold backend .NET en api/

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-api-scaffold-plan.md)

## Qué cambió

- **Nueva solución .NET 10 en `api/`** (Clean Architecture, ADR-003):
  - `AuraLearn.Api` — `Program.cs` (DI, JWT bearer, CORS localhost:3000, Swagger dev, `/health`), `Controllers/TutorsController.cs`, `appsettings.json` (sin secretos).
  - `AuraLearn.Application` — `Dto/TutorDto.cs` (+`PagedResult<T>`), `Interfaces/ITutorRepository.cs` (+`TutorSearchCriteria`), `Services/TutorService.cs` (+`TutorSearchCriteriaValidator` FluentValidation).
  - `AuraLearn.Domain` — `Entities/Tutor.cs`, `Enums/Role.cs` (Estudiante/Tutor/Admin), `Enums/VerificationStatus.cs` (PendingReview/UnderReview/Verified/Rejected). Sin dependencias externas.
  - `AuraLearn.Infrastructure` — `Persistence/AuraLearnDbContext.cs` (snake_case, índices, seed 12 tutores), `Persistence/TutorRepository.cs` (AsNoTracking, paginación en SQL, solo `Verified`).
- **Migración** `20260928075820_InitialCreate` en `AuraLearn.Infrastructure/Migrations/` con `// ROLLBACK MANUAL` completo (regla del repo). Aplicada a BD local `auralearn_dev`.
- **Tests** `tests/AuraLearn.Tests/TutorSearchCriteriaValidatorTests.cs` (5 casos).
- **BD local:** rol `auralearn` y BD `auralearn_dev` creados en el PostgreSQL ya presente en la máquina.
- `.gitkeep` de `api/` eliminado (la carpeta ya no está vacía).

## Qué NO cambió

- `web/` sigue sin conectarse a la API (usa su mock de `lib/data/`); la integración es la tarea siguiente.
- Sin endpoints de autenticación (JWT configurado pero inactivo).
- Sin motor de slots, sin pagos SINPE, sin verificación de tutores (solo el enum y el filtro por `Verified`).

## Endpoints disponibles

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/health` | Healthcheck |
| GET | `/api/tutors` | Catálogo público: `query`, `university`, `minRating`, `priceMin`, `priceMax`, `page`, `pageSize` (máx 50) |
| GET | `/swagger` | Documentación interactiva (solo dev) |

## Deuda / próximos pasos

1. **Conectar el frontend** a `GET /api/tutors` (reemplazar mock de `web/lib/data/tutors.ts` por hook de datos).
2. Endpoints de auth (`POST /api/auth/login` y registro) con JWT + roles.
3. Motor de slots/reservas (skill `slots-reservations`) para reemplazar el `nextSlot` demo.
4. Mover la validación de `pageSize` al pipeline HTTP con `ProblemDetails` consistente (hoy valida el servicio; el controller no devuelve 400 tipado).
5. Docker/compose para reproducibilidad del entorno dev.

## Verificación

Ver [verification](./2026-09-28-api-scaffold-verification.md): build ✅, tests ✅ (5), migración aplicada ✅, smoke de endpoints ✅.

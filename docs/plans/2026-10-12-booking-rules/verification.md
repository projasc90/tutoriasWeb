# Verificación — Reglas de reserva definidas por Admin (booking rules)

**Fecha:** 2026-10-12 · **Plan:** [plan](./plan.md)

## Gates

| Gate | Comando | Resultado |
|------|---------|-----------|
| Backend build | `cd api && dotnet build` | ✅ 0 errores, 0 warnings |
| Backend tests | `cd api && dotnet test` | ✅ **144/144** (121 previos + 23 nuevos) |
| Frontend lint | `cd web && npm run lint` | ✅ 0 errores (2 warnings preexistentes: `zonaLabel`, `children`) |
| Frontend tsc | `cd web && npx tsc --noEmit` | ✅ sin errores |
| Frontend build | `cd web && npm run build` | ✅ 18 rutas (nueva `/admin/configuracion`) |

## Tests nuevos (TDD)

- `BookingRulesServiceTests` (10): lectura con/sin fila, edición con auditoría (`UpdatedAt`/`UpdatedByUserId`), limpieza de reglas (null), rangos por campo con códigos específicos (`BOOKING_RULES_INVALID_*`), límites inferior/superior válidos, no persiste si es inválido.
- `ReservationServiceTests` (+5): ventana de pago desde BD (30 min → `ExpiresAt` ≈ +30 min; sin fila → 60 min), cancelación con regla 24 h (20 h restantes → no acredita; 30 h → acredita), sin fila → default 12 h.
- Constructores actualizados: `ReservationServiceTests` (×2) y `ReservationCompletionTests` reciben `IBookingRulesRepository` mockeado.

## Migración

- `AddBookingRules` aplicada a `auralearn_dev` (`20261012071303`). Fila seed verificada por psql: `12 / 60 / 60 / 14`, id `00000000-0000-0000-0000-000000000001`. `Down()` + bloque `// ROLLBACK MANUAL` en el archivo.

## Smoke API (curl)

| Caso | Resultado |
|------|-----------|
| `GET /api/booking-rules` sin token | ✅ 200 con seed `{12,60,60,14}` |
| `PUT /api/admin/booking-rules` con JWT Admin | ✅ 200, persiste `{24,45,null,21}` + `updatedAt` |
| `PUT` con JWT Estudiante | ✅ 403 |
| `PUT` con `cancellationNoticeHours: 200` | ✅ 422 `BOOKING_RULES_INVALID_CANCELLATION_NOTICE` |
| `GET` tras PUT | ✅ refleja valores persistidos |
| Restauración | ✅ PUT `{12,60,60,14}` → estado final = valores actuales |

> Usuario Admin de prueba creado para el smoke: `admin.test@auralearn.dev` / `Admin1234!` (rol promovido a 3 en BD). Eliminable tras revisión.

## Smoke navegador

- `/tutor/disponibilidad`: tarjetas muestran valores de BD — "12 horas", "60 minutos", "2 semanas" (14 días → formato semanas) + nota "Definidas por la administración de AuraLearn (solo lectura)". Ya no aparece "(AuraLearn)" hardcodeado.
- `/admin/configuracion` (sesión no Admin): renderiza guard "Acceso restringido" correctamente. El formulario completo queda verificado por contrato API + tsc (no se inició sesión Admin en el navegador del usuario para no sobrescribir su sesión).
- Nota: el 403 del `GET me/availability` en el navegador es la sesión preexistente del usuario (rol no tutor), ajeno a este cambio.

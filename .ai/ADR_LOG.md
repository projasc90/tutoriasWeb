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

---
applyTo: "**/*.test.ts, **/*.test.tsx, **/*.spec.ts, test/**, web/__tests__/**, api/tests/**"
description: Testing gate, planes de prueba y ejecución ligera para AuraLearn
---

# Testing — AuraLearn

## Estado actual

- Frontend: **sin suite de tests** (landing + directorio estático). Al añadir el primer test: Vitest + Testing Library en `web/`.
- Backend: `api/` reservada. Al crear el proyecto .NET: xUnit + FluentAssertions; integración con Testcontainers (PostgreSQL).
- E2E: Playwright **no configurado**; no aplica su gate hasta configurarlo.

## Testing gate (comandos exactos)

```bash
cd web && npm run lint && npx tsc --noEmit      # Frontend (activo hoy)
cd api && dotnet build && dotnet test           # Backend (cuando exista)
cd web && npm run build                          # Build cuando el riesgo lo amerite
```

Excepciones válidas al gate: cambios solo `.md`, CSS puro sin lógica, `.env`, `docker-compose`, monitoreo.

## Estructura de tests por capa

| Capa | Ubicación | Alcance |
|------|-----------|---------|
| Dominio (.NET futuro) | `api/tests/AuraLearn.Domain.Tests/` | Reglas invariantes puras |
| Application (.NET futuro) | `api/tests/AuraLearn.Application.Tests/` | Servicios, validators, casos de uso |
| Integración (.NET futuro) | `api/tests/AuraLearn.IntegrationTests/` | API + BD real (Testcontainers) |
| Unidad frontend | `web/__tests__/` o junto al módulo | Hooks, filtros (`/tutores`), utilidades |
| Componentes frontend | `web/__tests__/components/` | Render + interacción con Testing Library |
| E2E (futuro) | `web/e2e/` con Playwright | Flujos críticos: reserva, pago SINPE, login |

## Planes de prueba

No solo funcionales generales; todo plan cubre como mínimo:

- **Unitarios** del módulo o servicio afectado.
- **Integración/integrales** para flujos entre capas o dependencias reales.
- **Frontend** si hay UI o componentes interactivos.
- **Regresión** cuando se corrige un bug o se evita que un fallo vuelva a ocurrir.

Reglas:

1. Si existe carpeta `test/` para planes `.md`: crear/actualizar el **plan padre** del módulo afectado.
2. Cambios grandes → planes segmentados padre/hijos por foco: `<plan>-unit.md`, `<plan>-integration.md`, `<plan>-frontend.md`, `<plan>-regression.md`.
3. Bug productivo corregido → **caso de regresión** en el plan correspondiente + test automatizado cuando aplique.
4. Al cambiar código, actualizar el plan afectado en la misma tarea (protocolo de actualización): marcar escenarios nuevos, obsoletos o modificados con fecha.

## Ejecución ligera por cambio (regla de oro)

1. Correr primero la validación más acotada y falsable del área tocada.
2. Si existe test puntual o suite de módulo, usarla antes que toda la suite (`dotnet test --filter "FullyQualifiedName~Slots"`, `vitest run path/to/file`).
3. Frontend con TS: mínimo `npm run lint` + `npx tsc --noEmit`.
4. Backend: mínimo unit o integración según la capa afectada.
5. Escalar a suites mayores o build solo si riesgo/alcance lo justifican.

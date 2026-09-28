---
id: protocols
priority: 2
loadWhen: testing-migracion-despliegue-seguridad
maxLines: 150
---

# Protocolos — AuraLearn

## 1. Testing Gate (obligatorio)

Comandos reales de este repo:

| Área | Comando | Estado |
|------|---------|--------|
| Backend | `cd api && dotnet build && dotnet test` | Aplicable cuando exista el proyecto .NET (`api/` hoy reservada) |
| Frontend | `cd web && npm run lint && npx tsc --noEmit` | Activo |
| E2E | Playwright | **No configurado — no aplica por ahora** |
| Build | `cd web && npm run build` | Activo (cuando el riesgo/alcance lo amerite) |

Reglas:
1. Si fallan tests existentes, se corrige antes de continuar.
2. Si no hay tests para el código modificado, se crea al menos uno cuando el cambio sea de código y aplique.
3. Si el cambio afecta más de una capa, se validan todas las capas tocadas.
4. Cambios solo `.md`, CSS puro, `.env`, `docker-compose` o monitoreo sin lógica: el gate puede no aplicar.

## 2. Ejecución ligera por cambio

1. Correr primero la validación más acotada y falsable del área tocada (test puntual o suite de módulo antes que toda la suite).
2. Frontend con TS: mínimo `npm run lint` + `npx tsc --noEmit` (no hay script `type-check` dedicado).
3. Backend: mínimo unit o integración según la capa afectada.
4. Escalar a `npm run build` o suites completas solo si el riesgo o el alcance lo justifican (cambios estructurales, rutas, SSR, bundling, config).

## 3. TDD escalado por riesgo

- **Obligatorio test-first:** auth/seguridad (JWT, roles), pagos SINPE y validación de firma digital, motor de slots/reservas, migraciones críticas, reglas de negocio sensibles.
- **Recomendado test-first:** servicios compartidos, schemas complejos, helpers nucleares.
- **Test-after permitido:** documentación, refactors de bajo riesgo, ajustes visuales menores.

## 4. Revisión en dos etapas (cambios grandes)

Cuando el cambio activa `docs/plans/`:
- **Gate 1 — Cumplimiento de especificación:** coincide con lo pedido en `plan`? criterios de aceptación cubiertos?
- **Gate 2 — Calidad técnica:** arquitectura, seguridad, performance (si aplica), tests ejecutados, deuda técnica generada.

## 5. Metodología por fases (`docs/plans/`)

**Activar cuando:** toque múltiples archivos o módulos relevantes, impacto transversal (backend↔frontend↔contratos↔docs), riesgo medio/alto para producción, o cambio de más de cinco archivos relevantes.

Artefactos `YYYY-MM-DD-<tema>-<tipo>.md`:
- `context`: alcance, decisiones, restricciones, dudas cerradas.
- `plan`: tareas atómicas, archivos esperados, criterios de aceptación.
- `verification`: comandos ejecutados, evidencia, riesgos residuales. **No reemplaza el testing gate.**
- `summary`: qué cambió, qué no cambió, deuda y próximos pasos. Se completa **antes** del cierre de `.ai/`.

## 6. Auditoría funcional vs logs técnicos

- El backend debe loguear con correlation-id por request y niveles estructurados; el QA funcional se hace contra la UI/contrato, no contra logs.
- Al depurar: comparar "lo que el usuario vio" contra "lo que el log dice"; cualquier divergencia es bug de observabilidad.

## 7. Async / workers / colas

- No existen aún. Cuando se introduzcan (ej. verificación de atestados, envío de comprobantes): operación idempotente, reintentos con backoff, dead-letter, y nada crítico de pagos dependiendo solo de un worker.

## 8. Observabilidad

- Mínimo en backend: request logging, excepciones con stack, healthcheck, métricas básicas (latencia, errores 5xx).
- Frontend: errores de runtime reportados; nunca tragar excepciones de fetch en silencio.

## 9. Seguridad

- JWT emitido por el backend .NET; roles ESTUDIANTE, TUTOR, ADMIN. El frontend no firma ni valida tokens de negocio.
- Secretos: nunca en el repo; `.env.local` en dev, Key Vault/equivalentes en prod.
- Validación siempre en backend (FluentValidation); la validación del frontend es solo UX.
- SINPE: el backend valida la firma digital del comprobante; el frontend solo muestra estados.
- CORS restringido a dominios de despliegue reales.

## 10. Integraciones externas

- Google Fonts (Material Symbols) y `next/font/google` — únicas dependencias de red actuales del frontend.
- Toda integración nueva (pagos, correo, storage) requiere ADR y se registra en `STACK.md`.

## 11. ORM / acceso a datos (EF Core)

- Migraciones con `dotnet ef` en `api/Migrations/`; nunca SQL ad-hoc en producción sin script versionado con rollback manual.
- Queries en la capa de infraestructura, no en controllers; `AsNoTracking()` en lecturas; paginación obligatoria en listados.

## 12. Versionado

- SEMVER en `web/package.json` (actual `0.1.0`): `MAJOR.MINOR.PATCH`.
- Al cerrar tarea significativa: bump acorde + entrada en `.ai/version-changes.md` con clasificación `feat|fix|refactor|db|test|docs`.

## 13. Gobernanza full-stack de testing

- Planes de prueba cubren: unitarios del módulo, integración/integrales, frontend (UI interactiva), y regresión cuando se corrige un bug.
- Si existe carpeta `test/` para planes `.md`: crear/actualizar el plan padre del módulo afectado; cambios grandes → planes padre/hijos por foco (`unit`, `integration`, `frontend`, `regression`).
- Bug productivo corregido → caso de regresión en el plan correspondiente y, cuando aplique, test automatizado de regresión.
- Detalle operativo en `.github/instructions/testing.instructions.md`.

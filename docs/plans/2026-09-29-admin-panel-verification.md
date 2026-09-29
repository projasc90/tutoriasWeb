# Verification — Panel admin de aprobación de tutores

**Fecha:** 2026-09-29
**Plan:** [plan](./2026-09-29-admin-panel-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `cd web && npm run lint` | ✅ 0 errores (3 warnings pre-existentes en archivos no tocados) |
| `cd web && npx tsc --noEmit` | ✅ exit 0 |
| `cd web && npx next build` | ✅ 7 rutas incluida `/admin/tutores` |

## Smoke de integración (navegador real + backend `dotnet run`)

| Escenario | Resultado |
|-----------|-----------|
| Login como admin (pabrc99@gmail.com, rol 3) | ✅ redirige a `/tutores`, header "Hola, Jose" |
| `/admin/tutores` con rol Admin | ✅ cola visible: "1 postulación pendiente" con tarjeta completa (nombre, credenciales, universidad, bio, subjects, tarifa, fecha) |
| Modal de rechazo se abre | ✅ dialog con textarea y validación |
| Confirmar rechazo sin motivo | ✅ error client-side: "Describe el motivo (mínimo 10 caracteres)" |
| Confirmar rechazo con motivo válido | ✅ cola se recarga → "Cola al día" (item sale de la lista) |
| Estado en BD | ✅ `verification_status = 4 (Rejected)` con `rejection_reason` persistido (verificado con psql) |
| Nota de logs | El navegador reportó `ERR_ABORTED` en el PATCH (comportamiento de dev-tools al navegar; la acción SÍ se procesó — confirmado por la BD y la recarga de la cola) |

## Evidencia estructural

- `web/lib/api.ts` — `fetchAdminApplications` + `verifyApplication` (espejo de `AdminTutorApplicationsController`: GET cola, PATCH `{id}/verify` con `{decision, reason}`).
- `web/hooks/use-admin-applications.ts` — cola con estados loading/error/success, `decide()` con recarga automática, `deciding` (id en curso).
- `web/app/admin/tutores/page.tsx` — guard por rol (loading/anon/403/cola), estados vacíos y de error con retry.
- `web/components/admin/application-card.tsx` — tarjeta con datos del postulante + acciones.
- `web/components/admin/reject-dialog.tsx` — modal con motivo obligatorio (≥10 chars).

## Problemas resueltos durante la implementación

1. **Hook inicial enredado** (tipos genéricos innecesarios) → reescrito con `AdminState` simple y `decide()` limpio.
2. **`useRouter` sin uso en currency-toggle** (warning pre-existente de la tarea auth-ui) → import eliminado.

## Riesgos residuales

- El guard es client-side; la autorización real es del backend (`[Authorize(Roles=Admin)]`) — correcto por diseño, pero la página expone su existencia a cualquiera (solo muestra 403 UI).
- Sin paginación de la cola (volumen bajo; el backend la soporta).
- Sin notificaciones al tutor (email es tarea aparte).
- El smoke de "aprobar" no se ejecutó para no alterar el catálogo con datos de prueba; el flujo approve usa el mismo `decide()` verificado con reject (mismo endpoint, decisión distinta).

> Nota: esta verificación no sustituye al testing gate (ejecutado por separado con evidencia arriba).

# Context — Panel admin de aprobación de tutores

**Fecha:** 2026-09-29
**Plan asociado:** [plan](./2026-09-29-admin-panel-plan.md)

## Alcance

UI admin en `web/` para la cola de verificación de tutores (el backend ya existe desde v0.4.0):

- `web/lib/api.ts` — `fetchAdminApplications(token)` y `verifyApplication(token, id, decision, reason?)`.
- `web/hooks/use-admin-applications.ts` — cola con estados y reload tras acción.
- `web/app/admin/tutores/page.tsx` — página protegida por rol Admin (guard client-side).
- `web/components/admin/` — `ApplicationCard` y `RejectDialog` (motivo obligatorio).

## Decisiones

1. **Guard client-side con `useAuth`:** el rol Admin viene del JWT; el backend ya valida `[Authorize(Roles=Admin)]` — la UI solo oculta, no autoriza.
2. **Rechazo con motivo obligatorio** (modal con textarea) — espejo del backend (`reason` obligatorio en reject).
3. **Sin librerías nuevas:** tokens MD3 + `MaterialIcon`.
4. **Cola:** muestra `PendingReview` y `UnderReview` (ambas revisables); sin paginación UI por ahora (volumen bajo).
5. **Acción optimista:** tras aprobar/rechazar se recarga la cola (el item desaparece de la lista revisable).

## Restricciones

- El backend debe estar corriendo (`dotnet run` en `localhost:5037`).
- Usuario admin de prueba ya existe: `pabrc99@gmail.com` (rol 3 en BD).
- Copy en español; tokens MD3; sin cambios en el backend.

## Dudas cerradas

- **¿Paginación?** Backend la soporta; UI simple sin paginar (volumen bajo).
- **¿Notificaciones al tutor?** Fuera de alcance (email es tarea aparte).
- **¿Historial de decisiones?** Solo si el backend devuelve Verified/Rejected en el listado; la cola prioriza lo revisable.

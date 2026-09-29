# Contexto — Onboarding del Profesor (postulación a tutor)

**Fecha:** 2026-09-28
**Pantalla de referencia:** Stitch "Onboarding del Profesor – Registro Paso a Paso" (proyecto 12227662788794145381, screen 9d54a40de7cc478a9800c4ee1d0ecfac). El HTML/PNG no estaba descargado en `design/stitch/`; la UI se construye con el design system del repo (tokens MD3 de `web/app/globals.css`, patrones de la landing y de `/tutores`) para mantener el estándar visual.

## Alcance

Página web `/postular` donde un profesor:
1. Crea su cuenta (o continúa si ya tiene sesión).
2. Llena sus datos de perfil: credenciales, universidad, bio, materias y tarifa.
3. Envía su postulación → queda `PendingReview`, invisible en el catálogo público.
4. Ve pantalla de confirmación con la promesa de revisión ≤48 h.

Backend necesario para soportar el flujo (hoy inexistente):
- Vínculo `User ↔ Tutor` (columna `tutors.user_id`).
- `POST /api/tutor-applications` (autenticado) → crea fila `Tutor` con `VerificationStatus = PendingReview`.
- `GET /api/tutor-applications/status` (autenticado) → progreso de la postulación.
- `GET /api/admin/tutor-applications` + `PATCH /api/admin/tutor-applications/{id}/verify` (rol Admin) → aprobación/rechazo con motivo.

## Decisiones

| # | Decisión | Justificación |
|---|----------|---------------|
| D1 | El wizard incluye creación de cuenta (paso 1) | La pantalla Stitch es "Registro Paso a Paso"; sin auth UI previa, el flujo debe ser autosuficiente |
| D2 | El rol `Tutor` se otorga al aprobar (no al postular) | El postulante sigue siendo `Estudiante` hasta `Verified`; evita auto-elevación de rol |
| D3 | Re-postulación permitida si la anterior fue `Rejected` (actualiza la fila y vuelve a `PendingReview`) | Regla de la skill `tutor-verification` |
| D4 | Sin upload de documentos en v1 | Storage con URLs firmadas es otra pieza; los campos de texto cubren el flujo de aprobación |
| D5 | UI admin de aprobación fuera de alcance (solo endpoints) | La tarea del usuario es la página del profesor; el panel admin es tarea separada |
| D6 | Validación frontend manual (sin zod/react-hook-form) | Evita librería nueva sin ADR; la validación autoritativa es FluentValidation en backend |
| D7 | Token de sesión en `localStorage` (`auralearn.auth`) | Contexto de sesión global es tarea aparte (Auth UI); el wizard necesita lo mínimo |
| D8 | Endpoints de aplicación con `[Authorize]` (cualquier usuario autenticado) | El postulante aún no tiene rol `Tutor`; los endpoints admin sí quedan gated por rol |

## Restricciones

- TDD obligatorio en backend (auth/verificación es dominio crítico): tests ANTES de implementación.
- Migración con `// ROLLBACK MANUAL` (convención del repo).
- Catálogo público (`GET /api/tutors`) ya filtra `Verified` — los pendientes nunca son visibles (verificado en `TutorRepository.SearchAsync`).
- Sin terminal disponible en esta sesión: migración EF escrita a mano siguiendo el patrón de `AddUsersTable` (incluye Designer + Snapshot).
- Testing gate: `cd api && dotnet build && dotnet test` + `cd web && npm run lint && npx tsc --noEmit` + `npm run build` (cambio estructural: ruta nueva).

## Dudas cerradas

- ¿Panel admin UI? → No (D5). ¿Documentos? → No en v1 (D4). ¿Wizard con registro? → Sí (D1).

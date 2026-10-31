# Plan — Completar sección del tutor

## Fase 0 — Promoción de rol (bloqueo crítico)
1. `IUserRepository.UpdateAsync(User)` + implementación EF.
2. TDD: `Decide_ApprovePendingApplication_PromotesUserToTutorRole`, `Decide_RejectPendingApplication_DoesNotChangeUserRole`, `Decide_ApproveWithoutUserId_DoesNotTouchUsers`.
3. `TutorApplicationService.DecideAsync` (approve): cargar user por `tutor.UserId`, `Role = Role.Tutor`, `UpdateAsync`.

## Fase 1 — Perfil editable (`GET/PUT /api/tutors/me/profile`)
1. Migración EF `AddTutorSpecialization` (columna nullable `tutors.specialization` varchar(100)) + rollback manual.
2. `Tutor.Specialization` + config DbContext; `TutorDto` + `Specialization` (mapeos en `TutorService`).
3. DTOs nuevos: `TutorProfileResponse`, `UpdateTutorProfileRequest`, `TutorProfileResult<T>`, `TutorProfileErrors`.
4. `UpdateTutorProfileValidator` (reglas espejo de la postulación) + `TutorProfileService` (get/update por userId; Rejected → Conflict).
5. `TutorProfileController` `[Authorize]`: GET 200/404 · PUT 200/400/404/409 (devuelve estado canónico, desviación del 204 planificado).
6. `SubmitTutorApplicationRequest` + `AdminTutorApplicationDto` + `Specialization`; DI en `Program.cs`.

## Fase 2 — Frontend `/tutor/perfil` + navegación
1. `lib/types/tutor-profile.ts`; `specialization` en `Tutor` (api.ts + types/tutor.ts) y `AdminTutorApplication`; `fetchTutorProfile`/`updateTutorProfile`.
2. Hook `use-tutor-profile` (estados tipados + save con saving/saved/saveError).
3. `app/tutor/layout.tsx` (SiteHeader + `TutorSubnav` Perfil·Disponibilidad·Sesiones); `app/tutor/page.tsx` redirect a perfil; SiteHeader retirado de sesiones/disponibilidad.
4. `app/tutor/perfil/page.tsx` + `ProfileForm` (Field/inputClass, tag-editor, tarifa USD ÷520, barra sticky) + `ProfileStatusCard` (badge, progreso 3 pasos, motivo de rechazo + CTA re-postular, nota re-login).
5. `PanelMenu`: "Mi perfil de tutor"; perfil público y tarjeta admin muestran la especialización.

## Fase 3 — Wizard `/postular`
Campo Especialización en `StepProfile` (opcional ≤100), payload, fila en `StepReview`, ítem de timeline "Configura tus horarios" en `Confirmation`.

## Fase 4 — Verificación y cierre
Gates (`dotnet build/test`, `lint/tsc/build`), smoke E2E API, bump 0.7.0, cierre `.ai/`.

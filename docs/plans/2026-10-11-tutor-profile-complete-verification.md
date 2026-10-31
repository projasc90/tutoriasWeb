# Verificación — Completar sección del tutor

| Verificación | Resultado |
|---|---|
| `dotnet build` (api/) | ✅ 0 errores |
| `dotnet test` (api/) | ✅ **90/90** (79 previos + 11 nuevos: 4 promoción de rol, 6 perfil, 1 especialización en submit) |
| Migración `AddTutorSpecialization` | ✅ generada con tooling EF + aplicada a `auralearn_dev`; rollback manual incluido |
| `npm run lint` (web/) | ✅ 0 errores (4 warnings preexistentes de fuentes) |
| `npx tsc --noEmit` (web/) | ✅ verde |
| `npm run build` (web/) | ✅ 16 rutas (3 nuevas: `/tutor`, `/tutor/perfil` + layout) |
| Smoke E2E API | ✅ registro → postulación con especialización (202) → `GET me/profile` 200 (PendingReview) → `PUT me/profile` 200 persiste → admin approve 204 → **`users.role` pasa a 2 (Tutor) en BD** → re-login `role: Tutor` → `GET me/availability` 200 (antes 403) → limpieza de datos de smoke (DELETE cascade) |
| Test flaky arreglado | ✅ `GenerateSlots_con_reglas_solo_genera_dentro_de_las_franjas` era dependiente de la fecha (ventana de 8 días desde hoy+7 no siempre contiene 2 jueves); ahora alinea el inicio por fecha local CR → determinista |

## Bugs encontrados durante la implementación

1. **DI faltante:** `TutorProfileService` no estaba registrado en `Program.cs` → 500 al activar el controller. Detectado por el smoke, corregido.
2. **Test flaky por fecha** (ver tabla): preexistente, no causado por este cambio; arreglado con alineación local CR.
3. `LayoutProps<"/tutor">` no satisface la restricción de tipos en esta config de Next 16 (solo `"/"`) → layout con `{ children: ReactNode }`.
4. Regla lint `react-hooks/set-state-in-effect`: la re-sincronización del formulario tras guardar usa el patrón "adjust state during render" en vez de `useEffect`.

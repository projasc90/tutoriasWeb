# Summary — Auth UI en el frontend (login/registro + sesión)

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-auth-ui-plan.md)

## Qué cambió

- **Nuevos:**
  - `web/app/login/page.tsx` — formulario de login (validación client-side, estados loading/error, redirección a `/tutores`).
  - `web/app/registro/page.tsx` — formulario de registro (espejo del backend: nombre, email, password ≥8).
  - `web/hooks/use-auth.tsx` — `AuthProvider` + `useAuth`: contexto global de sesión con login/register/logout y restauración al montar.
- **Ampliado:**
  - `web/lib/auth.ts` — `decodeJwt`, `toSession`, `isExpired`; `StoredSession` con `tokenExpiresAt`; `readSession` descarta sesiones expiradas (compat con sesiones antiguas sin expiración).
  - `web/lib/api.ts` — `fetchMe(token)` + `MeResponse` (valida Bearer contra `/api/auth/me`).
  - `web/components/landing/currency-toggle.tsx` — `SiteHeader` con sesión: "Hola, {nombre}" + "Cerrar sesión" (autenticado) o link "Iniciar Sesión" (anónimo); client component.
  - `web/app/layout.tsx` — `AuthProvider` envuelve toda la app.
  - `onboarding-wizard.tsx` — adaptado a `saveSession(toSession(...))`.
- **Versión:** `web/package.json` `0.2.1` → `0.3.1` (clasificación `feat`).

## Qué NO cambió

- El backend no se tocó (endpoints ya probados en la tarea anterior).
- Sin librerías nuevas (React Context + fetch nativo + localStorage).
- `useTutors` y el directorio no cambiaron de comportamiento.
- El wizard `/postular` conserva su flujo; solo se adaptó al tipo de sesión con expiración.

## Logro de la tarea

**Ciclo de autenticación completo end-to-end:** registro y login contra el backend .NET real, sesión persistente con expiración JWT, y header reactivo en todo el sitio. El flujo full-stack funciona: UI → `useAuth` → `lib/api.ts` → `AuthController` → BD.

## Deuda / próximos pasos

1. **Cookies httpOnly** en lugar de localStorage (mitigación XSS — ADR necesario).
2. **Middleware de Next** para guard de rutas server-side cuando existan rutas privadas reales.
3. **Refresh tokens** (deuda backend) — el frontend ya maneja expiración mostrando login.
4. Página de perfil/panel de estudiante con datos de `/api/auth/me` (el claim email aún no se mapea — deuda backend menor).

## Verificación

Ver [verification](./2026-09-28-auth-ui-verification.md): gate verde + smoke completo con navegador (login, registro, logout, persistencia en recarga, usuario en BD).

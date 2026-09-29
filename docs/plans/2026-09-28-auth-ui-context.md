# Context — Auth UI en el frontend (login/registro + sesión)

**Fecha:** 2026-09-28
**Plan asociado:** [plan](./2026-09-28-auth-ui-plan.md)

## Alcance

UI de autenticación en `web/` conectada a los endpoints reales del backend:

- `web/lib/auth.ts` — tipos de sesión, storage en localStorage, `decodeJwt` para expiración.
- `web/lib/api.ts` — `registerUser()`, `loginUser()`, `fetchMe(token)`.
- `web/hooks/use-auth.tsx` — `AuthProvider` + `useAuth` (login/register/logout, restauración al montar).
- `web/app/login/page.tsx` y `web/app/registro/page.tsx` — formularios con estados loading/error.
- `SiteHeader` — muestra "Hola, {nombre}" + "Cerrar sesión" si autenticado; links a login/registro si no.
- `layout.tsx` — envuelve la app con `AuthProvider`.

## Decisiones

1. **Token en localStorage** (clave `auralearn.session`): simplicidad para dev. Migración a cookies httpOnly sería ADR aparte (deuda de seguridad documentada).
2. **Sin librerías nuevas:** React Context + fetch nativo.
3. **Validación client-side espejo del backend** (email formato, password ≥8, nombre requerido) — la validación real sigue siendo backend.
4. **Redirección post-login a `/tutores`.**
5. **Guard client-side** con `useAuth` (sin middleware de Next por ahora).
6. **Expiración:** al montar se decodifica el JWT; si `exp` < ahora, la sesión se descarta.

## Restricciones

- El backend debe estar corriendo para el smoke (`dotnet run` en `localhost:5037`).
- Sin cambios en el backend (los endpoints ya existen y están probados).
- Copy en español; tokens MD3; `MaterialIcon` en producto.
- El usuario de prueba ya existe en la BD (`estudiante@ucr.ac.cr` / `Password123!`).

## Dudas cerradas

- **¿Middleware de Next?** No por ahora; guard client-side basta para el alcance.
- **¿Refresh token?** Deuda del backend; el frontend maneja expiración mostrando login de nuevo.
- **Claim email de `/me`:** no se mapea (deuda backend menor); el frontend usa los datos del `AuthResponse`.
- **Bump:** `0.3.1` con clasificación `feat`.

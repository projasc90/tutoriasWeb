# Verification — Auth UI en el frontend (login/registro + sesión)

**Fecha:** 2026-09-28
**Plan:** [plan](./2026-09-28-auth-ui-plan.md)

## Comandos ejecutados

| Comando | Resultado |
|---------|-----------|
| `cd web && npm run lint` | ✅ 0 errores (4 warnings pre-existentes en archivos no tocados) |
| `cd web && npx tsc --noEmit` | ✅ exit 0 |
| `cd web && npx next build` | ✅ 6 rutas: `/`, `/_not-found`, `/login`, `/postular`, `/registro`, `/tutores` |

## Smoke de integración (frontend dev + backend `dotnet run`)

Verificado con navegador real sobre `http://localhost:3000`:

| Escenario | Resultado |
|-----------|-----------|
| `/login` renderiza formulario | ✅ campos Correo/Contraseña + link a registro |
| Login con credenciales existentes (estudiante@ucr.ac.cr) | ✅ redirige a `/tutores`; header "Hola, Ana" + "Cerrar sesión" |
| Cerrar sesión | ✅ header vuelve a "Iniciar Sesión" (link a `/login`) |
| `/registro` renderiza formulario | ✅ Nombre/Correo/Contraseña con hint "Mínimo 8 caracteres" |
| Registro nuevo (pedro.prueba@tec.ac.cr) | ✅ redirige a `/tutores`; header "Hola, Pedro" |
| Usuario en BD | ✅ `SELECT ... FROM users` confirma 3 usuarios (incluido el nuevo, rol 1 Estudiante) |
| Recarga de página | ✅ sesión persiste (localStorage): "Hola, Pedro" tras F5 |
| Expiración del JWT | ✅ implementada en `readSession` (descarta sesión si `exp` < ahora) |

## Evidencia estructural

- `web/lib/auth.ts` — ampliado con `decodeJwt`, `toSession`, `isExpired`; `StoredSession` incluye `tokenExpiresAt`; `readSession` descarta sesiones expiradas/legadas.
- `web/lib/api.ts` — añadido `fetchMe(token)` + tipo `MeResponse`.
- `web/hooks/use-auth.tsx` — `AuthProvider` + `useAuth` (login/register/logout, restauración diferida al montar).
- `web/app/login/page.tsx`, `web/app/registro/page.tsx` — formularios con validación client-side espejo del backend, estados loading/error.
- `web/components/landing/currency-toggle.tsx` — `SiteHeader` ahora client component con sesión (3 estados: loading/anon/auth).
- `web/app/layout.tsx` — `AuthProvider` envuelve la app.
- `onboarding-wizard.tsx` — adaptado a la nueva firma de `saveSession(toSession(...))`.

## Problemas resueltos durante la implementación

1. **TS2345 en onboarding-wizard** (`saveSession(result.data)` — `AuthResponse` sin `tokenExpiresAt`) → envolver con `toSession()`.
2. **Error de lint `react-hooks/set-state-in-effect`** (setState sincrónico en efecto) → restauración diferida con `setTimeout(..., 0)`.

## Hallazgo de contexto

El wizard `/postular` ya implementaba registro + sesión con `lib/auth.ts` y `lib/api.ts` (registerUser/saveSession). Esta tarea construyó sobre esa base: mismo storage key (`auralearn.auth`), misma clave de API, y el wizard se adaptó a la nueva firma de sesión con expiración.

## Riesgos residuales

- Token en localStorage (XSS mitigable con CSP; migración a cookies httpOnly sería ADR — deuda documentada en `context`).
- `logout` solo limpia el cliente (no hay endpoint de revocación — deuda del backend).
- El guard de rutas es client-side; no hay middleware de Next aún (decisión del plan).

> Nota: esta verificación no sustituye al testing gate (ejecutado por separado con evidencia arriba).

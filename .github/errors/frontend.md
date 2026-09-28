# Catálogo de errores — Frontend (web/)

> Cada error nuevo resuelto y no documentado debe agregarse automáticamente a este catálogo.
> Formato: nombre · mensaje o síntoma observable · causa · solución · contexto/fecha.

---

## ERR-FE-001 — APIs de Next 16 propuestas desde entrenamiento antiguo

- **Nombre:** APIs de Next.js 16 no existentes
- **Síntoma:** el agente escribe convenciones de Next 14/15 (p. ej. props de layout tipados manualmente, server actions con firmas antiguas) y TypeScript falla o el comportamiento difiere.
- **Causa:** Next.js 16.3.6 tiene cambios rompientes respecto a los datos de entrenamiento; `web/AGENTS.md` lo advierte explícitamente.
- **Solución:** leer la guía correspondiente en `web/node_modules/next/dist/docs/` antes de escribir código; copiar patrones reales del repo (`app/layout.tsx` usa `LayoutProps<"/">`).
- **Contexto:** 2026-09-27 — detectado al configurar la gobernanza (registrado también como GOTCHA-001).

## ERR-FE-002 — Primitivas Radix inexistentes en el proyecto

- **Nombre:** importación de Radix UI en `components/ui`
- **Síntoma:** error de módulo `@radix-ui/react-*` no encontrado, o props tipo `asChild` que no aplican.
- **Causa:** el design system usa shadcn estilo `base-nova` sobre `@base-ui/react` (v1.8), no Radix.
- **Solución:** usar/practicar con los componentes existentes (`button.tsx`, `sheet.tsx`, `accordion.tsx`) o generar con `npx shadcn add <componente>`; APIs relevantes: `useRender`, `mergeProps`.
- **Contexto:** 2026-09-27 — registrado como GOTCHA-002.

## ERR-FE-003 — Estilos fuera del sistema de tokens

- **Nombre:** colores/estilos crudos fuera de tokens MD3
- **Síntoma:** clases tipo `bg-[#hex]` o paleta Tailwind default que desentonan con el tema y rompen consistencia visual.
- **Causa:** Tailwind 4 es CSS-first; los tokens MD3 viven en `app/globals.css` (`@theme inline`) y no hay `tailwind.config`.
- **Solución:** usar clases de tokens (`bg-surface-container-low`, `text-on-surface-variant`, `text-title-md`…); extender `globals.css` solo si falta un token, con justificación.
- **Contexto:** 2026-09-27 — registrado como GOTCHA-003.

## ERR-FE-004 — Doble carga o icono incorrecto por contexto

- **Nombre:** iconos Material vs lucide mal ubicados
- **Síntoma:** la landing carga lucide (estilo inconsistente) o `components/ui` carga Material Symbols; o la variante `filled` de MaterialIcon no aplica.
- **Causa:** dos sistemas de iconos con ámbito claro: Material Symbols para producto (con `filled`), lucide-react para primitivas `ui/`.
- **Solución:** `MaterialIcon` en copy/landing; `lucide-react` dentro de `components/ui/`; el font se carga una única vez vía `<MaterialSymbols />` en `app/layout.tsx`.
- **Contexto:** 2026-09-27 — registrado como GOTCHA-004.

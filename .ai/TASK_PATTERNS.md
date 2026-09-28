---
id: task-patterns
priority: 3
loadWhen: debug-o-bug-recurrente
maxLines: 90
---

# Task Patterns — AuraLearn

Patrones, bugs recurrentes y gotchas permanentes. Máximo: patrones realmente útiles; archivar los que dejen de aplicar.

Formato: **Contexto → Error observado → Causa → Solución → Aprendido en (fecha)**

---

## GOTCHA-001 — Next.js 16 difiere de los datos de entrenamiento

- **Contexto:** cualquier tarea de código en `web/` (App Router, server actions, tipos de layout).
- **Error observado:** el agente propone APIs de Next 14/15 que ya no aplican (p. ej. convenciones de `layout.tsx`, tipos como `LayoutProps<"/">` generados por Next 16).
- **Causa:** Next.js 16.3.6 tiene cambios rompientes respecto a versiones conocidas; `web/AGENTS.md` lo advierte.
- **Solución:** antes de escribir código, revisar `web/node_modules/next/dist/docs/` (o usar `context7` si está disponible). Copiar patrones existentes del repo (`app/layout.tsx` usa `LayoutProps<"/">`).
- **Aprendido en:** 2026-09-27 (configuración inicial).

## GOTCHA-002 — shadcn base-nova se monta sobre @base-ui/react, no Radix

- **Contexto:** crear o extender componentes en `web/components/ui/`.
- **Error observado:** importar primitivas Radix (`@radix-ui/react-*`) o APIs de Radix (slots, `asChild`) que no existen aquí.
- **Causa:** el proyecto usa `@base-ui/react` (v1.8) con estilo `base-nova`; las APIs difieren (p. ej. `useRender`, `mergeProps`, props `data-*` propias).
- **Solución:** copiar el patrón de un componente existente (`button.tsx`, `sheet.tsx`) o usar `npx shadcn add <componente>`; iconos lucide-react dentro de `ui/`.
- **Aprendido en:** 2026-09-27 (configuración inicial).

## GOTCHA-003 — Tokens MD3, no colores crudos

- **Contexto:** estilizar cualquier sección en `web/`.
- **Error observado:** clases como `bg-[#3525cd]` o `text-slate-500` fuera del sistema.
- **Causa:** el tema define tokens Material 3 (`--primary`, `--surface-container-*`, `--on-surface-variant`, …) en `app/globals.css` vía `@theme inline`; Tailwind 4 es CSS-first (no hay `tailwind.config`).
- **Solución:** usar las clases de tokens (`bg-primary`, `text-on-surface-variant`, `bg-surface-container-low`) y la escala tipográfica MD3 (`text-title-md`, `text-display-lg`…). Extender `globals.css` si falta un token.
- **Aprendido en:** 2026-09-27 (configuración inicial).

## GOTCHA-004 — Iconos duales por contexto

- **Contexto:** añadir iconos.
- **Error observado:** mezclar Material Symbols con lucide en la misma superficie visual, o romper la variante `filled`.
- **Causa:** producto usa `MaterialIcon` (Material Symbols, con soporte `filled`); las primitivas `ui/` usan `lucide-react`.
- **Solución:** en copy/landing usar `<MaterialIcon name="verified" />` (los nombres son identificadores del set de Google, en inglés); en `components/ui` usar lucide. El font se carga una sola vez vía `<MaterialSymbols />` en el root layout.
- **Aprendido en:** 2026-09-27 (configuración inicial).

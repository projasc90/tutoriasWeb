---
applyTo: "web/**/*.ts, web/**/*.tsx, web/**/*.css"
description: Reglas para el frontend Next.js 16 de AuraLearn
---

# Frontend — AuraLearn (web/)

## Stack real y restricciones

- Next.js **16.3.6** App Router + React **19.2.8** + TypeScript 5 (`strict`) + Tailwind **4** (CSS-first, tokens en `app/globals.css`) + shadcn/ui **base-nova** sobre `@base-ui/react`.
- ⚠️ **Next.js 16 no es el que conoces**: antes de escribir código, verificar en `web/node_modules/next/dist/docs/` o con `context7`. Copiar patrones existentes (`app/layout.tsx` usa `LayoutProps<"/">`).
- No introducir otras librerías UI (Radix, MUI, Chakra). Para nuevas primitivas: `npx shadcn add <componente>`.

## Estructura de features

- Rutas en `web/app/<segmento>/page.tsx` (minúsculas). Server component por defecto; `"use client"` solo con estado/eventos (ejemplo real: `app/tutores/page.tsx`).
- Secciones compuestas en `web/components/landing/`; datos y tipos del módulo junto a su componente; cuando el mock deba compartirse, moverlo a `web/lib/data/`.
- Primitivas reutilizables solo en `web/components/ui/`; lógica de datos en `web/lib/`; nunca mezclar copy de producto con primitivas.

## Convenciones de UI

- Tokens MD3 de `app/globals.css`: `bg-surface-container-lowest`, `text-on-surface-variant`, `text-title-md`, etc. **Prohibido** color crudo fuera del theme.
- Iconos: `MaterialIcon` en producto/landing (nombres en inglés, soporta `filled`); `lucide-react` dentro de `components/ui/`. El font se carga una vez vía `<MaterialSymbols />` en el root layout.
- Tipografía con la escala MD3 definida en `@theme inline` (`text-display-lg`, `text-headline-lg`, `text-title-md`, `text-body-md`, `text-label-md`…).

## Modales, estados de carga y errores UX

- Overlays con `components/ui/sheet.tsx` (base-ui Dialog) o futuros Dialog; siempre con título accesible (`SheetTitle`) y cierre explícito.
- Toda acción asíncrona debe mostrar estado: loading (skeleton o spinner con MaterialIcon `progress_activity`), éxito, error con mensaje accionable y retry. Nunca dejar el botón en "cargando" infinito.
- Estados vacíos ilustrados con acción de salida (ejemplo real: `EmptyState` en `app/tutores/page.tsx`).
- Formularios: labels asociados (`htmlFor`), `aria-label` cuando no hay label visible, mensajes de error en español.

## Accesibilidad y design system

- Contraste según tokens MD3 (los pares on-color ya lo garantizan; no invertir pares).
- Elementos interactivos con `<button>`/`<a>` reales (no divs clicables); foco visible (`focus-visible:ring-*` ya presente en las primitivas).
- `aria-pressed` en toggles (ejemplo: `CurrencyToggle`), `aria-hidden` en decoraciones, `aria-label` en landmarks y grupos.
- Imágenes decorativas con `aria-hidden`; no depender solo del color para comunicar estado.

## Testing gate frontend

```bash
cd web && npm run lint && npx tsc --noEmit
```

- Obligatorio tras cambios en `.ts/.tsx/.css`.
- `npm run build` cuando el cambio sea estructural (rutas, layout, configuración, SSR/bundling).
- No hay script `type-check` dedicado: usar `npx tsc --noEmit` siempre.

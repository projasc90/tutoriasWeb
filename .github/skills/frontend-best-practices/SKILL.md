# Skill — Mejores prácticas Frontend (Next.js 16 + base-nova)

## Cuándo usarla
Al crear o modificar páginas/secciones en `web/`: composición de server/client components, datos, estados, accesibilidad.

## Patrones recomendados

### Server vs Client components
```tsx
// app/tutores/page.tsx — "use client" SOLO por el estado de filtros (patrón real del repo)
"use client";
import { useState } from "react";
```
- Server component por defecto. `"use client"` únicamente cuando hay `useState`/eventos/efectos.
- Pasar datos de servidor a client components por props serializables; no fetch de negocio en el cliente.

### Sección de landing (patrón real del repo)
```tsx
// components/landing/<seccion>.tsx — datos arriba, componente abajo, todo co-localizado
import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

const ITEMS = [/* datos del módulo */];

export function Seccion() {
  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <header>
          <SectionEyebrow>Etiqueta</SectionEyebrow>
          <SectionTitle>Título</SectionTitle>
        </header>
        {/* contenido */}
      </div>
    </section>
  );
}
```

### Estados de página (patrón `tutores/page.tsx`)
- `EmptyState` con icono MaterialIcon, título, descripción y botón de salida (limpiar filtros).
- Conteo de resultados visible (`{filtered.length} resultados`).
- Ordenación/filtrado en memo derivado del estado — lógica de presentación, nunca negocio.

## Anti-patrones
- `"use client"` en todo el árbol por comodidad (rompe SSR y metadata).
- Color crudo fuera de los tokens MD3 (`bg-[#fff]`, `text-slate-*`).
- Iconos lucide en la landing o Material Symbols dentro de `components/ui/`.
- Fetch a la API real dentro de `components/ui/` (las primitivas son puro render).
- Duplicar constantes compartidas entre secciones en vez de moverlas a `lib/`.
- Añadir mocks "persistentes" para flujos productivos (los mocks actuales son transitorios).

## Checklist final
- [ ] ¿Verifiqué las APIs en `node_modules/next/dist/docs/` (Next 16 difiere)?
- [ ] Server components por defecto; solo `"use client"` con justificación
- [ ] Tokens MD3 y escala tipográfica; sin colores crudos
- [ ] `MaterialIcon` en producto; lucide solo en `ui/`
- [ ] Estados loading/empty/error definidos en toda acción asíncrona
- [ ] A11y: labels, aria-pressed en toggles, foco visible, landmarks
- [ ] `cd web && npm run lint && npx tsc --noEmit` verde

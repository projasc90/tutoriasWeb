# Template — Componente Frontend (sección)

> Patrón real de `web/components/landing/`. Datos del módulo arriba, componente abajo; server component por defecto; tokens MD3; MaterialIcon para producto.

```tsx
// web/components/landing/<seccion>.tsx
import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

// 1. Datos del módulo (constantes en inglés si son nombres de código; copy en español)
type Item = {
  icon: string;
  title: string;
  description: string;
};

const ITEMS: Item[] = [
  {
    icon: "verified",
    title: "Título del ítem",
    description: "Descripción del ítem.",
  },
];

// 2. Componente de sección (export nombrado, PascalCase)
export function Seccion() {
  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <header className="mb-12 text-center">
          <SectionEyebrow>Etiqueta</SectionEyebrow>
          <SectionTitle className="mt-1">Título de la sección</SectionTitle>
        </header>

        <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          {ITEMS.map((item) => (
            <article
              key={item.title}
              className="flex flex-col rounded-xl bg-surface-container-lowest p-6 shadow-sm transition-all hover:shadow-primary-card"
            >
              <div className="mb-5 grid h-12 w-12 place-items-center rounded-xl bg-surface-container-low text-primary">
                <MaterialIcon name={item.icon} className="text-[28px]" />
              </div>
              <h3 className="mb-2 text-title-md font-bold text-on-surface">
                {item.title}
              </h3>
              <p className="text-body-md leading-relaxed text-on-surface-variant">
                {item.description}
              </p>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
```

## Puntos clave
- `"use client"` solo si hay estado/eventos (ver `tutores/page.tsx` como referencia).
- Solo tokens MD3 (`bg-surface`, `text-on-surface-variant`, `shadow-primary-card`…) — prohibido color crudo.
- Escala tipográfica MD3 (`text-title-md`, `text-body-md`, `text-label-md`…).
- Iconos con `MaterialIcon` (nombres en inglés, `filled` opcional).
- Máx ~300 líneas; dividir la sección o extraer datos a `lib/` si crece.

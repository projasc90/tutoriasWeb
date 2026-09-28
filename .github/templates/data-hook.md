# Template — Hook de datos (client)

> Para consumir la API del backend desde `web/`. Se crea cuando exista backend real (los mocks actuales son transitorios).

```ts
// web/hooks/use-tutors.ts
import { useCallback, useEffect, useState } from "react";

// Contrato espejo del backend (documentar la fuente: endpoint real)
type Tutor = {
  id: number;
  name: string;
  university: string;
  rating: number;
  price: number; // CRC
};

type ApiState<T> =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: T };

const BASE_URL = process.env.NEXT_PUBLIC_API_URL; // nunca hardcodear la URL

export function useTutors(query: string) {
  const [state, setState] = useState<ApiState<Tutor[]>>({ status: "idle" });

  const load = useCallback(async (q: string) => {
    setState({ status: "loading" });
    try {
      const res = await fetch(`${BASE_URL}/api/tutors?q=${encodeURIComponent(q)}`);
      if (!res.ok) {
        setState({ status: "error", error: `Error ${res.status} al cargar tutores` });
        return;
      }
      setState({ status: "success", data: (await res.json()) as Tutor[] });
    } catch {
      setState({ status: "error", error: "No se pudo conectar con el servidor" });
    }
  }, []);

  useEffect(() => {
    void load(query);
  }, [query, load]);

  return { ...state, reload: () => void load(query) };
}
```

## Puntos clave
- Estados explícitos `idle | loading | error | success` — el componente renderiza los cuatro.
- Errores siempre con mensaje accionable en español; nunca silenciados (prohibido `catch {}`).
- URL base por variable de entorno (`NEXT_PUBLIC_API_URL`); sin hardcode.
- AbortController si hay race conditions con búsquedas rápidas (añadir cuando aplique).
- Validación de respuesta contra el contrato tipado; el contrato vive junto al hook o en `lib/types/`.

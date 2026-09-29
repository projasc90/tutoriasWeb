"use client";

/**
 * Hook de datos del catálogo de tutores.
 *
 * Estados explícitos (idle | loading | error | success), AbortController para
 * cancelar requests obsoletos y debounce de 300ms en el query de búsqueda.
 * Patrón según .github/templates/data-hook.md.
 */

import { useEffect, useState } from "react";
import { fetchTutors, type PagedResult, type Tutor, type TutorSearchParams } from "@/lib/api";

export type ApiState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: PagedResult<Tutor> };

const DEBOUNCE_MS = 300;

export function useTutors(params: TutorSearchParams) {
  const [state, setState] = useState<ApiState>({ status: "loading" });
  const [reloadKey, setReloadKey] = useState(0);

  // Serializamos los params para usarlos como dependencia estable del efecto.
  const paramsKey = JSON.stringify(params);

  useEffect(() => {
    const controller = new AbortController();

    // Debounce solo cuando hay texto de búsqueda (los filtros discretos van al toque).
    const hasQuery = params.query !== undefined && params.query.length > 0;
    const delay = hasQuery ? DEBOUNCE_MS : 0;

    const timer = setTimeout(async () => {
      setState((prev) => (prev.status === "success" ? prev : { status: "loading" }));
      const result = await fetchTutors(params, controller.signal);

      if (result.ok) {
        setState({ status: "success", data: result.data });
      } else if (result.error !== "aborted") {
        setState({ status: "error", error: result.error });
      }
    }, delay);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [paramsKey, reloadKey]);

  const reload = () => setReloadKey((k) => k + 1);

  return { state, reload };
}

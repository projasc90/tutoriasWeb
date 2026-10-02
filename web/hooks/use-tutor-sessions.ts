"use client";

/**
 * Hook de sesiones del tutor autenticado (panel /tutor/sesiones).
 *
 * Patrón idéntico a use-my-reservations (estados explícitos + reload).
 * La mutación de cierre de sesión (complete) la maneja la página con `completeSession`.
 */

import { useEffect, useState, useCallback } from "react";
import { fetchTutorSessions, type Reservation } from "@/lib/api";

export type TutorSessionsState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: Reservation[] };

export function useTutorSessions(token: string | null) {
  const [state, setState] = useState<TutorSessionsState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!token) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchTutorSessions(token);
      if (cancelled || controller.signal.aborted) return;

      if (result.ok) {
        setState({ status: "success", data: result.data });
      } else {
        setState({ status: "error", error: result.error });
      }
    })();

    return () => {
      cancelled = true;
      controller.abort();
    };
  }, [token, reloadKey]);

  const reload = useCallback(() => setReloadKey((k) => k + 1), []);

  return { state, reload };
}

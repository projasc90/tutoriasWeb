"use client";

/**
 * Hook de reservas del estudiante autenticado (panel /mis-tutorias).
 *
 * Estados explícitos (idle | loading | error | success). Se recarga con `reload()`
 * tras cancelar una reserva o cuando se regresa desde el checkout.
 */

import { useEffect, useState, useCallback } from "react";
import { fetchMyReservations, type Reservation } from "@/lib/api";

export type MyReservationsState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: Reservation[] };

export function useMyReservations(token: string | null) {
  const [state, setState] = useState<MyReservationsState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!token) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchMyReservations(token);
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

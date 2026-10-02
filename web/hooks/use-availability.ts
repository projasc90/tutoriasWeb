"use client";

/**
 * Hook de reglas semanales de disponibilidad del tutor (GET/PUT).
 * Estados explícitos; la mutación reemplaza la lista completa (PUT).
 */

import { useEffect, useState, useCallback } from "react";
import {
  fetchAvailability,
  replaceAvailability,
  type AvailabilityRule,
} from "@/lib/api";

export type AvailabilityState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: AvailabilityRule[] };

export function useAvailability(token: string | null) {
  const [state, setState] = useState<AvailabilityState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );

  useEffect(() => {
    if (!token) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchAvailability(token);
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
  }, [token]);

  const save = useCallback(
    async (rules: AvailabilityRule[]) => {
      if (!token) return { ok: false as const, error: "Sesión requerida" };
      return replaceAvailability(token, rules);
    },
    [token]
  );

  return { state, save };
}

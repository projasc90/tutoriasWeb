"use client";

/**
 * Hook de la configuración de agenda del tutor (GET/PUT me/availability):
 * zona horaria, interruptor maestro del horario semanal, franjas y excepciones.
 * La mutación reemplaza la configuración completa (PUT).
 */

import { useEffect, useState, useCallback } from "react";
import {
  fetchAvailability,
  replaceAvailability,
  type AvailabilitySettings,
} from "@/lib/api";

export type AvailabilityState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: AvailabilitySettings };

export function useAvailability(token: string | null) {
  const [state, setState] = useState<AvailabilityState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );

  useEffect(() => {
    if (!token) return;

    let cancelled = false;

    (async () => {
      const result = await fetchAvailability(token);
      if (cancelled) return;

      if (result.ok) {
        setState({ status: "success", data: result.data });
      } else {
        setState({ status: "error", error: result.error });
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [token]);

  const save = useCallback(
    async (settings: AvailabilitySettings) => {
      if (!token) return { ok: false as const, error: "Sesión requerida" };
      return replaceAvailability(token, settings);
    },
    [token]
  );

  return { state, save };
}

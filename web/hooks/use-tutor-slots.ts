"use client";

/**
 * Hook de slots disponibles de un tutor.
 *
 * Patrón idéntico a use-tutors.ts (ApiState + AbortController).
 * Token opcional: los slots son públicos; solo se envía si el usuario está
 * autenticado para mantener compatibilidad con futuras reglas server-side.
 */

import { useEffect, useState } from "react";
import { fetchTutorSlots, type ApiResult, type Slot } from "@/lib/api";

export type SlotsState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: Slot[] };

export function useTutorSlots(tutorId: string | null, token?: string) {
  const [state, setState] = useState<SlotsState>(() =>
    tutorId ? { status: "loading" } : { status: "idle" }
  );

  useEffect(() => {
    if (!tutorId) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchTutorSlots(tutorId, token);
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
  }, [tutorId, token]);

  return { state };
}

/** Tipo exportado por compatibilidad con ApiResult. */
export type { ApiResult };

"use client";

/**
 * Hook de detalle de un tutor (GET /api/tutors/{id}).
 * Endpoint dedicado expuesto por TutorsController (Fase 0 del cierre de reservas).
 */

import { useEffect, useState } from "react";
import { fetchTutor, type Tutor } from "@/lib/api";

export type TutorDetailState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: Tutor };

export function useTutor(id: string | null) {
  const [state, setState] = useState<TutorDetailState>(() =>
    id ? { status: "loading" } : { status: "idle" }
  );

  useEffect(() => {
    if (!id) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchTutor(id);
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
  }, [id]);

  const isLoading = state.status === "loading";
  const isError = state.status === "error";
  const error = state.status === "error" ? state.error : null;
  const tutor = state.status === "success" ? state.data : null;

  return { state, tutor, isLoading, isError, error };
}

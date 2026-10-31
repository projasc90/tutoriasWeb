"use client";

/**
 * Hook de lectura de las reglas de reserva de la plataforma (GET público
 * /api/booking-rules). Las consumen el gestor de disponibilidad (tarjetas
 * informativas), el panel de mis tutorías (política de cancelación) y el
 * checkout (textos dinámicos). La edición es exclusiva del Admin.
 */

import { useCallback, useEffect, useState } from "react";
import { fetchBookingRules, type BookingRules } from "@/lib/api";

export type BookingRulesState =
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: BookingRules };

export function useBookingRules() {
  const [state, setState] = useState<BookingRulesState>({ status: "loading" });

  useEffect(() => {
    let cancelled = false;

    (async () => {
      const result = await fetchBookingRules();
      if (cancelled) return;

      setState(
        result.ok
          ? { status: "success", data: result.data }
          : { status: "error", error: result.error },
      );
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  const reload = useCallback(async () => {
    const result = await fetchBookingRules();
    setState(
      result.ok
        ? { status: "success", data: result.data }
        : { status: "error", error: result.error },
    );
  }, []);

  return { state, reload };
}

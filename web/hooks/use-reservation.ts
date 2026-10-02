"use client";

/**
 * Hook de mutación de reserva: crear, comprobante, cancelar.
 * Más el polling de estado (GET /api/reservations/{id}) para el checkout.
 *
 * Patrón similar a use-tutor-application: estado de submit + error inline.
 */

import { useEffect, useState, useCallback } from "react";
import {
  cancelReservation,
  createReservation,
  fetchReservation,
  submitComprobante,
  type CreateReservationPayload,
  type Reservation,
  type SubmitComprobantePayload,
} from "@/lib/api";

export type SubmitState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: Reservation };

export function useReservation() {
  const [state, setState] = useState<SubmitState>({ status: "idle" });
  const reset = () => setState({ status: "idle" });

  const create = useCallback(async (token: string, payload: CreateReservationPayload) => {
    setState({ status: "loading" });
    const result = await createReservation(token, payload);
    setState(result.ok
      ? { status: "success", data: result.data }
      : { status: "error", error: result.error });
    return result;
  }, []);

  const submitComprobante_ = useCallback(async (token: string, reservationId: string, payload: SubmitComprobantePayload) => {
    setState({ status: "loading" });
    const result = await submitComprobante(token, reservationId, payload);
    setState(result.ok
      ? { status: "success", data: result.data }
      : { status: "error", error: result.error });
    return result;
  }, []);

  const cancel = useCallback(async (token: string, reservationId: string) => {
    setState({ status: "loading" });
    const result = await cancelReservation(token, reservationId);
    setState(result.ok
      ? { status: "success", data: result.data }
      : { status: "error", error: result.error });
    return result;
  }, []);

  return { state, reset, create, submitComprobante: submitComprobante_, cancel };
}

/**
 * Polling de estado de una reserva (para el checkout mientras espera confirmación).
 * Hace refetch cada `intervalMs` hasta que la reserva NO esté en PendingPayment.
 */
export function useReservationPoll(token: string | null, reservationId: string | null, intervalMs = 3000) {
  const [reservation, setReservation] = useState<Reservation | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token || !reservationId) return;

    let cancelled = false;
    let timer: ReturnType<typeof setTimeout> | null = null;

    const fetchOnce = async () => {
      if (cancelled) return;
      const result = await fetchReservation(token, reservationId);
      if (cancelled) return;
      if (result.ok) {
        setReservation(result.data);
        setError(null);
        // Detener el polling si la reserva ya no está esperando pago.
        if (result.data.status !== "PendingPayment") {
          return;
        }
      } else {
        setError(result.error);
      }
      if (!cancelled) timer = setTimeout(fetchOnce, intervalMs);
    };

    void fetchOnce();

    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, [token, reservationId, intervalMs]);

  return { reservation, error };
}

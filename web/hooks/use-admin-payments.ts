"use client";

/**
 * Hook de cola de pagos pendientes para administradores.
 *
 * Patrón idéntico a use-admin-applications.ts: estado + mutación decide().
 * Decidir actualiza la cola localmente (estado idle→loading→success/error).
 */

import { useCallback, useEffect, useState } from "react";
import {
  decidePayment,
  fetchAdminPayments,
  type AdminPayment,
  type DecidePaymentDecision,
} from "@/lib/api";

export type AdminPaymentsState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; items: AdminPayment[] };
export type AdminPaymentsList = { status: "success"; items: AdminPayment[] };

export function useAdminPayments(token: string | null) {
  const [state, setState] = useState<AdminPaymentsState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );
  const [deciding, setDeciding] = useState<string | null>(null);
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    if (!token) return;

    const controller = new AbortController();
    let cancelled = false;

    (async () => {
      const result = await fetchAdminPayments(token);
      if (cancelled || controller.signal.aborted) return;

      if (result.ok) {
        setState({ status: "success", items: result.data });
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

  const decide = useCallback(
    async (reservationId: string, decision: DecidePaymentDecision, reason?: string) => {
      if (!token) return { ok: false as const, error: "No autenticado" };
      setDeciding(reservationId);
      const result = await decidePayment(token, reservationId, decision, reason);
      setDeciding(null);
      if (result.ok) reload();
      return result;
    },
    [token, reload],
  );

  return { state, deciding, decide, reload };
}

"use client";

/**
 * Hook de la cola admin de postulaciones: carga la lista revisable y expone
 * decide() para aprobar/rechazar con recarga automática tras la acción.
 */

import { useCallback, useEffect, useState } from "react";
import {
  fetchAdminApplications,
  verifyApplication,
  type AdminTutorApplication,
  type VerifyDecision,
} from "@/lib/api";

type AdminState =
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; items: AdminTutorApplication[] };

export function useAdminApplications(token: string) {
  const [state, setState] = useState<AdminState>({ status: "loading" });
  const [reloadKey, setReloadKey] = useState(0);
  const [deciding, setDeciding] = useState<string | null>(null); // id en curso

  useEffect(() => {
    let cancelled = false;

    // Diferido al siguiente tick (evita setState sincrónico en el efecto).
    const id = setTimeout(async () => {
      const result = await fetchAdminApplications(token);
      if (cancelled) return;
      if (result.ok) {
        setState({ status: "success", items: result.data });
      } else {
        setState({ status: "error", error: result.error });
      }
    }, 0);

    return () => {
      cancelled = true;
      clearTimeout(id);
    };
  }, [token, reloadKey]);

  const reload = useCallback(() => setReloadKey((k) => k + 1), []);

  /** Aprueba o rechaza; recarga la cola al terminar. */
  const decide = useCallback(
    async (appId: string, decision: VerifyDecision, reason?: string) => {
      setDeciding(appId);
      const result = await verifyApplication(token, appId, decision, reason);
      setDeciding(null);
      if (result.ok) reload();
      return result;
    },
    [token, reload],
  );

  return { state, deciding, decide, reload };
}

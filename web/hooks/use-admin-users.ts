"use client";

/**
 * Hook de datos del listado de usuarios para el panel Admin.
 * Espejo de use-tutors: estados explícitos (idle | loading | error | success),
 * AbortController y debounce de 300 ms en el query de búsqueda.
 */

import { useEffect, useState } from "react";
import {
  fetchAdminUsers,
  type AdminUser,
  type AdminUsersSearchParams,
  type PagedResult,
} from "@/lib/api";

export type AdminUsersState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: PagedResult<AdminUser> };

const DEBOUNCE_MS = 300;

export function useAdminUsers(token: string | null, params: AdminUsersSearchParams) {
  const [state, setState] = useState<AdminUsersState>({ status: "idle" });
  const [reloadKey, setReloadKey] = useState(0);

  // Serializamos los params para usarlos como dependencia estable del efecto.
  const paramsKey = JSON.stringify(params);

  useEffect(() => {
    const controller = new AbortController();

    // Debounce solo cuando hay texto de búsqueda (los filtros discretos van al toque).
    const hasQuery = params.query !== undefined && params.query.length > 0;
    const delay = hasQuery ? DEBOUNCE_MS : 0;

    const timer = setTimeout(async () => {
      if (!token) {
        setState({ status: "idle" });
        return;
      }
      setState((prev) => (prev.status === "success" ? prev : { status: "loading" }));
      const result = await fetchAdminUsers(token, params);

      if (result.ok) {
        setState({ status: "success", data: result.data });
      } else if (result.error !== "aborted") {
        setState({ status: "error", error: result.error });
      }
    }, delay);

    return () => {
      clearTimeout(timer);
      controller.abort();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, paramsKey, reloadKey]);

  const reload = () => setReloadKey((k) => k + 1);

  return { state, reload };
}

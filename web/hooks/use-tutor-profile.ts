"use client";

/**
 * Hook del perfil del tutor autenticado (GET/PUT /api/tutors/me/profile).
 * Patrón de use-tutor.ts (estados tipados) + mutación save() con estado
 * saving/saved/saveError para la barra sticky de persistencia.
 */

import { useCallback, useEffect, useState } from "react";
import { fetchTutorProfile, updateTutorProfile } from "@/lib/api";
import type {
  TutorProfile,
  UpdateTutorProfilePayload,
} from "@/lib/types/tutor-profile";

export type TutorProfileState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success"; data: TutorProfile };

export function useTutorProfile(token: string | null) {
  const [state, setState] = useState<TutorProfileState>(() =>
    token ? { status: "loading" } : { status: "idle" }
  );
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;

    let cancelled = false;
    (async () => {
      const result = await fetchTutorProfile(token);
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
    async (payload: UpdateTutorProfilePayload): Promise<boolean> => {
      if (!token) return false;
      setSaving(true);
      setSaveError(null);
      setSaved(false);

      const result = await updateTutorProfile(token, payload);
      setSaving(false);

      if (result.ok) {
        setState({ status: "success", data: result.data });
        setSaved(true);
        return true;
      }
      setSaveError(result.error);
      return false;
    },
    [token]
  );

  const profile = state.status === "success" ? state.data : null;

  return {
    state,
    profile,
    isLoading: state.status === "loading",
    isError: state.status === "error",
    error: state.status === "error" ? state.error : null,
    saving,
    saved,
    saveError,
    save,
  };
}

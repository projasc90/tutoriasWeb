"use client";

/**
 * Hook de mutación de la postulación de tutores.
 *
 * Estados explícitos (idle | submitting | error | success), sin debounce/abort
 * (es una mutación, no una query). Plantilla análoga a hooks/use-tutors.ts.
 */

import { useState } from "react";
import { submitTutorApplication } from "@/lib/api";
import type { TutorApplicationPayload } from "@/lib/types/tutor-application";

export type SubmitState =
  | { status: "idle" }
  | { status: "submitting" }
  | { status: "error"; error: string }
  | { status: "success" };

export function useTutorApplication() {
  const [state, setState] = useState<SubmitState>({ status: "idle" });

  const submit = async (payload: TutorApplicationPayload, token: string) => {
    setState({ status: "submitting" });
    const result = await submitTutorApplication(payload, token);

    if (result.ok) {
      setState({ status: "success" });
      return true;
    }
    setState({ status: "error", error: result.error });
    return false;
  };

  const reset = () => setState({ status: "idle" });

  return { state, submit, reset };
}

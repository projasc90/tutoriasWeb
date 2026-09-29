"use client";

/**
 * Diálogo de rechazo: el motivo es obligatorio (espejo de la validación del
 * backend para decision=reject).
 */

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";

export function RejectDialog({
  tutorName,
  submitting,
  onConfirm,
  onCancel,
}: {
  tutorName: string;
  submitting: boolean;
  onConfirm: (reason: string) => void;
  onCancel: () => void;
}) {
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);

  const handleConfirm = () => {
    if (reason.trim().length < 10) {
      setError("Describe el motivo (mínimo 10 caracteres).");
      return;
    }
    onConfirm(reason.trim());
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="reject-dialog-title"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-6 backdrop-blur-xs"
    >
      <div className="w-full max-w-md rounded-2xl bg-surface-container-lowest p-6 shadow-lg">
        <div className="mb-4 flex items-start gap-3">
          <div className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-error-container text-on-error-container">
            <MaterialIcon name="cancel" className="text-[22px]" />
          </div>
          <div>
            <h2 id="reject-dialog-title" className="text-title-md font-bold text-on-surface">
              Rechazar postulación
            </h2>
            <p className="mt-0.5 text-body-md text-on-surface-variant">
              {tutorName} podrá corregir y volver a postular.
            </p>
          </div>
        </div>

        <div className="flex flex-col gap-1.5">
          <label
            htmlFor="reject-reason"
            className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant"
          >
            Motivo del rechazo
          </label>
          <textarea
            id="reject-reason"
            rows={4}
            placeholder="Ej. El título no aparece en el registro universitario…"
            value={reason}
            onChange={(e) => {
              setReason(e.target.value);
              setError(null);
            }}
            aria-invalid={!!error}
            className="w-full resize-none rounded-lg border border-outline-variant bg-surface-container-lowest px-3.5 py-2.5 text-sm text-on-surface placeholder:text-outline transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30 aria-invalid:border-error aria-invalid:ring-2 aria-invalid:ring-error/20"
          />
          {error && (
            <p role="alert" className="text-label-md text-error">
              {error}
            </p>
          )}
        </div>

        <div className="mt-5 flex items-center justify-end gap-2">
          <button
            type="button"
            onClick={onCancel}
            disabled={submitting}
            className="rounded-lg border border-outline px-4 py-2 text-label-md font-semibold text-on-surface-variant transition-colors hover:border-primary hover:text-primary disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={handleConfirm}
            disabled={submitting}
            className="inline-flex items-center gap-1.5 rounded-lg bg-error px-4 py-2 text-label-md font-semibold text-on-error shadow-sm transition-colors hover:bg-error/90 disabled:opacity-50"
          >
            {submitting ? (
              <>
                <MaterialIcon name="progress_activity" className="animate-spin text-[16px]" />
                Rechazando…
              </>
            ) : (
              <>
                <MaterialIcon name="send" className="text-[16px]" />
                Confirmar rechazo
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

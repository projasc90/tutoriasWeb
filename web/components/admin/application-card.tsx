"use client";

/**
 * Tarjeta de postulación en la cola admin: datos del postulante + acciones
 * Aprobar/Rechazar. El rechazo abre el RejectDialog (motivo obligatorio).
 */

import { MaterialIcon } from "@/components/material-icon";
import type { AdminTutorApplication } from "@/lib/api";

const STATUS_LABELS: Record<string, { label: string; className: string }> = {
  PendingReview: { label: "Pendiente de revisión", className: "bg-primary-fixed text-primary" },
  UnderReview: { label: "En revisión", className: "bg-secondary-fixed-dim text-on-secondary-fixed" },
};

export function ApplicationCard({
  application,
  deciding,
  onApprove,
  onReject,
}: {
  application: AdminTutorApplication;
  deciding: boolean;
  onApprove: () => void;
  onReject: () => void;
}) {
  const initials = application.name
    .replace(/^(Dr\.|Dra\.|Ing\.|Prof\.|M\.Sc\.)\s+/, "")
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();

  const status = STATUS_LABELS[application.status] ?? {
    label: application.status,
    className: "bg-surface-container text-on-surface-variant",
  };
  const submitted = new Date(application.submittedAt).toLocaleDateString("es-CR", {
    day: "numeric",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
  });

  return (
    <article className="flex flex-col gap-4 rounded-xl bg-surface-container-lowest p-5 shadow-sm">
      <div className="flex items-start gap-3.5">
        <div className="grid h-14 w-14 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-secondary to-secondary-container font-bold text-lg text-on-secondary">
          {initials}
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex items-start justify-between gap-2">
            <h3 className="truncate text-title-md font-bold text-on-surface">
              {application.name}
            </h3>
            <span className={`shrink-0 rounded px-2 py-0.5 text-[10px] font-bold ${status.className}`}>
              {status.label}
            </span>
          </div>
          <p className="text-primary text-label-md font-semibold">{application.credentials}</p>
          <p className="text-label-sm text-on-surface-variant">
            {application.university} · postuló el {submitted}
          </p>
        </div>
      </div>

      <p className="line-clamp-2 text-body-md leading-relaxed text-on-surface-variant">
        {application.bio}
      </p>

      <div className="flex flex-wrap gap-1.5">
        {application.subjects.map((s) => (
          <span
            key={s}
            className="rounded bg-surface-container-low px-2 py-0.5 text-label-sm text-on-surface-variant"
          >
            {s}
          </span>
        ))}
      </div>

      <div className="flex items-center justify-between rounded-lg bg-surface-container-low p-2.5">
        <span className="inline-flex items-center gap-1.5 text-label-sm text-on-surface-variant">
          <MaterialIcon name="payments" className="text-[16px] text-primary" />
          Tarifa propuesta
        </span>
        <span className="text-label-md font-bold text-on-surface">
          ₡{application.priceCrc.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")} / hora
          <span className="ml-1 font-normal text-on-surface-variant">(~${application.priceUsd})</span>
        </span>
      </div>

      <div className="flex items-center justify-end gap-2 border-t border-outline-variant/40 pt-4">
        <button
          type="button"
          onClick={onReject}
          disabled={deciding}
          className="inline-flex items-center gap-1.5 rounded-lg border border-error/40 px-4 py-2 text-label-md font-semibold text-error transition-colors hover:bg-error-container/30 disabled:opacity-50"
        >
          <MaterialIcon name="cancel" className="text-[16px]" />
          Rechazar
        </button>
        <button
          type="button"
          onClick={onApprove}
          disabled={deciding}
          className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant disabled:opacity-50"
        >
          {deciding ? (
            <>
              <MaterialIcon name="progress_activity" className="animate-spin text-[16px]" />
              Procesando…
            </>
          ) : (
            <>
              <MaterialIcon name="check_circle" className="text-[16px]" />
              Aprobar
            </>
          )}
        </button>
      </div>
    </article>
  );
}

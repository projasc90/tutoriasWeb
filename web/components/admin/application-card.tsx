"use client";

/**
 * Tarjeta de postulación en la cola admin: datos del postulante + documentos
 * de verificación (descarga protegida, solo Admin) + acciones Aprobar/Rechazar.
 * El rechazo abre el RejectDialog (motivo obligatorio).
 */

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { fetchTutorDocumentBlobUrl, type AdminTutorApplication } from "@/lib/api";
import type { TutorDocumentItem, TutorDocumentKindName } from "@/lib/types/tutor-application";

const STATUS_LABELS: Record<string, { label: string; className: string }> = {
  PendingReview: { label: "Pendiente de revisión", className: "bg-primary-fixed text-primary" },
  UnderReview: { label: "En revisión", className: "bg-secondary-fixed-dim text-on-secondary-fixed" },
};

const KIND_LABELS: Record<TutorDocumentKindName, string> = {
  Titulo: "Título",
  Certificacion: "Certificación",
  Identificacion: "Identificación",
};

const REQUIRED_KINDS: TutorDocumentKindName[] = ["Titulo", "Certificacion", "Identificacion"];

export function ApplicationCard({
  application,
  token,
  deciding,
  onApprove,
  onReject,
}: {
  application: AdminTutorApplication;
  token: string;
  deciding: boolean;
  onApprove: () => void;
  onReject: () => void;
}) {
  const [viewError, setViewError] = useState<string | null>(null);
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

  const missingKinds = REQUIRED_KINDS.filter(
    (kind) => !application.documents.some((d) => d.kind === kind),
  );

  const handleView = async (doc: TutorDocumentItem) => {
    setViewError(null);
    const result = await fetchTutorDocumentBlobUrl(token, doc.id);
    if (!result.ok) {
      setViewError(result.error);
      return;
    }
    window.open(result.data, "_blank", "noopener");
    // La pestaña necesita la URL viva unos segundos; se revoca después.
    setTimeout(() => URL.revokeObjectURL(result.data), 60_000);
  };

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
          <p className="text-primary text-label-md font-semibold">
            {application.credentials}
            {application.specialization ? ` — ${application.specialization}` : ""}
          </p>
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

      <div className="flex flex-col gap-2 rounded-lg border border-outline-variant/50 p-3">
        <span className="inline-flex items-center gap-1.5 text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
          <MaterialIcon name="folder_open" className="text-[16px] text-primary" />
          Documentos de verificación
        </span>
        {application.documents.length > 0 ? (
          <ul className="flex flex-col gap-1.5">
            {application.documents.map((doc) => (
              <li key={doc.id} className="flex items-center gap-2 text-label-md">
                <span className="shrink-0 rounded bg-primary-container/20 px-2 py-0.5 font-semibold text-primary">
                  {KIND_LABELS[doc.kind]}
                </span>
                <span className="min-w-0 flex-1 truncate text-on-surface">{doc.fileName}</span>
                <span className="shrink-0 text-on-surface-variant">{formatDocSize(doc.sizeBytes)}</span>
                <button
                  type="button"
                  onClick={() => void handleView(doc)}
                  className="inline-flex shrink-0 items-center gap-1 rounded-lg border border-outline px-2.5 py-1 font-semibold text-on-surface-variant transition-colors hover:border-primary hover:text-primary"
                >
                  <MaterialIcon name="visibility" className="text-[14px]" />
                  Ver
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-label-md text-on-surface-variant">Sin documentos subidos.</p>
        )}
        {missingKinds.length > 0 && (
          <p className="flex flex-wrap items-center gap-1.5 text-label-md text-error">
            <MaterialIcon name="warning" className="text-[16px]" />
            Sin documento:
            {missingKinds.map((kind) => (
              <span key={kind} className="rounded bg-error-container/30 px-2 py-0.5 font-semibold text-error">
                {KIND_LABELS[kind]}
              </span>
            ))}
          </p>
        )}
        {viewError && (
          <p role="alert" className="text-label-md text-error">
            {viewError}
          </p>
        )}
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

function formatDocSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${Math.max(Math.round(bytes / 1024), 1)} KB`;
}

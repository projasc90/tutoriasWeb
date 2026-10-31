"use client";

/**
 * Tarjeta de estado del perfil docente (Stitch 05): badge de verificación,
 * progreso de 3 pasos y acciones según el estado (rechazo → re-postular;
 * verificado → nota de re-login por el rol del JWT).
 */

import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";
import type { TutorProfile, VerificationStatusName } from "@/lib/types/tutor-profile";

const STATUS_META: Record<
  VerificationStatusName,
  { label: string; icon: string; badgeClass: string }
> = {
  PendingReview: {
    label: "En revisión",
    icon: "hourglass_top",
    badgeClass: "bg-secondary-container/40 text-on-secondary-container",
  },
  UnderReview: {
    label: "En revisión",
    icon: "hourglass_top",
    badgeClass: "bg-secondary-container/40 text-on-secondary-container",
  },
  Verified: {
    label: "Verificado",
    icon: "verified",
    badgeClass: "bg-primary-fixed text-on-primary-fixed",
  },
  Rejected: {
    label: "Rechazado",
    icon: "cancel",
    badgeClass: "bg-error-container/40 text-error",
  },
};

export function ProfileStatusCard({ profile }: { profile: TutorProfile }) {
  const meta = STATUS_META[profile.verificationStatus];
  const rejected = profile.verificationStatus === "Rejected";
  const verified = profile.verificationStatus === "Verified";

  const steps = [
    {
      icon: "send",
      title: "Postulación enviada",
      body: "Recibimos tus datos de alta.",
      state: "done" as const,
    },
    {
      icon: "verified_user",
      title: "Revisión de credenciales",
      body: "Verificamos tu título contra el registro universitario (≤48 h).",
      state: verified ? ("done" as const) : ("current" as const),
    },
    {
      icon: "event_available",
      title: verified ? "Perfil activo" : "Verificado",
      body: verified
        ? "Tu perfil es visible y reservable en el catálogo."
        : "Al aprobarse, tu perfil queda visible y reservable.",
      state: verified ? ("done" as const) : ("todo" as const),
    },
  ];

  return (
    <aside className="flex flex-col gap-4">
      <div className="rounded-xl bg-surface-container-lowest p-5 shadow-sm">
        <div className="flex items-center justify-between gap-2">
          <h2 className="text-title-md font-bold text-on-surface">Estado del perfil</h2>
          <span
            className={`inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-label-sm font-bold ${meta.badgeClass}`}
          >
            <MaterialIcon name={meta.icon} filled className="text-[14px]" />
            {meta.label}
          </span>
        </div>

        <ol className="mt-4 flex flex-col gap-3">
          {steps.map((step) => (
            <li key={step.title} className="flex items-start gap-3">
              <span
                className={
                  step.state === "done"
                    ? "grid h-8 w-8 shrink-0 place-items-center rounded-full bg-tertiary-fixed/40 text-tertiary"
                    : step.state === "current"
                      ? "grid h-8 w-8 shrink-0 place-items-center rounded-full bg-primary/15 text-primary"
                      : "grid h-8 w-8 shrink-0 place-items-center rounded-full bg-surface-container-high text-on-surface-variant"
                }
              >
                <MaterialIcon
                  name={step.state === "done" ? "check" : step.icon}
                  className="text-[16px]"
                />
              </span>
              <div>
                <p className="text-label-md font-bold text-on-surface">{step.title}</p>
                <p className="text-label-sm text-on-surface-variant">{step.body}</p>
              </div>
            </li>
          ))}
        </ol>

        {rejected && profile.rejectionReason && (
          <div className="mt-4 rounded-lg bg-error-container/30 p-3">
            <p className="text-label-md font-bold text-error">Motivo del rechazo</p>
            <p className="mt-1 text-label-md text-on-surface">{profile.rejectionReason}</p>
            <Link
              href="/postular"
              className="mt-3 inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-label-md font-semibold text-on-primary transition-colors hover:bg-primary-container"
            >
              <MaterialIcon name="refresh" className="text-[16px]" />
              Reenviar postulación
            </Link>
          </div>
        )}

        {verified && (
          <p className="mt-4 rounded-lg bg-surface-container-low p-3 text-label-md text-on-surface-variant">
            <MaterialIcon name="info" className="mr-1 inline align-text-bottom text-[16px] text-primary" />
            Si acabas de ser aprobado, cierra sesión y vuelve a entrar para que tu menú
            de tutor (sesiones y disponibilidad) se active con tu nuevo rol.
          </p>
        )}
      </div>

      <Link
        href="/tutor/disponibilidad"
        className="flex items-center justify-between gap-3 rounded-xl bg-surface-container-lowest p-5 shadow-sm transition-colors hover:bg-surface-container-low"
      >
        <div className="flex items-center gap-3">
          <span className="grid h-10 w-10 place-items-center rounded-xl bg-surface-variant text-primary">
            <MaterialIcon name="edit_calendar" className="text-[22px]" />
          </span>
          <div>
            <p className="text-label-md font-bold text-on-surface">Horarios disponibles</p>
            <p className="text-label-sm text-on-surface-variant">
              Configura tus franjas semanales para generar cupos.
            </p>
          </div>
        </div>
        <MaterialIcon name="arrow_forward" className="text-[20px] text-on-surface-variant" />
      </Link>
    </aside>
  );
}

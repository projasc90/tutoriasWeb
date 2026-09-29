"use client";

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import type { AccountData } from "./step-account";
import type { ProfileData } from "./step-profile";
import type { SubjectsData } from "./step-subjects";

export function StepReview({
  account,
  profile,
  subjects,
  submitting,
  serverError,
  onBack,
  onSubmit,
}: {
  account: AccountData;
  profile: ProfileData;
  subjects: SubjectsData;
  submitting: boolean;
  serverError: string | null;
  onBack: () => void;
  onSubmit: () => void;
}) {
  const [accepted, setAccepted] = useState(false);
  const priceUsd = subjects.priceCrc > 0 ? Math.round(subjects.priceCrc / 520) : 0;

  return (
    <div className="flex flex-col gap-5">
      <header>
        <h2 className="text-headline-sm font-bold text-on-surface">Revisa y envía</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Nuestro equipo verificará tu título y atestados. Te notificaremos en ≤48 horas.
        </p>
      </header>

      <dl className="flex flex-col gap-4 rounded-2xl border border-outline-variant/60 bg-surface-container-lowest p-5">
        <Row label="Nombre" value={account.fullName} />
        <Row label="Correo" value={account.email} />
        <Row label="Título" value={profile.credentials} />
        <Row label="Universidad" value={profile.university} />
        <div className="flex flex-col gap-1">
          <dt className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
            Materias
          </dt>
          <dd className="flex flex-wrap gap-1.5">
            {subjects.subjects.map((s) => (
              <span
                key={s}
                className="rounded-full bg-primary-container/15 px-2.5 py-1 text-label-md font-semibold text-primary"
              >
                {s}
              </span>
            ))}
          </dd>
        </div>
        <Row
          label="Tarifa"
          value={`₡${subjects.priceCrc.toLocaleString("es-CR")} / hora${priceUsd > 0 ? ` (≈ $${priceUsd})` : ""}`}
        />
        <div className="flex flex-col gap-1">
          <dt className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
            Sobre ti
          </dt>
          <dd className="text-body-md leading-relaxed text-on-surface">{profile.bio}</dd>
        </div>
      </dl>

      <label className="flex cursor-pointer items-start gap-3 rounded-xl border border-outline-variant/60 bg-surface-container-low p-4">
        <input
          type="checkbox"
          checked={accepted}
          onChange={(e) => setAccepted(e.target.checked)}
          className="mt-0.5 h-4 w-4 shrink-0 accent-primary"
        />
        <span className="text-body-md text-on-surface">
          Declaro que mi título y credenciales son auténticos y acepto que AuraLearn los
          verifique contra el registro de mi universidad y el colegio profesional.
        </span>
      </label>

      {serverError && (
        <p
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container/20 px-3.5 py-2.5 text-body-md text-on-error-container"
        >
          <MaterialIcon name="error" className="text-[18px]" />
          {serverError}
        </p>
      )}

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
        <button
          type="button"
          onClick={onBack}
          disabled={submitting}
          className="inline-flex items-center justify-center gap-2 rounded-xl px-6 py-3.5 text-title-md font-semibold text-on-surface-variant transition-colors hover:bg-surface-container-low hover:text-on-surface disabled:opacity-60"
        >
          <MaterialIcon name="arrow_back" className="text-[18px]" />
          <span>Atrás</span>
        </button>
        <button
          type="button"
          onClick={onSubmit}
          disabled={!accepted || submitting}
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant disabled:opacity-60"
        >
          {submitting ? (
            <>
              <MaterialIcon name="progress_activity" className="animate-spin text-[18px]" />
              <span>Enviando postulación…</span>
            </>
          ) : (
            <>
              <MaterialIcon name="send" className="text-[18px]" />
              <span>Enviar postulación</span>
            </>
          )}
        </button>
      </div>
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col gap-0.5 sm:flex-row sm:items-baseline sm:justify-between sm:gap-4">
      <dt className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
        {label}
      </dt>
      <dd className="text-body-md font-semibold text-on-surface sm:text-right">{value}</dd>
    </div>
  );
}

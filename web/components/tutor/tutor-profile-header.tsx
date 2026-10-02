"use client";

/**
 * Header del perfil del tutor (Stitch 02): avatar con badge "En Línea",
 * chips de verificación, KPIs de reputación y strip de features.
 */

import { MaterialIcon } from "@/components/material-icon";
import type { Tutor } from "@/lib/api";

export function TutorProfileHeader({ tutor, initials }: { tutor: Tutor; initials: string }) {
  return (
    <div className="rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <div className="flex flex-col gap-6 sm:flex-row sm:items-start">
        {/* Avatar + En Línea */}
        <div className="relative shrink-0">
          <div className="grid h-28 w-28 place-items-center rounded-xl bg-gradient-to-br from-primary to-primary-container text-headline-lg font-bold text-on-primary shadow-md sm:h-36 sm:w-36">
            {initials}
          </div>
          <span className="absolute -bottom-2 -right-2 flex items-center gap-1 rounded-full bg-surface-container-lowest px-2.5 py-1 text-label-sm font-bold text-primary shadow-md">
            <span className="h-2 w-2 animate-pulse rounded-full bg-primary-container" />
            En Línea
          </span>
        </div>

        {/* Metadata */}
        <div className="min-w-0 flex-1">
          <div className="mb-1.5 flex flex-wrap items-center gap-1.5">
            <span className="inline-flex items-center gap-1 rounded-full bg-primary-fixed px-2.5 py-0.5 text-label-sm font-bold uppercase tracking-wide text-on-primary-fixed">
              <MaterialIcon name="verified" filled className="text-[14px]" />
              Identidad y Títulos Verificados
            </span>
            <span className="inline-flex items-center gap-1 rounded-full bg-surface-container-high px-2.5 py-0.5 text-label-sm text-secondary">
              <MaterialIcon name="school" className="text-[14px]" />
              {tutor.university} Docente
            </span>
          </div>
          <h1 className="mb-1 text-headline-lg tracking-tight text-on-surface">{tutor.name}</h1>
          <p className="mb-4 text-title-md text-on-surface-variant">
            {tutor.credentials} • Catedrático en {tutor.university}
          </p>

          {/* KPI strip */}
          <div className="grid grid-cols-2 gap-2 pt-1 sm:grid-cols-4">
            <div className="rounded-lg bg-surface-container-low p-2.5 text-center sm:text-left">
              <div className="flex items-center justify-center gap-1 text-headline-sm text-tertiary sm:justify-start">
                <MaterialIcon name="star" filled className="text-[20px] text-tertiary-container" />
                <span>{tutor.rating.toFixed(2)}</span>
              </div>
              <p className="text-label-sm text-on-surface-variant">{tutor.reviews} reseñas reales</p>
            </div>
            <div className="rounded-lg bg-surface-container-low p-2.5 text-center sm:text-left">
              <span className="text-headline-sm font-bold text-on-surface">99.4%</span>
              <p className="text-label-sm text-on-surface-variant">Tasa de asistencia</p>
            </div>
            <div className="rounded-lg bg-surface-container-low p-2.5 text-center sm:text-left">
              <div className="flex items-center justify-center gap-1 text-headline-sm font-bold text-on-surface sm:justify-start">
                <MaterialIcon name="bolt" className="text-[18px] text-primary" />
                &lt;15 min
              </div>
              <p className="text-label-sm text-on-surface-variant">Tiempo de respuesta</p>
            </div>
            <div className="rounded-lg bg-surface-container-low p-2.5 text-center sm:text-left">
              <span className="text-headline-sm font-bold text-on-surface">{tutor.reviews}+</span>
              <p className="text-label-sm text-on-surface-variant">Horas impartidas</p>
            </div>
          </div>
        </div>
      </div>

      {/* Feature pills */}
      <div className="mt-4 flex flex-wrap items-center justify-between gap-2 rounded-lg bg-surface-container-low/60 p-3 text-on-surface">
        <div className="flex items-center gap-2 text-label-md">
          <MaterialIcon name="cast_for_education" className="text-[20px] text-primary" />
          <span>Pizarra interactiva vectorial</span>
        </div>
        <div className="flex items-center gap-2 text-label-md">
          <MaterialIcon name="picture_as_pdf" className="text-[20px] text-primary" />
          <span>Bitácora de apuntes PDF</span>
        </div>
        <div className="flex items-center gap-2 text-label-md">
          <MaterialIcon name="video_library" className="text-[20px] text-primary" />
          <span>Grabación opcional incluida</span>
        </div>
      </div>
    </div>
  );
}

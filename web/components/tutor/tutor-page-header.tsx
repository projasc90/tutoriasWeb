"use client";

/**
 * Cabecera estándar de las páginas del portal del tutor (/tutor/*).
 * Banda gris + badge "Panel Académico Docente" + título + descripción,
 * con slot opcional para contenido a la derecha (métricas, monedero, etc.).
 * Garantiza consistencia visual entre Perfil · Disponibilidad · Sesiones ·
 * Documentos (antes cada página tenía su propio estilo de header).
 */

import type { ReactNode } from "react";
import { MaterialIcon } from "@/components/material-icon";

export function TutorPageHeader({
  title,
  description,
  aside,
}: {
  title: string;
  description: string;
  /** Contenido opcional alineado a la derecha (tarjeta de métricas, monedero…). */
  aside?: ReactNode;
}) {
  return (
    <section className="border-b border-outline-variant/40 bg-surface-container-low">
      <div className="mx-auto flex max-w-7xl flex-col justify-between gap-4 px-6 py-10 lg:flex-row lg:items-end lg:px-12">
        <div>
          <span className="mb-1 inline-flex items-center gap-1.5 rounded-full bg-surface-container-high px-2.5 py-0.5 text-label-sm uppercase tracking-wider text-primary">
            <span className="h-2 w-2 animate-pulse rounded-full bg-primary" />
            Panel Académico Docente
          </span>
          <h1 className="text-headline-lg font-bold tracking-tight text-on-surface">{title}</h1>
          <p className="mt-1 max-w-2xl text-body-md text-on-surface-variant">{description}</p>
        </div>
        {aside && <div className="shrink-0">{aside}</div>}
      </div>
    </section>
  );
}

/** Tarjeta de métrica reutilizable para los headers (capacidad, monedero…). */
export function HeaderMetric({
  icon,
  label,
  value,
}: {
  icon: string;
  label: string;
  value: string;
}) {
  return (
    <div className="inline-flex items-center gap-3 rounded-xl bg-surface-container-lowest px-4 py-3 shadow-sm">
      <MaterialIcon name={icon} className="text-[24px] text-primary" />
      <div>
        <p className="text-label-md text-on-surface-variant">{label}</p>
        <p className="text-title-md font-bold text-on-surface">{value}</p>
      </div>
    </div>
  );
}

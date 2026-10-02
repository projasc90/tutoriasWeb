"use client";

/**
 * Cabecera del Gestor de Disponibilidad (Stitch 05): badge "Panel Académico
 * Docente", display-lg, descripción y ribbon de métricas (capacidad semanal,
 * ocupación actual).
 */

import { MaterialIcon } from "@/components/material-icon";

export function AvailabilityHeader(props: {
  weeklyCapacityHours: number;
  occupiedHours: number;
}) {
  const { weeklyCapacityHours, occupiedHours } = props;
  const pct = weeklyCapacityHours > 0
    ? Math.round((occupiedHours / weeklyCapacityHours) * 1000) / 10
    : 0;

  return (
    <div className="mb-6 flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
      <div>
        <div className="mb-1 flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 rounded-full bg-surface-container-high px-2.5 py-0.5 text-label-sm uppercase tracking-wider text-primary">
            <span className="h-2 w-2 animate-pulse rounded-full bg-primary" />
            Panel Académico Docente
          </span>
          <span className="text-label-sm text-on-surface-variant">•</span>
          <span className="text-label-sm text-on-surface-variant">ID Tutor: AuraLearn</span>
        </div>
        <h1 className="text-display-lg tracking-tight text-on-surface">Gestor de Disponibilidad</h1>
        <p className="mt-1 text-body-lg text-on-surface-variant">
          Define tus bloques de horas recurrentes para la agenda docente. Los alumnos verán
          tus franjas al instante con tarifas en CRC y USD.
        </p>
      </div>

      <div className="flex items-center gap-3 self-start rounded-xl bg-surface-container-lowest p-2 shadow-sm lg:self-auto">
        <div className="rounded-lg bg-surface-container-low px-3 py-1.5 text-left">
          <p className="text-label-sm uppercase tracking-wider text-on-surface-variant">Capacidad Semanal</p>
          <p className="font-numeric text-numeric-table text-on-surface">{weeklyCapacityHours} Horas</p>
        </div>
        <div className="h-8 w-px bg-surface-container-high" />
        <div className="rounded-lg bg-surface-container-low px-3 py-1.5 text-left">
          <p className="text-label-sm uppercase tracking-wider text-on-surface-variant">Ocupación Actual</p>
          <p className="font-numeric text-numeric-table text-primary">
            {pct}% ({occupiedHours}h)
          </p>
        </div>
      </div>
    </div>
  );
}

export function AvailabilityToolbar() {
  return (
    <div className="mb-6 rounded-xl bg-surface-container-lowest p-4 shadow-sm">
      <div className="flex flex-col justify-between gap-4 xl:flex-row xl:items-center">
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2 rounded-lg bg-surface-container-low px-3 py-1.5">
            <MaterialIcon name="schedule" className="text-[18px] text-on-surface-variant" />
            <div className="flex flex-col">
              <span className="text-label-sm text-on-surface-variant">Zona horaria activa</span>
              <span className="font-numeric text-numeric-table text-on-surface">América/Costa_Rica (GMT-6)</span>
            </div>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-lg bg-surface-container-high px-4 py-2.5 text-label-md text-on-surface transition-colors hover:bg-surface-variant"
          >
            <MaterialIcon name="sync" className="text-[18px]" />
            <span>Sincronizar ahora</span>
          </button>
        </div>
      </div>
    </div>
  );
}

export function AvailabilityTabs(props: { excepciones: number }) {
  return (
    <div className="mb-4 flex items-center gap-2 overflow-x-auto pb-1">
      <button
        type="button"
        className="flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-label-md text-on-primary shadow-sm"
      >
        <MaterialIcon name="date_range" className="text-[18px]" />
        <span>Horario Semanal Recurrente</span>
        <span className="ml-1 rounded-full bg-white/20 px-1.5 py-0.5 text-[10px] font-bold">Activo</span>
      </button>
      <button
        type="button"
        className="flex items-center gap-2 rounded-xl bg-surface-container-lowest px-4 py-2.5 text-label-md text-on-surface-variant shadow-sm transition-colors hover:text-on-surface"
      >
        <MaterialIcon name="event_note" className="text-[18px]" />
        <span>Excepciones y Feriados ({props.excepciones})</span>
      </button>
    </div>
  );
}

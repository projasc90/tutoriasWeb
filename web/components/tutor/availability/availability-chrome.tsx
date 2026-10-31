"use client";

/**
 * Chrome del Gestor de Disponibilidad (Stitch 05). El header usa el estándar
 * del portal del tutor (TutorPageHeader); toolbar y tabs son específicas.
 */

import { MaterialIcon } from "@/components/material-icon";
import { TutorPageHeader } from "@/components/tutor/tutor-page-header";

export function AvailabilityHeader(props: {
  weeklyCapacityHours: number;
  occupiedHours: number;
}) {
  const { weeklyCapacityHours, occupiedHours } = props;
  const pct = weeklyCapacityHours > 0
    ? Math.round((occupiedHours / weeklyCapacityHours) * 1000) / 10
    : 0;

  return (
    <TutorPageHeader
      title="Gestor de Disponibilidad"
      description="Define tus bloques de horas recurrentes para la agenda docente. Los alumnos verán tus franjas al instante con tarifas en CRC y USD."
      aside={
        <div className="flex items-center gap-3 rounded-xl bg-surface-container-lowest p-2 shadow-sm">
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
      }
    />
  );
}

export function AvailabilityTabs(props: {
  excepciones: number;
  tab: "semanal" | "excepciones";
  onTabChange: (tab: "semanal" | "excepciones") => void;
}) {
  return (
    <div className="mb-4 flex items-center gap-2 overflow-x-auto pb-1">
      <button
        type="button"
        onClick={() => props.onTabChange("semanal")}
        aria-pressed={props.tab === "semanal"}
        className={
          props.tab === "semanal"
            ? "flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-label-md text-on-primary shadow-sm"
            : "flex items-center gap-2 rounded-xl bg-surface-container-lowest px-4 py-2.5 text-label-md text-on-surface-variant shadow-sm transition-colors hover:text-on-surface"
        }
      >
        <MaterialIcon name="date_range" className="text-[18px]" />
        <span>Horario Semanal Recurrente</span>
      </button>
      <button
        type="button"
        onClick={() => props.onTabChange("excepciones")}
        aria-pressed={props.tab === "excepciones"}
        className={
          props.tab === "excepciones"
            ? "flex items-center gap-2 rounded-xl bg-primary px-4 py-2.5 text-label-md text-on-primary shadow-sm"
            : "flex items-center gap-2 rounded-xl bg-surface-container-lowest px-4 py-2.5 text-label-md text-on-surface-variant shadow-sm transition-colors hover:text-on-surface"
        }
      >
        <MaterialIcon name="event_note" className="text-[18px]" />
        <span>Excepciones y Feriados ({props.excepciones})</span>
      </button>
    </div>
  );
}

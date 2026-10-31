"use client";

/**
 * Columna de un día en la matriz semanal (Stitch 05): head con "cupos libres"
 * + badge Activo/Cerrado, stack de franjas (hora local CR + cupos), y acciones
 * "Añadir franja" / "Copiar a otros días". Día sin franjas = empty state de
 * descanso con "Habilitar día".
 */

import { MaterialIcon } from "@/components/material-icon";
import type { AvailabilityRule } from "@/lib/api";

export type DayRule = AvailabilityRule & { localId: string };

export function DayColumn(props: {
  name: string;
  weekday: number;
  rules: DayRule[];
  slotsOpenByRule: Map<string, number>;
  busySlotsByHour: Map<string, string[]>; // "weekday|hour" → alumnos (decorativo)
  onAdd: (weekday: number) => void;
  onRemove: (localId: string) => void;
  onChange: (localId: string, field: "startLocal" | "endLocal" | "isRecurring", value: string | boolean) => void;
  onCopyDay: (weekday: number) => void;
}) {
  const {
    name, weekday, rules, slotsOpenByRule, busySlotsByHour,
    onAdd, onRemove, onChange, onCopyDay,
  } = props;

  const cupsLibres = rules.reduce(
    (acc, r) => acc + (slotsOpenByRule.get(r.localId) ?? 0),
    0
  );
  const activo = rules.length > 0;

  return (
    <div
      className={`flex flex-col justify-between rounded-xl p-4 shadow-sm transition-all hover:shadow-md ${
        activo ? "bg-surface-container-lowest" : "bg-surface-container-low/70 opacity-85"
      }`}
    >
      <div>
        <div className="mb-3 flex items-center justify-between gap-2 pb-3">
          <div className="flex min-w-0 flex-1 items-center justify-between gap-2">
            <span className={`block text-title-md font-bold ${activo ? "text-on-surface" : "text-on-surface-variant"}`}>
              {name}
            </span>
            <span className={`shrink-0 rounded-full px-2 py-0.5 text-label-sm ${activo ? "bg-surface-container-high text-primary" : "bg-surface-container-high text-on-surface-variant"}`}>
              {activo ? "Activo" : "Cerrado"}
            </span>
          </div>
        </div>
        {activo && cupsLibres > 0 && (
          <p className="-mt-2 mb-2 text-label-sm text-primary">{cupsLibres} cupos libres esta semana</p>
        )}

        {!activo ? (
          <div className="flex flex-col items-center justify-center py-6 text-center">
            <div className="mb-2 flex h-10 w-10 items-center justify-center rounded-full bg-surface-container text-outline">
              <MaterialIcon name="bedtime" className="text-[20px]" />
            </div>
            <p className="text-label-md text-on-surface-variant">Día de descanso</p>
            <p className="mt-0.5 text-label-sm text-outline">Sin franjas asignadas</p>
          </div>
        ) : (
          <div className="mb-4 space-y-2">
            {rules.map((r) => {
              const open = slotsOpenByRule.get(r.localId) ?? 0;
              const busy = busySlotsByHour.get(`${weekday}`) ?? [];
              return (
                <div key={r.localId} className="group relative rounded-lg bg-surface-container-low p-2.5 pr-6">
                  {/* X de eliminación en la esquina superior derecha de la franja.
                      fontSize inline: la clase .material-symbols-outlined de Google
                      fija 24px y gana el cascade sobre las utilidades de Tailwind. */}
                  <button
                    type="button"
                    onClick={() => onRemove(r.localId)}
                    className="absolute right-1 top-1 grid h-3.5 w-3.5 place-items-center rounded-full text-on-surface-variant transition-colors hover:bg-error-container/30 hover:text-error"
                    aria-label={`Eliminar franja ${name}`}
                  >
                    <MaterialIcon name="close" style={{ fontSize: 11 }} />
                  </button>
                  {/* Horario a lo ancho del cuadro: inicio – fin en una sola línea */}
                  <div className="flex items-center gap-0.5">
                    <HoraInput
                      value={r.startLocal}
                      label={`Inicio ${name}`}
                      onChange={(v) => onChange(r.localId, "startLocal", v)}
                    />
                    <span aria-hidden className="shrink-0 text-[12px] text-on-surface-variant">–</span>
                    <HoraInput
                      value={r.endLocal}
                      label={`Fin ${name}`}
                      onChange={(v) => onChange(r.localId, "endLocal", v)}
                    />
                  </div>
                  <div className="mt-1.5 flex flex-wrap items-center justify-between gap-x-2 gap-y-1">
                    <span className="inline-flex items-center gap-1 whitespace-nowrap rounded bg-surface-variant px-1.5 py-0.5 text-label-sm text-on-primary-fixed-variant">
                      <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-primary" />
                      {open} cupos abiertos
                    </span>
                    {/* Toggle recurrente: semanal vs solo la próxima ocurrencia */}
                    <label className="inline-flex cursor-pointer items-center gap-1.5 whitespace-nowrap text-label-sm text-on-surface-variant">
                      <input
                        type="checkbox"
                        checked={r.isRecurring}
                        onChange={(e) => onChange(r.localId, "isRecurring", e.target.checked)}
                        className="h-3.5 w-3.5 accent-primary"
                      />
                      {r.isRecurring ? "Recurrente" : "Solo próxima vez"}
                    </label>
                  </div>
                  {busy.length > 0 && (
                    <div className="mt-1.5 space-y-1">
                      {busy.slice(0, 2).map((b) => (
                        <div key={b} className="flex items-center justify-between rounded bg-primary-container/10 px-1.5 py-0.5 text-label-sm text-primary">
                          <span className="truncate">{b}</span>
                          <MaterialIcon name="lock" className="text-[14px]" />
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>

      <div className="space-y-1.5 pt-2">
        {activo ? (
          <>
            <button
              type="button"
              onClick={() => onAdd(weekday)}
              className="flex w-full items-center justify-center gap-1 rounded-lg bg-surface-container py-1.5 px-2 text-label-sm text-primary transition-colors hover:bg-surface-container-high"
            >
              <MaterialIcon name="add" className="text-[16px]" /> Franja horaria
            </button>
            <button
              type="button"
              onClick={() => onCopyDay(weekday)}
              className="flex w-full items-center justify-center gap-1 py-1 px-2 text-label-sm text-on-surface-variant transition-colors hover:text-on-surface"
            >
              <MaterialIcon name="content_copy" className="text-[14px]" /> Copiar a otros días
            </button>
          </>
        ) : (
          <button
            type="button"
            onClick={() => onAdd(weekday)}
            className="flex w-full items-center justify-center gap-1 rounded-lg bg-surface-container-lowest py-1.5 px-2 text-label-sm text-on-surface-variant shadow-sm transition-colors hover:text-primary"
          >
            <MaterialIcon name="add_circle" className="text-[16px]" /> Habilitar día
          </button>
        )}
      </div>
    </div>
  );
}

function HoraInput(props: { value: string; label: string; onChange: (v: string) => void }) {
  return (
    <input
      type="time"
      step={3600}
      aria-label={props.label}
      value={props.value}
      onChange={(e) => props.onChange(e.target.value.slice(0, 5))}
      className="min-w-0 flex-1 rounded bg-transparent text-left font-numeric text-[12px] leading-none text-on-surface focus:outline-none focus:ring-1 focus:ring-primary [&::-webkit-calendar-picker-indicator]:hidden [&::-webkit-datetime-edit-ampm-field]:hidden [&::-webkit-datetime-edit]:w-full [&::-webkit-datetime-edit]:px-0 [&::-webkit-datetime-edit-fields-wrapper]:gap-0"
    />
  );
}

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
  onChange: (localId: string, field: "startLocal" | "endLocal", value: string) => void;
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
        <div className="mb-3 flex items-center justify-between pb-3">
          <div>
            <span className={`block text-headline-sm ${activo ? "text-on-surface" : "text-on-surface-variant"}`}>
              {name}
            </span>
            <span className={`text-label-sm font-bold ${activo ? "text-primary" : "text-outline"}`}>
              {activo ? `${cupsLibres} cupos libres` : "Sin actividad"}
            </span>
          </div>
          <span className={`rounded-full px-2 py-0.5 text-label-sm ${activo ? "bg-surface-container-high text-primary" : "bg-surface-container-high text-on-surface-variant"}`}>
            {activo ? "Activo" : "Cerrado"}
          </span>
        </div>

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
                <div key={r.localId} className="group relative rounded-lg bg-surface-container-low p-2.5">
                  <div className="mb-1 flex items-center justify-between">
                    <span className="text-on-surface">
                      <HoraInput
                        value={r.startLocal}
                        label={`Inicio ${name}`}
                        onChange={(v) => onChange(r.localId, "startLocal", v)}
                      />
                      {" - "}
                      <HoraInput
                        value={r.endLocal}
                        label={`Fin ${name}`}
                        onChange={(v) => onChange(r.localId, "endLocal", v)}
                      />
                    </span>
                    <div className="flex items-center gap-1 opacity-80 group-hover:opacity-100">
                      <button
                        type="button"
                        onClick={() => onRemove(r.localId)}
                        className="p-0.5 text-on-surface-variant hover:text-error"
                        aria-label={`Eliminar franja ${name}`}
                      >
                        <MaterialIcon name="delete" className="text-[16px]" />
                      </button>
                    </div>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="inline-flex items-center gap-1 rounded bg-surface-variant px-1.5 py-0.5 text-label-sm text-on-primary-fixed-variant">
                      <span className="h-1.5 w-1.5 rounded-full bg-primary" />
                      {open} cupos abiertos
                    </span>
                    <span className="text-label-sm text-on-surface-variant">Recurrente</span>
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
      className="rounded bg-transparent px-1 font-numeric text-numeric-table text-on-surface focus:outline-none focus:ring-1 focus:ring-primary"
      style={{ width: "4.6rem", display: "inline-block" }}
    />
  );
}

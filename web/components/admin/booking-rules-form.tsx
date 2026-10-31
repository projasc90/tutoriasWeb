"use client";

/**
 * Formulario de edición de las reglas de reserva de la plataforma (solo Admin).
 * Cada regla es opcional: input vacío = "sin definir" (null) — esa regla no se
 * muestra al tutor y la lógica del backend usa su default. Validación inline
 * con los mismos rangos que el backend (espejo de UpdateBookingRulesValidator).
 */

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import type { BookingRules, BookingRulesInput } from "@/lib/api";

type RuleFieldKey = keyof BookingRulesInput;

type Campo = {
  key: RuleFieldKey;
  label: string;
  desc: string;
  min: number;
  max: number;
  unidad: string;
  def: number;
  icon: string;
};

/** Espejo de los rangos de UpdateBookingRulesValidator (backend). */
const CAMPOS: Campo[] = [
  {
    key: "cancellationNoticeHours",
    label: "Anticipación mínima de cancelación",
    desc: "Horas antes de la sesión en que el estudiante aún puede cancelar con crédito a su monedero.",
    min: 0,
    max: 168,
    unidad: "horas",
    def: 12,
    icon: "hourglass_top",
  },
  {
    key: "paymentWindowMinutes",
    label: "Ventana de pago SINPE",
    desc: "Minutos que tiene el estudiante para subir el comprobante antes de que expire la reserva.",
    min: 15,
    max: 240,
    unidad: "minutos",
    def: 60,
    icon: "payments",
  },
  {
    key: "slotDurationMinutes",
    label: "Duración de slots",
    desc: "Minutos de cada bloque publicado. Nota: la generación de cupos sigue fijada a 60 min (configuración declarativa).",
    min: 15,
    max: 180,
    unidad: "minutos",
    def: 60,
    icon: "snooze",
  },
  {
    key: "bookingHorizonDays",
    label: "Ventana máxima de reserva",
    desc: "Días de horizonte en que se generan cupos por adelantado (ensure-ahead).",
    min: 1,
    max: 56,
    unidad: "días",
    def: 14,
    icon: "calendar_month",
  },
];

export function BookingRulesForm(props: {
  initial: BookingRules;
  saving: boolean;
  saved: boolean;
  error: string | null;
  onSave: (rules: BookingRulesInput) => void;
}) {
  const [form, setForm] = useState<BookingRulesInput>({
    cancellationNoticeHours: props.initial.cancellationNoticeHours,
    paymentWindowMinutes: props.initial.paymentWindowMinutes,
    slotDurationMinutes: props.initial.slotDurationMinutes,
    bookingHorizonDays: props.initial.bookingHorizonDays,
  });
  const [localError, setLocalError] = useState<string | null>(null);

  const setField = (key: RuleFieldKey, raw: string) => {
    const value = raw.trim() === "" ? null : Number(raw);
    setForm((prev) => ({ ...prev, [key]: value }));
  };

  const validar = (): string | null => {
    for (const c of CAMPOS) {
      const v = form[c.key];
      if (v != null && (Number.isNaN(v) || v < c.min || v > c.max)) {
        return `${c.label}: debe estar entre ${c.min} y ${c.max} ${c.unidad}.`;
      }
    }
    return null;
  };

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    const err = validar();
    if (err) {
      setLocalError(err);
      return;
    }
    setLocalError(null);
    props.onSave({
      cancellationNoticeHours: form.cancellationNoticeHours,
      paymentWindowMinutes: form.paymentWindowMinutes,
      slotDurationMinutes: form.slotDurationMinutes,
      bookingHorizonDays: form.bookingHorizonDays,
    });
  };

  const error = localError ?? props.error;
  const updatedAt = props.initial.updatedAt
    ? new Date(props.initial.updatedAt).toLocaleString("es-CR", {
        dateStyle: "medium",
        timeStyle: "short",
      })
    : null;

  return (
    <form onSubmit={submit} className="flex flex-col gap-4">
      {CAMPOS.map((c) => {
        const value = form[c.key];
        const definida = value != null;
        return (
          <div key={c.key} className="rounded-xl bg-surface-container-lowest p-5 shadow-sm">
            <div className="flex flex-wrap items-start gap-4">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                <MaterialIcon name={c.icon} className="text-[22px]" />
              </span>
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <label htmlFor={`regla-${c.key}`} className="text-title-md font-bold text-on-surface">
                    {c.label}
                  </label>
                  <span
                    className={`rounded-full px-2.5 py-0.5 text-label-sm font-semibold ${
                      definida
                        ? "bg-primary-container text-on-primary-container"
                        : "bg-surface-container-high text-on-surface-variant"
                    }`}
                  >
                    {definida ? "Definida" : "Sin definir"}
                  </span>
                </div>
                <p className="mt-1 text-body-md text-on-surface-variant">{c.desc}</p>
                <div className="mt-3 flex flex-wrap items-center gap-3">
                  <input
                    id={`regla-${c.key}`}
                    type="number"
                    inputMode="numeric"
                    min={c.min}
                    max={c.max}
                    step={1}
                    value={value ?? ""}
                    onChange={(e) => setField(c.key, e.target.value)}
                    placeholder="Sin definir"
                    aria-label={`${c.label} (vacío = sin definir)`}
                    className="w-32 rounded-lg border border-outline bg-surface-container-lowest px-3 py-2 text-body-md text-on-surface focus-visible:ring-2 focus-visible:ring-primary focus:outline-none"
                  />
                  <span className="text-label-md text-on-surface-variant">{c.unidad}</span>
                  {definida ? (
                    <button
                      type="button"
                      onClick={() => setField(c.key, "")}
                      className="inline-flex items-center gap-1 rounded-lg px-2 py-1 text-label-md text-on-surface-variant hover:bg-surface-container-high hover:text-on-surface"
                    >
                      <MaterialIcon name="close" style={{ fontSize: 14 }} />
                      Quitar (sin definir)
                    </button>
                  ) : (
                    <span className="text-label-md text-outline">Default: {c.def} {c.unidad}</span>
                  )}
                </div>
              </div>
            </div>
          </div>
        );
      })}

      {error && (
        <p role="alert" className="rounded-lg bg-error-container p-3 text-label-md text-on-error-container">
          {error}
        </p>
      )}
      {props.saved && !error && (
        <p className="rounded-lg bg-primary-container p-3 text-label-md text-on-primary-container">
          ¡Reglas guardadas exitosamente!
        </p>
      )}

      <div className="flex flex-wrap items-center justify-between gap-3">
        <p className="text-label-md text-on-surface-variant">
          {updatedAt ? `Última actualización: ${updatedAt}` : "Aún sin actualizaciones registradas."}
        </p>
        <button
          type="submit"
          disabled={props.saving}
          className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary hover:bg-on-primary-fixed-variant disabled:opacity-50"
        >
          {props.saving ? (
            <MaterialIcon name="progress_activity" className="animate-spin text-[18px]" />
          ) : (
            <MaterialIcon name="save" className="text-[18px]" />
          )}
          Guardar reglas
        </button>
      </div>
    </form>
  );
}

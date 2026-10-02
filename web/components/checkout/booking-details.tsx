"use client";

/**
 * Tarjeta "Detalles de la Clase" del checkout (Stitch 03): materia/horario,
 * modalidad en línea y nota pedagógica opcional para el tutor.
 */

import { MaterialIcon } from "@/components/material-icon";
import { formatSlotEsCr, formatHourEsCr } from "@/lib/time";

export function BookingDetails({
  tutorName,
  subject,
  startAt,
  endAt,
}: {
  tutorName: string;
  subject: string;
  startAt: string;
  endAt: string;
}) {
  return (
    <section className="rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <div className="mb-4 flex items-center justify-between border-b border-surface-container pb-2">
        <div className="flex items-center gap-2">
          <MaterialIcon name="school" className="text-[22px] text-primary" />
          <h2 className="text-headline-sm text-on-surface">Detalles de la Clase</h2>
        </div>
        <span className="rounded bg-surface-container px-2 py-0.5 text-label-sm font-bold uppercase tracking-wider text-secondary">
          Reserva Individual
        </span>
      </div>

      <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="rounded-lg bg-surface-container-low p-4">
          <span className="text-label-sm block mb-1 text-secondary">Materia &amp; Objetivo</span>
          <p className="text-title-md text-on-surface">{subject}</p>
          <p className="text-body-md flex items-center gap-1 text-on-surface-variant">
            <span className="h-1.5 w-1.5 rounded-full bg-primary" />
            con {tutorName}
          </p>
        </div>
        <div className="rounded-lg bg-surface-container-low p-4">
          <span className="text-label-sm block mb-1 text-secondary">Fecha y Horario Exacto</span>
          <p className="text-title-md capitalize text-on-surface">{formatSlotEsCr(startAt)}</p>
          <p className="text-body-md flex items-center gap-1 text-on-surface-variant">
            <MaterialIcon name="schedule" className="text-[16px] text-primary" />
            {formatHourEsCr(startAt)} – {formatHourEsCr(endAt)} (Hora Costa Rica, GMT-6)
          </p>
        </div>
      </div>

      <div className="mb-4 flex flex-col justify-between gap-2 rounded-lg bg-surface-container-low p-4 sm:flex-row sm:items-center">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-surface-container-highest">
            <MaterialIcon name="video_camera_front" className="text-[22px] text-primary" />
          </div>
          <div>
            <p className="text-title-md text-on-surface">En línea vía Google Meet + Pizarra colaborativa</p>
            <p className="text-body-md text-on-surface-variant">
              El enlace privado se sincroniza tras confirmar el pago.
            </p>
          </div>
        </div>
        <span className="self-start rounded bg-primary-fixed px-2.5 py-1 text-label-md font-bold text-primary sm:self-center">
          HD &amp; Pizarra incluida
        </span>
      </div>

      <div className="space-y-1.5">
        <label htmlFor="student-notes" className="flex items-center justify-between text-label-md text-on-surface">
          <span>
            ¿Qué temas o ejercicios específicos deseas resolver?{" "}
            <span className="font-normal text-secondary">(Opcional)</span>
          </span>
          <span className="text-label-sm text-outline">Máx. 300 caracteres</span>
        </label>
        <textarea
          id="student-notes"
          maxLength={300}
          rows={2}
          placeholder="Ej. Repasar derivadas de funciones trigonométricas para el parcial del viernes."
          className="w-full rounded-lg bg-surface-container-low px-3 py-2.5 text-body-md text-on-surface placeholder:text-outline focus:outline-none focus:ring-2 focus:ring-primary/40"
        />
      </div>
    </section>
  );
}

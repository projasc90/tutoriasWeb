"use client";

/**
 * Banner ámbar de "slot reservado temporalmente" + cuenta regresiva (Stitch 03).
 * El contador muestra los minutos restantes de la ventana de pago (60 min).
 */

import { MaterialIcon } from "@/components/material-icon";

export function TimerBanner({ minutesLeft }: { minutesLeft: number }) {
  const m = String(minutesLeft).padStart(2, "0");
  const mmss = `${m}:00`;

  return (
    <div className="relative mb-4 flex items-center justify-between overflow-hidden rounded-xl bg-tertiary-fixed p-4 text-on-tertiary-fixed shadow-sm">
      <div className="relative z-10 flex items-center gap-3">
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-tertiary/10">
          <MaterialIcon name="timer" className="text-[22px] text-tertiary" />
        </div>
        <div className="min-w-0">
          <p className="text-title-md leading-tight text-on-tertiary-fixed">Slot reservado temporalmente</p>
          <p className="text-body-md text-on-tertiary-fixed-variant">
            Bloque protegido para evitar colisiones de agenda con otros alumnos.
          </p>
        </div>
      </div>
      <div className="relative z-10 flex shrink-0 items-center gap-2 rounded-lg bg-black/10 px-3 py-1.5">
        <MaterialIcon name="lock_clock" className="text-[18px] text-tertiary" />
        <span className="font-numeric text-headline-sm font-bold tracking-tight text-tertiary">{mmss}</span>
      </div>
    </div>
  );
}

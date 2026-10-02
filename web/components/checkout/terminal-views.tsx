"use client";

/**
 * Estados terminales del checkout (Stitch 03, paso 3 "Confirmación"):
 * pago acreditado, rechazado, expirado y cancelado.
 */

import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";
import type { Reservation } from "@/lib/api";

function Shell({ children }: { children: React.ReactNode }) {
  return (
    <main className="flex min-h-screen flex-1 flex-col bg-background">
      <div className="mx-auto w-full max-w-md px-6 py-16 text-center">{children}</div>
    </main>
  );
}

export function ConfirmedView({ reservation }: { reservation: Reservation }) {
  return (
    <Shell>
      <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-primary-container text-on-primary-container">
          <MaterialIcon name="check" className="text-[28px]" />
        </div>
        <h1 className="mt-4 text-headline-md font-bold text-on-surface">Pago acreditado</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">
          Tu reserva con <strong>{reservation.tutorName}</strong> está confirmada. Te avisaremos antes de la sesión.
        </p>
        <Link href="/mis-tutorias" className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary">
          Ver mis tutorías
        </Link>
      </div>
    </Shell>
  );
}

export function RejectedView({ reservation }: { reservation: Reservation }) {
  return (
    <Shell>
      <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-error-container text-on-error-container">
          <MaterialIcon name="cancel" className="text-[28px]" />
        </div>
        <h1 className="mt-4 text-headline-md font-bold text-on-surface">Comprobante rechazado</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">
          Motivo: {reservation.decisionReason ?? "No especificado"}. El slot ha sido liberado.
        </p>
        <Link href="/tutores" className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary">
          Volver al directorio
        </Link>
      </div>
    </Shell>
  );
}

export function ExpiredView() {
  return (
    <Shell>
      <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-surface-container-high text-on-surface-variant">
          <MaterialIcon name="timer_off" className="text-[28px]" />
        </div>
        <h1 className="mt-4 text-headline-md font-bold text-on-surface">Reserva expirada</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">
          Pasaron los 60 minutos sin subir un comprobante válido. El slot fue liberado.
        </p>
        <Link href="/tutores" className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary">
          Buscar otro horario
        </Link>
      </div>
    </Shell>
  );
}

export function CancelledView() {
  return (
    <Shell>
      <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
        <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-surface-container-high text-on-surface-variant">
          <MaterialIcon name="event_busy" className="text-[28px]" />
        </div>
        <h1 className="mt-4 text-headline-md font-bold text-on-surface">Reserva cancelada</h1>
        <p className="mt-2 text-body-md text-on-surface-variant">
          Cancelaste esta reserva. Si fue con ≥12 h de anticipación, el monto volvió a tu monedero.
        </p>
        <Link href="/tutores" className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary">
          Buscar otro horario
        </Link>
      </div>
    </Shell>
  );
}

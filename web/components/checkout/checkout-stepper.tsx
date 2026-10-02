"use client";

/**
 * Stepper de 3 pasos + badge "Sesión Segura" del checkout (Stitch 03).
 * Paso 1 (horario) completado; paso 2 (pago) activo; paso 3 (confirmación) pendiente.
 */

import { MaterialIcon } from "@/components/material-icon";

export function CheckoutStepper() {
  return (
    <div className="flex items-center justify-between gap-2 rounded-xl bg-surface-container-lowest p-4 shadow-sm">
      <div className="flex min-w-0 items-center gap-2">
        <div className="flex items-center gap-2">
          <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-surface-container text-secondary">
            <MaterialIcon name="check" className="text-[16px] text-primary" />
          </span>
          <span className="text-label-md text-on-surface-variant hidden sm:inline">1. Selección de horario</span>
          <span className="text-label-md text-on-surface-variant sm:hidden">1. Horario</span>
        </div>
        <MaterialIcon name="chevron_right" className="text-outline-variant text-[18px] shrink-0" />
        <div className="flex items-center gap-2">
          <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary text-label-sm font-bold text-on-primary">2</span>
          <span className="text-label-md font-bold text-primary">2. Pago &amp; Facturación</span>
        </div>
        <MaterialIcon name="chevron_right" className="text-outline-variant text-[18px] shrink-0" />
        <div className="flex min-w-0 items-center gap-2 opacity-50">
          <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-surface-container text-label-sm text-on-surface-variant">3</span>
          <span className="text-label-md text-on-surface-variant hidden sm:inline">3. Confirmación</span>
          <span className="text-label-md text-on-surface-variant sm:hidden">3. Listo</span>
        </div>
      </div>
      <div className="hidden items-center gap-1.5 rounded-full bg-surface-container-high px-3 py-1 text-label-sm text-on-surface-variant md:flex">
        <span className="h-2 w-2 animate-pulse rounded-full bg-primary" />
        <span>Sesión Segura · SSL 256-bit</span>
      </div>
    </div>
  );
}

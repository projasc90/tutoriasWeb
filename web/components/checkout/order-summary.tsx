"use client";

/**
 * Columna derecha del checkout (Stitch 03): snapshot del tutor, resumen de orden
 * con breakdown, total, botón de confirmación y políticas de cancelación (12 h
 * del repo — el mock de Stitch decía 6 h, se corrige a la regla real del glosario).
 */

import { MaterialIcon } from "@/components/material-icon";
import { formatCrc } from "@/lib/time";

export function OrderSummary(props: {
  tutorName: string;
  tutorCredentials: string;
  tutorRating: number;
  tutorReviews: number;
  priceCrc: number;
  priceUsd: number;
  minutesLeft: number;
  submitState: SubmitStateLite;
  submitLabel: string;
  onSubmit: () => void;
}) {
  const {
    tutorName, tutorCredentials, tutorRating, tutorReviews,
    priceCrc, priceUsd, minutesLeft, submitState, submitLabel, onSubmit,
  } = props;

  return (
    <div className="space-y-4 lg:sticky lg:top-28">
      <div className="rounded-xl bg-surface-container-lowest p-4 shadow-sm">
        <div className="flex items-center gap-3">
          <div className="relative shrink-0">
            <div className="grid h-16 w-16 place-items-center rounded-xl bg-primary-container text-on-primary-container">
              <MaterialIcon name="person" className="text-[28px]" />
            </div>
            <span
              className="absolute -bottom-1 -right-1 h-4 w-4 rounded-full border-2 border-surface-container-lowest bg-[#10b981]"
              aria-label="Docente verificado activo"
            />
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex items-center gap-1.5">
              <h3 className="truncate text-title-md font-bold text-on-surface">{tutorName}</h3>
              <MaterialIcon name="verified" className="text-[18px] text-primary" aria-label="Verificado" />
            </div>
            <p className="truncate text-label-sm text-secondary">{tutorCredentials}</p>
            <div className="mt-1 flex items-center gap-2">
              <div className="flex items-center text-tertiary">
                <MaterialIcon name="star" filled className="text-[16px]" />
                <span className="ml-0.5 font-bold text-on-surface">{tutorRating.toFixed(2)}</span>
              </div>
              <span className="text-label-sm text-outline">•</span>
              <span className="text-label-sm text-on-surface-variant">{tutorReviews} clases impartidas</span>
            </div>
          </div>
        </div>
      </div>

      <div className="space-y-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
        <h3 className="text-headline-sm text-on-surface">Resumen de Orden</h3>
        <div className="space-y-2 text-body-md text-on-surface-variant">
          <div className="flex items-center justify-between">
            <span>Sesión Magistral (1 hora)</span>
            <span className="font-bold text-on-surface">{formatCrc(priceCrc)}</span>
          </div>
          <div className="flex items-center justify-between">
            <span className="flex items-center gap-1">
              Tarifa de servicio y plataforma
              <MaterialIcon name="info" className="text-[16px] text-outline" aria-label="Cero comisiones" />
            </span>
            <span className="rounded bg-surface-container px-2 py-0.5 text-label-sm font-bold text-secondary">GRATIS</span>
          </div>
          <div className="flex items-center justify-between">
            <span>Grabación en la nube &amp; Pizarra</span>
            <span className="rounded bg-surface-container px-2 py-0.5 text-label-sm font-bold text-secondary">INCLUIDO</span>
          </div>
        </div>

        <div className="flex items-baseline justify-between rounded-xl bg-surface-container-low p-4">
          <div>
            <span className="block text-label-sm font-bold uppercase tracking-wider text-secondary">Total a Pagar</span>
            <span className="text-label-sm text-on-surface-variant">Tipo de cambio ref: ~${priceUsd} USD</span>
          </div>
          <div className="text-right">
            <span className="font-numeric text-headline-lg font-extrabold tracking-tight text-primary">{formatCrc(priceCrc)}</span>
            <span className="ml-1 font-semibold text-on-surface">CRC</span>
          </div>
        </div>

        <button
          type="button"
          onClick={onSubmit}
          disabled={submitState.status === "loading"}
          className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-4 py-3.5 text-title-md font-bold text-on-primary shadow-md transition-all hover:shadow-lg active:scale-[0.99] disabled:opacity-50"
        >
          <MaterialIcon name="lock" className="text-[20px]" />
          <span>{submitLabel}</span>
        </button>
        {minutesLeft <= 0 && (
          <p className="text-label-md text-error">La ventana de pago venció: la reserva expirará al recargar.</p>
        )}
        <div className="space-y-2 pt-2">
          <div className="flex items-start gap-2 text-on-surface-variant">
            <MaterialIcon name="event_available" className="mt-0.5 shrink-0 text-[18px] text-primary" />
            <p className="leading-tight">
              <strong>Cancelación sin costo:</strong> hasta 12 horas antes del inicio, con reembolso del 100% a tu monedero.
            </p>
          </div>
          <div className="flex items-start gap-2 text-on-surface-variant">
            <MaterialIcon name="verified" className="mt-0.5 shrink-0 text-[18px] text-primary" />
            <p className="leading-tight">
              <strong>Garantía de Excelencia Docente:</strong> si la tutoría no cumple tus expectativas, te reasignamos otro tutor o te reembolsamos.
            </p>
          </div>
        </div>

        <div className="flex items-center justify-center gap-3 pt-2 text-label-sm text-outline">
          <span className="flex items-center gap-1">
            <MaterialIcon name="shield" className="text-[14px]" /> Protección al comprador
          </span>
          <span>•</span>
          <span>AuraLearn Costa Rica</span>
        </div>
      </div>

      <div className="flex items-center gap-3 rounded-xl bg-surface-container-low p-4">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-surface-container-highest text-primary">
          <MaterialIcon name="support_agent" className="text-[20px]" />
        </div>
        <div>
          <p className="text-title-md leading-tight text-on-surface">¿Dudas con SINPE o la reserva?</p>
          <p className="text-label-sm text-on-surface-variant">Soporte por WhatsApp de AuraLearn activo 24/7 en Costa Rica.</p>
        </div>
      </div>
    </div>
  );
}

export type SubmitStateLite =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "error"; error: string }
  | { status: "success" };

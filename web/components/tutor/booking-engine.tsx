"use client";

/**
 * Motor de reserva sticky (Stitch 02, columna derecha):
 * 1. Modalidad (individual 60 min — packs/express en la UI con nota de próximamente)
 * 2. Tira horizontal de días (solo los que tienen slots)
 * 3. Horarios agrupados Mañana / Tarde / Noche (hora CR)
 * 4. Cita seleccionada + subtotal + CTA "Continuar al Pago/Checkout"
 * 5. Garantía + métodos de pago aceptados
 */

import { MaterialIcon } from "@/components/material-icon";
import { formatCrc, formatHourEsCr } from "@/lib/time";
import type { Slot } from "@/lib/api";

function franjaDe(horaLocal: number): "Mañana" | "Tarde" | "Noche" {
  if (horaLocal < 12) return "Mañana";
  if (horaLocal < 18) return "Tarde";
  return "Noche";
}

export function BookingEngine(props: {
  tutorName: string;
  slots: Slot[];
  selectedSlotId: string | null;
  onSelectSlot: (id: string) => void;
  priceCrc: number;
  priceUsd: number;
  wallet: number | null;
  payWithWallet: boolean;
  onPayWithWalletChange: (v: boolean) => void;
  disabled: boolean;
  loading: boolean;
  onSubmit: () => void;
}) {
  const {
    slots, selectedSlotId, onSelectSlot,
    priceCrc, priceUsd,
    wallet, payWithWallet, onPayWithWalletChange,
    disabled, loading, onSubmit,
  } = props;

  // Agrupa por día (key determinista YYYY-MM-DD en zona CR) + label es-CR
  const byDay = new Map<string, { weekdayAbbr: string; dayNum: string; label: string; sortKey: string; slots: Slot[] }>();
  for (const s of slots) {
    const key = dayKeyOf(s);
    const d = new Date(s.startAt);
    if (!byDay.has(key)) {
      const fmt = new Intl.DateTimeFormat("es-CR", {
        timeZone: "America/Costa_Rica",
        weekday: "short",
        day: "2-digit",
        month: "short",
      });
      const parts = fmt.formatToParts(d);
      const dayNum = parts.find((p) => p.type === "day")?.value ?? "";
      const abbr = (parts.find((p) => p.type === "weekday")?.value ?? "").toUpperCase().slice(0, 3);
      byDay.set(key, { weekdayAbbr: abbr, dayNum, label: fmt.format(d), sortKey: key, slots: [] });
    }
    byDay.get(key)!.slots.push(s);
  }
  const days = [...byDay.entries()].sort(([, a], [, b]) => a.sortKey.localeCompare(b.sortKey));

  const selected = slots.find((s) => s.id === selectedSlotId) ?? null;
  // Día seleccionado: el del slot elegido; sin slot elegido, el primero con slots
  const selectedDayKey = selected ? dayKeyOf(selected) : days[0]?.[0] ?? null;
  const walletOk = wallet !== null && wallet >= priceCrc;

  // Selecciona día: auto-elega el primer slot del día recién activado
  const handleSelectDay = (key: string) => {
    if (selectedDayKey === key) return;
    onSelectSlot(byDay.get(key)?.slots[0]?.id ?? "");
  };

  return (
    <aside className="space-y-4 lg:sticky lg:top-24">
      <div className="relative overflow-hidden rounded-2xl bg-surface-container-lowest p-6 shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between pb-2">
          <div className="flex items-center gap-2">
            <MaterialIcon name="event_available" className="text-[22px] text-primary" />
            <h3 className="text-headline-sm text-on-surface">Motor de Reserva</h3>
          </div>
          <span className="inline-flex items-center rounded-full bg-surface-container px-2 py-0.5 text-label-sm font-bold text-primary">
            En Vivo
          </span>
        </div>

        {/* 1. Modalidad */}
        <div className="space-y-2 pt-2">
          <label className="block text-label-md font-semibold text-on-surface">
            1. Modalidad y Duración de la Sesión
          </label>
          <div className="flex cursor-pointer items-center justify-between rounded-xl bg-primary-fixed/30 p-3 transition-all">
            <div className="flex items-center gap-3">
              <div className="flex h-5 w-5 items-center justify-center rounded-full bg-primary text-on-primary">
                <MaterialIcon name="done" className="text-[14px]" />
              </div>
              <div>
                <h4 className="text-title-md text-on-surface">Clase Individual 1-a-1</h4>
                <p className="text-label-sm text-on-surface-variant">60 minutos intensivos</p>
              </div>
            </div>
            <div className="text-right">
              <span className="font-bold text-primary">{formatCrc(priceCrc)}</span>
              <span className="block text-[10px] text-on-surface-variant">~${priceUsd} USD</span>
            </div>
          </div>
          <div className="flex cursor-not-allowed items-center justify-between rounded-xl bg-surface-container-low p-3 opacity-60">
            <div className="flex items-center gap-3">
              <div className="flex h-5 w-5 items-center justify-center rounded-full bg-surface-container-highest text-transparent">
                <MaterialIcon name="done" className="text-[14px]" />
              </div>
              <div>
                <h4 className="text-title-md text-on-surface">Pack 4 Sesiones</h4>
                <p className="text-label-sm text-on-surface-variant">Acompañamiento a parcial</p>
              </div>
            </div>
            <span className="rounded bg-tertiary-fixed px-1.5 py-0.5 text-[10px] font-bold text-on-tertiary-fixed">
              -10% OFF · próximamente
            </span>
          </div>
        </div>

        {/* 2. Tira de días */}
        <div className="space-y-2 pt-4">
          <label className="block text-label-md font-semibold text-on-surface">
            2. Fecha
          </label>
          {days.length === 0 ? (
            <p className="rounded-lg bg-surface-container-low p-3 text-body-md text-on-surface-variant">
              Sin horarios publicados en los próximos 14 días.
            </p>
          ) : (
            <div className="grid grid-cols-6 gap-1.5 overflow-x-auto text-center">
              {days.slice(0, 6).map(([key, { weekdayAbbr, dayNum }]) => {
                const isSel = selectedDayKey === key;
                return (
                  <button
                    key={key}
                    type="button"
                    onClick={() => handleSelectDay(key)}
                    className={`p-2 transition-colors ${
                      isSel
                        ? "rounded-lg bg-primary text-on-primary shadow-sm"
                        : "rounded-lg bg-surface-container-low text-on-surface hover:bg-surface-container"
                    }`}
                  >
                    <span className={`block text-label-sm ${isSel ? "font-bold text-on-primary" : "text-on-surface-variant"}`}>
                      {weekdayAbbr}
                    </span>
                    <span className={`font-bold ${isSel ? "text-on-primary" : "text-on-surface"}`}>{dayNum}</span>
                  </button>
                );
              })}
            </div>
          )}
        </div>

        {/* 3. Horarios del día seleccionado */}
        <div className="space-y-3 pt-4">
          <div className="flex items-center justify-between">
            <label className="text-label-md font-semibold text-on-surface">
              3. Horarios Disponibles (Hora San José - UTC-6)
            </label>
            <span className="flex items-center gap-0.5 text-label-sm text-primary">
              <span className="h-1.5 w-1.5 animate-ping rounded-full bg-primary" />
              {slotsOfSelectedDay().length} libres
            </span>
          </div>
          {(["Mañana", "Tarde", "Noche"] as const).map((franja) => {
            const slotsFranja = slotsOfSelectedDay().filter(
              (s) => franjaDe(localHour(s.startAt)) === franja
            );
            if (slotsFranja.length === 0) return null;
            return (
              <div key={franja}>
                <span className="mb-1.5 flex items-center gap-1 text-label-sm uppercase tracking-wider text-secondary">
                  <MaterialIcon
                    name={franja === "Mañana" ? "wb_sunny" : franja === "Tarde" ? "wb_twilight" : "nightlight"}
                    className="text-[14px]"
                  />{" "}
                  {franja}
                </span>
                <div className="grid grid-cols-3 gap-2">
                  {slotsFranja.map((s) => {
                    const isSel = s.id === selectedSlotId;
                    return (
                      <button
                        key={s.id}
                        type="button"
                        onClick={() => onSelectSlot(s.id)}
                        className={`rounded-lg py-2 px-1 transition-colors ${
                          isSel
                            ? "bg-primary font-bold text-on-primary shadow-sm"
                            : "bg-surface-container-low text-on-surface hover:bg-primary-fixed/40"
                        }`}
                      >
                        {formatHourEsCr(s.startAt)}
                      </button>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>

        {/* Cita seleccionada */}
        <div className="mt-4 space-y-1 rounded-xl bg-surface-container-low p-3">
          <div className="flex items-center gap-1.5 text-label-md font-bold text-primary">
            <MaterialIcon name="event" className="text-[18px]" />
            <span>Cita Seleccionada:</span>
          </div>
          <p className="pl-6 text-body-md font-semibold text-on-surface">
            {selected
              ? `${dayLabelOf(selected)}, ${formatHourEsCr(selected.startAt)} (Hora de Costa Rica / UTC-6)`
              : selectedDayKey
                ? `Día elegido: ${byDay.get(selectedDayKey)?.label}. Falta el horario`
                : "Elige día y horario"}
          </p>
        </div>

        {/* Precio + CTA */}
        <div className="mt-4 space-y-2 pt-2">
          <div className="flex items-baseline justify-between">
            <span className="text-body-md text-on-surface-variant">Subtotal de la sesión:</span>
            <span className="text-headline-md font-extrabold text-on-surface">{formatCrc(priceCrc)}</span>
          </div>
          {wallet !== null && (
            <label className={`flex items-center gap-2 rounded-lg p-2 text-label-md ${
              walletOk ? "bg-surface-container-low text-on-surface" : "bg-surface-container-low text-on-surface-variant"
            }`}>
              <input
                type="checkbox"
                checked={payWithWallet}
                disabled={!walletOk}
                onChange={(e) => onPayWithWalletChange(e.target.checked)}
                className="size-4 accent-[var(--primary)]"
              />
              {walletOk
                ? `Pagar con monedero (saldo ${formatCrc(wallet)})`
                : `Saldo insuficiente (${formatCrc(wallet)})`}
            </label>
          )}
          <button
            type="button"
            onClick={onSubmit}
            disabled={disabled || loading || (!payWithWallet && !selected)}
            className="group mt-2 flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-bold text-on-primary shadow-lg transition-all hover:scale-[1.01] disabled:opacity-50"
          >
            <span>{loading ? "Creando reserva…" : payWithWallet ? "Reservar con mi saldo" : "Continuar al Pago / Checkout"}</span>
            <MaterialIcon name="arrow_forward" className="text-[20px] transition-transform group-hover:translate-x-1" />
          </button>
        </div>

        {/* Garantía */}
        <div className="mt-4 flex items-start gap-2.5 rounded-xl bg-surface-container p-3">
          <MaterialIcon name="verified_user" className="shrink-0 text-[22px] text-primary" />
          <div className="space-y-0.5">
            <p className="text-label-md font-bold text-on-surface">Garantía 100% AuraLearn</p>
            <p className="text-label-sm text-on-surface-variant">
              Si la clase no cumple tus expectativas o el tutor no se presenta, te reintegramos el 100%.
            </p>
          </div>
        </div>

        {/* Métodos aceptados */}
        <div className="mt-4 flex items-center justify-center gap-4 pt-3 text-label-sm text-on-surface-variant">
          <span className="flex items-center gap-1 font-semibold text-primary">
            <span className="h-2 w-2 rounded-full bg-primary" />
            SINPE Móvil
          </span>
          <span>•</span>
          <span>Visa / Mastercard</span>
          <span>•</span>
          <span>Transferencia IBAN</span>
        </div>
      </div>

      {/* Horario especial */}
      <div className="flex items-center justify-between rounded-xl bg-surface-container-lowest p-4 shadow-sm">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-fixed text-primary">
            <MaterialIcon name="calendar_month" className="text-[20px]" />
          </div>
          <div>
            <p className="text-label-md font-bold text-on-surface">¿Requieres un horario especial?</p>
            <p className="text-label-sm text-on-surface-variant">El tutor responde consultas de agenda por mensaje privado</p>
          </div>
        </div>
        <button
          type="button"
          className="rounded-lg bg-surface-container px-3 py-1.5 text-label-md font-bold text-on-surface transition-colors hover:bg-surface-container-high"
        >
          Enviar Mensaje
        </button>
      </div>
    </aside>
  );

  function slotsOfSelectedDay(): Slot[] {
    if (!selectedDayKey) return [];
    return byDay.get(selectedDayKey)?.slots ?? [];
  }
}

// ── Helpers locales ──

function localHour(isoUtc: string): number {
  const v = new Intl.DateTimeFormat("es-CR", {
    timeZone: "America/Costa_Rica",
    hour: "2-digit",
    hour12: false,
  }).formatToParts(new Date(isoUtc))
    .find((p) => p.type === "hour")
    ?.value.replace(/\u200e/g, "") ?? "0";
  return Number(v) % 24; // "24" del formatter 00h → 0
}

function dayKeyOf(slot: Slot): string {
  return new Intl.DateTimeFormat("en-CA", {
    timeZone: "America/Costa_Rica",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(new Date(slot.startAt)); // "YYYY-MM-DD"
}

function dayLabelOf(slot: Slot): string {
  return new Intl.DateTimeFormat("es-CR", {
    timeZone: "America/Costa_Rica",
    weekday: "long",
    day: "2-digit",
    month: "long",
  }).format(new Date(slot.startAt));
}

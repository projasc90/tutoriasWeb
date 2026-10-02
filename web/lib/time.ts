/**
 * Helpers de tiempo para fechas UTC del backend → zona America/Costa_Rica.
 *
 * Regla del proyecto (ADR-005): las fechas se almacenan en UTC y se formatean
 * solo en presentación (capa UI).
 */

const COSTA_RICA_TZ = "America/Costa_Rica";

/** Formato corto: "Jueves, 14 Nov · 10:00". */
export function formatSlotEsCr(isoUtc: string): string {
  const d = new Date(isoUtc);
  return new Intl.DateTimeFormat("es-CR", {
    weekday: "long",
    day: "2-digit",
    month: "short",
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: COSTA_RICA_TZ,
  }).format(d);
}

/** Formato de fecha larga: "Jueves 14 de noviembre de 2026". */
export function formatLongDateEsCr(isoUtc: string): string {
  const d = new Date(isoUtc);
  return new Intl.DateTimeFormat("es-CR", {
    weekday: "long",
    day: "2-digit",
    month: "long",
    year: "numeric",
    timeZone: COSTA_RICA_TZ,
  }).format(d);
}

/** Solo hora HH:mm en zona CR. */
export function formatHourEsCr(isoUtc: string): string {
  const d = new Date(isoUtc);
  return new Intl.DateTimeFormat("es-CR", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: COSTA_RICA_TZ,
  }).format(d);
}

/** Moneda CRC con separadores de miles. */
export function formatCrc(amount: number): string {
  return `₡${amount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")}`;
}

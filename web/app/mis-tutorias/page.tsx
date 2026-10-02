"use client";

/**
 * Panel del estudiante: reservas, monedero y cancelación con política 12 h.
 * Patrón espejo de /admin/tutores: guard de sesión + lista de cards + estado.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useMyReservations } from "@/hooks/use-my-reservations";
import { useReservation } from "@/hooks/use-reservation";
import { fetchWallet } from "@/lib/api";
import { formatCrc, formatSlotEsCr } from "@/lib/time";
import type { Reservation, ReservationStatus } from "@/lib/api";

const STATUS_LABEL: Record<ReservationStatus, string> = {
  PendingPayment: "Esperando pago",
  Confirmed: "Confirmada",
  Rejected: "Rechazada",
  Expired: "Expirada",
  Cancelled: "Cancelada",
  Completed: "Completada",
};

const STATUS_TONE: Record<ReservationStatus, string> = {
  PendingPayment: "bg-tertiary-container text-on-tertiary-container",
  Confirmed: "bg-primary-container text-on-primary-container",
  Rejected: "bg-error-container text-on-error-container",
  Expired: "bg-surface-container-high text-on-surface-variant",
  Cancelled: "bg-surface-container-high text-on-surface-variant",
  Completed: "bg-primary-container text-on-primary-container",
};

function hoursUntil(isoUtc: string): number {
  return (new Date(isoUtc).getTime() - Date.now()) / 3_600_000;
}

export default function MyTutorialsPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { state, reload } = useMyReservations(token);
  const reservationApi = useReservation();
  const [wallet, setWallet] = useState<number | null>(null);
  const [confirmCancel, setConfirmCancel] = useState<Reservation | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    if (authState.status === "anonymous") router.replace("/login");
  }, [authState.status, router]);

  useEffect(() => {
    if (!token) return;
    let cancelled = false;
    (async () => {
      const result = await fetchWallet(token);
      if (cancelled) return;
      if (result.ok) setWallet(result.data.balanceCrc);
    })();
    return () => {
      cancelled = true;
    };
  }, [token, state]); // recarga después de cancelar

  if (authState.status === "loading" || authState.status === "anonymous") {
    return (
      <>
        <SiteHeader />
        <main className="flex flex-1 flex-col bg-background min-h-screen">
          <div className="mx-auto max-w-4xl px-6 py-12">
            <p className="text-body-lg text-on-surface-variant">Cargando…</p>
          </div>
        </main>
      </>
    );
  }

  const onCancel = async (r: Reservation, reschedule = false) => {
    setActionError(null);
    const result = await reservationApi.cancel(token!, r.id);
    if (result.ok) {
      setConfirmCancel(null);
      reload();
      if (reschedule) {
        // Reprogramación = cancelar + crear: elige nuevo horario con el mismo tutor
        router.push(`/tutores/${r.tutorId}`);
      }
    } else {
      setActionError(result.error);
    }
  };

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 flex-col bg-background min-h-screen">
        <section className="bg-surface-container-low border-b border-outline-variant/40">
          <div className="mx-auto max-w-4xl px-6 py-10 lg:px-12">
            <h1 className="text-headline-lg font-bold text-on-surface">Mis tutorías</h1>
            <p className="mt-1 text-body-md text-on-surface-variant">
              Gestiona tus reservas, completa pagos pendientes y revisa tu monedero.
            </p>
            {/* Wallet */}
            <div className="mt-6 inline-flex items-center gap-3 rounded-xl bg-surface-container-lowest px-4 py-3 shadow-sm">
              <MaterialIcon name="account_balance_wallet" className="text-[24px] text-primary" />
              <div>
                <p className="text-label-md text-on-surface-variant">Monedero</p>
                <p className="text-title-md font-bold text-on-surface">
                  {wallet !== null ? formatCrc(wallet) : "—"}
                </p>
              </div>
            </div>
          </div>
        </section>

        <section className="mx-auto max-w-4xl px-6 py-10 lg:px-12">
          {state.status === "loading" ? (
            <p className="text-body-lg text-on-surface-variant">Cargando reservas…</p>
          ) : state.status === "error" ? (
            <p className="text-body-lg text-error">{state.error}</p>
          ) : state.status === "success" && state.data.length === 0 ? (
            <div className="rounded-xl bg-surface-container-lowest p-12 text-center shadow-sm">
              <MaterialIcon name="event_available" className="text-[40px] text-on-surface-variant" />
              <p className="mt-2 text-body-md text-on-surface-variant">Aún no tienes reservas.</p>
              <Link href="/tutores" className="mt-4 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-on-primary text-label-md font-bold">
                Explorar tutores
              </Link>
            </div>
          ) : state.status === "success" ? (
            <div className="flex flex-col gap-4">
              {state.data.map((r) => {
                const hrs = hoursUntil(r.startAt);
                const canCancel = (r.status === "PendingPayment" || r.status === "Confirmed") && hrs > 0;
                const cancelPolicy =
                  r.status === "PendingPayment"
                    ? hrs > 0
                      ? "Sin pago confirmado: cancelar no genera crédito."
                      : "La ventana de pago ya venció."
                    : hrs >= 12
                      ? "Cancelación con ≥12 h: el monto vuelve a tu monedero."
                      : hrs > 0
                        ? "Cancelación con <12 h: el tutor ya reservó su tiempo, no hay reembolso."
                        : "La sesión ya pasó.";
                return (
                  <article key={r.id} className="rounded-xl bg-surface-container-lowest p-5 shadow-sm">
                    <div className="flex flex-wrap items-center gap-3">
                      <h3 className="text-title-md font-bold text-on-surface">{r.tutorName}</h3>
                      <span className={`rounded-full px-2.5 py-0.5 text-label-sm font-semibold ${STATUS_TONE[r.status]}`}>
                        {STATUS_LABEL[r.status]}
                      </span>
                    </div>
                    <p className="mt-1 text-body-md text-on-surface">
                      {formatSlotEsCr(r.startAt)}
                    </p>
                    <p className="font-numeric text-label-md text-on-surface-variant">
                      Reserva #{r.id.slice(0, 8)} · <span className="text-numeric-table">{formatCrc(r.priceCrc)}</span>
                    </p>
                    {r.status === "Rejected" && r.decisionReason && (
                      <p className="mt-2 text-label-md text-error">Motivo: {r.decisionReason}</p>
                    )}
                    <div className="mt-4 flex flex-wrap gap-2">
                      {r.status === "PendingPayment" && (
                        <Link href={`/checkout/${r.id}`} className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-bold hover:bg-on-primary-fixed-variant">
                          Completar pago
                        </Link>
                      )}
                      {canCancel && (
                        <button
                          type="button"
                          onClick={() => { setConfirmCancel(r); setActionError(null); }}
                          className="rounded-lg border border-error px-4 py-2 text-error text-label-md font-bold hover:bg-error hover:text-on-error"
                        >
                          Cancelar reserva
                        </button>
                      )}
                    </div>
                    {canCancel && (
                      <p className="mt-2 text-label-sm text-on-surface-variant">{cancelPolicy}</p>
                    )}
                  </article>
                );
              })}
            </div>
          ) : null}
        </section>

        {/* Modal de cancelación */}
        {confirmCancel && (
          <div className="fixed inset-0 z-50 grid place-items-center bg-black/50 p-4" role="dialog" aria-modal="true">
            <div className="w-full max-w-md rounded-xl bg-surface-container-lowest p-6 shadow-xl">
              <h2 className="text-title-md font-bold text-on-surface">Cancelar reserva</h2>
              <p className="mt-2 text-body-md text-on-surface-variant">
                Vas a cancelar la sesión con <strong>{confirmCancel.tutorName}</strong> programada para {formatSlotEsCr(confirmCancel.startAt)}.
              </p>
              <p className="mt-3 rounded-lg bg-tertiary-container p-3 text-label-md text-on-tertiary-container">
                {confirmCancel.status === "PendingPayment"
                  ? "La reserva aún no tiene pago confirmado: no se generará crédito."
                  : hoursUntil(confirmCancel.startAt) >= 12
                    ? "Faltan ≥12 h. El monto se acreditará a tu monedero."
                    : "Faltan <12 h. No hay reembolso (el tutor ya reservó su tiempo)."}
              </p>
              {actionError && <p className="mt-3 text-label-md text-error">{actionError}</p>}
              <div className="mt-4 flex justify-end gap-2">
                {confirmCancel.status === "Confirmed" && hoursUntil(confirmCancel.startAt) >= 12 && (
                  <button
                    type="button"
                    onClick={() => onCancel(confirmCancel, true)}
                    className="mr-auto rounded-lg border border-primary px-4 py-2 text-primary text-label-md font-bold hover:bg-primary hover:text-on-primary"
                  >
                    Reprogramar (acredita y elige otro horario)
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => setConfirmCancel(null)}
                  className="rounded-lg border border-outline px-4 py-2 text-on-surface"
                >
                  Volver
                </button>
                <button
                  type="button"
                  onClick={() => onCancel(confirmCancel)}
                  className="rounded-lg bg-error px-4 py-2 text-on-error font-bold"
                >
                  Sí, cancelar
                </button>
              </div>
            </div>
          </div>
        )}
      </main>
    </>
  );
}

"use client";

/**
 * Panel del tutor: sesiones confirmadas por completar y liquidadas.
 * Patrón espejo de /mis-tutorias: guard de rol Tutor + lista de cards + estado.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useTutorSessions } from "@/hooks/use-tutor-sessions";
import { completeSession, fetchWallet } from "@/lib/api";
import { formatCrc, formatSlotEsCr } from "@/lib/time";
import type { Reservation } from "@/lib/api";

export default function TutorSessionsPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { state, reload } = useTutorSessions(token);
  const [wallet, setWallet] = useState<number | null>(null);
  const [completingId, setCompletingId] = useState<string | null>(null);
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
    return () => { cancelled = true; };
  }, [token, state]); // recarga tras completar una sesión

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

  const onComplete = async (r: Reservation) => {
    setActionError(null);
    setCompletingId(r.id);
    const result = await completeSession(token!, r.id);
    setCompletingId(null);
    if (result.ok) {
      reload();
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
            <h1 className="text-headline-lg font-bold text-on-surface">Mis sesiones</h1>
            <p className="mt-1 text-body-md text-on-surface-variant">
              Cierra tus sesiones confirmadas una vez concluidas: el pago se liquida a tu monedero.
            </p>
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
            <p className="text-body-lg text-on-surface-variant">Cargando sesiones…</p>
          ) : state.status === "error" ? (
            <p className="text-body-lg text-error">{state.error}</p>
          ) : state.status === "success" && state.data.length === 0 ? (
            <div className="rounded-xl bg-surface-container-lowest p-12 text-center shadow-sm">
              <MaterialIcon name="school" className="text-[40px] text-on-surface-variant" />
              <p className="mt-2 text-body-md text-on-surface-variant">
                Aún no tienes sesiones reservadas.
              </p>
              <Link href="/tutores" className="mt-4 inline-block text-primary hover:underline">
                Ver el directorio
              </Link>
            </div>
          ) : state.status === "success" ? (
            <div className="flex flex-col gap-4">
              {state.data.map((r) => {
                const canComplete = r.status === "Confirmed" && hasEnded(r.endAt);
                return (
                  <article key={r.id} className="rounded-xl bg-surface-container-lowest p-5 shadow-sm">
                    <div className="flex flex-wrap items-center gap-3">
                      <h3 className="text-title-md font-bold text-on-surface">{r.studentName}</h3>
                      <span className="rounded-full bg-surface-container-high px-3 py-1 text-label-sm text-on-surface-variant">
                        {r.status === "Confirmed"
                          ? "Confirmada"
                          : r.status === "Completed"
                            ? "Completada"
                            : r.status === "PendingPayment"
                              ? "Esperando pago"
                              : r.status === "Expired"
                                ? "Expirada"
                                : r.status === "Cancelled"
                                  ? "Cancelada"
                                  : "Rechazada"}
                      </span>
                    </div>
                    <p className="mt-2 text-body-md text-on-surface-variant capitalize font-numeric">
                      <span className="text-numeric-table">{formatSlotEsCr(r.startAt)}</span>
                      {" — "}
                      <span className="text-numeric-table">{formatHourEnd(r.endAt)}</span>
                      {" · "}
                      <span className="text-numeric-table">{formatCrc(r.priceCrc)}</span>
                    </p>
                    {r.status === "Completed" && (
                      <p className="mt-2 text-label-md text-on-primary-container rounded-lg bg-primary-container p-3">
                        Liquidado: {formatCrc(r.priceCrc)} acreditados a tu monedero.
                      </p>
                    )}
                    {canComplete && (
                      <button
                        type="button"
                        disabled={completingId === r.id}
                        onClick={() => onComplete(r)}
                        className="mt-3 rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-bold hover:bg-on-primary-fixed-variant disabled:opacity-50"
                      >
                        {completingId === r.id ? "Completando…" : "Completar sesión"}
                      </button>
                    )}
                    {actionError && completingId === r.id && (
                      <p className="mt-2 text-label-md text-error">{actionError}</p>
                    )}
                  </article>
                );
              })}
            </div>
          ) : null}
        </section>
      </main>
    </>
  );
}

function hasEnded(isoUtc: string): boolean {
  return new Date(isoUtc).getTime() <= Date.now();
}

function formatHourEnd(isoUtc: string): string {
  return new Intl.DateTimeFormat("es-CR", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
    timeZone: "America/Costa_Rica",
  }).format(new Date(isoUtc));
}

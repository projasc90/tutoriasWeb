"use client";

/**
 * Cola admin de pagos pendientes (confirmación manual SINPE).
 * Patrón espejo de /admin/tutores: guard de rol Admin + cola + tarjeta
 * con datos del comprobante + aprobar/rechazar con motivo obligatorio.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useAdminPayments } from "@/hooks/use-admin-payments";
import { formatCrc, formatSlotEsCr } from "@/lib/time";
import type { AdminPayment } from "@/lib/api";

export default function AdminPaymentsPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;
  const isAdmin = authState.status === "authenticated" && authState.session.role === "Admin";

  const { state, deciding, decide } = useAdminPayments(isAdmin ? token : null);
  const [rejectFor, setRejectFor] = useState<AdminPayment | null>(null);
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (authState.status === "anonymous") router.replace("/login");
  }, [authState.status, router]);

  if (authState.status === "loading" || authState.status === "anonymous") {
    return (
      <>
        <SiteHeader />
        <main className="flex flex-1 flex-col bg-background min-h-screen">
          <div className="mx-auto max-w-5xl px-6 py-12">
            <p className="text-body-lg text-on-surface-variant">Verificando acceso…</p>
          </div>
        </main>
      </>
    );
  }

  if (!isAdmin) {
    return (
      <>
        <SiteHeader />
        <main className="flex flex-1 flex-col bg-background min-h-screen">
          <div className="mx-auto max-w-5xl px-6 py-12 text-center">
            <h1 className="text-headline-md font-bold text-on-surface">Acceso restringido</h1>
            <p className="mt-2 text-body-md text-on-surface-variant">
              Esta sección es solo para administradores de AuraLearn.
            </p>
            <Link href="/login" className="mt-4 inline-block text-primary hover:underline">
              Cambiar de cuenta
            </Link>
          </div>
        </main>
      </>
    );
  }

  const submitDecision = async (p: AdminPayment, decision: "approve" | "reject") => {
    setError(null);
    if (decision === "reject" && reason.trim().length < 10) {
      setError("El motivo debe tener al menos 10 caracteres.");
      return;
    }
    const result = await decide(p.reservationId, decision, decision === "reject" ? reason : undefined);
    if (result.ok) {
      setRejectFor(null);
      setReason("");
    } else {
      setError(result.error);
    }
  };

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 flex-col bg-background min-h-screen">
        <section className="bg-surface-container-low border-b border-outline-variant/40">
          <div className="mx-auto max-w-5xl px-6 py-10 lg:px-12">
            <h1 className="text-headline-lg font-bold text-on-surface">Cola de pagos</h1>
            <p className="mt-1 text-body-md text-on-surface-variant">
              Aprueba o rechaza los comprobantes SINPE que los estudiantes subieron.
            </p>
          </div>
        </section>

        <section className="mx-auto max-w-5xl px-6 py-10 lg:px-12">
          {state.status === "loading" ? (
            <p className="text-body-lg text-on-surface-variant">Cargando cola…</p>
          ) : state.status === "error" ? (
            <p className="text-body-lg text-error">{state.error}</p>
          ) : state.status === "success" && state.items.length === 0 ? (
            <div className="rounded-xl bg-surface-container-lowest p-12 text-center shadow-sm">
              <MaterialIcon name="inbox" className="text-[40px] text-on-surface-variant" />
              <p className="mt-2 text-body-md text-on-surface-variant">
                No hay comprobantes pendientes de revisión.
              </p>
            </div>
          ) : (
            <div className="flex flex-col gap-4">
              {(state as { status: "success"; items: AdminPayment[] }).items.map((p: AdminPayment) => {
                const isDeciding = deciding === p.reservationId;
                return (
                  <article key={p.reservationId} className="flex flex-col gap-4 rounded-xl bg-surface-container-lowest p-5 shadow-sm md:flex-row md:items-center md:justify-between">
                    <div className="flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="text-title-md font-bold text-on-surface">{p.tutorName}</h3>
                        <span className="text-label-md text-on-surface-variant">con</span>
                        <span className="text-title-md font-semibold text-on-surface">{p.studentName}</span>
                      </div>
                      <dl className="mt-2 grid grid-cols-2 gap-x-4 gap-y-1 text-label-md md:grid-cols-4">
                        <div>
                          <dt className="text-on-surface-variant">Fecha</dt>
                          <dd className="font-semibold text-on-surface">{formatSlotEsCr(p.startAt)}</dd>
                        </div>
                        <div>
                          <dt className="text-on-surface-variant">Precio</dt>
                          <dd className="font-semibold text-on-surface">{formatCrc(p.priceCrc)}</dd>
                        </div>
                        <div>
                          <dt className="text-on-surface-variant">Nº confirmación</dt>
                          <dd className="font-mono text-on-surface">{p.confirmationNumber}</dd>
                        </div>
                        <div>
                          <dt className="text-on-surface-variant">Monto reportado</dt>
                          <dd className="font-semibold text-on-surface">
                            {p.comprobanteAmountCrc ? formatCrc(p.comprobanteAmountCrc) : "—"}
                          </dd>
                        </div>
                      </dl>
                      {p.comprobantePhone && (
                        <p className="mt-2 text-label-sm text-on-surface-variant">
                          Teléfono reportado: {p.comprobantePhone}
                        </p>
                      )}
                    </div>
                    <div className="flex gap-2 md:flex-col">
                      <button
                        type="button"
                        disabled={isDeciding}
                        onClick={() => submitDecision(p, "approve")}
                        className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-bold hover:bg-on-primary-fixed-variant disabled:opacity-50"
                      >
                        Aprobar
                      </button>
                      <button
                        type="button"
                        disabled={isDeciding}
                        onClick={() => { setRejectFor(p); setReason(""); setError(null); }}
                        className="rounded-lg border border-error px-4 py-2 text-error text-label-md font-bold hover:bg-error hover:text-on-error disabled:opacity-50"
                      >
                        Rechazar
                      </button>
                    </div>
                  </article>
                );
              })}
            </div>
          )}
          {error && <p className="mt-4 text-label-md text-error">{error}</p>}
        </section>

        {/* Modal de rechazo */}
        {rejectFor && (
          <div className="fixed inset-0 z-50 grid place-items-center bg-black/50 p-4" role="dialog" aria-modal="true">
            <div className="w-full max-w-md rounded-xl bg-surface-container-lowest p-6 shadow-xl">
              <h2 className="text-title-md font-bold text-on-surface">Rechazar comprobante</h2>
              <p className="mt-2 text-body-md text-on-surface-variant">
                {rejectFor.tutorName} · {rejectFor.confirmationNumber}
              </p>
              <label className="mt-4 flex flex-col gap-1.5">
                <span className="text-label-md font-semibold text-on-surface">Motivo (mínimo 10 caracteres)</span>
                <textarea
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  rows={3}
                  className="rounded-lg bg-surface-container-low px-3 py-2 text-on-surface focus:outline-none focus:ring-2 focus:ring-primary"
                  placeholder="Ej. el monto reportado no coincide con el precio."
                />
              </label>
              <div className="mt-4 flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => { setRejectFor(null); setReason(""); }}
                  className="rounded-lg border border-outline px-4 py-2 text-on-surface"
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  onClick={() => submitDecision(rejectFor, "reject")}
                  disabled={reason.trim().length < 10 || deciding === rejectFor.reservationId}
                  className="rounded-lg bg-error px-4 py-2 text-on-error font-bold disabled:opacity-50"
                >
                  Confirmar rechazo
                </button>
              </div>
            </div>
          </div>
        )}
      </main>
    </>
  );
}

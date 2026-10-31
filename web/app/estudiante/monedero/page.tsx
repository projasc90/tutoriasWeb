"use client";

/**
 * Panel del estudiante → Monedero: saldo disponible y explicación de los
 * créditos de cancelación. El saldo solo se acredita al cancelar con
 * anticipación ≥ la regla del Admin (default 12 h) y se puede usar para
 * pagar reservas futuras.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useBookingRules } from "@/hooks/use-booking-rules";
import { fetchWallet, BOOKING_RULES_DEFAULTS } from "@/lib/api";
import { formatCrc } from "@/lib/time";

export default function StudentMonederoPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { state: reglasState } = useBookingRules();
  const noticeHoras =
    reglasState.status === "success"
      ? (reglasState.data.cancellationNoticeHours ?? BOOKING_RULES_DEFAULTS.cancellationNoticeHours)
      : BOOKING_RULES_DEFAULTS.cancellationNoticeHours;

  const [wallet, setWallet] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

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
      else setError(result.error);
    })();
    return () => { cancelled = true; };
  }, [token]);

  if (authState.status === "loading" || authState.status === "anonymous") {
    return (
      <main className="flex flex-1 flex-col bg-background min-h-screen">
        <div className="mx-auto max-w-4xl px-6 py-12">
          <p className="text-body-lg text-on-surface-variant">Cargando…</p>
        </div>
      </main>
    );
  }

  return (
    <main className="flex flex-1 flex-col bg-background min-h-screen">
      <section className="bg-surface-container-low border-b border-outline-variant/40">
        <div className="mx-auto max-w-4xl px-6 py-10 lg:px-12">
          <h1 className="text-headline-lg font-bold text-on-surface">Monedero</h1>
          <p className="mt-1 text-body-md text-on-surface-variant">
            Créditos disponibles para pagar tus próximas tutorías.
          </p>
        </div>
      </section>

      <section className="mx-auto max-w-4xl space-y-6 px-6 py-10 lg:px-12">
        <div className="flex items-center gap-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
          <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-xl bg-primary-fixed text-primary">
            <MaterialIcon name="account_balance_wallet" className="text-[28px]" />
          </div>
          <div>
            <p className="text-label-md text-on-surface-variant">Saldo disponible</p>
            {error ? (
              <p role="alert" className="text-body-md text-error">{error}</p>
            ) : (
              <p className="font-numeric text-headline-md font-extrabold text-on-surface">
                {wallet !== null ? formatCrc(wallet) : "…"}
              </p>
            )}
          </div>
        </div>

        <div className="space-y-3 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
          <h2 className="flex items-center gap-2 text-title-md font-bold text-on-surface">
            <MaterialIcon name="info" className="text-[20px] text-primary" />
            ¿Cómo se acredita el saldo?
          </h2>
          <ul className="space-y-2 text-body-md text-on-surface-variant">
            <li className="flex items-start gap-2">
              <MaterialIcon name="check_circle" className="mt-0.5 shrink-0 text-[16px] text-primary" />
              Si cancelas una reserva confirmada con ≥{noticeHoras} horas de anticipación, el monto
              vuelve a tu monedero como crédito.
            </li>
            <li className="flex items-start gap-2">
              <MaterialIcon name="check_circle" className="mt-0.5 shrink-0 text-[16px] text-primary" />
              Al crear una reserva puedes marcar “Pagar con monedero” si tu saldo cubre el precio de
              la sesión: la reserva nace confirmada, sin pasar por SINPE.
            </li>
            <li className="flex items-start gap-2">
              <MaterialIcon name="check_circle" className="mt-0.5 shrink-0 text-[16px] text-primary" />
              El saldo no se puede retirar; solo se usa para reservar en AuraLearn.
            </li>
          </ul>
        </div>

        <Link
          href="/estudiante/reservas"
          className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-on-primary text-label-md font-bold hover:bg-on-primary-fixed-variant"
        >
          <MaterialIcon name="event_available" className="text-[18px]" />
          Ver mis reservas
        </Link>
      </section>
    </main>
  );
}

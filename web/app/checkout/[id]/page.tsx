"use client";

/**
 * Checkout (Stitch 03): stepper 3 pasos + banner temporizador + layout 2 columnas.
 * Izquierda: detalles de la clase + método de pago (SINPE activo; tarjeta/IBAN
 * próximamente). Derecha sticky: tutor snapshot + resumen de orden + confirmación.
 * El endpoint POST /comprobante valida automáticamente y deja la reserva en
 * PendingPayment con el comprobante adjunto; polling cada 3s hasta confirmar.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useReservation, useReservationPoll } from "@/hooks/use-reservation";
import { useTutor } from "@/hooks/use-tutor";
import { formatCrc } from "@/lib/time";
import type { Reservation } from "@/lib/api";
import { CheckoutStepper } from "@/components/checkout/checkout-stepper";
import { TimerBanner } from "@/components/checkout/timer-banner";
import { BookingDetails } from "@/components/checkout/booking-details";
import { PaymentMethodCard } from "@/components/checkout/payment-method-card";
import { OrderSummary, type SubmitStateLite } from "@/components/checkout/order-summary";
import {
  ConfirmedView,
  RejectedView,
  ExpiredView,
  CancelledView,
} from "@/components/checkout/terminal-views";

const POLL_MS = 3000;

export default function CheckoutPage() {
  const params = useParams<{ id: string }>();
  const reservationId = params?.id ?? "";
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { reservation, error: pollError } = useReservationPoll(token, reservationId || null, POLL_MS);
  const reservationApi = useReservation();

  const [confirmationNumber, setConfirmationNumber] = useState("");
  const [phone, setPhone] = useState("");

  // El monto del comprobante es siempre el precio de la reserva (validación
  // exacta del backend); se muestra y envía tal cual.

  // Reloj para el banner temporizador (refresco por minuto)
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    const t = setInterval(() => setNow(Date.now()), 30_000);
    return () => clearInterval(t);
  }, []);

  if (!token) return <NotLoggedIn />;

  if (!reservation) {
    return (
      <Shell>
        <div className="mx-auto max-w-md px-6 py-16 text-center">
          <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
            <p className="text-body-lg text-on-surface-variant">{pollError ?? "Cargando reserva…"}</p>
          </div>
        </div>
      </Shell>
    );
  }

  if (reservation.status === "Confirmed") return <ConfirmedView reservation={reservation} />;
  if (reservation.status === "Rejected") return <RejectedView reservation={reservation} />;
  if (reservation.status === "Expired") return <ExpiredView />;
  if (reservation.status === "Cancelled") return <CancelledView />;

  return (
    <CheckoutForm
      reservation={reservation}
      confirmationNumber={confirmationNumber}
      setConfirmationNumber={setConfirmationNumber}
      phone={phone}
      setPhone={setPhone}
      now={now}
      submitState={reservationApi.state}
      submit={async () => reservationApi.submitComprobante(token, reservation.id, {
        confirmationNumber, amountCrc: reservation.priceCrc, phone: phone || null,
      })}
      onSubmitted={() => setConfirmationNumber("")}
    />
  );
}

function Shell({ children }: { children: React.ReactNode }) {
  return (
    <>
      <SiteHeader />
      <main className="flex min-h-screen flex-1 flex-col bg-background">{children}</main>
    </>
  );
}

function NotLoggedIn() {
  return (
    <Shell>
      <div className="mx-auto max-w-md px-6 py-16 text-center">
        <div className="rounded-xl bg-surface-container-lowest p-8 shadow-sm">
          <div className="mx-auto grid h-16 w-16 place-items-center rounded-full bg-surface-container-high text-on-surface-variant">
            <MaterialIcon name="lock" className="text-[28px]" />
          </div>
          <h1 className="mt-4 text-headline-md font-bold text-on-surface">Inicia sesión para continuar</h1>
          <p className="mt-2 text-body-md text-on-surface-variant">
            Necesitas una cuenta para subir el comprobante y completar la reserva.
          </p>
          <Link href="/login" className="mt-6 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary">
            Iniciar sesión
          </Link>
        </div>
      </div>
    </Shell>
  );
}

type CheckoutFormProps = {
  reservation: Reservation;
  confirmationNumber: string;
  setConfirmationNumber: (v: string) => void;
  phone: string;
  setPhone: (v: string) => void;
  now: number;
  submitState: SubmitStateLite;
  submit: () => Promise<{ ok: boolean; error?: string }>;
  onSubmitted: () => void;
};

function CheckoutForm(props: CheckoutFormProps) {
  const {
    reservation, confirmationNumber, setConfirmationNumber,
    phone, setPhone,
    now, submitState, submit, onSubmitted,
  } = props;

  // Datos del tutor para el snapshot (catálogo real; null-safe si aún carga)
  const { tutor } = useTutor(reservation.tutorId || null);

  const payment = reservation.paymentInfo;
  const receptorPhone = payment?.receptorPhone ?? "88888888";
  const expectedAmount = payment?.amountCrc ?? reservation.priceCrc;
  const expiresAt = payment?.expiresAt ?? reservation.expiresAt;
  const submitted = !!reservation.confirmationNumber;
  const minutesLeft = Math.max(0, Math.floor((new Date(expiresAt).getTime() - now) / 60000));

  const onFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const result = await submit();
    if (result.ok) onSubmitted();
  };

  // El botón "Confirmar y Pagar" del resumen dispara el form del método de pago
  const confirmClick = () => {
    const form = document.getElementById("checkout-form") as HTMLFormElement | null;
    form?.requestSubmit();
  };

  const submitLabel = submitted
    ? "Comprobante enviado · en revisión"
    : submitState.status === "loading"
      ? "Enviando…"
      : `Confirmar y Pagar ${formatCrc(expectedAmount)}`;

  return (
    <>
      <SiteHeader />
      <main className="min-h-screen flex-1 bg-background">
        <div className="mx-auto w-full max-w-7xl px-6 py-6 lg:px-12">
          <CheckoutStepper />

          <div className="mt-4">
            <TimerBanner minutesLeft={minutesLeft} />
          </div>

          <div className="grid grid-cols-1 items-start gap-6 lg:grid-cols-12">
            {/* Columna izquierda: clase + método de pago */}
            <div className="space-y-6 lg:col-span-7 xl:col-span-8">
              <BookingDetails
                tutorName={reservation.tutorName}
                subject={tutor?.subjects[0] ?? "Tutoría 1-a-1"}
                startAt={reservation.startAt}
                endAt={reservation.endAt}
              />

              <form id="checkout-form" onSubmit={onFormSubmit}>
                <PaymentMethodCard
                  receptorPhone={receptorPhone}
                  amountCrc={expectedAmount}
                  confirmationNumber={confirmationNumber}
                  setConfirmationNumber={setConfirmationNumber}
                  phone={phone}
                  setPhone={setPhone}
                  submitted={submitted}
                />
                <button type="submit" className="sr-only" tabIndex={-1}>
                  Enviar comprobante
                </button>
                {submitState.status === "error" && (
                  <p className="mt-2 text-label-md text-error">{submitState.error}</p>
                )}
                {submitState.status === "success" && (
                  <p className="mt-2 text-label-md text-primary">
                    Comprobante recibido. Estamos revisándolo.
                  </p>
                )}
              </form>
            </div>

            {/* Columna derecha (sticky): tutor + resumen + confirmación */}
            <aside className="lg:col-span-5 xl:col-span-4">
              <OrderSummary
                tutorName={reservation.tutorName}
                tutorCredentials={tutor ? `${tutor.credentials} · ${tutor.university}` : "Tutor verificado"}
                tutorRating={tutor?.rating ?? 0}
                tutorReviews={tutor?.reviews ?? 0}
                priceCrc={expectedAmount}
                priceUsd={tutor?.priceUsd ?? 0}
                minutesLeft={minutesLeft}
                submitState={submitState}
                submitLabel={submitLabel}
                onSubmit={confirmClick}
              />
            </aside>
          </div>

          <Link href="/tutores" className="mt-6 inline-flex items-center gap-2 text-label-md text-on-surface-variant hover:text-primary">
            <MaterialIcon name="arrow_back" />
            Volver al directorio
          </Link>
        </div>
      </main>
    </>
  );
}

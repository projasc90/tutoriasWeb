"use client";

/**
 * Perfil del tutor (Stitch 02): layout 2 columnas.
 * Izquierda: header del perfil + enfoque pedagógico + materias (seleccionables)
 * + credenciales + reseñas. Derecha sticky: motor de reserva sincronizado
 * con la materia elegida. Datos reales: GET /api/tutors/{id} + /slots.
 */

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useTutor } from "@/hooks/use-tutor";
import { useTutorSlots } from "@/hooks/use-tutor-slots";
import { useReservation } from "@/hooks/use-reservation";
import { useAuth } from "@/hooks/use-auth";
import { fetchWallet } from "@/lib/api";
import type { Slot } from "@/lib/api";
import { TutorProfileHeader } from "@/components/tutor/tutor-profile-header";
import { SubjectCards } from "@/components/tutor/subject-cards";
import { BookingEngine } from "@/components/tutor/booking-engine";

export default function TutorDetailPage() {
  const params = useParams<{ id: string }>();
  const tutorId = params?.id ?? "";
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;
  const userId = authState.status === "authenticated" ? authState.session.id : null;

  const { tutor, isLoading, isError, error } = useTutor(tutorId || null);
  const { state: slotsState } = useTutorSlots(tutorId || null, token ?? undefined);
  const reservation = useReservation();

  const [selectedSubject, setSelectedSubject] = useState<string | null>(null);
  const [selectedSlotId, setSelectedSlotId] = useState<string | null>(null);
  const [wallet, setWallet] = useState<number | null>(null);
  const [payWithWallet, setPayWithWallet] = useState(false);

  // Saldo disponible solo con sesión (para el toggle de pago con monedero)
  useEffect(() => {
    if (!token) return;
    let cancelled = false;
    (async () => {
      const result = await fetchWallet(token);
      if (cancelled) return;
      if (result.ok) setWallet(result.data.balanceCrc);
    })();
    return () => { cancelled = true; };
  }, [token]);

  const slots: Slot[] = slotsState.status === "success" ? slotsState.data : [];

  const initials = useMemo(() => {
    if (!tutor) return "";
    return tutor.name
      .replace(/^(Dr\.|Dra\.|Ing\.|Prof\.|M\.Sc\.)\s+/, "")
      .split(" ")
      .slice(0, 2)
      .map((w) => w[0])
      .join("")
      .toUpperCase();
  }, [tutor]);

  const onReserve = async () => {
    if (!selectedSlotId || !token || !userId) return;
    const idempotencyKey = `slot-${selectedSlotId}-student-${userId}-${Date.now()}`;
    const result = await reservation.create(token, {
      slotId: selectedSlotId,
      idempotencyKey,
      useWalletBalance: payWithWallet || undefined,
    });
    if (result.ok) {
      // Pagado con saldo: la reserva nace Confirmed → directo al panel
      if (result.data.status === "Confirmed") {
        router.push("/mis-tutorias");
      } else {
        router.push(`/checkout/${result.data.id}`);
      }
    }
  };

  if (isLoading) {
    return (
      <>
        <SiteHeader />
        <main className="flex min-h-screen flex-1 flex-col bg-background">
          <div className="mx-auto max-w-7xl px-6 py-16 text-center">
            <p className="text-body-lg text-on-surface-variant">Cargando tutor…</p>
          </div>
        </main>
      </>
    );
  }

  if (isError || !tutor) {
    return (
      <>
        <SiteHeader />
        <main className="flex min-h-screen flex-1 flex-col bg-background">
          <div className="mx-auto max-w-7xl px-6 py-16 text-center">
            <p className="text-body-lg text-on-surface-variant">{error ?? "Tutor no disponible"}</p>
            <Link href="/tutores" className="mt-4 inline-block text-primary hover:underline">
              Volver al directorio
            </Link>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      <SiteHeader />
      <main className="flex-1 bg-background">
        <div className="relative mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-12">
          {/* Glow ambiental (solo decorativo) */}
          <div className="pointer-events-none absolute -z-10 left-1/4 top-10 h-96 w-96 rounded-full bg-primary-fixed/30 blur-3xl" />

          {/* Breadcrumb */}
          <div className="mb-6 flex items-center gap-1.5 text-label-md text-on-surface-variant">
            <Link href="/tutores" className="flex items-center gap-1 transition-colors hover:text-primary">
              <MaterialIcon name="home" className="text-[16px]" />
              Tutores
            </Link>
            <MaterialIcon name="chevron_right" className="text-[14px]" />
            <span className="font-semibold text-on-surface">{tutor.name}</span>
          </div>

          {/* Grid 2 columnas */}
          <div className="grid grid-cols-1 items-start gap-8 lg:grid-cols-12">
            {/* Izquierda */}
            <div className="space-y-6 lg:col-span-7 xl:col-span-8">
              <TutorProfileHeader tutor={tutor} initials={initials} />

              {/* Enfoque pedagógico */}
              <div className="space-y-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
                <div className="flex items-center justify-between">
                  <h2 className="flex items-center gap-2 text-headline-md text-on-surface">
                    <MaterialIcon name="psychology" className="text-primary" />
                    Enfoque Pedagógico &amp; Metodología
                  </h2>
                  <span className="rounded-full bg-surface-container px-2.5 py-0.5 text-label-sm font-bold text-on-surface">
                    12 Años de Cátedra
                  </span>
                </div>
                <p className="text-body-lg text-on-surface-variant">{tutor.bio}</p>
                <div className="grid grid-cols-1 gap-4 pt-2 md:grid-cols-3">
                  {[
                    ["Diagnóstico Conceptual", "Identificamos vacíos en pre-cálculo y álgebra previa que frenan la comprensión del tema actual."],
                    ["Resolución Guiada", "Trabajo en vivo en pizarra infinita: desarrollo sistemático paso a paso de problemas con trampas típicas de examen."],
                    ["Entregable Post-Sesión", "Envío inmediato del PDF indexado con ejercicios resueltos y banco de práctica."],
                  ].map(([t, d], i) => (
                    <div key={t} className="space-y-2 rounded-lg bg-surface-container-low p-4">
                      <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary-fixed font-bold text-primary">{i + 1}</div>
                      <h3 className="text-title-md text-on-surface">{t}</h3>
                      <p className="text-body-md text-on-surface-variant">{d}</p>
                    </div>
                  ))}
                </div>
              </div>

              {/* Materias (seleccionables → sincronizan el motor) */}
              <SubjectCards
                subjects={tutor.subjects}
                priceCrc={tutor.priceCrc}
                priceUsd={tutor.priceUsd}
                selected={selectedSubject}
                onSelect={setSelectedSubject}
              />

              {/* Credenciales */}
              <div className="space-y-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
                <h2 className="flex items-center gap-2 text-headline-md text-on-surface">
                  <MaterialIcon name="workspace_premium" className="text-primary" />
                  Atestados y Credenciales Académicas
                </h2>
                <div className="flex items-start gap-3">
                  <div className="shrink-0 rounded-lg bg-surface-container p-2 text-primary">
                    <MaterialIcon name="school" className="text-[24px]" />
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between">
                      <h4 className="text-title-md font-bold text-on-surface">{tutor.credentials}</h4>
                      <span className="flex items-center gap-1 text-label-sm font-bold text-primary">
                        <MaterialIcon name="verified" className="text-[14px]" /> Verificado por AuraLearn
                      </span>
                    </div>
                    <p className="text-body-md text-on-surface-variant">{tutor.university}</p>
                  </div>
                </div>
              </div>

              {/* Reseñas */}
              <div className="space-y-6 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
                <div className="flex flex-col justify-between gap-2 sm:flex-row sm:items-center">
                  <div>
                    <h2 className="flex items-center gap-2 text-headline-md text-on-surface">
                      <MaterialIcon name="star" filled className="text-tertiary" />
                      Reseñas Verificadas de Estudiantes
                    </h2>
                    <p className="text-body-md text-on-surface-variant">
                      Solo alumnos que completaron sesiones pagadas pueden calificar.
                    </p>
                  </div>
                  <div className="flex items-center gap-2 rounded-lg bg-surface-container-low px-3 py-1.5">
                    <div className="flex text-tertiary">
                      {[...Array(5)].map((_, i) => (
                        <MaterialIcon key={i} name="star" filled className="text-[18px]" />
                      ))}
                    </div>
                    <span className="font-bold text-on-surface">{tutor.rating.toFixed(2)} / 5.0</span>
                  </div>
                </div>
                <div className="rounded-xl bg-surface-container-low p-4 space-y-2">
                  <div className="flex items-center gap-3">
                    <div className="grid h-10 w-10 place-items-center rounded-full bg-primary-container text-label-md font-bold text-on-primary-container">
                      MS
                    </div>
                    <div>
                      <h4 className="leading-tight text-title-md text-on-surface">Mariana Chacón M.</h4>
                      <span className="text-label-sm text-secondary">Ingeniería Civil • ITCR</span>
                    </div>
                  </div>
                  <p className="text-body-md italic text-on-surface-variant">
                    “Me salvó por completo el examen parcial. En hora y media me explicó con gráficas vectoriales
                    que él mismo dibuja en la pizarra. Saqué 94 en el parcial. Súper recomendado.”
                  </p>
                  <div className="flex items-center gap-1 text-label-sm font-semibold text-primary">
                    <MaterialIcon name="check_circle" className="text-[14px]" />
                    Tutoría: 2 sesiones de Cálculo Superior
                  </div>
                </div>
              </div>
            </div>

            {/* Derecha: motor de reserva sticky */}
            <div className="lg:col-span-5 xl:col-span-4">
              <BookingEngine
                tutorName={tutor.name}
                slots={slots}
                selectedSlotId={selectedSlotId}
                onSelectSlot={setSelectedSlotId}
                priceCrc={tutor.priceCrc}
                priceUsd={tutor.priceUsd}
                wallet={wallet}
                payWithWallet={payWithWallet}
                onPayWithWalletChange={setPayWithWallet}
                disabled={!token}
                loading={reservation.state.status === "loading"}
                onSubmit={onReserve}
              />
            </div>
          </div>
        </div>
      </main>
    </>
  );
}

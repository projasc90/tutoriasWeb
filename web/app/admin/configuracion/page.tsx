"use client";

/**
 * Panel admin: configuración de las reglas de reserva de la plataforma
 * (fila única en BD, editable por el Admin). El gestor de disponibilidad del
 * tutor las muestra como solo lectura; la lógica del backend las aplica con
 * fallback a los defaults cuando una regla queda "sin definir".
 */

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { BookingRulesForm } from "@/components/admin/booking-rules-form";
import { useAuth } from "@/hooks/use-auth";
import { useBookingRules } from "@/hooks/use-booking-rules";
import { updateBookingRules, type BookingRulesInput } from "@/lib/api";

export default function AdminConfiguracionPage() {
  const { state: authState } = useAuth();
  const isAdmin = authState.status === "authenticated" && authState.session.role === "Admin";
  const token = authState.status === "authenticated" ? authState.session.token : "";

  const { state, reload } = useBookingRules();
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSave = async (rules: BookingRulesInput) => {
    setSaving(true);
    setSaved(false);
    setError(null);
    const result = await updateBookingRules(token, rules);
    setSaving(false);
    if (result.ok) {
      setSaved(true);
      void reload();
    } else {
      setError(result.error ?? "No se pudieron guardar las reglas.");
    }
  };

  return (
    <>
      <SiteHeader />
      <main className="mx-auto w-full max-w-4xl flex-1 px-6 py-10 lg:px-12">
        <header className="mb-8 flex flex-col gap-2">
          <span className="inline-flex w-fit items-center gap-1.5 rounded-full bg-secondary-container px-3 py-1 text-label-md font-semibold text-on-secondary-container">
            <MaterialIcon name="admin_panel_settings" className="text-[16px]" />
            Panel de administración
          </span>
          <h1 className="text-headline-lg font-bold tracking-tight text-on-surface">
            Reglas de reserva
          </h1>
          <p className="text-body-md text-on-surface-variant">
            Define la política de reserva de AuraLearn: anticipación de cancelación, ventana de
            pago SINPE, duración de slots y horizonte de reserva. Una regla &laquo;sin definir&raquo;
            usa su valor por defecto y no se muestra en el portal del tutor.
          </p>
        </header>

        {!isAdmin && authState.status !== "loading" ? (
          <div className="flex flex-col items-center justify-center rounded-xl border border-error/30 bg-error-container/20 py-20 text-center">
            <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-error-container">
              <MaterialIcon name="lock" className="text-[28px] text-on-error-container" />
            </div>
            <h2 className="mb-2 text-title-md font-bold text-on-surface">Acceso restringido</h2>
            <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">
              Este panel es solo para administradores. Inicia sesión con una cuenta Admin.
            </p>
          </div>
        ) : state.status === "loading" ? (
          <div className="flex items-center gap-3 rounded-xl bg-surface-container-lowest p-8 shadow-sm">
            <MaterialIcon name="progress_activity" className="animate-spin text-[24px] text-primary" />
            <p className="text-body-lg text-on-surface-variant">Cargando reglas…</p>
          </div>
        ) : state.status === "error" ? (
          <div className="flex flex-col items-center gap-4 rounded-xl bg-surface-container-lowest p-12 text-center shadow-sm">
            <MaterialIcon name="error" className="text-[40px] text-error" />
            <p className="text-body-lg text-error">{state.error}</p>
            <button
              type="button"
              onClick={() => void reload()}
              className="rounded-lg bg-primary px-5 py-3 text-label-md font-bold text-on-primary hover:bg-on-primary-fixed-variant"
            >
              Reintentar
            </button>
          </div>
        ) : (
          <BookingRulesForm
            initial={state.data}
            saving={saving}
            saved={saved}
            error={error}
            onSave={(rules) => void handleSave(rules)}
          />
        )}
      </main>
    </>
  );
}

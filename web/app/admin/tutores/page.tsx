"use client";

/**
 * Panel admin: cola de verificación de tutores (solo rol Admin).
 *
 * Guard client-side con useAuth — la autorización real la aplica el backend
 * con [Authorize(Roles=Admin)]; esta página solo oculta la vista.
 */

import Link from "next/link";
import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { ApplicationCard } from "@/components/admin/application-card";
import { RejectDialog } from "@/components/admin/reject-dialog";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { useAuth } from "@/hooks/use-auth";
import { useAdminApplications } from "@/hooks/use-admin-applications";
import type { AdminTutorApplication } from "@/lib/api";

export default function AdminTutoresPage() {
  const { state: authState } = useAuth();
  const [rejectTarget, setRejectTarget] = useState<AdminTutorApplication | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const isAdmin = authState.status === "authenticated" && authState.session.role === "Admin";
  const token = authState.status === "authenticated" ? authState.session.token : "";
  const { state, deciding, decide, reload } = useAdminApplications(isAdmin ? token : "");

  const handleApprove = async (app: AdminTutorApplication) => {
    setActionError(null);
    const result = await decide(app.id, "approve");
    if (!result.ok) setActionError(result.error ?? "No se pudo aprobar la postulación.");
  };

  const handleReject = async (app: AdminTutorApplication, reason: string) => {
    setActionError(null);
    const result = await decide(app.id, "reject", reason);
    if (result.ok) {
      setRejectTarget(null);
    } else {
      setActionError(result.error ?? "No se pudo rechazar la postulación.");
    }
  };

  return (
    <>
      <SiteHeader />
      <main className="mx-auto w-full max-w-[1400px] flex-1 px-6 py-10 lg:px-12">
        <header className="mb-8 flex flex-col gap-2">
          <span className="inline-flex w-fit items-center gap-1.5 rounded-full bg-secondary-container px-3 py-1 text-label-md font-semibold text-on-secondary-container">
            <MaterialIcon name="admin_panel_settings" className="text-[16px]" />
            Panel de administración
          </span>
          <h1 className="text-headline-lg font-bold tracking-tight text-on-surface">
            Verificación de tutores
          </h1>
          <p className="text-body-md text-on-surface-variant">
            Revisa títulos y atestados contra el registro universitario. Promesa: decisión en ≤48 h.
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
            <Link
              href="/login"
              className="rounded-lg bg-primary px-6 py-2.5 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
            >
              Iniciar sesión
            </Link>
          </div>
        ) : isAdmin ? (
          <>
            {actionError && (
              <p
                role="alert"
                className="mb-6 flex items-center gap-2 rounded-lg border border-error/30 bg-error-container/20 px-3.5 py-2.5 text-body-md text-on-error-container"
              >
                <MaterialIcon name="error" className="text-[18px]" />
                {actionError}
              </p>
            )}

            {state.status === "loading" ? (
              <div className="flex flex-col gap-5" aria-busy="true">
                {Array.from({ length: 3 }).map((_, i) => (
                  <div key={i} className="h-48 animate-pulse rounded-xl bg-surface-container-lowest shadow-sm" />
                ))}
              </div>
            ) : state.status === "error" ? (
              <div className="flex flex-col items-center justify-center rounded-xl border border-error/30 bg-error-container/20 py-16 text-center">
                <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-error-container">
                  <MaterialIcon name="cloud_off" className="text-[28px] text-on-error-container" />
                </div>
                <h2 className="mb-2 text-title-md font-bold text-on-surface">No se pudo cargar la cola</h2>
                <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">{state.error}</p>
                <button
                  type="button"
                  onClick={reload}
                  className="rounded-lg bg-primary px-6 py-2.5 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
                >
                  Reintentar
                </button>
              </div>
            ) : state.items.length === 0 ? (
              <div className="flex flex-col items-center justify-center rounded-xl border border-outline-variant/50 bg-surface-container-low py-20 text-center">
                <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-surface-container">
                  <MaterialIcon name="task_alt" className="text-[28px] text-primary" />
                </div>
                <h2 className="mb-2 text-title-md font-bold text-on-surface">Cola al día</h2>
                <p className="max-w-xs text-body-md text-on-surface-variant">
                  No hay postulaciones pendientes de revisión.
                </p>
              </div>
            ) : (
              <div className="flex flex-col gap-5">
                <p className="text-body-md text-on-surface-variant">
                  <span className="font-bold text-on-surface">{state.items.length}</span>{" "}
                  {state.items.length === 1 ? "postulación pendiente" : "postulaciones pendientes"}
                </p>
                <div className="grid grid-cols-1 gap-5 xl:grid-cols-2">
                  {state.items.map((app) => (
                    <ApplicationCard
                      key={app.id}
                      application={app}
                      deciding={deciding === app.id}
                      onApprove={() => void handleApprove(app)}
                      onReject={() => setRejectTarget(app)}
                    />
                  ))}
                </div>
              </div>
            )}

            {rejectTarget && (
              <RejectDialog
                tutorName={rejectTarget.name}
                submitting={deciding === rejectTarget.id}
                onConfirm={(reason) => void handleReject(rejectTarget, reason)}
                onCancel={() => setRejectTarget(null)}
              />
            )}
          </>
        ) : (
          // authState.status === "loading": restaurando sesión del localStorage
          <div className="flex flex-col gap-5" aria-busy="true">
            {Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="h-48 animate-pulse rounded-xl bg-surface-container-lowest shadow-sm" />
            ))}
          </div>
        )}
      </main>
      <footer className="border-t border-outline-variant/40 bg-surface-container-lowest py-8">
        <div className="mx-auto flex max-w-[1400px] flex-col items-center justify-between gap-4 px-6 sm:flex-row lg:px-12">
          <p className="text-label-md text-on-surface-variant">
            © {new Date().getFullYear()} AuraLearn · Todos los derechos reservados
          </p>
          <p className="text-label-md text-on-surface-variant">Panel interno · uso administrativo</p>
        </div>
      </footer>
    </>
  );
}

"use client";

/**
 * Pestaña "Documentos" del panel del tutor (/tutor/documentos): gestor de
 * documentos de verificación del tutor autenticado. Guard client-side por
 * sesión (patrón de /tutor/perfil); sin fila de tutor → CTA a /postular.
 */

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useTutorProfile } from "@/hooks/use-tutor-profile";
import { DocumentsManager } from "@/components/tutor/documents/documents-manager";
import { TutorPageHeader } from "@/components/tutor/tutor-page-header";

export default function DocumentosTutorPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;
  const { state, profile, isLoading, isError } = useTutorProfile(token);

  // Guard: sin sesión → /login (efecto tras restauración diferida de useAuth).
  useEffect(() => {
    if (authState.status === "anonymous") {
      router.replace("/login");
    }
  }, [authState.status, router]);

  if (authState.status !== "authenticated") {
    return (
      <div className="flex items-center justify-center py-24" aria-busy="true">
        <MaterialIcon name="progress_activity" className="animate-spin text-[32px] text-primary" />
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-24" aria-busy="true">
        <MaterialIcon name="progress_activity" className="animate-spin text-[32px] text-primary" />
      </div>
    );
  }

  if (isError || !profile) {
    return (
      <div className="mx-auto flex max-w-md flex-col items-center gap-4 py-24 text-center">
        <span className="grid h-16 w-16 place-items-center rounded-full bg-surface-container">
          <MaterialIcon name="folder_off" className="text-[28px] text-on-surface-variant" />
        </span>
        <h1 className="text-headline-sm font-bold text-on-surface">Aún no tienes un perfil de tutor</h1>
        <p className="text-body-md text-on-surface-variant">
          Los documentos se gestionan desde tu postulación. Publica tu perfil para comenzar.
        </p>
        <Link
          href="/postular"
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          Postular como docente
        </Link>
      </div>
    );
  }

  return (
    <main className="flex min-h-screen flex-1 flex-col bg-background">
      <TutorPageHeader
        title="Mis documentos"
        description="Títulos, certificaciones e identificación presentados para la verificación de credenciales."
      />
      <div className="mx-auto w-full max-w-3xl flex-1 px-6 py-10 lg:px-8">
        <DocumentsManager token={token!} verificationStatus={profile.verificationStatus} />
      </div>
    </main>
  );
}

"use client";

/**
 * Panel del Profesor — Mi Perfil (Stitch 05). Datos de alta del tutor:
 * título académico, especialización, universidad, materias, tarifas y bio,
 * con tarjeta de estado de verificación. Cualquier usuario autenticado con
 * postulación puede editarlo (el backend responde 404 si no tiene fila).
 */

import { useEffect, useRef } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useTutorProfile } from "@/hooks/use-tutor-profile";
import { ProfileForm } from "@/components/tutor/profile/profile-form";
import { ProfilePhotoCard } from "@/components/tutor/profile/profile-photo-card";
import { ProfileStatusCard } from "@/components/tutor/profile/profile-status-card";
import { SaveActionBar } from "@/components/tutor/availability/save-action-bar";
import { TutorPageHeader } from "@/components/tutor/tutor-page-header";

export default function PerfilTutorPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { profile, isLoading, isError, error, saving, saved, saveError, save } =
    useTutorProfile(token);
  const formRef = useRef<HTMLFormElement | null>(null);

  useEffect(() => {
    if (authState.status === "anonymous") router.replace("/login");
  }, [authState.status, router]);

  if (authState.status === "loading" || authState.status === "anonymous" || isLoading) {
    return (
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto w-full max-w-7xl px-6 py-12">
          <p className="text-body-lg text-on-surface-variant">Cargando tu perfil…</p>
        </div>
      </main>
    );
  }

  if (isError || !profile) {
    return (
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto flex w-full max-w-7xl flex-col items-center gap-4 px-6 py-24 text-center">
          <span className="grid h-16 w-16 place-items-center rounded-full bg-surface-container-high">
            <MaterialIcon name="person_off" className="text-[32px] text-on-surface-variant" />
          </span>
          <h1 className="text-headline-sm font-bold text-on-surface">
            Aún no tienes un perfil de tutor
          </h1>
          <p className="max-w-md text-body-md text-on-surface-variant">
            {error ?? "No se pudo cargar tu perfil."}
          </p>
          <Link
            href="/postular"
            className="inline-flex items-center gap-2 rounded-xl bg-primary px-6 py-3 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-primary-container"
          >
            <MaterialIcon name="school" className="text-[18px]" />
            Postular como docente
          </Link>
        </div>
      </main>
    );
  }

  return (
    <main className="flex min-h-screen flex-1 flex-col bg-background">
      <TutorPageHeader
        title="Mi perfil de tutor"
        description="Completa y mantén tus datos de alta: títulos, materias y tarifas que verán tus estudiantes."
      />

      <div className="mx-auto w-full max-w-7xl flex-1 px-6 py-8 lg:px-12">
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-[1fr_360px]">
          <div className="rounded-xl bg-surface-container-lowest p-6 shadow-sm">
            <ProfileForm
              formRef={formRef}
              profile={profile}
              saving={saving}
              saved={saved}
              saveError={saveError}
              onSave={(payload) => void save(payload)}
            />
          </div>
          <ProfileStatusCard profile={profile} />
        </div>
        {/* Barra de persistencia al ancho de la página (mismo componente que disponibilidad) */}
        <SaveActionBar
          context="perfil"
          dirty={!saved}
          saving={saving}
          saved={saved}
          onSave={() => formRef.current?.requestSubmit()}
          onDiscard={() => formRef.current?.reset()}
        />
      </div>
    </main>
  );
}

"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { fetchTutorApplicationStatus, registerUser } from "@/lib/api";
import { readSession, saveSession, toSession } from "@/lib/auth";
import { useTutorApplication } from "@/hooks/use-tutor-application";
import { Stepper, type StepDef } from "./shared";
import { StepAccount, type AccountData } from "./step-account";
import { StepProfile, type ProfileData } from "./step-profile";
import { StepSubjects, type SubjectsData } from "./step-subjects";
import { StepReview } from "./step-review";
import { Confirmation } from "./confirmation";

const STEPS: StepDef[] = [
  { label: "Cuenta", icon: "person" },
  { label: "Perfil", icon: "school" },
  { label: "Cursos", icon: "menu_book" },
  { label: "Revisión", icon: "rate_review" },
];

type Phase = "loading" | "wizard" | "already-submitted" | "confirmed";

export function OnboardingWizard() {
  const [phase, setPhase] = useState<Phase>("loading");
  const [step, setStep] = useState(0);
  const [account, setAccount] = useState<AccountData>({ fullName: "", email: "", password: "" });
  const [profile, setProfile] = useState<ProfileData>({ credentials: "", university: "", bio: "" });
  const [subjects, setSubjects] = useState<SubjectsData>({ subjects: [], priceCrc: 0 });
  const [accountError, setAccountError] = useState<string | null>(null);
  const [accountSubmitting, setAccountSubmitting] = useState(false);
  const [statusInfo, setStatusInfo] = useState<{ status: string; rejectionReason: string | null } | null>(null);

  const { state: submitState, submit } = useTutorApplication();

  // Al montar: si ya hay sesión, precargar nombre, saltar al paso de perfil y
  // consultar el estado de la postulación (si ya postuló, mostrar su estado).
  useEffect(() => {
    let cancelled = false;

    const init = async () => {
      const session = readSession();
      if (session) {
        setAccount((a) => ({ ...a, fullName: session.fullName, email: session.email }));
        setStep(1);

        const status = await fetchTutorApplicationStatus(session.token);
        if (!cancelled && status.ok && status.data && status.data.status !== "Rejected") {
          setStatusInfo({ status: status.data.status, rejectionReason: status.data.rejectionReason });
          setPhase("already-submitted");
          return;
        }
      }
      if (!cancelled) setPhase("wizard");
    };

    void init();
    return () => {
      cancelled = true;
    };
  }, []);

  const handleAccount = async (data: AccountData) => {
    setAccount(data);
    setAccountError(null);

    // Si ya hay sesión (vino logueado), no volvemos a registrar.
    if (readSession()) {
      setStep(1);
      return;
    }

    setAccountSubmitting(true);
    const result = await registerUser({
      email: data.email,
      password: data.password,
      fullName: data.fullName,
    });
    setAccountSubmitting(false);

    if (!result.ok) {
      setAccountError(result.error);
      return;
    }
    saveSession(toSession(result.data));
    setStep(1);
  };

  const handleSubmit = async () => {
    const session = readSession();
    if (!session) {
      setStep(0);
      return;
    }
    const ok = await submit(
      {
        fullName: account.fullName,
        credentials: profile.credentials,
        university: profile.university,
        bio: profile.bio,
        subjects: subjects.subjects,
        priceCrc: subjects.priceCrc,
        priceUsd: subjects.priceCrc > 0 ? Math.round(subjects.priceCrc / 520) : 0,
      },
      session.token,
    );
    if (ok) setPhase("confirmed");
  };

  if (phase === "loading") {
    return (
      <div className="flex items-center justify-center py-24" aria-busy="true">
        <MaterialIcon name="progress_activity" className="animate-spin text-[32px] text-primary" />
      </div>
    );
  }

  if (phase === "confirmed") {
    return <Confirmation fullName={account.fullName} email={account.email} />;
  }

  if (phase === "already-submitted" && statusInfo) {
    return <AlreadySubmitted status={statusInfo.status} rejectionReason={statusInfo.rejectionReason} />;
  }

  return (
    <div className="flex flex-col gap-8">
      <Stepper steps={STEPS} current={step} />

      <div className="rounded-2xl border border-outline-variant/60 bg-surface-container-lowest p-6 shadow-sm sm:p-8">
        {step === 0 && (
          <StepAccount
            defaultValues={account}
            submitting={accountSubmitting}
            serverError={accountError}
            onSubmit={handleAccount}
          />
        )}
        {step === 1 && (
          <StepProfile
            defaultValues={profile}
            onBack={() => setStep(0)}
            onSubmit={(data) => {
              setProfile(data);
              setStep(2);
            }}
          />
        )}
        {step === 2 && (
          <StepSubjects
            defaultValues={subjects}
            onBack={() => setStep(1)}
            onSubmit={(data) => {
              setSubjects(data);
              setStep(3);
            }}
          />
        )}
        {step === 3 && (
          <StepReview
            account={account}
            profile={profile}
            subjects={subjects}
            submitting={submitState.status === "submitting"}
            serverError={submitState.status === "error" ? submitState.error : null}
            onBack={() => setStep(2)}
            onSubmit={handleSubmit}
          />
        )}
      </div>

      <p className="flex items-center justify-center gap-2 text-center text-label-md text-on-surface-variant">
        <MaterialIcon name="lock" className="text-[16px]" />
        Tu información se usa solo para la verificación. Nunca se comparte sin tu consentimiento.
      </p>
    </div>
  );
}

function AlreadySubmitted({
  status,
  rejectionReason,
}: {
  status: string;
  rejectionReason: string | null;
}) {
  const isRejected = status === "Rejected";
  return (
    <div className="flex flex-col items-center gap-5 py-8 text-center">
      <span
        className={
          isRejected
            ? "grid h-16 w-16 place-items-center rounded-full bg-error-container/40"
            : "grid h-16 w-16 place-items-center rounded-full bg-primary/15"
        }
      >
        <MaterialIcon
          name={isRejected ? "cancel" : "hourglass_top"}
          className={isRejected ? "text-[32px] text-error" : "text-[32px] text-primary"}
        />
      </span>
      <h2 className="text-headline-sm font-bold text-on-surface">
        {isRejected ? "Tu postulación fue rechazada" : "Tu postulación está en revisión"}
      </h2>
      <p className="max-w-md text-body-md text-on-surface-variant">
        {isRejected
          ? rejectionReason ?? "Puedes corregir tu información y volver a postular."
          : "Nuestro equipo está verificando tus credenciales. Te notificaremos en ≤48 horas."}
      </p>
      <Link
        href="/"
        className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3 text-title-md font-semibold text-on-primary transition-colors hover:bg-on-primary-fixed-variant"
      >
        Volver al inicio
      </Link>
    </div>
  );
}

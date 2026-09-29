"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";

export default function RegistroPage() {
  const router = useRouter();
  const { register } = useAuth();
  const [data, setData] = useState({ fullName: "", email: "", password: "" });
  const [errors, setErrors] = useState<{ fullName?: string; email?: string; password?: string }>({});
  const [serverError, setServerError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const set = (key: keyof typeof data) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setData((d) => ({ ...d, [key]: e.target.value }));

  // Validación client-side espejo del backend (la validación real es server-side).
  const validate = (): boolean => {
    const next: typeof errors = {};
    if (data.fullName.trim().length < 3) next.fullName = "Ingresa tu nombre completo.";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) next.email = "Ingresa un correo válido.";
    if (data.password.length < 8) next.password = "Mínimo 8 caracteres.";
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setSubmitting(true);
    setServerError(null);
    const result = await register(data);
    setSubmitting(false);

    if (!result.ok) {
      setServerError(result.error ?? "No se pudo crear la cuenta.");
      return;
    }
    router.push("/tutores");
  };

  const inputClass =
    "w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-3.5 py-2.5 text-sm text-on-surface placeholder:text-outline transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30 aria-invalid:border-error aria-invalid:ring-2 aria-invalid:ring-error/20";

  return (
    <main className="flex flex-1 items-center justify-center bg-surface px-6 py-16">
      <div className="w-full max-w-md rounded-2xl bg-surface-container-lowest p-8 shadow-sm">
        <div className="mb-6 flex flex-col items-center gap-3 text-center">
          <span
            aria-hidden
            className="grid h-12 w-12 place-items-center rounded-xl bg-gradient-to-br from-primary to-primary-container text-on-primary text-xl font-extrabold"
          >
            A
          </span>
          <h1 className="text-headline-sm font-bold tracking-tight text-on-surface">
            Crea tu cuenta de estudiante
          </h1>
          <p className="text-body-md text-on-surface-variant">
            Reserva tutorías verificadas y paga con SINPE Móvil.
          </p>
        </div>

        <form
          className="flex flex-col gap-5"
          onSubmit={(e) => void handleSubmit(e)}
          noValidate
        >
          <div className="flex flex-col gap-1.5">
            <label
              htmlFor="fullName"
              className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant"
            >
              Nombre completo
            </label>
            <input
              id="fullName"
              type="text"
              autoComplete="name"
              placeholder="Ej. Ana Estudiante"
              value={data.fullName}
              onChange={set("fullName")}
              aria-invalid={!!errors.fullName}
              className={inputClass}
            />
            {errors.fullName && (
              <p role="alert" className="text-label-md text-error">
                {errors.fullName}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label
              htmlFor="email"
              className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant"
            >
              Correo
            </label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              placeholder="tu@correo.com"
              value={data.email}
              onChange={set("email")}
              aria-invalid={!!errors.email}
              className={inputClass}
            />
            {errors.email && (
              <p role="alert" className="text-label-md text-error">
                {errors.email}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label
              htmlFor="password"
              className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant"
            >
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              autoComplete="new-password"
              placeholder="••••••••"
              value={data.password}
              onChange={set("password")}
              aria-invalid={!!errors.password}
              className={inputClass}
            />
            {errors.password ? (
              <p role="alert" className="text-label-md text-error">
                {errors.password}
              </p>
            ) : (
              <p className="text-label-md text-on-surface-variant">Mínimo 8 caracteres.</p>
            )}
          </div>

          {serverError && (
            <p
              role="alert"
              className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container/20 px-3.5 py-2.5 text-body-md text-on-error-container"
            >
              <MaterialIcon name="error" className="text-[18px]" />
              {serverError}
            </p>
          )}

          <button
            type="submit"
            disabled={submitting}
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant disabled:opacity-60"
          >
            {submitting ? (
              <>
                <MaterialIcon name="progress_activity" className="animate-spin text-[18px]" />
                <span>Creando cuenta…</span>
              </>
            ) : (
              <>
                <span>Crear cuenta</span>
                <MaterialIcon name="arrow_forward" className="text-[18px]" />
              </>
            )}
          </button>

          <p className="text-center text-body-md text-on-surface-variant">
            ¿Ya tienes cuenta?{" "}
            <a href="/login" className="text-primary font-semibold hover:underline">
              Inicia sesión
            </a>
          </p>
        </form>
      </div>
    </main>
  );
}

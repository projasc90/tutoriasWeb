"use client";

import { useState } from "react";
import { Field, inputClass } from "./shared";
import { MaterialIcon } from "@/components/material-icon";

export type AccountData = {
  fullName: string;
  email: string;
  password: string;
};

export function StepAccount({
  defaultValues,
  submitting,
  serverError,
  onSubmit,
}: {
  defaultValues: AccountData;
  submitting: boolean;
  serverError: string | null;
  onSubmit: (data: AccountData) => void;
}) {
  const [data, setData] = useState<AccountData>(defaultValues);
  const [errors, setErrors] = useState<Partial<Record<keyof AccountData, string>>>({});

  const set = (key: keyof AccountData) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setData((d) => ({ ...d, [key]: e.target.value }));

  const validate = (): boolean => {
    const next: typeof errors = {};
    if (data.fullName.trim().length < 3) next.fullName = "Ingresa tu nombre completo.";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) next.email = "Ingresa un correo válido.";
    if (data.password.length < 8) next.password = "Mínimo 8 caracteres.";
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  return (
    <form
      className="flex flex-col gap-5"
      onSubmit={(e) => {
        e.preventDefault();
        if (validate()) onSubmit(data);
      }}
      noValidate
    >
      <header>
        <h2 className="text-headline-sm font-bold text-on-surface">Crea tu cuenta</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Usaremos este correo para notificarte la decisión de verificación.
        </p>
      </header>

      <Field label="Nombre completo" htmlFor="fullName" error={errors.fullName}>
        <input
          id="fullName"
          type="text"
          autoComplete="name"
          placeholder="Ej. Dra. Ana Mora"
          value={data.fullName}
          onChange={set("fullName")}
          aria-invalid={!!errors.fullName}
          className={inputClass}
        />
      </Field>

      <Field label="Correo institucional o personal" htmlFor="email" error={errors.email}>
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
      </Field>

      <Field
        label="Contraseña"
        htmlFor="password"
        error={errors.password}
        hint="Mínimo 8 caracteres."
      >
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
      </Field>

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
            <span>Continuar</span>
            <MaterialIcon name="arrow_forward" className="text-[18px]" />
          </>
        )}
      </button>
    </form>
  );
}

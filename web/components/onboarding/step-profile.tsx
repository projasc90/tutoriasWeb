"use client";

import { useState } from "react";
import { Field, inputClass } from "./shared";
import { MaterialIcon } from "@/components/material-icon";

export type ProfileData = {
  credentials: string;
  university: string;
  bio: string;
};

const UNIVERSITIES = ["UCR", "TEC", "UNA", "LEAD", "ULACIT", "U Latina", "Otra"];

export function StepProfile({
  defaultValues,
  onBack,
  onSubmit,
}: {
  defaultValues: ProfileData;
  onBack: () => void;
  onSubmit: (data: ProfileData) => void;
}) {
  const [data, setData] = useState<ProfileData>(defaultValues);
  const [errors, setErrors] = useState<Partial<Record<keyof ProfileData, string>>>({});

  const validate = (): boolean => {
    const next: typeof errors = {};
    if (data.credentials.trim().length < 3) next.credentials = "Ingresa tu título o credencial.";
    if (!data.university) next.university = "Selecciona tu universidad.";
    if (data.bio.trim().length < 40) next.bio = "Cuéntanos un poco más (mínimo 40 caracteres).";
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
        <h2 className="text-headline-sm font-bold text-on-surface">Tu perfil académico</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Verificamos tu título contra el registro de tu universidad.
        </p>
      </header>

      <Field
        label="Título o credencial"
        htmlFor="credentials"
        error={errors.credentials}
        hint="Ej. PhD Matemáticas, M.Sc. Computación, Lic. Física."
      >
        <input
          id="credentials"
          type="text"
          placeholder="Ej. M.Sc. Computación"
          value={data.credentials}
          onChange={(e) => setData((d) => ({ ...d, credentials: e.target.value }))}
          aria-invalid={!!errors.credentials}
          className={inputClass}
        />
      </Field>

      <Field label="Universidad de procedencia" htmlFor="university" error={errors.university}>
        <div className="relative">
          <select
            id="university"
            value={data.university}
            onChange={(e) => setData((d) => ({ ...d, university: e.target.value }))}
            aria-invalid={!!errors.university}
            className={`${inputClass} appearance-none pr-10 ${data.university ? "" : "text-outline"}`}
          >
            <option value="" disabled>
              Selecciona una universidad
            </option>
            {UNIVERSITIES.map((u) => (
              <option key={u} value={u}>
                {u}
              </option>
            ))}
          </select>
          <MaterialIcon
            name="expand_more"
            className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-[20px] text-on-surface-variant"
          />
        </div>
      </Field>

      <Field
        label="Cuéntanos sobre ti"
        htmlFor="bio"
        error={errors.bio}
        hint={`${data.bio.length}/1000 · Experiencia, metodología, a quién ayudas.`}
      >
        <textarea
          id="bio"
          rows={5}
          maxLength={1000}
          placeholder="Ej. 8 años de experiencia en tutoría universitaria. Metodología orientada a resultados, con más de 500 estudiantes acompañados…"
          value={data.bio}
          onChange={(e) => setData((d) => ({ ...d, bio: e.target.value }))}
          aria-invalid={!!errors.bio}
          className={`${inputClass} resize-y`}
        />
      </Field>

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
        <button
          type="button"
          onClick={onBack}
          className="inline-flex items-center justify-center gap-2 rounded-xl px-6 py-3.5 text-title-md font-semibold text-on-surface-variant transition-colors hover:bg-surface-container-low hover:text-on-surface"
        >
          <MaterialIcon name="arrow_back" className="text-[18px]" />
          <span>Atrás</span>
        </button>
        <button
          type="submit"
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          <span>Continuar</span>
          <MaterialIcon name="arrow_forward" className="text-[18px]" />
        </button>
      </div>
    </form>
  );
}

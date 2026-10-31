"use client";

/**
 * Formulario de datos de alta del tutor (Stitch 05 — tarjeta "Estado del
 * Perfil Docente" + resumen de perfil). Reutiliza Field/inputClass del kit
 * del wizard y el patrón tag-editor de StepSubjects. Validación espejo del
 * backend (UpdateTutorProfileValidator).
 */

import { useMemo, useState, forwardRef } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { Field, inputClass } from "@/components/onboarding/shared";
import { normalizeSubject, suggestSubjectCorrection } from "@/lib/subjects";
import type {
  TutorProfile,
  UpdateTutorProfilePayload,
} from "@/lib/types/tutor-profile";

const UNIVERSITIES = ["UCR", "TEC", "UNA", "LEAD", "ULACIT", "U Latina", "Otra"];
const SUGGESTED = [
  "Cálculo I", "Cálculo II", "Álgebra Lineal", "Física I", "Química Orgánica",
  "Python", "Estadística", "Econometría", "Estructuras de Datos", "TOEFL",
];
const MAX_SUBJECTS = 8;
/** Tipo de cambio de referencia usado en todo el producto (wizard /postular). */
const CRC_PER_USD = 520;

type FormState = {
  credentials: string;
  specialization: string;
  university: string;
  bio: string;
  subjects: string[];
  priceCrc: string;
};

const toForm = (p: TutorProfile): FormState => ({
  credentials: p.credentials,
  specialization: p.specialization ?? "",
  university: p.university,
  bio: p.bio,
  subjects: p.subjects,
  priceCrc: String(p.priceCrc),
});

export function ProfileForm({
  profile,
  saving,
  saved,
  saveError,
  onSave,
  formRef,
}: {
  profile: TutorProfile;
  saving: boolean;
  saved: boolean;
  saveError: string | null;
  onSave: (payload: UpdateTutorProfilePayload) => void;
  /** Ref al <form> para que la página dispare submit/reset desde una barra externa. */
  formRef?: React.RefObject<HTMLFormElement | null>;
}) {
  const [form, setForm] = useState<FormState>(() => toForm(profile));
  const [draft, setDraft] = useState("");
  const [errors, setErrors] = useState<Partial<Record<keyof FormState, string>>>({});

  // Tras guardar (o recargar), el perfil canónico cambia → re-sincroniza el
  // baseline. Patrón "adjust state during render" (React docs): setState
  // condicional durante el render en lugar de un effect.
  const [lastProfile, setLastProfile] = useState(profile);
  if (lastProfile !== profile) {
    setLastProfile(profile);
    setForm(toForm(profile));
  }

  const priceNumber = Number(form.priceCrc.replace(/[^\d]/g, "")) || 0;
  const priceUsd = priceNumber > 0 ? Math.round(priceNumber / CRC_PER_USD) : 0;

  const dirty = useMemo(
    () => JSON.stringify(form) !== JSON.stringify(toForm(profile)),
    [form, profile]
  );

  const addSubject = (value: string) => {
    const v = normalizeSubject(value);
    if (!v || form.subjects.length >= MAX_SUBJECTS) return;
    if (form.subjects.some((s) => s.toLowerCase() === v.toLowerCase())) return;
    setForm((f) => ({ ...f, subjects: [...f.subjects, v] }));
    setDraft("");
  };

  const removeSubject = (value: string) =>
    setForm((f) => ({ ...f, subjects: f.subjects.filter((s) => s !== value) }));

  const validate = (): boolean => {
    const next: typeof errors = {};
    if (form.credentials.trim().length < 3) next.credentials = "Ingresa tu título o credencial.";
    if (form.specialization.length > 100) next.specialization = "Máximo 100 caracteres.";
    if (!form.university) next.university = "Selecciona tu universidad.";
    if (form.subjects.length === 0) next.subjects = "Agrega al menos una materia.";
    if (!priceNumber || priceNumber < 1000) next.priceCrc = "Ingresa una tarifa válida (mínimo ₡1.000).";
    if (priceNumber > 50000) next.priceCrc = "La tarifa máxima es ₡50.000 por hora.";
    if (form.bio.trim().length < 40) next.bio = "Cuéntanos un poco más (mínimo 40 caracteres).";
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    onSave({
      credentials: form.credentials.trim(),
      specialization: form.specialization.trim() || null,
      university: form.university,
      bio: form.bio.trim(),
      subjects: form.subjects,
      priceCrc: priceNumber,
      priceUsd,
    });
  };

  return (
    <form
      ref={formRef}
      className="flex flex-col gap-5"
      onSubmit={handleSubmit}
      noValidate
    >
      <header>
        <h2 className="text-headline-sm font-bold text-on-surface">Datos de alta docente</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Estos datos aparecen en tu perfil público del catálogo.
        </p>
      </header>

      <Field label="Nombre" htmlFor="profile-name" hint="Viene de tu cuenta de AuraLearn.">
        <input
          id="profile-name"
          type="text"
          value={profile.name}
          disabled
          className={`${inputClass} cursor-not-allowed opacity-70`}
        />
      </Field>

      <Field
        label="Título académico"
        htmlFor="profile-credentials"
        error={errors.credentials}
        hint="Ej. PhD Matemáticas, M.Sc. Computación, Lic. Física."
      >
        <input
          id="profile-credentials"
          type="text"
          placeholder="Ej. M.Sc. Computación"
          value={form.credentials}
          onChange={(e) => setForm((f) => ({ ...f, credentials: e.target.value }))}
          aria-invalid={!!errors.credentials}
          className={inputClass}
        />
      </Field>

      <Field
        label="Especialización (opcional)"
        htmlFor="profile-specialization"
        error={errors.specialization}
        hint="Énfasis o mención del título. Ej. Matemática Aplicada."
      >
        <input
          id="profile-specialization"
          type="text"
          placeholder="Ej. Matemática Aplicada"
          maxLength={100}
          value={form.specialization}
          onChange={(e) => setForm((f) => ({ ...f, specialization: e.target.value }))}
          aria-invalid={!!errors.specialization}
          className={inputClass}
        />
      </Field>

      <Field label="Universidad de procedencia" htmlFor="profile-university" error={errors.university}>
        <div className="relative">
          <select
            id="profile-university"
            value={form.university}
            onChange={(e) => setForm((f) => ({ ...f, university: e.target.value }))}
            aria-invalid={!!errors.university}
            className={`${inputClass} appearance-none pr-10 ${form.university ? "" : "text-outline"}`}
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
        label="Materias que impartes"
        htmlFor="profile-subject-input"
        error={errors.subjects}
        hint={`${form.subjects.length}/${MAX_SUBJECTS} · Presiona Enter para agregar.`}
      >
        <div className="flex flex-col gap-3">
          <div className="relative flex items-center">
            <MaterialIcon
              name="school"
              className="pointer-events-none absolute left-3.5 text-[20px] text-primary"
            />
            <input
              id="profile-subject-input"
              type="text"
              placeholder="Ej. Cálculo Diferencial"
              value={draft}
              onChange={(e) => setDraft(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  e.preventDefault();
                  addSubject(draft);
                }
              }}
              className={`${inputClass} pl-10`}
            />
          </div>

          {(() => {
            const suggestion = suggestSubjectCorrection(draft, SUGGESTED);
            return suggestion ? (
              <p className="flex flex-wrap items-center gap-2 text-label-md text-on-surface-variant">
                ¿Quisiste decir
                <span className="font-semibold text-primary">{suggestion}</span>?
                <button
                  type="button"
                  onClick={() => addSubject(suggestion)}
                  className="inline-flex items-center gap-1 rounded-full border border-primary/40 px-2.5 py-0.5 font-semibold text-primary transition-colors hover:bg-primary-container/20"
                >
                  <MaterialIcon name="auto_fix_high" className="text-[14px]" />
                  Usar
                </button>
              </p>
            ) : null;
          })()}

          {form.subjects.length > 0 && (
            <ul className="flex flex-wrap gap-2" aria-label="Materias seleccionadas">
              {form.subjects.map((s) => (
                <li key={s}>
                  <span className="inline-flex items-center gap-1.5 rounded-full bg-primary-container/15 px-3 py-1.5 text-label-md font-semibold text-primary">
                    {s}
                    <button
                      type="button"
                      onClick={() => removeSubject(s)}
                      aria-label={`Quitar ${s}`}
                      className="grid place-items-center rounded-full transition-colors hover:text-error"
                    >
                      <MaterialIcon name="close" className="text-[16px]" />
                    </button>
                  </span>
                </li>
              ))}
            </ul>
          )}

          <div className="flex flex-wrap gap-1.5">
            {SUGGESTED.filter((s) => !form.subjects.includes(s)).slice(0, 6).map((s) => (
              <button
                key={s}
                type="button"
                onClick={() => addSubject(s)}
                className="rounded-full border border-outline-variant px-3 py-1 text-label-md text-on-surface-variant transition-colors hover:border-primary hover:text-primary"
              >
                + {s}
              </button>
            ))}
          </div>
        </div>
      </Field>

      <Field
        label="Tarifa por hora (CRC)"
        htmlFor="profile-price"
        error={errors.priceCrc}
        hint={priceUsd > 0 ? `≈ $${priceUsd} USD por hora (tipo de cambio de referencia).` : "Entre ₡1.000 y ₡50.000."}
      >
        <div className="relative flex items-center">
          <MaterialIcon
            name="payments"
            className="pointer-events-none absolute left-3.5 text-[20px] text-primary"
          />
          <input
            id="profile-price"
            type="text"
            inputMode="numeric"
            placeholder="Ej. 14000"
            value={form.priceCrc}
            onChange={(e) => setForm((f) => ({ ...f, priceCrc: e.target.value }))}
            aria-invalid={!!errors.priceCrc}
            className={`${inputClass} pl-10`}
          />
        </div>
      </Field>

      <Field
        label="Cuéntanos sobre ti"
        htmlFor="profile-bio"
        error={errors.bio}
        hint={`${form.bio.length}/1000 · Experiencia, metodología, a quién ayudas.`}
      >
        <textarea
          id="profile-bio"
          rows={5}
          maxLength={1000}
          placeholder="Ej. 8 años de experiencia en tutoría universitaria…"
          value={form.bio}
          onChange={(e) => setForm((f) => ({ ...f, bio: e.target.value }))}
          aria-invalid={!!errors.bio}
          className={`${inputClass} resize-y`}
        />
      </Field>

      {saveError && (
        <p role="alert" className="rounded-lg bg-error-container/30 px-4 py-3 text-label-md text-error">
          {saveError}
        </p>
      )}
    </form>
  );
}

"use client";

import { useState } from "react";
import { Field, inputClass } from "./shared";
import { MaterialIcon } from "@/components/material-icon";

export type SubjectsData = {
  subjects: string[];
  priceCrc: number;
};

const SUGGESTED = [
  "Cálculo I", "Cálculo II", "Álgebra Lineal", "Física I", "Química Orgánica",
  "Python", "Estadística", "Econometría", "Estructuras de Datos", "TOEFL",
];

const MAX_SUBJECTS = 8;

export function StepSubjects({
  defaultValues,
  onBack,
  onSubmit,
}: {
  defaultValues: SubjectsData;
  onBack: () => void;
  onSubmit: (data: SubjectsData) => void;
}) {
  const [subjects, setSubjects] = useState<string[]>(defaultValues.subjects);
  const [draft, setDraft] = useState("");
  const [price, setPrice] = useState<string>(defaultValues.priceCrc > 0 ? String(defaultValues.priceCrc) : "");
  const [errors, setErrors] = useState<{ subjects?: string; price?: string }>({});

  const addSubject = (value: string) => {
    const v = value.trim();
    if (!v) return;
    if (subjects.length >= MAX_SUBJECTS) return;
    if (subjects.some((s) => s.toLowerCase() === v.toLowerCase())) return;
    setSubjects((s) => [...s, v]);
    setDraft("");
  };

  const removeSubject = (value: string) =>
    setSubjects((s) => s.filter((x) => x !== value));

  const priceNumber = Number(price.replace(/[^\d]/g, ""));
  const priceUsd = priceNumber > 0 ? Math.round(priceNumber / 520) : 0;

  const validate = (): boolean => {
    const next: typeof errors = {};
    if (subjects.length === 0) next.subjects = "Agrega al menos una materia.";
    if (!priceNumber || priceNumber < 1000) next.price = "Ingresa una tarifa válida (mínimo ₡1.000).";
    if (priceNumber > 50000) next.price = "La tarifa máxima es ₡50.000 por hora.";
    setErrors(next);
    return Object.keys(next).length === 0;
  };

  return (
    <form
      className="flex flex-col gap-5"
      onSubmit={(e) => {
        e.preventDefault();
        if (validate()) onSubmit({ subjects, priceCrc: priceNumber });
      }}
      noValidate
    >
      <header>
        <h2 className="text-headline-sm font-bold text-on-surface">Cursos y tarifa</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Las materias que dominas y tu tarifa por hora. Podrás ajustarla luego.
        </p>
      </header>

      <Field
        label="Materias que impartes"
        htmlFor="subject-input"
        error={errors.subjects}
        hint={`${subjects.length}/${MAX_SUBJECTS} · Presiona Enter para agregar.`}
      >
        <div className="flex flex-col gap-3">
          <div className="relative flex items-center">
            <MaterialIcon
              name="school"
              className="pointer-events-none absolute left-3.5 text-[20px] text-primary"
            />
            <input
              id="subject-input"
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

          {subjects.length > 0 && (
            <ul className="flex flex-wrap gap-2" aria-label="Materias seleccionadas">
              {subjects.map((s) => (
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
            {SUGGESTED.filter((s) => !subjects.includes(s)).slice(0, 6).map((s) => (
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
        label="Tarifa por hora (₡ CRC)"
        htmlFor="price"
        error={errors.price}
        hint={priceUsd > 0 ? `≈ $${priceUsd} USD por hora` : "Se muestra a los estudiantes en CRC y USD."}
      >
        <div className="relative flex items-center">
          <span className="pointer-events-none absolute left-3.5 text-sm font-semibold text-on-surface-variant">
            ₡
          </span>
          <input
            id="price"
            type="text"
            inputMode="numeric"
            placeholder="14.000"
            value={price}
            onChange={(e) => setPrice(e.target.value)}
            aria-invalid={!!errors.price}
            className={`${inputClass} pl-8`}
          />
        </div>
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

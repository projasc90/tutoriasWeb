import type { ReactNode } from "react";
import { MaterialIcon } from "@/components/material-icon";

/**
 * Piezas compartidas del wizard /postular (stepper + campo de formulario).
 * Patrón FieldGroup del hero: label sobre el control, tokens MD3.
 */

export type StepDef = { label: string; icon: string };

export function Stepper({
  steps,
  current,
}: {
  steps: StepDef[];
  current: number;
}) {
  return (
    <ol
      aria-label="Progreso del registro"
      className="flex items-center gap-2 sm:gap-3"
    >
      {steps.map((step, i) => {
        const state =
          i < current ? "done" : i === current ? "current" : "todo";
        return (
          <li key={step.label} className="flex items-center gap-2 sm:gap-3">
            <div className="flex items-center gap-2">
              <span
                aria-current={state === "current" ? "step" : undefined}
                className={
                  state === "done"
                    ? "grid h-9 w-9 place-items-center rounded-full bg-primary text-on-primary"
                    : state === "current"
                      ? "grid h-9 w-9 place-items-center rounded-full bg-primary text-on-primary ring-4 ring-primary/20"
                      : "grid h-9 w-9 place-items-center rounded-full bg-surface-container-high text-on-surface-variant"
                }
              >
                <MaterialIcon
                  name={state === "done" ? "check" : step.icon}
                  className="text-[18px]"
                />
              </span>
              <span
                className={
                  state === "todo"
                    ? "hidden text-label-md font-semibold text-on-surface-variant md:block"
                    : "hidden text-label-md font-semibold text-on-surface md:block"
                }
              >
                {step.label}
              </span>
            </div>
            {i < steps.length - 1 && (
              <span
                aria-hidden
                className={
                  i < current
                    ? "h-px w-6 bg-primary sm:w-10"
                    : "h-px w-6 bg-outline-variant sm:w-10"
                }
              />
            )}
          </li>
        );
      })}
    </ol>
  );
}

export function Field({
  label,
  htmlFor,
  error,
  hint,
  children,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  hint?: string;
  children: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1.5">
      <label
        htmlFor={htmlFor}
        className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant"
      >
        {label}
      </label>
      {children}
      {error ? (
        <p role="alert" className="text-label-md text-error">
          {error}
        </p>
      ) : hint ? (
        <p className="text-label-md text-on-surface-variant">{hint}</p>
      ) : null}
    </div>
  );
}

export const inputClass =
  "w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-3.5 py-2.5 text-sm text-on-surface placeholder:text-outline transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30 aria-invalid:border-error aria-invalid:ring-2 aria-invalid:ring-error/20";

"use client";

import { MaterialIcon } from "@/components/material-icon";

const FREQUENT_TAGS = [
  "Cálculo Diferencial",
  "Python",
  "Física General",
  "Bioquímica",
];

const POPULAR_SUBJECTS = [
  "Cálculo Diferencial",
  "Termodinámica",
  "Estructuras de Datos",
  "Ingeniería Económica",
  "Bioquímica Clínica",
  "Química Orgánica",
];

const AVAILABILITY = ["Hoy mismo", "Esta semana", "Fines de semana"];

const LEVELS = [
  "Universidad (Grado)",
  "Bachillerato Internacional (IB)",
  "Examen de Admisión (PAA)",
  "Posgrado / Maestría",
];

export function Hero() {
  return (
    <section className="relative w-full overflow-hidden pb-16 lg:pb-24 pt-6 lg:pt-10">
      <div
        aria-hidden
        className="pointer-events-none absolute -top-32 -left-32 h-96 w-96 rounded-full bg-primary-container/10 blur-3xl"
      />
      <div
        aria-hidden
        className="pointer-events-none absolute top-1/2 -right-40 h-[30rem] w-[30rem] rounded-full bg-secondary-container/30 blur-3xl"
      />
      <div className="relative z-10 mx-auto max-w-[1400px] px-6 lg:px-12">
        <div className="grid grid-cols-1 items-center gap-12 lg:grid-cols-12 lg:gap-8">
          <div className="lg:col-span-7 flex flex-col items-start">
            <div className="mb-6 inline-flex items-center gap-2.5 rounded-full bg-surface-container-low px-3.5 py-1.5 text-primary shadow-sm">
              <span className="h-2 w-2 rounded-full bg-primary animate-pulse" />
              <span className="text-label-md tracking-wide font-semibold">
                La red #1 de tutorías universitarias verificadas UCR · TEC · UNA
              </span>
            </div>
            <h1 className="mb-5 max-w-2xl text-display-lg font-extrabold tracking-tight text-on-surface text-4xl sm:text-5xl lg:text-6xl">
              Domina tus materias más difíciles con{" "}
              <span className="text-primary">profesores de élite</span>.
            </h1>
            <p className="mb-8 max-w-xl text-body-lg leading-relaxed text-on-surface-variant">
              Tutorías personalizadas 1-a-1 online con docentes universitarios
              certificados, pizarra digital sincronizada y pago seguro en colones
              (SINPE Móvil) o dólares.
            </p>
            <SearchCard />
            <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-on-surface-variant text-sm font-semibold">
              <span className="inline-flex items-center gap-1.5">
                <MaterialIcon
                  name="verified_user"
                  className="text-primary text-[18px]"
                />
                Docentes con título revisado
              </span>
              <span className="inline-flex items-center gap-1.5">
                <MaterialIcon
                  name="lock_reset"
                  className="text-primary text-[18px]"
                />
                Garantía de reembolso 100%
              </span>
            </div>
          </div>
          <div className="lg:col-span-5 relative">
            <HeroShowcase />
          </div>
        </div>
      </div>
    </section>
  );
}

function SearchCard() {
  return (
    <div
      id="como-funciona"
      className="mb-6 w-full rounded-xl bg-surface-container-lowest p-4 sm:p-6 shadow-elev-lg"
    >
      <form className="flex flex-col gap-4" onSubmit={(e) => e.preventDefault()}>
        <div className="grid grid-cols-1 gap-3 md:grid-cols-12">
          <FieldGroup label="Materia o Tema" className="md:col-span-6">
            <div className="relative flex items-center">
              <MaterialIcon
                name="search"
                className="absolute left-3.5 text-primary text-[20px] pointer-events-none"
              />
              <input
                type="text"
                defaultValue="Cálculo Diferencial"
                placeholder="Ej. Cálculo Diferencial, Termodinámica..."
                aria-label="Materia o Tema"
                className="w-full rounded-lg bg-surface-container-low py-2.5 pl-10 pr-4 text-on-surface placeholder:text-outline focus:bg-surface-container-lowest focus:outline-none transition-all text-sm"
              />
            </div>
          </FieldGroup>
          <FieldGroup label="Nivel Académico" className="md:col-span-3">
            <Select label="Nivel Académico" options={LEVELS} />
          </FieldGroup>
          <FieldGroup label="Disponibilidad" className="md:col-span-3">
            <Select label="Disponibilidad" options={AVAILABILITY} />
          </FieldGroup>
        </div>
        <div className="flex flex-col items-stretch justify-between gap-3 pt-2 sm:flex-row sm:items-center">
          <div className="flex flex-wrap items-center gap-1.5">
            <span className="text-label-sm text-outline mr-1 uppercase tracking-wider">
              Frecuentes:
            </span>
            {FREQUENT_TAGS.map((tag) => (
              <button
                key={tag}
                type="button"
                className="rounded-full bg-surface-container px-2.5 py-1 text-label-sm text-on-surface-variant hover:bg-primary-fixed hover:text-primary transition-colors"
              >
                {tag}
              </button>
            ))}
          </div>
          <a
            href="#catalogo"
            className="inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-lg bg-primary px-6 py-3 text-title-md font-semibold text-on-primary hover:bg-on-primary-fixed-variant transition-all shadow-primary-glow"
          >
            <span>Encontrar Tutor Ideal</span>
            <MaterialIcon name="arrow_forward" className="text-[18px]" />
          </a>
        </div>
      </form>
    </div>
  );
}

function FieldGroup({
  label,
  className,
  children,
}: {
  label: string;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <div className={className}>
      <label className="flex flex-col gap-1.5">
        <span className="text-label-sm text-on-surface-variant uppercase tracking-wider font-semibold">
          {label}
        </span>
        {children}
      </label>
    </div>
  );
}

function Select({
  label,
  options,
}: {
  label: string;
  options: string[];
}) {
  return (
    <div className="relative flex items-center">
      <select
        aria-label={label}
        defaultValue={options[0]}
        className="w-full appearance-none rounded-lg bg-surface-container-low py-2.5 pl-3 pr-8 text-on-surface focus:bg-surface-container-lowest focus:outline-none transition-all text-sm"
      >
        {options.map((opt) => (
          <option key={opt} value={opt}>
            {opt}
          </option>
        ))}
      </select>
      <MaterialIcon
        name="expand_more"
        className="absolute right-3 text-on-surface-variant text-[18px] pointer-events-none"
      />
    </div>
  );
}

function HeroShowcase() {
  return (
    <div className="relative mx-auto max-w-md lg:max-w-none">
      <div className="absolute inset-0 translate-y-3 rotate-2 rounded-2xl bg-primary-container/10" />
      <div className="relative overflow-hidden rounded-2xl bg-surface-container-lowest shadow-elev-xl">
        <div
          aria-hidden
          className="h-[460px] w-full bg-gradient-to-br from-primary-container/30 via-secondary-container/40 to-tertiary-fixed/40"
        />
        <div className="absolute inset-0 bg-gradient-to-t from-inverse-surface/80 via-transparent to-transparent" />
        <div className="absolute bottom-4 left-4 right-4 rounded-xl bg-surface-container-lowest/95 p-4 shadow-lg backdrop-blur-md">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="grid h-10 w-10 place-items-center rounded-full bg-primary-fixed text-primary font-bold">
                CS
              </div>
              <div>
                <span className="block text-label-sm uppercase tracking-wider text-primary font-bold">
                  Sesión en Curso
                </span>
                <p className="text-title-md font-semibold text-on-surface">
                  Cálculo Multivariable
                </p>
              </div>
            </div>
            <span className="inline-flex items-center gap-1 rounded-md bg-surface-container-low px-2.5 py-1 text-on-surface-variant text-xs font-semibold">
              <span className="mr-1 h-2 w-2 rounded-full bg-error animate-ping" />
              En Vivo
            </span>
          </div>
        </div>
      </div>
      <div className="absolute -top-4 -right-4 sm:-right-6 flex animate-bounce items-center gap-2.5 rounded-full bg-surface-container-lowest px-4 py-2 shadow-elev-pill [animation-duration:4s]">
        <div className="grid h-7 w-7 place-items-center rounded-full bg-tertiary-fixed">
          <MaterialIcon
            name="star"
            filled
            className="text-tertiary text-[18px]"
          />
        </div>
        <div className="pr-1 leading-tight">
          <span className="block text-title-md font-bold text-on-surface">
            4.98 / 5.0
          </span>
          <span className="block text-label-sm text-on-surface-variant">
            1,200+ evaluaciones
          </span>
        </div>
      </div>
      <div className="absolute top-1/3 -left-6 hidden rounded-xl bg-surface-container-lowest p-3 shadow-elev-pill sm:flex items-center gap-3">
        <div className="grid h-9 w-9 place-items-center rounded-lg bg-surface-container-low text-primary">
          <MaterialIcon name="bolt" className="text-[22px]" />
        </div>
        <div>
          <p className="text-label-md font-bold text-on-surface">
            Confirmación Instantánea
          </p>
          <p className="text-label-sm text-on-surface-variant">
            Sin esperas de aprobación
          </p>
        </div>
      </div>
    </div>
  );
}

export { POPULAR_SUBJECTS };

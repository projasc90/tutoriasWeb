import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

type Discipline = {
  icon: string;
  count: string | number;
  countLabel: string;
  title: string;
  description: string;
  tags: string[];
  variant: "default" | "featured";
  footer?: { label: string; icon: string };
};

const DISCIPLINES: Discipline[] = [
  {
    icon: "functions",
    count: 42,
    countLabel: "Tutores",
    title: "Ciencias Exactas & Matemáticas",
    description:
      "Cálculo I, II y III, Álgebra Lineal, Ecuaciones Diferenciales, Variable Compleja y Estadística Matemática.",
    tags: ["Cálculo Diferencial", "Álgebra Lineal", "Probabilidad"],
    variant: "default",
  },
  {
    icon: "terminal",
    count: 35,
    countLabel: "Tutores",
    title: "Ingeniería & Ciencias de la Computación",
    description:
      "Programación en Python, C++, Estructuras de Datos, Arquitectura de Redes y Circuitos Eléctricos.",
    tags: ["Python Avanzado", "Estructuras", "Circuitos"],
    variant: "default",
  },
  {
    icon: "science",
    count: 28,
    countLabel: "Tutores",
    title: "Física, Química & Salud",
    description:
      "Física Mecánica, Electromagnetismo, Química Orgánica, Bioquímica Médica y Farmacología.",
    tags: ["Física II", "Química Orgánica", "Termodinámica"],
    variant: "default",
  },
  {
    icon: "finance_mode",
    count: 19,
    countLabel: "Tutores",
    title: "Economía, Finanzas & Negocios",
    description:
      "Microeconomía Intermedia, Econometría, Contabilidad de Costos y Finanzas Corporativas.",
    tags: ["Econometría", "Microeconomía", "Finanzas"],
    variant: "default",
  },
  {
    icon: "translate",
    count: 24,
    countLabel: "Tutores",
    title: "Idiomas & Redacción Académica",
    description:
      "Preparación intensiva para TOEFL iBT, IELTS Académico, redacción de artículos científicos y tesis.",
    tags: ["TOEFL iBT", "IELTS 7.5+", "Paper Writing"],
    variant: "default",
  },
  {
    icon: "school",
    count: "Alto Rendimiento",
    countLabel: "",
    title: "Preparación PAA & Admisión UCR / TEC",
    description:
      "Estrategias probadas de razonamiento lógico-matemático y verbal para obtener puntajes de admisión +700.",
    tags: [],
    variant: "featured",
    footer: { label: "Ver programa de preparación", icon: "arrow_forward" },
  },
];

export function Disciplines() {
  return (
    <section id="disciplinas" className="w-full bg-surface-container-low py-16">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <div className="mb-10 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <SectionEyebrow>Catálogo Académico</SectionEyebrow>
            <SectionTitle className="mt-1">
              Explora por disciplina especializada
            </SectionTitle>
          </div>
          <a
            href="#"
            className="inline-flex items-center gap-1.5 text-primary hover:underline text-label-md font-bold"
          >
            <span>Ver todas las 140+ materias</span>
            <MaterialIcon name="east" className="text-[18px]" />
          </a>
        </div>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-3">
          {DISCIPLINES.map((d) =>
            d.variant === "featured" ? (
              <FeaturedDiscipline key={d.title} d={d} />
            ) : (
              <DefaultDiscipline key={d.title} d={d} />
            )
          )}
        </div>
      </div>
    </section>
  );
}

function DefaultDiscipline({ d }: { d: Discipline }) {
  return (
    <article className="group flex cursor-pointer flex-col justify-between rounded-xl bg-surface-container-lowest p-6 shadow-sm transition-shadow hover:shadow-md">
      <div>
        <div className="mb-4 flex items-center justify-between">
          <div className="grid h-10 w-10 place-items-center rounded-lg bg-surface-container text-primary transition-colors group-hover:bg-primary group-hover:text-on-primary">
            <MaterialIcon name={d.icon} className="text-[22px]" />
          </div>
          <span className="rounded-full bg-surface-container px-3 py-1 text-on-surface-variant text-xs font-semibold">
            {d.count} Tutores
          </span>
        </div>
        <h3 className="mb-2 text-title-md font-bold text-on-surface">
          {d.title}
        </h3>
        <p className="mb-4 text-body-md text-on-surface-variant">
          {d.description}
        </p>
      </div>
      <div className="flex flex-wrap gap-1.5 pt-3">
        {d.tags.map((tag) => (
          <span
            key={tag}
            className="rounded bg-surface-container-low px-2.5 py-1 text-label-sm text-on-surface-variant"
          >
            {tag}
          </span>
        ))}
      </div>
    </article>
  );
}

function FeaturedDiscipline({ d }: { d: Discipline }) {
  return (
    <article className="flex flex-col justify-between rounded-xl bg-primary-container p-6 text-on-primary shadow-md">
      <div>
        <div className="mb-4 flex items-center justify-between">
          <div className="grid h-10 w-10 place-items-center rounded-lg bg-on-primary/10 text-on-primary">
            <MaterialIcon name={d.icon} className="text-[22px]" />
          </div>
          <span className="rounded-full bg-on-primary/20 px-3 py-1 text-on-primary text-xs font-semibold">
            {d.count}
          </span>
        </div>
        <h3 className="mb-2 text-title-md font-bold text-on-primary">
          {d.title}
        </h3>
        <p className="mb-4 text-body-md text-primary-fixed">
          {d.description}
        </p>
      </div>
      {d.footer ? (
        <a
          href="#"
          className="inline-flex items-center gap-2 text-on-primary text-label-md font-bold hover:underline"
        >
          <span>{d.footer.label}</span>
          <MaterialIcon name={d.footer.icon} className="text-[16px]" />
        </a>
      ) : null}
    </article>
  );
}

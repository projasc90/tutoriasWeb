const INSTITUTIONS = [
  {
    acronym: "UCR",
    full: "Univ. de\nCosta Rica",
    tone: "text-primary",
    weight: "font-extrabold",
  },
  {
    acronym: "TEC",
    full: "Tecnológico\nde Costa Rica",
    tone: "text-primary-container",
    weight: "font-extrabold",
  },
  {
    acronym: "UNA",
    full: "Universidad\nNacional",
    tone: "text-tertiary",
    weight: "font-extrabold",
  },
  {
    acronym: "LEAD",
    full: "University\nCR",
    tone: "text-on-surface",
    weight: "font-bold",
  },
  {
    acronym: "IB",
    full: "Bachillerato\nInternacional",
    tone: "text-secondary",
    weight: "font-bold",
  },
];

export function TrustStrip() {
  return (
    <section
      aria-label="Acreditación institucional"
      className="w-full bg-surface-container-low py-10"
    >
      <div className="mx-auto flex max-w-[1400px] flex-col items-center justify-between gap-8 px-6 md:flex-row lg:px-12">
        <div className="max-w-xs text-center md:text-left">
          <p className="mb-1 text-label-sm uppercase tracking-widest text-on-surface-variant font-bold">
            Aprobación Comprobada
          </p>
          <p className="text-title-md font-semibold text-on-surface">
            Estudiantes de las principales instituciones académicas aprenden con
            nosotros:
          </p>
        </div>
        <div className="flex flex-wrap items-center justify-center gap-6 opacity-90 sm:gap-8 md:justify-end">
          {INSTITUTIONS.map((inst) => (
            <div
              key={inst.acronym}
              className="flex items-center gap-2 rounded-lg bg-surface-container-lowest px-3 py-2 shadow-sm"
            >
              <span
                className={`text-headline-sm ${inst.weight} ${inst.tone}`}
                style={{ fontSize: "1.5rem" }}
              >
                {inst.acronym}
              </span>
              <span className="text-label-sm text-on-surface-variant leading-tight whitespace-pre">
                {inst.full}
              </span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

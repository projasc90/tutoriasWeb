import { MaterialIcon } from "@/components/material-icon";

const PROS = [
  "Fija tus propias tarifas por hora sin techos",
  "Pagos directos a tu cuenta vía SINPE o IBAN",
  "Gestión de agenda automatizada",
];

export function TutorCta() {
  return (
    <section id="ser-tutor" className="w-full bg-surface py-16">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <div className="relative overflow-hidden rounded-3xl bg-inverse-surface p-8 text-inverse-on-surface sm:p-12 lg:p-16">
          <div
            aria-hidden
            className="pointer-events-none absolute -right-20 -bottom-20 h-80 w-80 rounded-full bg-primary-container/30 blur-3xl"
          />
          <div className="relative z-10 grid grid-cols-1 items-center gap-10 lg:grid-cols-12">
            <div className="lg:col-span-8">
              <span className="mb-4 inline-block rounded-full bg-surface-container-lowest/10 px-3 py-1 text-primary-fixed text-label-md font-semibold">
                Conviértete en Mentor AuraLearn
              </span>
              <h2 className="mb-4 max-w-2xl text-headline-lg font-bold tracking-tight text-inverse-on-surface">
                ¿Eres docente universitario o profesional destacado? Comparte tu
                conocimiento y monetiza tus horas libres.
              </h2>
              <p className="mb-8 max-w-xl text-body-lg leading-relaxed text-inverse-on-surface/80">
                Únete a la red académica más prestigiosa de Centroamérica. Te
                brindamos la infraestructura tecnológica, gestión de cobros y
                estudiantes listos para aprender.
              </p>
              <div className="mb-8 grid grid-cols-1 gap-6 sm:grid-cols-3">
                {PROS.map((pro) => (
                  <div key={pro} className="flex items-start gap-2.5">
                    <MaterialIcon
                      name="check_circle"
                      className="mt-0.5 text-tertiary-fixed text-[20px]"
                    />
                    <span className="text-body-md text-inverse-on-surface/90">
                      {pro}
                    </span>
                  </div>
                ))}
              </div>
              <div className="flex flex-col items-stretch gap-4 sm:flex-row sm:items-center">
                <a
                  href="/postular"
                  className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary-container px-8 py-4 text-on-primary text-title-md font-semibold shadow-lg transition-colors hover:bg-primary"
                >
                  <span>Postular como Docente</span>
                  <MaterialIcon name="arrow_forward" className="text-[18px]" />
                </a>
                <a
                  href="#"
                  className="inline-flex items-center justify-center gap-2 rounded-xl bg-surface-container-lowest/10 px-6 py-4 text-inverse-on-surface text-title-md font-semibold transition-colors hover:bg-surface-container-lowest/20"
                >
                  <span>Conocer Requisitos</span>
                </a>
              </div>
            </div>
            <aside className="flex flex-col gap-4 lg:col-span-4">
              <div className="rounded-2xl bg-surface-container-lowest/10 p-6 backdrop-blur-md">
                <span className="mb-1 block uppercase tracking-wider text-inverse-on-surface/60 text-label-sm font-bold">
                  Ingresos promedio estimados
                </span>
                <p className="text-display-lg font-black text-inverse-on-surface">
                  ₡380,000
                </p>
                <p className="mt-1 text-label-md text-inverse-on-surface/75">
                  al mes impartiendo 6 horas semanales
                </p>
              </div>
              <div className="rounded-2xl bg-surface-container-lowest/10 p-6 backdrop-blur-md">
                <div className="flex items-center gap-3">
                  <MaterialIcon
                    name="speed"
                    className="text-primary-fixed text-[28px]"
                  />
                  <div>
                    <p className="text-title-md font-bold text-inverse-on-surface">
                      Aprobación en 48h
                    </p>
                    <p className="text-label-sm text-inverse-on-surface/70">
                      Revisión ágil de atestados y títulos
                    </p>
                  </div>
                </div>
              </div>
            </aside>
          </div>
        </div>
      </div>
    </section>
  );
}

import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle, SectionSubtitle } from "./section-eyebrow";

const TESTIMONIALS = [
  {
    quote:
      "Llevaba Cálculo II en el TEC y no lograba pasar del 50 en las prácticas. Con tres semanas de sesiones con el Dr. Carlos logré un 92 en el parcial final. Poder pagar con SINPE en 30 segundos hace todo súper práctico.",
    name: "Andrés Lobo",
    context: "Ingeniería en Computadores · TEC",
    initials: "AL",
    avatarTone: "bg-primary-fixed text-primary",
  },
  {
    quote:
      "La calidad de la pizarra digital y que todo te quede en PDF sin tener que estar copiando a las carreras es una ventaja enorme. Mi tutora de Bioquímica me explicó temas que el profesor de la facultad daba por sentado.",
    name: "Mariana Salas",
    context: "Medicina y Cirugía · UCR",
    initials: "MS",
    avatarTone: "bg-surface-container text-on-surface",
  },
  {
    quote:
      "Me preparé para el examen de admisión PAA con simulacros semanales. Obtuve 742 puntos y logré entrar a la carrera que quería en primera opción. 100% recomendados.",
    name: "Felipe Esquivel",
    context: "Bachillerato Internacional (IB)",
    initials: "FE",
    avatarTone: "bg-secondary-fixed text-on-secondary-fixed",
  },
];

export function Testimonials() {
  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <header className="mx-auto mb-14 max-w-2xl text-center">
          <SectionEyebrow>Historias de Éxito</SectionEyebrow>
          <SectionTitle>Estudiantes que lograron su meta</SectionTitle>
          <SectionSubtitle>
            De la frustración a calificaciones sobresalientes en parciales
            decisivos.
          </SectionSubtitle>
        </header>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          {TESTIMONIALS.map((t) => (
            <article
              key={t.name}
              className="flex flex-col justify-between rounded-xl bg-surface-container-lowest p-6 shadow-sm"
            >
              <div>
                <div className="mb-4 flex items-center gap-1 text-tertiary">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <MaterialIcon
                      key={i}
                      name="star"
                      filled
                      className="text-[18px]"
                    />
                  ))}
                </div>
                <p className="mb-6 text-body-md italic leading-relaxed text-on-surface-variant">
                  “{t.quote}”
                </p>
              </div>
              <div className="flex items-center gap-3 pt-4">
                <div
                  className={`grid h-10 w-10 place-items-center rounded-full font-bold ${t.avatarTone}`}
                >
                  {t.initials}
                </div>
                <div>
                  <p className="text-title-md font-bold leading-tight text-on-surface">
                    {t.name}
                  </p>
                  <p className="text-label-sm text-on-surface-variant">
                    {t.context}
                  </p>
                </div>
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}

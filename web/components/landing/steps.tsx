import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle, SectionSubtitle } from "./section-eyebrow";

const STEPS = [
  {
    number: "1",
    title: "Filtra y Selecciona",
    description:
      "Busca según tu materia específica, carrera y universidad. Consulta opiniones reales de otros estudiantes, credenciales verificadas y tarifas por hora.",
    icon: "tune",
    footnote: "Perfiles detallados y muestras de video",
  },
  {
    number: "2",
    title: "Reserva en 1 Clic",
    description:
      "Elige el horario exacto en el calendario sincronizado del profesor. Paga de forma segura mediante SINPE Móvil o tarjeta sin recargos ocultos.",
    icon: "credit_card",
    footnote: "Confirmación instantánea",
  },
  {
    number: "3",
    title: "Conéctate y Aprueba",
    description:
      "Ingresa a la sala interactiva con pizarra digital. Al terminar, descarga los apuntes en PDF y accede a la grabación completa de la sesión para repasar antes del examen.",
    icon: "smart_display",
    footnote: "Acceso ilimitado a repeticiones",
  },
];

export function Steps() {
  return (
    <section className="w-full bg-surface-container-low py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <header className="mx-auto mb-16 max-w-2xl text-center">
          <SectionEyebrow className="mb-2">Paso a Paso</SectionEyebrow>
          <SectionTitle>Tu tutoría en 3 sencillos pasos</SectionTitle>
          <SectionSubtitle>
            Diseñado para que dediques tu tiempo a aprender, no a coordinar
            mensajes interminables.
          </SectionSubtitle>
        </header>
        <div className="relative grid grid-cols-1 gap-8 md:grid-cols-3">
          {STEPS.map((step) => (
            <article
              key={step.number}
              className="relative rounded-2xl bg-surface-container-lowest p-8 shadow-sm"
            >
              <div className="mb-6 grid h-12 w-12 place-items-center rounded-xl bg-primary text-on-primary text-headline-sm font-bold">
                {step.number}
              </div>
              <h3 className="mb-3 text-title-md font-bold text-on-surface">
                {step.title}
              </h3>
              <p className="text-body-md leading-relaxed text-on-surface-variant">
                {step.description}
              </p>
              <div className="mt-6 flex items-center gap-2 text-primary text-label-md font-semibold">
                <MaterialIcon name={step.icon} className="text-[18px]" />
                <span>{step.footnote}</span>
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}

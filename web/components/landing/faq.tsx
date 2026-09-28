import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

const QUESTIONS = [
  {
    q: "¿Cómo funciona el pago mediante SINPE Móvil?",
    a: "Al seleccionar tu clase, la plataforma genera un número de confirmación y el teléfono oficial de recepción de fondos. Transfieres desde la app de tu banco y el sistema valida la acreditación en segundos sin comisiones adicionales.",
  },
  {
    q: "¿Qué ocurre si necesito reprogramar o cancelar una sesión?",
    a: "Puedes reprogramar sin penalización alguna con hasta 12 horas de anticipación directamente desde tu panel de estudiante. Si cancelas con tiempo, el saldo queda acreditado en tu monedero o devuelto a tu método original.",
  },
  {
    q: "¿Las clases quedan grabadas y puedo descargarlas?",
    a: "Sí. Todo el video en alta definición y los apuntes generados en la pizarra digital se guardan en tu repositorio personal con acceso ilimitado de por vida, ideal para tus repasos finales antes de los exámenes parciales.",
  },
  {
    q: "¿Cómo aseguran la calidad y preparación del tutor?",
    a: "Nuestro equipo revisa formalmente atestados ante el colegio profesional respectivo y valida el título en el registro universitario de la UCR, TEC o UNA. Además, contamos con la garantía de satisfacción durante los primeros 15 minutos de tutoría.",
  },
];

export function Faq() {
  return (
    <section className="w-full bg-surface-container-low py-20">
      <div className="mx-auto max-w-[960px] px-6 lg:px-12">
        <header className="mb-12 text-center">
          <SectionEyebrow>Preguntas Frecuentes</SectionEyebrow>
          <SectionTitle className="mt-1">
            Resolvemos tus dudas sobre el servicio
          </SectionTitle>
        </header>
        <div className="flex flex-col gap-4">
          {QUESTIONS.map((item) => (
            <details
              key={item.q}
              className="group rounded-xl bg-surface-container-lowest p-5 shadow-sm transition-all open:shadow-md"
            >
              <summary className="flex cursor-pointer list-none items-center justify-between text-title-md font-bold text-on-surface">
                <span>{item.q}</span>
                <MaterialIcon
                  name="expand_more"
                  className="text-primary transition-transform group-open:rotate-180"
                />
              </summary>
              <p className="pt-3 text-body-md leading-relaxed text-on-surface-variant">
                {item.a}
              </p>
            </details>
          ))}
        </div>
      </div>
    </section>
  );
}

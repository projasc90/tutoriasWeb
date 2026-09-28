import { MaterialIcon } from "@/components/material-icon";
import { SectionEyebrow, SectionTitle, SectionSubtitle } from "./section-eyebrow";

const PILLARS = [
  {
    icon: "verified",
    title: "Docencia Rigurosa",
    badge: "Top 8%",
    description:
      "Solo admitimos al 8% de los postulantes. Verificamos títulos de grado, posgrados y trayectoria pedagógica en universidades reconocidas.",
    footer: { label: "Filtro de acreditación", icon: "check_circle" },
  },
  {
    icon: "draw",
    title: "Pizarra y Video HD",
    description:
      "Trabaja con fórmulas complejas en tiempo real. Al terminar, descarga automáticamente el archivo PDF con todas las anotaciones y el video de la clase.",
    footer: { label: "Grabaciones de por vida", icon: "cloud_download" },
  },
  {
    icon: "payments",
    title: "SINPE Móvil y Tarjetas",
    description:
      "Paga en Colones exactos mediante SINPE Móvil o en USD con tarjeta internacional. Retención segura hasta concluida tu clase exitosamente.",
    footer: { label: "Comprobante instantáneo", icon: "receipt_long" },
  },
  {
    icon: "shield",
    title: "Garantía Primeros 15'",
    description:
      "Si en los primeros 15 minutos sientes que el enfoque del docente no encaja con tu objetivo, pausamos la sesión y te reasignamos o reembolsamos.",
    footer: { label: "Satisfacción asegurada", icon: "verified" },
  },
];

export function Rigor() {
  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <header className="mx-auto mb-16 flex max-w-2xl flex-col items-center text-center">
          <SectionEyebrow className="mb-2">Excelencia Académica</SectionEyebrow>
          <SectionTitle>Diseñado para resultados de alto calibre</SectionTitle>
          <SectionSubtitle>
            Un entorno estructurado y sin fricción donde cada detalle maximiza tu
            comprensión y optimiza tus horas de estudio.
          </SectionSubtitle>
        </header>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
          {PILLARS.map((pillar) => (
            <article
              key={pillar.title}
              className="flex flex-col justify-between rounded-xl bg-surface-container-lowest p-6 shadow-sm transition-all hover:shadow-primary-card"
            >
              <div>
                <div className="mb-5 grid h-12 w-12 place-items-center rounded-xl bg-surface-container-low text-primary">
                  <MaterialIcon name={pillar.icon} className="text-[28px]" />
                </div>
                <div className="mb-2 flex items-center gap-2">
                  <h3 className="text-title-md font-bold text-on-surface">
                    {pillar.title}
                  </h3>
                  {pillar.badge ? (
                    <span className="rounded bg-primary-fixed px-2 py-0.5 text-[10px] font-bold text-primary">
                      {pillar.badge}
                    </span>
                  ) : null}
                </div>
                <p className="text-body-md leading-relaxed text-on-surface-variant">
                  {pillar.description}
                </p>
              </div>
              <div className="mt-6 flex items-center gap-2 pt-4 text-primary text-label-md font-semibold">
                <span>{pillar.footer.label}</span>
                <MaterialIcon name={pillar.footer.icon} className="text-[16px]" />
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}

export function FinalCta() {
  return (
    <section className="w-full bg-surface py-16">
      <div className="mx-auto max-w-[1400px] px-6 text-center lg:px-12">
        <div className="mx-auto flex max-w-3xl flex-col items-center">
          <h2 className="mb-4 text-headline-lg font-bold tracking-tight text-on-surface">
            Comienza a dominar tus cursos hoy mismo
          </h2>
          <p className="mb-8 max-w-xl text-body-lg text-on-surface-variant">
            Elige a tu docente, selecciona la hora y asegura tu cupo en menos de
            2 minutos.
          </p>
          <div className="flex flex-col items-center gap-4 sm:flex-row">
            <a
              href="#catalogo"
              className="rounded-full bg-primary px-8 py-4 text-title-md font-semibold text-on-primary shadow-primary-ring transition-all hover:bg-on-primary-fixed-variant"
            >
              Buscar Mi Tutor Ahora
            </a>
            <a
              href="#"
              className="rounded-full bg-surface-container-low px-6 py-4 text-title-md font-semibold text-on-surface transition-colors hover:bg-surface-container"
            >
              Explorar Paquetes para Exámenes
            </a>
          </div>
        </div>
      </div>
    </section>
  );
}

import type { Metadata } from "next";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { OnboardingWizard } from "@/components/onboarding/onboarding-wizard";

export const metadata: Metadata = {
  title: "Postular como Docente — AuraLearn",
  description:
    "Regístrate como tutor verificado de AuraLearn. Completa tu perfil en 4 pasos; nuestro equipo verifica tu título y atestados en ≤48 horas.",
};

export default function PostularPage() {
  return (
    <>
      <SiteHeader />
      <main className="flex-1 bg-surface">
        <div className="mx-auto max-w-[1400px] px-6 py-12 lg:px-12 lg:py-16">
          <div className="grid grid-cols-1 gap-12 lg:grid-cols-12">
            {/* Columna de contexto (propuesta de valor) */}
            <aside className="lg:col-span-5">
              <span className="mb-4 inline-block rounded-full bg-primary-container/15 px-3 py-1 text-label-md font-semibold text-primary">
                Conviértete en Mentor AuraLearn
              </span>
              <h1 className="text-headline-lg font-bold tracking-tight text-on-surface">
                Comparte tu conocimiento y monetiza tus horas libres
              </h1>
              <p className="mt-4 max-w-md text-body-lg leading-relaxed text-on-surface-variant">
                Únete a la red académica más prestigiosa de Centroamérica. Solo admitimos
                al 8% de los postulantes: tu título se verifica contra el registro
                universitario.
              </p>

              <ul className="mt-8 flex flex-col gap-4">
                <Perk icon="payments" text="Fija tus propias tarifas por hora, sin techos" />
                <Perk icon="account_balance" text="Pagos directos a tu cuenta vía SINPE o IBAN" />
                <Perk icon="calendar_month" text="Gestión de agenda automatizada" />
                <Perk icon="verified_user" text="Verificación en ≤48 horas" />
              </ul>

              <div className="mt-10 rounded-2xl bg-surface-container-low p-6">
                <span className="text-label-sm font-bold uppercase tracking-wider text-on-surface-variant">
                  Ingresos promedio estimados
                </span>
                <p className="mt-1 text-display-lg font-black text-on-surface">₡380,000</p>
                <p className="mt-1 text-label-md text-on-surface-variant">
                  al mes impartiendo 6 horas semanales
                </p>
              </div>
            </aside>

            {/* Wizard */}
            <section className="lg:col-span-7" aria-label="Formulario de postulación">
              <OnboardingWizard />
            </section>
          </div>
        </div>
      </main>
    </>
  );
}

function Perk({ icon, text }: { icon: string; text: string }) {
  return (
    <li className="flex items-center gap-3">
      <span className="grid h-10 w-10 shrink-0 place-items-center rounded-lg bg-primary-container/15 text-primary">
        <MaterialIcon name={icon} className="text-[20px]" />
      </span>
      <span className="text-body-md font-semibold text-on-surface">{text}</span>
    </li>
  );
}

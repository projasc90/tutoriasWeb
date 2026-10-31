import Link from "next/link";

import { MaterialIcon } from "@/components/material-icon";

/**
 * Sin `href` = sección aún sin página pública: se renderiza como texto para no
 * ofrecer enlaces muertos (antes eran `href="#"` y saltaban al inicio).
 */
type FooterLink = { label: string; href?: string };

const PLATFORM: FooterLink[] = [
  { label: "Buscar Tutores", href: "/tutores" },
  { label: "Materias Populares", href: "/#disciplinas" },
  { label: "Calendarios en Vivo" },
  { label: "Centro de Ayuda" },
  { label: "Estado del Sistema" },
];

const EDUCATORS: FooterLink[] = [
  { label: "Publicar tu Perfil", href: "/postular" },
  { label: "Política de Precios" },
  { label: "Comisiones y Pagos" },
  { label: "Foro de Mentores" },
];

const LEGAL: FooterLink[] = [
  { label: "Términos y Condiciones" },
  { label: "Política de Privacidad" },
  { label: "Política de Cookies" },
  { label: "Tratamiento de Datos" },
];

export function SiteFooter() {
  return (
    <footer className="w-full bg-surface-container-lowest shadow-[0_-1px_8px_rgba(0,0,0,0.03)]">
      <div className="mx-auto max-w-[1400px] px-6 pt-16 pb-12 lg:px-12">
        <div className="mb-14 grid grid-cols-1 gap-10 md:grid-cols-2 lg:grid-cols-5">
          <div className="flex flex-col gap-4 lg:col-span-2">
            <div className="flex items-center gap-3">
              <span
                aria-hidden
                className="grid h-8 w-8 place-items-center rounded-md bg-gradient-to-br from-primary to-primary-container text-lg font-extrabold text-on-primary leading-none"
              >
                A
              </span>
              <span className="text-headline-sm font-bold tracking-tight text-primary">
                AuraLearn
              </span>
            </div>
            <p className="max-w-xs text-body-md text-on-surface-variant">
              La primera red de tutorías universitarias con verificación de
              credenciales y pago instantáneo en colones costarricenses.
            </p>
            <div className="flex flex-col gap-2 pt-2">
              <span className="inline-flex items-center gap-2 text-label-md text-on-surface-variant">
                <MaterialIcon
                  name="payments"
                  className="text-primary text-[18px]"
                />
                SINPE Móvil & Tarjeta Internacional
              </span>
              <span className="inline-flex items-center gap-2 text-label-md text-on-surface-variant">
                <MaterialIcon name="location_on" className="text-primary text-[18px]" />
                San Pedro, Montes de Oca · Costa Rica
              </span>
            </div>
          </div>
          <FooterColumn title="Plataforma" items={PLATFORM} />
          <FooterColumn title="Para Educadores" items={EDUCATORS} />
          <FooterColumn title="Soporte y Legal" items={LEGAL} />
        </div>
        <div className="flex flex-col items-start justify-between gap-3 border-t border-outline-variant/60 pt-6 sm:flex-row sm:items-center">
          <p className="text-label-md text-on-surface-variant">
            © {new Date().getFullYear()} AuraLearn, S.A. · Todos los derechos
            reservados.
          </p>
          <p className="text-label-md text-on-surface-variant">
            Hecho en Costa Rica con ☕ y mucho café.
          </p>
        </div>
      </div>
    </footer>
  );
}

function FooterColumn({
  title,
  items,
}: {
  title: string;
  items: FooterLink[];
}) {
  return (
    <div className="flex flex-col gap-3">
      <h3 className="text-on-surface text-title-md font-bold">{title}</h3>
      <ul className="flex flex-col gap-2">
        {items.map((item) => (
          <li key={item.label}>
            {item.href ? (
              <Link
                href={item.href}
                className="text-on-surface-variant hover:text-on-surface text-body-md transition-colors"
              >
                {item.label}
              </Link>
            ) : (
              <span className="text-on-surface-variant/70 text-body-md">
                {item.label}
              </span>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}

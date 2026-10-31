"use client";

/**
 * Sub-navegación del panel del estudiante (/estudiante/*): Reservas ·
 * Monedero. Espejo de TutorSubnav; resaltado por ruta activa con usePathname.
 */

import Link from "next/link";
import { usePathname } from "next/navigation";
import { MaterialIcon } from "@/components/material-icon";

const LINKS = [
  { href: "/estudiante/reservas", label: "Reservas", icon: "event_available" },
  { href: "/estudiante/monedero", label: "Monedero", icon: "account_balance_wallet" },
];

export function StudentSubnav() {
  const pathname = usePathname();

  return (
    <nav
      aria-label="Panel del estudiante"
      className="border-b border-outline-variant/40 bg-surface-container-lowest"
    >
      <div className="mx-auto flex max-w-7xl gap-1 overflow-x-auto px-4 sm:px-6">
        {LINKS.map((link) => {
          const active = pathname === link.href || pathname.startsWith(`${link.href}/`);
          return (
            <Link
              key={link.href}
              href={link.href}
              aria-current={active ? "page" : undefined}
              className={
                active
                  ? "flex shrink-0 items-center gap-1.5 border-b-2 border-primary px-3 py-3 text-label-md font-bold text-primary"
                  : "flex shrink-0 items-center gap-1.5 border-b-2 border-transparent px-3 py-3 text-label-md font-semibold text-on-surface-variant transition-colors hover:text-on-surface"
              }
            >
              <MaterialIcon name={link.icon} className="text-[18px]" />
              {link.label}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}

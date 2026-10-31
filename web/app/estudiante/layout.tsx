import type { ReactNode } from "react";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { StudentSubnav } from "@/components/estudiante/student-subnav";

/**
 * Layout del panel del estudiante (/estudiante/*): header público + subnav
 * (Reservas · Monedero). Las páginas hijas ya no renderizan su propio
 * SiteHeader (patrón espejo de /tutor/*).
 */
export default function EstudianteLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <SiteHeader />
      <StudentSubnav />
      {children}
    </>
  );
}

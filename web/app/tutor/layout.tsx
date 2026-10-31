import type { ReactNode } from "react";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { TutorSubnav } from "@/components/tutor/profile/tutor-subnav";

/**
 * Layout del panel del tutor (/tutor/*): header público + subnav
 * (Perfil · Disponibilidad · Sesiones). Las páginas hijas ya no renderizan
 * su propio SiteHeader.
 */
export default function TutorLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <SiteHeader />
      <TutorSubnav />
      {children}
    </>
  );
}

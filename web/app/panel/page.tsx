"use client";

/**
 * Redirección del panel según el rol del JWT (entry point único "Mi Panel").
 * Estudiante → /mis-tutorias · Tutor → /tutor/sesiones · Admin → /admin/pagos.
 */

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { useAuth } from "@/hooks/use-auth";

export default function PanelRedirectPage() {
  const router = useRouter();
  const { state } = useAuth();

  useEffect(() => {
    if (state.status === "anonymous") {
      router.replace("/login");
      return;
    }
    if (state.status !== "authenticated") return;

    const role = state.session.role;
    if (role === "Admin") router.replace("/admin/pagos");
    else if (role === "Tutor") router.replace("/tutor/sesiones");
    else router.replace("/mis-tutorias");
  }, [state, router]);

  return (
    <>
      <SiteHeader />
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto max-w-md px-6 py-16 text-center">
          <p className="text-body-lg text-on-surface-variant">Redirigiendo a tu panel…</p>
        </div>
      </main>
    </>
  );
}

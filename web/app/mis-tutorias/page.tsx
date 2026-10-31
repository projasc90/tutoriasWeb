"use client";

/**
 * Compatibilidad: /mis-tutorias ahora vive en /estudiante/reservas.
 * Este redirect conserva los enlaces existentes (landing, checkout, tutorías).
 */

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";

export default function MisTutoriasRedirectPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace("/estudiante/reservas");
  }, [router]);

  return (
    <>
      <SiteHeader />
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto max-w-md px-6 py-16 text-center">
          <p className="text-body-lg text-on-surface-variant">Redirigiendo a tus reservas…</p>
        </div>
      </main>
    </>
  );
}

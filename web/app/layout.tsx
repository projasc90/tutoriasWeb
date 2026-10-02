import type { Metadata } from "next";
import { Plus_Jakarta_Sans, JetBrains_Mono } from "next/font/google";
import { MaterialSymbols } from "@/components/material-icon";
import { AuthProvider } from "@/hooks/use-auth";
import "./globals.css";

const plusJakartaSans = Plus_Jakarta_Sans({
  variable: "--font-plus-jakarta-sans",
  subsets: ["latin"],
  weight: ["400", "500", "600", "700", "800"],
  display: "swap",
});

// Cifras tabulares/mono para horarios, códigos y montos (₡/$) del spec tipográfico.
const jetbrainsMono = JetBrains_Mono({
  variable: "--font-numeric",
  subsets: ["latin"],
  weight: ["400", "700"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "AuraLearn — Tutorías universitarias verificadas",
  description:
    "Tutorías personalizadas 1-a-1 online con docentes universitarios certificados de la UCR, TEC, UNA y LEAD. Pizarra digital, SINPE Móvil y garantía de satisfacción.",
  metadataBase: new URL("https://auralearn.cr"),
  openGraph: {
    title: "AuraLearn — Plataforma de Tutorías Universitarias de Alto Rigor",
    description:
      "Domina tus materias más difíciles con profesores de élite. Reserva en menos de 2 minutos y paga en colones o dólares.",
    locale: "es_CR",
    type: "website",
  },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="es"
      className={`${plusJakartaSans.variable} ${jetbrainsMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col bg-background text-on-surface font-sans">
        <MaterialSymbols />
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}

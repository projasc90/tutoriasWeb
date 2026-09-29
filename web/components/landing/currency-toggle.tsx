"use client";

import type { ReactNode } from "react";

import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";

const NAV_ITEMS = [
  { label: "Buscar Tutores", href: "#catalogo" },
  { label: "Cómo Funciona", href: "#como-funciona" },
  { label: "Materias Populares", href: "#disciplinas" },
  { label: "Para Universidades", href: "#" },
  { label: "Conviértete en Tutor", href: "/postular" },
];

export function SiteHeader() {
  const { state, logout } = useAuth();

  return (
    <header className="sticky top-0 z-50 w-full border-b border-outline-variant/60 bg-background/80 backdrop-blur-md">
      <div className="mx-auto flex max-w-[1400px] items-center justify-between gap-6 px-6 py-4 lg:px-12">
        <Logo />
        <nav
          aria-label="Navegación principal"
          className="hidden items-center gap-6 xl:flex"
        >
          {NAV_ITEMS.map((item) => (
            <a
              key={item.label}
              href={item.href}
              className="whitespace-nowrap text-on-surface-variant hover:text-on-surface transition-colors text-sm font-semibold tracking-tight"
            >
              {item.label}
            </a>
          ))}
        </nav>
        <div className="flex items-center gap-3">
          <CurrencyToggle />
          {state.status === "authenticated" ? (
            <>
              <span className="hidden items-center gap-1.5 whitespace-nowrap sm:inline-flex items-center px-3 py-2 text-on-surface-variant text-sm font-semibold">
                <MaterialIcon name="account_circle" className="text-[20px] text-primary" />
                Hola, {state.session.fullName.split(" ")[0]}
              </span>
              <button
                type="button"
                onClick={logout}
                className="hidden whitespace-nowrap sm:inline-flex items-center px-4 py-2 text-on-surface-variant hover:text-on-surface transition-colors text-sm font-semibold"
              >
                Cerrar sesión
              </button>
            </>
          ) : state.status === "anonymous" ? (
            <a
              href="/login"
              className="hidden whitespace-nowrap sm:inline-flex items-center px-4 py-2 text-on-surface-variant hover:text-on-surface transition-colors text-sm font-semibold"
            >
              Iniciar Sesión
            </a>
          ) : null}
          <a
            href="#catalogo"
            className="inline-flex items-center gap-1.5 whitespace-nowrap rounded-lg bg-primary px-4 py-2 text-on-primary text-sm font-semibold hover:bg-on-primary-fixed-variant transition-colors shadow-sm"
          >
            Reservar un Tutor
          </a>
        </div>
      </div>
    </header>
  );
}

function Logo() {
  return (
    <a href="#" className="flex items-center gap-2.5">
      <span
        aria-hidden
        className="grid h-8 w-8 place-items-center rounded-md bg-gradient-to-br from-primary to-primary-container text-on-primary text-lg font-extrabold leading-none"
      >
        A
      </span>
      <span className="text-headline-sm font-bold tracking-tight text-primary">
        AuraLearn
      </span>
    </a>
  );
}

function CurrencyToggle() {
  return (
    <div
      role="group"
      aria-label="Cambiar moneda"
      className="flex items-center gap-1 rounded-lg bg-surface-container-low p-1"
    >
      <button
        type="button"
        aria-pressed="true"
        className="rounded bg-surface-container-lowest px-2.5 py-1 text-on-surface text-xs font-semibold shadow-sm"
      >
        CRC ₡
      </button>
      <button
        type="button"
        aria-pressed="false"
        className="rounded px-2.5 py-1 text-on-surface-variant hover:text-on-surface transition-colors text-xs font-semibold"
      >
        USD $
      </button>
    </div>
  );
}

type IconBadgeProps = {
  icon: ReactNode;
  children: ReactNode;
  tone?: "primary" | "subtle";
};

export function IconBadge({ icon, children, tone = "primary" }: IconBadgeProps) {
  return (
    <span
      className={
        tone === "primary"
          ? "w-12 h-12 rounded-xl bg-primary-container/15 flex items-center justify-center text-primary"
          : "w-10 h-10 rounded-lg bg-surface-container-low flex items-center justify-center text-primary"
      }
    >
      {icon}
    </span>
  );
}

export function MiniTrustBadge({ children }: { children: ReactNode }) {
  return (
    <span className="inline-flex items-center gap-1.5 text-on-surface-variant text-sm font-semibold">
      {children}
    </span>
  );
}

export function StarGlyph() {
  return <MaterialIcon name="star" className="text-base" filled />;
}

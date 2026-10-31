"use client";

/**
 * Panel admin: gestión de estudiantes (solo rol Admin). Listado paginado de
 * usuarios con búsqueda por texto y filtro por rol, contra GET /api/admin/users.
 *
 * Guard client-side con useAuth — la autorización real la aplica el backend
 * con [Authorize(Roles=Admin)]; esta página solo oculta la vista.
 */

import Link from "next/link";
import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { useAuth } from "@/hooks/use-auth";
import { useAdminUsers } from "@/hooks/use-admin-users";
import { formatLongDateEsCr } from "@/lib/time";
import type { AdminUser } from "@/lib/api";

const ROLE_LABEL: Record<AdminUser["role"], string> = {
  Estudiante: "Estudiante",
  Tutor: "Tutor",
  Admin: "Admin",
};

const ROLE_TONE: Record<AdminUser["role"], string> = {
  Estudiante: "bg-primary-container text-on-primary-container",
  Tutor: "bg-secondary-container text-on-secondary-container",
  Admin: "bg-tertiary-container text-on-tertiary-container",
};

const ROLE_FILTERS = ["Todos", "Estudiante", "Tutor", "Admin"] as const;

export default function AdminEstudiantesPage() {
  const { state: authState } = useAuth();

  const isAdmin = authState.status === "authenticated" && authState.session.role === "Admin";
  const token = authState.status === "authenticated" ? authState.session.token : "";

  const [query, setQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState<(typeof ROLE_FILTERS)[number]>("Todos");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const { state, reload } = useAdminUsers(isAdmin ? token : "", {
    query: query || undefined,
    role: roleFilter === "Todos" ? undefined : roleFilter,
    page,
    pageSize,
  });

  const data = state.status === "success" ? state.data : null;
  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / pageSize)) : 1;

  return (
    <>
      <SiteHeader />
      <main className="mx-auto w-full max-w-[1400px] flex-1 px-6 py-10 lg:px-12">
        <header className="mb-8 flex flex-col gap-2">
          <span className="inline-flex w-fit items-center gap-1.5 rounded-full bg-secondary-container px-3 py-1 text-label-md font-semibold text-on-secondary-container">
            <MaterialIcon name="admin_panel_settings" className="text-[16px]" />
            Panel de administración
          </span>
          <h1 className="text-headline-lg font-bold tracking-tight text-on-surface">
            Estudiantes
          </h1>
          <p className="text-body-md text-on-surface-variant">
            Usuarios registrados de la plataforma, con filtro por rol y búsqueda por nombre o correo.
          </p>
        </header>

        {!isAdmin && authState.status !== "loading" ? (
          <div className="flex flex-col items-center justify-center rounded-xl border border-error/30 bg-error-container/20 py-20 text-center">
            <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-error-container">
              <MaterialIcon name="lock" className="text-[28px] text-on-error-container" />
            </div>
            <h2 className="mb-2 text-title-md font-bold text-on-surface">Acceso restringido</h2>
            <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">
              Este panel es solo para administradores. Inicia sesión con una cuenta Admin.
            </p>
            <Link
              href="/login"
              className="rounded-lg bg-primary px-6 py-2.5 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
            >
              Iniciar sesión
            </Link>
          </div>
        ) : isAdmin ? (
          <>
            {/* Búsqueda + filtro de rol */}
            <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="relative flex-1">
                <MaterialIcon
                  name="search"
                  className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-[20px] text-on-surface-variant"
                />
                <input
                  type="search"
                  value={query}
                  onChange={(e) => {
                    setQuery(e.target.value);
                    setPage(1);
                  }}
                  placeholder="Buscar por nombre o correo…"
                  aria-label="Buscar usuarios por nombre o correo"
                  className="w-full rounded-lg border border-outline-variant bg-surface-container-lowest py-2.5 pl-10 pr-3.5 text-sm text-on-surface placeholder:text-outline transition-colors focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
                />
              </div>
              <div
                role="group"
                aria-label="Filtrar por rol"
                className="flex items-center gap-1 rounded-lg bg-surface-container-low p-1"
              >
                {ROLE_FILTERS.map((r) => (
                  <button
                    key={r}
                    type="button"
                    aria-pressed={roleFilter === r}
                    onClick={() => {
                      setRoleFilter(r);
                      setPage(1);
                    }}
                    className={
                      roleFilter === r
                        ? "rounded bg-surface-container-lowest px-3 py-1.5 text-label-md font-semibold text-on-surface shadow-sm"
                        : "rounded px-3 py-1.5 text-label-md font-semibold text-on-surface-variant transition-colors hover:text-on-surface"
                    }
                  >
                    {r}
                  </button>
                ))}
              </div>
            </div>

            {state.status === "loading" ? (
              <div className="flex flex-col gap-3" aria-busy="true">
                {Array.from({ length: 5 }).map((_, i) => (
                  <div key={i} className="h-16 animate-pulse rounded-xl bg-surface-container-lowest shadow-sm" />
                ))}
              </div>
            ) : state.status === "error" ? (
              <div className="flex flex-col items-center justify-center rounded-xl border border-error/30 bg-error-container/20 py-16 text-center">
                <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-error-container">
                  <MaterialIcon name="cloud_off" className="text-[28px] text-on-error-container" />
                </div>
                <h2 className="mb-2 text-title-md font-bold text-on-surface">No se pudo cargar el listado</h2>
                <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">{state.error}</p>
                <button
                  type="button"
                  onClick={reload}
                  className="rounded-lg bg-primary px-6 py-2.5 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
                >
                  Reintentar
                </button>
              </div>
            ) : data && data.items.length === 0 ? (
              <div className="flex flex-col items-center justify-center rounded-xl bg-surface-container-lowest py-16 text-center shadow-sm">
                <MaterialIcon name="person_search" className="text-[40px] text-on-surface-variant" />
                <p className="mt-2 text-body-md text-on-surface-variant">
                  No hay usuarios que coincidan con la búsqueda.
                </p>
              </div>
            ) : data ? (
              <>
                <p className="mb-3 text-label-md text-on-surface-variant" aria-live="polite">
                  {data.totalCount} usuario{data.totalCount === 1 ? "" : "s"} · página {data.page} de {totalPages}
                </p>
                <ul className="flex flex-col gap-3">
                  {data.items.map((u) => (
                    <li
                      key={u.id}
                      className="flex flex-wrap items-center gap-3 rounded-xl bg-surface-container-lowest p-4 shadow-sm"
                    >
                      <div className="grid h-10 w-10 shrink-0 place-items-center rounded-full bg-primary-container text-label-md font-bold text-on-primary-container">
                        {initialsOf(u.fullName)}
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-title-md font-bold text-on-surface">{u.fullName}</p>
                        <p className="truncate text-label-md text-on-surface-variant">{u.email}</p>
                      </div>
                      <span className={`rounded-full px-2.5 py-0.5 text-label-sm font-semibold ${ROLE_TONE[u.role]}`}>
                        {ROLE_LABEL[u.role]}
                      </span>
                      <span className="text-label-sm text-on-surface-variant">
                        Registrado {formatLongDateEsCr(u.createdAt)}
                      </span>
                    </li>
                  ))}
                </ul>

                {/* Paginación simple */}
                {totalPages > 1 && (
                  <nav
                    aria-label="Paginación de usuarios"
                    className="mt-6 flex items-center justify-center gap-2"
                  >
                    <button
                      type="button"
                      disabled={page <= 1}
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      className="rounded-lg border border-outline px-3 py-1.5 text-label-md font-semibold text-on-surface disabled:opacity-40"
                    >
                      Anterior
                    </button>
                    <span className="text-label-md text-on-surface-variant">
                      {page} / {totalPages}
                    </span>
                    <button
                      type="button"
                      disabled={page >= totalPages}
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      className="rounded-lg border border-outline px-3 py-1.5 text-label-md font-semibold text-on-surface disabled:opacity-40"
                    >
                      Siguiente
                    </button>
                  </nav>
                )}
              </>
            ) : null}
          </>
        ) : null}
      </main>
    </>
  );
}

function initialsOf(fullName: string): string {
  return fullName
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
}

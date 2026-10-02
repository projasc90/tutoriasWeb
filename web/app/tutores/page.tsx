"use client";

import { useState } from "react";
import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { useTutors } from "@/hooks/use-tutors";
import { AVAILABILITIES, DEFAULT_FILTERS, LEVELS, RATINGS, UNIVERSITIES } from "@/lib/data/tutors";
import { applySort } from "@/lib/filters";
import { formatSlotEsCr } from "@/lib/time";
import type { FilterState, SortOption, Tutor } from "@/lib/types/tutor";

export default function TutoresPage() {
  const [filters, setFilters] = useState<FilterState>(DEFAULT_FILTERS);
  const [sort, setSort] = useState<SortOption>("rating");

  // Datos reales del backend (GET /api/tutors) con debounce y abort automático.
  const { state, reload } = useTutors({
    query: filters.query || undefined,
    university: filters.university,
    minRating: filters.minRating === "Cualquiera" ? undefined : parseFloat(filters.minRating),
    priceMin: filters.priceMin > 0 ? filters.priceMin : undefined,
    priceMax: filters.priceMax < 25000 ? filters.priceMax : undefined,
    page: 1,
    pageSize: 12,
  });

  // El orden es client-side sobre la página actual (contrato API sin cambios).
  const tutors = state.status === "success" ? applySort(state.data.items, sort) : [];
  const totalCount = state.status === "success" ? state.data.totalCount : 0;
  const isLoading = state.status === "loading";
  const isError = state.status === "error";
  const isEmpty = state.status === "success" && tutors.length === 0;

  return (
    <>
      <SiteHeader />
      <main className="flex flex-1 flex-col bg-background min-h-screen">
        {/* Page Hero */}
        <div className="bg-surface-container-low border-b border-outline-variant/40">
          <div className="mx-auto max-w-[1400px] px-6 py-10 lg:px-12">
            <div className="mb-6 flex flex-col gap-2">
              <h1 className="text-headline-lg font-bold text-on-surface tracking-tight">
                Directorio de Tutores
              </h1>
              <p className="text-body-md text-on-surface-variant">
                {isLoading
                  ? "Buscando tutores verificados..."
                  : `${totalCount} tutores verificados disponibles para reserva inmediata`}
              </p>
            </div>
            {/* Search Bar */}
            <div className="relative max-w-2xl">
              <MaterialIcon name="search" className="absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant text-[20px] pointer-events-none" />
              <input
                type="text"
                placeholder="Buscar por materia, nombre o tema..."
                value={filters.query}
                onChange={(e) => setFilters({ ...filters, query: e.target.value })}
                className="w-full rounded-xl bg-surface-container-lowest pl-12 pr-4 py-3 text-on-surface placeholder:text-outline focus:bg-surface-container-lowest focus:outline-none focus:ring-2 focus:ring-primary transition-all text-sm shadow-sm"
              />
            </div>
          </div>
        </div>

        <div className="mx-auto flex max-w-[1400px] flex-1 flex-col gap-6 px-6 py-8 lg:px-12 lg:flex-row">
          {/* Filters Sidebar */}
          <aside className="w-full lg:w-72 shrink-0">
            <FilterSidebar filters={filters} onChange={setFilters} />
          </aside>

          {/* Results */}
          <div className="flex-1">
            {/* Sort Bar */}
            <div className="mb-6 flex items-center justify-between gap-4">
              <p className="text-body-md text-on-surface-variant">
                <span className="font-bold text-on-surface">{totalCount}</span> resultados
              </p>
              <div className="flex items-center gap-2">
                <label className="whitespace-nowrap text-label-md text-on-surface-variant">Ordenar por:</label>
                <select
                  value={sort}
                  onChange={(e) => setSort(e.target.value as SortOption)}
                  className="rounded-lg bg-surface-container-low px-3 py-2 text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                >
                  <option value="rating">Mayor valoración</option>
                  <option value="reviews">Más evaluaciones</option>
                  <option value="price_asc">Precio: menor primero</option>
                  <option value="price_desc">Precio: mayor primero</option>
                </select>
              </div>
            </div>

            {/* Tutor Grid */}
            {isError ? (
              <ErrorState error={state.error} onRetry={reload} />
            ) : isLoading ? (
              <LoadingGrid />
            ) : isEmpty ? (
              <EmptyState onClear={() => setFilters(DEFAULT_FILTERS)} />
            ) : (
              <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
                {tutors.map((tutor) => (
                  <TutorCard key={tutor.id} tutor={tutor} />
                ))}
              </div>
            )}
          </div>
        </div>
      </main>
      <footer className="bg-surface-container-lowest border-t border-outline-variant/40 py-8">
        <div className="mx-auto flex max-w-[1400px] flex-col items-center justify-between gap-4 px-6 lg:px-12 sm:flex-row">
          <p className="text-label-md text-on-surface-variant">
            © {new Date().getFullYear()} AuraLearn · Todos los derechos reservados
          </p>
          <div className="flex items-center gap-6">
            <a href="#" className="text-label-md text-on-surface-variant hover:text-on-surface transition-colors">Términos</a>
            <a href="#" className="text-label-md text-on-surface-variant hover:text-on-surface transition-colors">Privacidad</a>
            <a href="#" className="text-label-md text-on-surface-variant hover:text-on-surface transition-colors">Ayuda</a>
          </div>
        </div>
      </footer>
    </>
  );
}

function FilterSidebar({
  filters,
  onChange,
}: {
  filters: FilterState;
  onChange: (f: FilterState) => void;
}) {
  return (
    <div className="flex flex-col gap-6 rounded-xl bg-surface-container-lowest p-5 shadow-sm">
      <div>
        <h3 className="mb-3 text-title-md font-bold text-on-surface">Universidad</h3>
        <div className="flex flex-col gap-2">
          {UNIVERSITIES.map((uni) => (
            <label key={uni} className="flex cursor-pointer items-center gap-2.5">
              <input
                type="radio"
                name="university"
                value={uni}
                checked={filters.university === uni}
                onChange={() => onChange({ ...filters, university: uni })}
                className="h-4 w-4 accent-primary"
              />
              <span className="text-body-md text-on-surface">{uni}</span>
            </label>
          ))}
        </div>
      </div>

      <div className="border-t border-outline-variant/50" />

      <div>
        <h3 className="mb-3 text-title-md font-bold text-on-surface">Nivel Académico</h3>
        <div className="flex flex-col gap-2">
          {LEVELS.map((lvl) => (
            <label key={lvl} className="flex cursor-pointer items-center gap-2.5">
              <input
                type="radio"
                name="level"
                value={lvl}
                checked={filters.level === lvl}
                onChange={() => onChange({ ...filters, level: lvl })}
                className="h-4 w-4 accent-primary"
              />
              <span className="text-body-md text-on-surface">{lvl}</span>
            </label>
          ))}
        </div>
      </div>

      <div className="border-t border-outline-variant/50" />

      <div>
        <h3 className="mb-3 text-title-md font-bold text-on-surface">Valoración mínima</h3>
        <div className="flex flex-col gap-2">
          {RATINGS.map((r) => (
            <label key={r} className="flex cursor-pointer items-center gap-2.5">
              <input
                type="radio"
                name="rating"
                value={r}
                checked={filters.minRating === r}
                onChange={() => onChange({ ...filters, minRating: r })}
                className="h-4 w-4 accent-primary"
              />
              <span className="text-body-md text-on-surface">
                {r === "Cualquiera" ? r : `${r} ★`}
              </span>
            </label>
          ))}
        </div>
      </div>

      <div className="border-t border-outline-variant/50" />

      <div>
        <h3 className="mb-3 text-title-md font-bold text-on-surface">Rango de precio (CRC)</h3>
        <div className="flex items-center gap-2">
          <input
            type="number"
            min={0}
            max={25000}
            step={1000}
            value={filters.priceMin}
            onChange={(e) => onChange({ ...filters, priceMin: Number(e.target.value) })}
            className="w-full rounded-lg bg-surface-container-low px-3 py-2 text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            placeholder="Mín"
          />
          <span className="text-on-surface-variant">—</span>
          <input
            type="number"
            min={0}
            max={25000}
            step={1000}
            value={filters.priceMax}
            onChange={(e) => onChange({ ...filters, priceMax: Number(e.target.value) })}
            className="w-full rounded-lg bg-surface-container-low px-3 py-2 text-on-surface text-sm focus:outline-none focus:ring-2 focus:ring-primary"
            placeholder="Máx"
          />
        </div>
        <div className="mt-2 flex justify-between text-label-sm text-on-surface-variant">
          <span>₡{filters.priceMin.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")}</span>
          <span>₡{filters.priceMax.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")}</span>
        </div>
      </div>

      <div className="border-t border-outline-variant/50" />

      <div>
        <h3 className="mb-3 text-title-md font-bold text-on-surface">Disponibilidad</h3>
        <div className="flex flex-col gap-2">
          {AVAILABILITIES.map((av) => (
            <label key={av} className="flex cursor-pointer items-center gap-2.5">
              <input
                type="radio"
                name="availability"
                value={av}
                checked={filters.availability === av}
                onChange={() => onChange({ ...filters, availability: av })}
                className="h-4 w-4 accent-primary"
              />
              <span className="text-body-md text-on-surface">{av}</span>
            </label>
          ))}
        </div>
      </div>

      <button
        type="button"
        onClick={() => onChange(DEFAULT_FILTERS)}
        className="mt-2 rounded-lg border border-outline px-4 py-2 text-label-md text-on-surface-variant hover:border-primary hover:text-primary transition-colors"
      >
        Limpiar filtros
      </button>
    </div>
  );
}

function TutorCard({ tutor }: { tutor: Tutor }) {
  // Iniciales derivadas del nombre (el backend no envía initials).
  const initials = tutor.name
    .replace(/^(Dr\.|Dra\.|Ing\.|Prof\.|M\.Sc\.)\s+/, "")
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();

  return (
    <article className="group flex flex-col rounded-xl bg-surface-container-lowest shadow-sm transition-all hover:shadow-primary-card">
      <div className="flex flex-1 flex-col gap-4 p-5">
        {/* Header */}
        <div className="flex items-start gap-3.5">
          <div className="grid h-14 w-14 shrink-0 place-items-center rounded-xl bg-gradient-to-br from-primary to-primary-container font-bold text-lg text-on-primary">
            {initials}
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex items-start justify-between gap-1">
              <h3 className="truncate text-title-md font-bold text-on-surface">
                {tutor.name}
              </h3>
              {tutor.featured && (
                <span className="shrink-0 rounded bg-primary-fixed px-1.5 py-0.5 text-[10px] font-bold text-primary">
                  Destacado
                </span>
              )}
            </div>
            <p className="text-primary text-label-md font-semibold">{tutor.credentials}</p>
            <p className="text-label-sm text-on-surface-variant">{tutor.university}</p>
            <div className="mt-1 flex items-center gap-1">
              <MaterialIcon name="star" filled className="text-tertiary text-[14px]" />
              <span className="text-xs font-bold text-on-surface">{tutor.rating.toFixed(2)}</span>
              <span className="text-label-sm text-on-surface-variant">({tutor.reviews} clases)</span>
            </div>
          </div>
        </div>

        {/* Bio */}
        <p className="line-clamp-2 text-body-md leading-relaxed text-on-surface-variant">
          {tutor.bio}
        </p>

        {/* Subjects */}
        <div className="flex flex-wrap gap-1.5">
          {tutor.subjects.slice(0, 4).map((s) => (
            <span key={s} className="rounded bg-surface-container-low px-2 py-0.5 text-label-sm text-on-surface-variant">
              {s}
            </span>
          ))}
          {tutor.subjects.length > 4 && (
            <span className="rounded bg-surface-container-low px-2 py-0.5 text-label-sm text-on-surface-variant">
              +{tutor.subjects.length - 4}
            </span>
          )}
        </div>

        {/* Next Slot */}
        <div className="flex items-center justify-between rounded-lg bg-surface-container-low p-2.5">
          <span className="text-label-sm text-on-surface-variant">Próximo cupo:</span>
          <span className="text-label-sm font-bold text-primary">
            {tutor.nextSlotAt ? formatSlotEsCr(tutor.nextSlotAt) : "Sin cupos"}
          </span>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-outline-variant/40 p-4">
        <div>
          <span className="text-headline-sm font-extrabold text-on-surface">₡{tutor.priceCrc.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")}</span>
          <span className="block text-label-sm text-on-surface-variant">/ hora (~${tutor.priceUsd})</span>
        </div>
        <Link
          href={`/tutores/${tutor.id}`}
          className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-semibold shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          Ver Perfil
        </Link>
      </div>
    </article>
  );
}

function EmptyState({ onClear }: { onClear: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-outline-variant/50 bg-surface-container-low py-20 text-center">
      <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-surface-container">
        <MaterialIcon name="search" className="text-[28px] text-on-surface-variant" />
      </div>
      <h3 className="mb-2 text-title-md font-bold text-on-surface">Sin resultados</h3>
      <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">
        Ajusta los filtros para encontrar más tutores disponibles.
      </p>
      <button
        type="button"
        onClick={onClear}
        className="rounded-lg bg-primary px-6 py-2.5 text-on-primary text-label-md font-semibold shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
      >
        Limpiar todos los filtros
      </button>
    </div>
  );
}

function LoadingGrid() {
  return (
    <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3" aria-busy="true" aria-live="polite">
      {Array.from({ length: 6 }).map((_, i) => (
        <div
          key={i}
          className="flex flex-col gap-4 rounded-xl bg-surface-container-lowest p-5 shadow-sm"
        >
          <div className="flex items-start gap-3.5">
            <div className="h-14 w-14 shrink-0 animate-pulse rounded-xl bg-surface-container" />
            <div className="flex-1 space-y-2">
              <div className="h-4 w-3/4 animate-pulse rounded bg-surface-container" />
              <div className="h-3 w-1/2 animate-pulse rounded bg-surface-container" />
            </div>
          </div>
          <div className="h-3 w-full animate-pulse rounded bg-surface-container" />
          <div className="h-3 w-5/6 animate-pulse rounded bg-surface-container" />
          <div className="flex gap-1.5">
            <div className="h-5 w-20 animate-pulse rounded bg-surface-container" />
            <div className="h-5 w-16 animate-pulse rounded bg-surface-container" />
          </div>
        </div>
      ))}
    </div>
  );
}

function ErrorState({ error, onRetry }: { error: string; onRetry: () => void }) {
  return (
    <div className="flex flex-col items-center justify-center rounded-xl border border-error/30 bg-error-container/20 py-20 text-center">
      <div className="mb-4 grid h-16 w-16 place-items-center rounded-full bg-error-container">
        <MaterialIcon name="cloud_off" className="text-[28px] text-on-error-container" />
      </div>
      <h3 className="mb-2 text-title-md font-bold text-on-surface">No se pudo cargar el directorio</h3>
      <p className="mb-6 max-w-xs text-body-md text-on-surface-variant">{error}</p>
      <button
        type="button"
        onClick={onRetry}
        className="rounded-lg bg-primary px-6 py-2.5 text-on-primary text-label-md font-semibold shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
      >
        Reintentar
      </button>
    </div>
  );
}

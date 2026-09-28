"use client";

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { SiteHeader } from "@/components/landing/currency-toggle";

const UNIVERSITIES = ["Todas", "UCR", "TEC", "UNA", "LEAD", "ULACIT", "U Latina"];
const LEVELS = ["Todos", "Grado", "Bachillerato Internacional", "Examen de Admisión", "Posgrado"];
const AVAILABILITIES = ["Cualquiera", "Hoy", "Esta semana", "Fines de semana"];
const RATINGS = ["Cualquiera", "4.5+", "4.0+", "3.5+"];

type FilterState = {
  query: string;
  university: string;
  level: string;
  availability: string;
  minRating: string;
  priceMin: number;
  priceMax: number;
};

const DEFAULT_FILTERS: FilterState = {
  query: "",
  university: "Todas",
  level: "Todos",
  availability: "Cualquiera",
  minRating: "Cualquiera",
  priceMin: 0,
  priceMax: 25000,
};

type Tutor = {
  id: number;
  initials: string;
  name: string;
  credentials: string;
  university: string;
  rating: number;
  reviews: number;
  subjects: string[];
  price: number;
  priceUsd: number;
  nextSlot: string;
  featured: boolean;
  bio: string;
  avatarTone: string;
};

const TUTORS: Tutor[] = [
  { id: 1, initials: "CS", name: "Dr. Carlos Solano", credentials: "PhD Matemáticas", university: "UCR", rating: 4.98, reviews: 184, subjects: ["Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs"], price: 14500, priceUsd: 28, nextSlot: "Hoy, 3:30 PM", featured: true, bio: "8 años de experiencia en tutoría universitaria. Metodología orientada a resultados.", avatarTone: "from-primary to-primary-container" },
  { id: 2, initials: "SH", name: "Ing. Sofía Hernández", credentials: "M.Sc. Computación", university: "TEC", rating: 5.0, reviews: 92, subjects: ["Python", "Algoritmos", "Estructuras de Datos", "IA"], price: 16000, priceUsd: 31, nextSlot: "Mañana, 10:00 AM", featured: true, bio: "Especialista en preparación técnica para entrevistas en big tech.", avatarTone: "from-tertiary-container to-tertiary" },
  { id: 3, initials: "DM", name: "Prof. David Morales", credentials: "M.Sc. Física", university: "UCR", rating: 4.9, reviews: 115, subjects: ["Física I", "Física II", "Termodinámica", "Mecánica"], price: 12000, priceUsd: 23, nextSlot: "Jueves, 5:00 PM", featured: false, bio: "Profesor agregado UCR. Enfoque práctico con más de 500 estudiantes ayudados.", avatarTone: "from-secondary-container to-secondary" },
  { id: 4, initials: "MV", name: "Dra. Marcela Vargas", credentials: "PhD Química", university: "UNA", rating: 4.95, reviews: 140, subjects: ["Química Orgánica", "Bioquímica", "Farmacología"], price: 15000, priceUsd: 29, nextSlot: "Viernes, 2:00 PM", featured: false, bio: "Investigadora postdoctoral. Especialista en química orgánica para carreras de salud.", avatarTone: "from-tertiary to-tertiary-container" },
  { id: 5, initials: "JR", name: "M.Sc. Javier Rodríguez", credentials: "M.Sc. Economía", university: "TEC", rating: 4.85, reviews: 78, subjects: ["Econometría", "Microeconomía", "Macroeconomía", "Finanzas"], price: 13500, priceUsd: 26, nextSlot: "Mañana, 9:00 AM", featured: false, bio: "Economista senior con experiencia en organismos internacionales.", avatarTone: "from-secondary-fixed to-secondary-fixed-dim" },
  { id: 6, initials: "LP", name: "Dra. Laura Prop", credentials: "PhD Biología", university: "UCR", rating: 4.92, reviews: 103, subjects: ["Biología Celular", "Genética", "Microbiología"], price: 14000, priceUsd: 27, nextSlot: "Hoy, 6:00 PM", featured: false, bio: "Docente-investigadora con énfasis en biología molecular y genética.", avatarTone: "from-primary-fixed to-primary-fixed-dim" },
  { id: 7, initials: "AG", name: "Ing. Andrés González", credentials: "M.Sc. Ing. Eléctrica", university: "TEC", rating: 4.78, reviews: 56, subjects: ["Circuitos", "Electrónica", "Señales", "Control"], price: 13000, priceUsd: 25, nextSlot: "Miércoles, 4:00 PM", featured: false, bio: "Ingeniero electricista con Maestría en TEC. 6 años de experiencia.", avatarTone: "from-primary-container to-primary" },
  { id: 8, initials: "CM", name: "Dra. Carolina Morales", credentials: "PhD Estadística", university: "UCR", rating: 4.88, reviews: 67, subjects: ["Estadística", "Probabilidad", "R", "Análisis de Datos"], price: 15500, priceUsd: 30, nextSlot: "Sábado, 11:00 AM", featured: false, bio: "Profesora jubilada UCR. 20 años en estadística aplicada.", avatarTone: "from-tertiary-fixed-dim to-tertiary-fixed" },
  { id: 9, initials: "RF", name: "M.Sc. Ricardo Fernández", credentials: "M.Sc. Matemáticas", university: "UNA", rating: 4.97, reviews: 201, subjects: ["Cálculo III", "Variable Compleja", "Topología"], price: 12500, priceUsd: 24, nextSlot: "Hoy, 8:00 PM", featured: true, bio: "Matemático puro. Doctorado en vías. Dominio completo del cálculo avanzado.", avatarTone: "from-secondary to-secondary-container" },
  { id: 10, initials: "MP", name: "M.Sc. María Pérez", credentials: "M.Sc. Lingüística", university: "UCR", rating: 4.93, reviews: 88, subjects: ["Inglés Académico", "TOEFL", "Redacción", "Español"], price: 11000, priceUsd: 21, nextSlot: "Domingo, 10:00 AM", featured: false, bio: "Preparadora certificada TOEFL. Metodología inmersiva con materiales auténticos.", avatarTone: "from-primary-fixed-dim to-primary-fixed" },
  { id: 11, initials: "JT", name: "Dr. Jorge Torres", credentials: "PhD Física Médica", university: "TEC", rating: 4.81, reviews: 44, subjects: ["Física Médica", "Radiología", "Protección Radiológica"], price: 17000, priceUsd: 33, nextSlot: "Viernes, 9:00 AM", featured: false, bio: "Físico médico hospitalario. Prepara para exámenes de boards profesionales.", avatarTone: "from-tertiary-container to-tertiary" },
  { id: 12, initials: "AS", name: "Ing. Ana Salas", credentials: "Ing. Civil", university: "TEC", rating: 4.76, reviews: 39, subjects: ["Estática", "Resistencia", "Hormigón", "Diseño Estructural"], price: 14000, priceUsd: 27, nextSlot: "Jueves, 3:00 PM", featured: false, bio: "Ingeniera civil con maestría en estructuras. Experiencia en proyecto y supervisión.", avatarTone: "from-secondary-container to-secondary-fixed-dim" },
];

type SortOption = "rating" | "price_asc" | "price_desc" | "reviews";

export default function TutoresPage() {
  const [filters, setFilters] = useState<FilterState>(DEFAULT_FILTERS);
  const [sort, setSort] = useState<SortOption>("rating");

  const filtered = TUTORS.filter((t) => {
    if (filters.query && !t.name.toLowerCase().includes(filters.query.toLowerCase()) && !t.subjects.some((s) => s.toLowerCase().includes(filters.query.toLowerCase()))) return false;
    if (filters.university !== "Todas" && t.university !== filters.university) return false;
    if (filters.minRating !== "Cualquiera") {
      const min = parseFloat(filters.minRating);
      if (t.rating < min) return false;
    }
    if (filters.priceMin > 0 && t.price < filters.priceMin) return false;
    if (filters.priceMax < 25000 && t.price > filters.priceMax) return false;
    return true;
  }).sort((a, b) => {
    if (sort === "rating") return b.rating - a.rating;
    if (sort === "price_asc") return a.price - b.price;
    if (sort === "price_desc") return b.price - a.price;
    if (sort === "reviews") return b.reviews - a.reviews;
    return 0;
  });

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
                {filtered.length} tutores verificados disponibles para reserva inmediata
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
                <span className="font-bold text-on-surface">{filtered.length}</span> resultados
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
            {filtered.length > 0 ? (
              <div className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
                {filtered.map((tutor) => (
                  <TutorCard key={tutor.id} tutor={tutor} />
                ))}
              </div>
            ) : (
              <EmptyState onClear={() => setFilters(DEFAULT_FILTERS)} />
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
  return (
    <article className="group flex flex-col rounded-xl bg-surface-container-lowest shadow-sm transition-all hover:shadow-primary-card">
      <div className="flex flex-1 flex-col gap-4 p-5">
        {/* Header */}
        <div className="flex items-start gap-3.5">
          <div className={`grid h-14 w-14 shrink-0 place-items-center rounded-xl bg-gradient-to-br ${tutor.avatarTone} font-bold text-lg text-on-primary`}>
            {tutor.initials}
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
          <span className="text-label-sm font-bold text-primary">{tutor.nextSlot}</span>
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-outline-variant/40 p-4">
        <div>
          <span className="text-headline-sm font-extrabold text-on-surface">₡{tutor.price.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",")}</span>
          <span className="block text-label-sm text-on-surface-variant">/ hora (~${tutor.priceUsd})</span>
        </div>
        <button
          type="button"
          className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-semibold shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          Ver Perfil
        </button>
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

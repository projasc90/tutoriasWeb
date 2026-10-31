"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";
import { fetchTutors, tutorPhotoUrl, type Tutor } from "@/lib/api";
import { formatCrc, formatSlotEsCr } from "@/lib/time";
import { SectionEyebrow, SectionTitle } from "./section-eyebrow";

/**
 * Tutores destacados de la landing: datos reales del catálogo público
 * (GET /api/tutors, sin login). Client component para que el fetch ocurra
 * en runtime y la landing prerenderizada nunca muestre datos congelados.
 * Reemplaza el mock MENTORS (archivado en .ai/archive/mentors-mock.ts).
 */

const FEATURED_COUNT = 4;

export function Mentors() {
  const [state, setState] = useState<
    | { status: "loading" }
    | { status: "error" }
    | { status: "success"; tutors: Tutor[]; total: number }
  >({ status: "loading" });

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      const result = await fetchTutors({ page: 1, pageSize: FEATURED_COUNT });
      if (cancelled) return;
      if (result.ok) {
        setState({ status: "success", tutors: result.data.items, total: result.data.totalCount });
      } else {
        setState({ status: "error" });
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  if (state.status === "loading") {
    return (
      <section className="w-full bg-surface py-20">
        <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
          <div className="mb-12">
            <SectionEyebrow>Docentes Destacados</SectionEyebrow>
            <SectionTitle className="mt-1">
              Conoce a algunos de nuestros mentores líderes
            </SectionTitle>
          </div>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4" aria-busy="true">
            {Array.from({ length: FEATURED_COUNT }).map((_, i) => (
              <div key={i} className="h-64 animate-pulse rounded-xl bg-surface-container-lowest shadow-sm" />
            ))}
          </div>
        </div>
      </section>
    );
  }

  if (state.status === "error") {
    return (
      <section className="w-full bg-surface py-20">
        <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
          <div className="mb-12">
            <SectionEyebrow>Docentes Destacados</SectionEyebrow>
            <SectionTitle className="mt-1">
              Conoce a algunos de nuestros mentores líderes
            </SectionTitle>
          </div>
          <div className="rounded-xl bg-surface-container-lowest p-10 text-center shadow-sm">
            <MaterialIcon name="cloud_off" className="text-[36px] text-on-surface-variant" />
            <p className="mt-2 text-body-md text-on-surface-variant">
              No pudimos cargar los tutores destacados.
            </p>
            <Link
              href="/tutores"
              className="mt-4 inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-label-md font-bold text-on-primary hover:bg-on-primary-fixed-variant"
            >
              Ver el catálogo completo
            </Link>
          </div>
        </div>
      </section>
    );
  }

  const { tutors, total } = state;

  return (
    <section className="w-full bg-surface py-20">
      <div className="mx-auto max-w-[1400px] px-6 lg:px-12">
        <div className="mb-12 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <SectionEyebrow>Docentes Destacados</SectionEyebrow>
            <SectionTitle className="mt-1">
              Conoce a algunos de nuestros mentores líderes
            </SectionTitle>
          </div>
          <span className="text-on-surface-variant text-label-md">
            {total > 0
              ? `${total} tutores verificados en Centroamérica`
              : "Tutores verificados en Centroamérica"}
          </span>
        </div>
        {tutors.length === 0 ? (
          <div className="rounded-xl bg-surface-container-lowest p-10 text-center shadow-sm">
            <MaterialIcon name="person_search" className="text-[36px] text-on-surface-variant" />
            <p className="mt-2 text-body-md text-on-surface-variant">
              Aún no hay tutores publicados. Vuelve pronto.
            </p>
          </div>
        ) : (
          <div
            id="catalogo"
            className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4"
          >
            {tutors.map((t, i) => (
              <MentorCard key={t.id} tutor={t} index={i} />
            ))}
          </div>
        )}
      </div>
    </section>
  );
}

const AVATAR_GRADIENTS = [
  "from-primary to-primary-container",
  "from-secondary to-secondary-container",
  "from-tertiary to-tertiary-container",
  "from-primary-container to-secondary-container",
];

function MentorCard({ tutor, index }: { tutor: Tutor; index: number }) {
  const initials = initialsOf(tutor.name);
  const badges = tutor.subjects.slice(0, 3);

  return (
    <article className="flex flex-col justify-between rounded-xl bg-surface-container-lowest p-5 shadow-sm transition-all hover:shadow-primary-card">
      <div>
        <div className="mb-4 flex items-start gap-3.5">
          <div className="relative">
            {tutor.photoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={tutorPhotoUrl(tutor.id)}
                alt={`Foto de ${tutor.name}`}
                className="h-16 w-16 rounded-xl object-cover"
              />
            ) : (
              <div
                className={`grid h-16 w-16 place-items-center rounded-xl bg-gradient-to-br ${
                  AVATAR_GRADIENTS[index % AVATAR_GRADIENTS.length]
                } text-on-primary font-bold text-lg`}
              >
                {initials}
              </div>
            )}
            <span className="absolute -bottom-1 -right-1 h-3.5 w-3.5 rounded-full border-2 border-surface-container-lowest bg-primary-container" />
          </div>
          <div className="min-w-0 flex-1">
            <h3 className="truncate text-title-md font-bold text-on-surface">
              {tutor.name}
            </h3>
            <p className="text-primary text-label-md font-semibold">
              {tutor.credentials}
            </p>
            <div className="mt-1 flex items-center gap-1">
              <MaterialIcon
                name="star"
                filled
                className="text-tertiary text-[15px]"
              />
              <span className="text-xs font-bold text-on-surface">
                {tutor.rating.toFixed(2)}
              </span>
              <span className="text-label-sm text-on-surface-variant">
                ({tutor.reviews} clases)
              </span>
            </div>
          </div>
        </div>
        <div className="mb-4 flex flex-wrap gap-1.5">
          {badges.map((badge) => (
            <span
              key={badge}
              className="rounded bg-surface-container-low px-2 py-0.5 text-label-sm text-on-surface-variant"
            >
              {badge}
            </span>
          ))}
        </div>
        <div className="mb-4 flex items-center justify-between rounded-lg bg-surface-container-low p-2.5">
          <span className="text-label-sm text-on-surface-variant">
            Próximo cupo libre:
          </span>
          <span className="text-primary text-xs font-bold">
            {tutor.nextSlotAt ? formatSlotEsCr(tutor.nextSlotAt) : "Consultar"}
          </span>
        </div>
      </div>
      <div className="flex items-center justify-between pt-4">
        <div>
          <span className="text-headline-sm font-extrabold text-on-surface">
            {formatCrc(tutor.priceCrc)}
          </span>
          <span className="block text-label-sm text-on-surface-variant">
            / hora (~${tutor.priceUsd})
          </span>
        </div>
        <Link
          href={`/tutores/${tutor.id}`}
          className="rounded-lg bg-primary px-4 py-2 text-on-primary text-label-md font-semibold hover:bg-on-primary-fixed-variant transition-colors shadow-sm"
        >
          Ver Perfil
        </Link>
      </div>
    </article>
  );
}

function initialsOf(fullName: string): string {
  return fullName
    .replace(/^(Dr\.|Dra\.|Ing\.|Prof\.|M\.Sc\.)\s+/, "")
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? "")
    .join("");
}


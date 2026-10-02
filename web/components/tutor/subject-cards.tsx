"use client";

/**
 * Materias que imparte (Stitch 02): cards interactivas por materia.
 * Al seleccionar una, sincroniza el motor de reserva (la materia viaja al checkout).
 */

import { MaterialIcon } from "@/components/material-icon";
import { formatCrc } from "@/lib/time";

export function SubjectCards({
  subjects,
  priceCrc,
  priceUsd,
  selected,
  onSelect,
}: {
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
  selected: string | null;
  onSelect: (subject: string | null) => void;
}) {
  return (
    <div className="space-y-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="flex items-center gap-2 text-headline-md text-on-surface">
            <MaterialIcon name="menu_book" className="text-primary" />
            Cursos y Materias Especializadas
          </h2>
          <p className="text-body-md text-on-surface-variant">
            Selecciona el área de tu interés para reservar
          </p>
        </div>
        <span className="rounded-full bg-primary-fixed/40 px-3 py-1 text-label-sm font-bold text-primary">
          {subjects.length} Especialidades
        </span>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {subjects.map((subject, i) => {
          const isSelected = selected === subject;
          return (
            <button
              key={subject}
              type="button"
              onClick={() => onSelect(isSelected ? null : subject)}
              className={`group rounded-xl p-4 text-left transition-all ${
                isSelected
                  ? "bg-primary-fixed/30 ring-2 ring-primary"
                  : "bg-surface-container-low hover:bg-surface-container"
              }`}
            >
              <div className="mb-2 flex items-start justify-between">
                <span className="rounded bg-surface-container-lowest px-2 py-0.5 text-label-sm font-bold uppercase tracking-wider text-primary">
                  Código: M-{1000 + i * 3}
                </span>
                <span className="font-bold text-on-surface">
                  {formatCrc(priceCrc)}{" "}
                  <span className="text-[12px] font-normal text-on-surface-variant">/h</span>
                </span>
              </div>
              <h3 className={`text-headline-sm transition-colors ${isSelected ? "text-primary" : "text-on-surface group-hover:text-primary"}`}>
                {subject}
              </h3>
              <p className="mt-1 text-body-md text-on-surface-variant">
                Clases 1-a-1 en línea con pizarra colaborativa (~${priceUsd} USD por hora).
              </p>
              <div className="mt-3 flex items-center gap-2 text-label-md text-primary">
                <span>{isSelected ? "Materia seleccionada" : "Reservar esta materia"}</span>
                <MaterialIcon name="arrow_forward" className="text-[16px] transition-transform group-hover:translate-x-1" />
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}

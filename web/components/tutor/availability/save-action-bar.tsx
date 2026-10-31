"use client";

/**
 * Barra sticky de persistencia (Stitch 05): estado de sincronización +
 * "Descartar Cambios" / "Guardar". Soporta dos contextos (disponibilidad,
 * perfil) con copy distinto. El contenedor es `w-full` y respeta el ancho
 * del padre: dentro de un grid queda al ancho de la columna; a nivel de
 * página abarca el 100% del `max-w-7xl` como en el gestor de disponibilidad.
 */

import { MaterialIcon } from "@/components/material-icon";

export type SaveActionBarContext = "disponibilidad" | "perfil";

const COPY: Record<SaveActionBarContext, {
  savedTitle: string;
  dirtyTitle: string;
  syncedTitle: string;
  helpText: string;
  saveLabel: string;
}> = {
  disponibilidad: {
    savedTitle: "¡Cambios Guardados Exitosamente!",
    dirtyTitle: "Cambios sin guardar",
    syncedTitle: "Matriz de Horarios Sincronizada",
    helpText: "Los alumnos verán tus franjas actualizadas al instante con tarifas en CRC y USD.",
    saveLabel: "Guardar Cambios de Disponibilidad",
  },
  perfil: {
    savedTitle: "¡Perfil Guardado Exitosamente!",
    dirtyTitle: "Cambios sin guardar",
    syncedTitle: "Perfil sincronizado",
    helpText: "Los cambios se reflejan en tu perfil público del catálogo.",
    saveLabel: "Guardar Cambios del Perfil",
  },
};

export function SaveActionBar(props: {
  dirty: boolean;
  saving: boolean;
  saved: boolean;
  onSave: () => void;
  onDiscard: () => void;
  /** Contexto de la página: ajusta título, ayuda y label del botón. */
  context?: SaveActionBarContext;
}) {
  const { dirty, saving, saved, onSave, onDiscard, context = "disponibilidad" } = props;
  const copy = COPY[context];

  return (
    <div className="sticky bottom-6 z-40 mt-8 flex w-full flex-col items-center justify-between gap-4 rounded-2xl bg-surface-container-lowest/95 p-4 shadow-xl backdrop-blur-md sm:flex-row">
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-surface-variant text-primary">
          <MaterialIcon name={saved ? "done_all" : "verified"} className="text-[24px]" />
        </div>
        <div>
          <p className="text-label-md font-bold text-on-surface">
            {saved ? copy.savedTitle : dirty ? copy.dirtyTitle : copy.syncedTitle}
          </p>
          <p className="text-label-sm text-on-surface-variant">
            {copy.helpText}
          </p>
        </div>
      </div>
      <div className="flex w-full items-center justify-end gap-2 sm:w-auto">
        <button
          type="button"
          disabled={!dirty || saving}
          onClick={onDiscard}
          className="rounded-xl px-4 py-2 text-label-md text-on-surface-variant transition-colors hover:text-on-surface disabled:opacity-40"
        >
          Descartar Cambios
        </button>
        <button
          type="button"
          disabled={!dirty || saving}
          onClick={onSave}
          className="flex items-center gap-2 rounded-xl bg-primary px-6 py-2.5 text-label-md text-on-primary shadow-md transition-all hover:bg-primary-container disabled:opacity-50"
        >
          <MaterialIcon name={saving ? "progress_activity" : "check_circle"} className={`text-[18px] ${saving ? "animate-spin" : ""}`} />
          <span>{saving ? "Guardando en AuraLearn..." : copy.saveLabel}</span>
        </button>
      </div>
    </div>
  );
}

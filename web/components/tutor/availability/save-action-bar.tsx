"use client";

/**
 * Barra sticky de persistencia (Stitch 05): estado de sincronización +
 * "Descartar Cambios" / "Guardar Cambios de Disponibilidad".
 */

import { MaterialIcon } from "@/components/material-icon";

export function SaveActionBar(props: {
  dirty: boolean;
  saving: boolean;
  saved: boolean;
  onSave: () => void;
  onDiscard: () => void;
}) {
  const { dirty, saving, saved, onSave, onDiscard } = props;

  return (
    <div className="sticky bottom-6 z-40 mt-8 flex flex-col items-center justify-between gap-4 rounded-2xl bg-surface-container-lowest/95 p-4 shadow-xl backdrop-blur-md sm:flex-row">
      <div className="flex items-center gap-3">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-surface-variant text-primary">
          <MaterialIcon name={saved ? "done_all" : "verified"} className="text-[24px]" />
        </div>
        <div>
          <p className="text-label-md font-bold text-on-surface">
            {saved ? "¡Cambios Guardados Exitosamente!" : dirty ? "Cambios sin guardar" : "Matriz de Horarios Sincronizada"}
          </p>
          <p className="text-label-sm text-on-surface-variant">
            Los alumnos verán tus franjas actualizadas al instante con tarifas en CRC y USD.
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
          <span>{saving ? "Guardando en AuraLearn..." : "Guardar Cambios de Disponibilidad"}</span>
        </button>
      </div>
    </div>
  );
}

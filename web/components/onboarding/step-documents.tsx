"use client";

/**
 * Paso "Documentos" del wizard /postular: subida de títulos académicos,
 * certificaciones (1..5) e identificación para la verificación de credenciales.
 *
 * Reglas (espejo del backend): Títulos y Certificaciones acumulan (máx 5 c/u
 * entre lo ya subido y lo nuevo); Identificación es única (reemplaza).
 * Validación client-side espejo del backend (PDF/JPEG/PNG, ≤ 5 MB). Si el
 * usuario ya subió documentos (re-postulación), se precargan del servidor y
 * cuentan para el requisito y los cupos.
 */

import { useEffect, useRef, useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { fetchTutorDocuments } from "@/lib/api";
import { readSession } from "@/lib/auth";
import {
  DOCUMENT_ACCEPT_ATTR,
  formatDocumentSize,
  validateDocumentFile,
} from "@/lib/documents";
import type { TutorDocumentItem } from "@/lib/types/tutor-application";

export type DocumentsData = {
  titulos: File[];
  certificaciones: File[];
  identificacion: File | null;
  /** Documentos ya subidos al servidor (re-postulación): cuentan para el gate. */
  existing: TutorDocumentItem[];
};

const MAX_PER_ACCUMULATED_KIND = 5;

export function StepDocuments({
  defaultValues,
  onBack,
  onSubmit,
}: {
  defaultValues: DocumentsData;
  onBack: () => void;
  onSubmit: (data: DocumentsData) => void;
}) {
  const [titulos, setTitulos] = useState<File[]>(defaultValues.titulos);
  const [certificaciones, setCertificaciones] = useState<File[]>(defaultValues.certificaciones);
  const [identificacion, setIdentificacion] = useState<File | null>(defaultValues.identificacion);
  const [existing, setExisting] = useState<TutorDocumentItem[]>(defaultValues.existing);
  const [errors, setErrors] = useState<{
    titulos?: string;
    certificaciones?: string;
    identificacion?: string;
  }>({});

  // Precarga de documentos ya subidos (re-postulación tras rechazo).
  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      const session = readSession();
      if (!session) return;
      const result = await fetchTutorDocuments(session.token);
      if (!cancelled && result.ok) setExisting(result.data);
    };

    void load();
    return () => {
      cancelled = true;
    };
  }, []);

  const existingTitulos = existing.filter((d) => d.kind === "Titulo");
  const existingIdentificacion = existing.find((d) => d.kind === "Identificacion") ?? null;
  const existingCertificaciones = existing.filter((d) => d.kind === "Certificacion");
  const titulosSlotsLeft = MAX_PER_ACCUMULATED_KIND - (existingTitulos.length + titulos.length);
  const certSlotsLeft = MAX_PER_ACCUMULATED_KIND - (existingCertificaciones.length + certificaciones.length);

  const validate = (): boolean => {
    const next: typeof errors = {};

    if (existingTitulos.length + titulos.length < 1) {
      next.titulos = "Sube al menos tu título académico principal.";
    }
    if (existingCertificaciones.length + certificaciones.length < 1) {
      next.certificaciones = "Agrega al menos una certificación o atestado.";
    }
    if (!identificacion && !existingIdentificacion) {
      next.identificacion = "Sube tu identificación (cédula o pasaporte).";
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      onSubmit({ titulos, certificaciones, identificacion, existing });
    }
  };

  return (
    <form className="flex flex-col gap-5" onSubmit={handleSubmit}>
      <header>
        <h2 className="text-headline-sm font-bold text-on-surface">Documentos de verificación</h2>
        <p className="mt-1 text-body-md text-on-surface-variant">
          Nuestro equipo revisa estos documentos contra el registro de tu universidad.
          Formatos PDF/JPEG/PNG, hasta 5 MB por archivo.
        </p>
      </header>

      <DocumentCard
        icon="school"
        title="Títulos académicos"
        description="Diplomas o certificados de grado (bachillerato, licenciatura, maestría, doctorado)."
        error={errors.titulos}
        existingFiles={existingTitulos}
        selectedFiles={titulos}
        multiple
        onRemove={(file) => setTitulos((prev) => prev.filter((f) => f !== file))}
        onFiles={(files) => {
          const problems = files.map(validateDocumentFile).filter((p): p is string => p !== null);
          if (problems.length > 0) {
            setErrors((prev) => ({ ...prev, titulos: problems[0] }));
            return;
          }
          if (titulosSlotsLeft - files.length < 0) {
            setErrors((prev) => ({
              ...prev,
              titulos: `Máximo ${MAX_PER_ACCUMULATED_KIND} títulos en total (te quedan ${Math.max(titulosSlotsLeft, 0)}).`,
            }));
            return;
          }
          setErrors((prev) => ({ ...prev, titulos: undefined }));
          setTitulos((prev) => [...prev, ...files]);
        }}
        emptyHint={`Requerido: al menos 1 (máx. ${MAX_PER_ACCUMULATED_KIND} en total).`}
      />

      <DocumentCard
        icon="workspace_premium"
        title="Certificaciones y atestados"
        description="Atestados, colegiatura, certificados de cursos o antecedentes penales."
        error={errors.certificaciones}
        existingFiles={existingCertificaciones}
        selectedFiles={certificaciones}
        multiple
        onRemove={(file) => setCertificaciones((prev) => prev.filter((f) => f !== file))}
        onFiles={(files) => {
          const problems = files.map(validateDocumentFile).filter((p): p is string => p !== null);
          if (problems.length > 0) {
            setErrors((prev) => ({ ...prev, certificaciones: problems[0] }));
            return;
          }
          if (certSlotsLeft - files.length < 0) {
            setErrors((prev) => ({
              ...prev,
              certificaciones: `Máximo ${MAX_PER_ACCUMULATED_KIND} certificaciones en total (te quedan ${Math.max(certSlotsLeft, 0)}).`,
            }));
            return;
          }
          setErrors((prev) => ({ ...prev, certificaciones: undefined }));
          setCertificaciones((prev) => [...prev, ...files]);
        }}
        emptyHint={`Requerido: al menos 1 (máx. ${MAX_PER_ACCUMULATED_KIND} en total).`}
      />

      <DocumentCard
        icon="badge"
        title="Identificación"
        description="Cédula de identidad o pasaporte vigente (un solo archivo)."
        error={errors.identificacion}
        existingFiles={existingIdentificacion ? [existingIdentificacion] : []}
        selectedFiles={identificacion ? [identificacion] : []}
        onRemove={() => setIdentificacion(null)}
        onFiles={(files) => {
          const file = files[0] ?? null;
          const problem = file ? validateDocumentFile(file) : null;
          setErrors((prev) => ({ ...prev, identificacion: problem ?? undefined }));
          if (!problem) setIdentificacion(file);
        }}
        emptyHint="Requerido: 1 archivo."
      />

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-between">
        <button
          type="button"
          onClick={onBack}
          className="inline-flex items-center justify-center gap-2 rounded-xl px-6 py-3.5 text-title-md font-semibold text-on-surface-variant transition-colors hover:bg-surface-container-low hover:text-on-surface"
        >
          <MaterialIcon name="arrow_back" className="text-[18px]" />
          <span>Atrás</span>
        </button>
        <button
          type="submit"
          className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          <span>Continuar</span>
          <MaterialIcon name="arrow_forward" className="text-[18px]" />
        </button>
      </div>
    </form>
  );
}

function DocumentCard({
  icon,
  title,
  description,
  error,
  hint,
  existingFiles,
  selectedFiles,
  multiple = false,
  onFiles,
  onRemove,
  emptyHint,
}: {
  icon: string;
  title: string;
  description: string;
  error?: string;
  hint?: string;
  existingFiles: TutorDocumentItem[];
  selectedFiles: File[];
  multiple?: boolean;
  onFiles: (files: File[]) => void;
  onRemove: (file: File) => void;
  emptyHint: string;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const inputId = `file-${icon}`;

  return (
    <div className="flex flex-col gap-3 rounded-2xl border border-outline-variant/60 bg-surface-container-lowest p-5">
      <div className="flex items-start gap-3">
        <span className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-primary-container/30">
          <MaterialIcon name={icon} className="text-[20px] text-primary" />
        </span>
        <div className="min-w-0">
          <h3 className="text-title-md font-bold text-on-surface">{title}</h3>
          <p className="text-body-sm text-on-surface-variant">{description}</p>
        </div>
      </div>

      {(existingFiles.length > 0 || selectedFiles.length > 0) && (
        <ul className="flex flex-col gap-1.5">
          {existingFiles.map((doc) => (
            <li
              key={doc.id}
              className="flex items-center gap-2 rounded-lg bg-tertiary-container/20 px-3 py-2 text-label-md text-on-surface"
            >
              <MaterialIcon name="check_circle" className="text-[16px] text-tertiary" />
              <span className="min-w-0 flex-1 truncate font-semibold">{doc.fileName}</span>
              <span className="shrink-0 text-on-surface-variant">ya subido</span>
            </li>
          ))}
          {selectedFiles.map((file) => (
            <li
              key={file.name}
              className="flex items-center gap-2 rounded-lg bg-surface-container-low px-3 py-2 text-label-md text-on-surface"
            >
              <MaterialIcon name="description" className="text-[16px] text-primary" />
              <span className="min-w-0 flex-1 truncate font-semibold">{file.name}</span>
              <span className="shrink-0 text-on-surface-variant">{formatDocumentSize(file.size)}</span>
              <button
                type="button"
                onClick={() => onRemove(file)}
                aria-label={`Quitar ${file.name}`}
                className="shrink-0 rounded p-0.5 text-on-surface-variant transition-colors hover:bg-error-container/30 hover:text-error"
              >
                <MaterialIcon name="close" className="text-[16px]" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept={DOCUMENT_ACCEPT_ATTR}
        multiple={multiple}
        className="sr-only"
        onChange={(e) => {
          onFiles(Array.from(e.target.files ?? []));
          e.target.value = "";
        }}
      />
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        className="inline-flex items-center justify-center gap-2 rounded-xl border border-dashed border-outline px-4 py-3 text-label-md font-semibold text-on-surface-variant transition-colors hover:border-primary hover:bg-primary-container/10 hover:text-primary"
      >
        <MaterialIcon name="upload_file" className="text-[18px]" />
        {selectedFiles.length > 0 || existingFiles.length > 0
          ? multiple
            ? "Agregar más archivos"
            : "Reemplazar archivo"
          : "Seleccionar archivo"}
      </button>

      {error ? (
        <p role="alert" className="text-label-md text-error">
          {error}
        </p>
      ) : (
        <p className="text-label-md text-on-surface-variant">{hint ?? emptyHint}</p>
      )}
    </div>
  );
}

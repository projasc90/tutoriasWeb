"use client";

/**
 * Gestor de documentos de verificación del tutor autenticado
 * (/tutor/documentos). Muestra los documentos agrupados por tipo y permite
 * AGREGAR títulos y certificaciones (máx 5 c/u, espejo del backend) en
 * cualquier estado; la identificación solo se puede subir antes de la
 * verificación (Verified → candado + 409 IDENTIFICATION_LOCKED del backend).
 * El estado de verificación no cambia al agregar documentos.
 */

import { useEffect, useRef, useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { fetchTutorDocuments, uploadTutorDocument } from "@/lib/api";
import {
  DOCUMENT_ACCEPT_ATTR,
  formatDocumentSize,
  validateDocumentFile,
} from "@/lib/documents";
import type { TutorDocumentItem, TutorDocumentKindName } from "@/lib/types/tutor-application";

const MAX_PER_ACCUMULATED_KIND = 5;

const KIND_META: Record<
  TutorDocumentKindName,
  { title: string; description: string; icon: string; accumulated: boolean }
> = {
  Titulo: {
    title: "Títulos académicos",
    description: "Diplomas o certificados de grado (bachillerato, licenciatura, maestría, doctorado).",
    icon: "school",
    accumulated: true,
  },
  Certificacion: {
    title: "Certificaciones y atestados",
    description: "Atestados, colegiatura, certificados de cursos o antecedentes penales.",
    icon: "workspace_premium",
    accumulated: true,
  },
  Identificacion: {
    title: "Identificación",
    description: "Cédula de identidad o pasaporte vigente (un solo archivo).",
    icon: "badge",
    accumulated: false,
  },
};

export function DocumentsManager({
  token,
  verificationStatus,
}: {
  token: string;
  verificationStatus: string;
}) {
  const [docs, setDocs] = useState<TutorDocumentItem[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [uploadingKind, setUploadingKind] = useState<TutorDocumentKindName | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [uploadedNotice, setUploadedNotice] = useState<string | null>(null);

  const load = async () => {
    const result = await fetchTutorDocuments(token);
    if (result.ok) {
      setDocs(result.data);
      setLoadError(null);
    } else {
      setLoadError(result.error);
    }
  };

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      if (cancelled) return;
      await load();
    })();
    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const isVerified = verificationStatus === "Verified";

  const handleUpload = async (kind: TutorDocumentKindName, files: File[]) => {
    setUploadError(null);
    setUploadedNotice(null);

    const problems = files.map(validateDocumentFile).filter((p): p is string => p !== null);
    if (problems.length > 0) {
      setUploadError(problems[0]);
      return;
    }

    const existingCount = (docs ?? []).filter((d) => d.kind === kind).length;
    if (KIND_META[kind].accumulated && existingCount + files.length > MAX_PER_ACCUMULATED_KIND) {
      setUploadError(`Máximo ${MAX_PER_ACCUMULATED_KIND} archivos en total.`);
      return;
    }

    setUploadingKind(kind);
    let failures = 0;
    for (const file of files) {
      const result = await uploadTutorDocument(token, kind, file);
      if (!result.ok) {
        setUploadError(result.error);
        failures += 1;
        break;
      }
    }
    setUploadingKind(null);

    if (failures === 0) {
      setUploadedNotice(
        kind === "Identificacion"
          ? "Identificación actualizada."
          : "Documento agregado. Queda disponible para el equipo de verificación.",
      );
    }
    await load();
  };

  if (loadError) {
    return (
      <div className="flex flex-col items-center gap-3 rounded-2xl border border-outline-variant/60 bg-surface-container-lowest py-16 text-center">
        <MaterialIcon name="error" className="text-[28px] text-error" />
        <p className="text-body-md text-on-surface-variant">{loadError}</p>
        <button
          type="button"
          onClick={() => void load()}
          className="rounded-lg bg-primary px-5 py-2 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          Reintentar
        </button>
      </div>
    );
  }

  if (docs === null) {
    return (
      <div className="flex items-center justify-center py-24" aria-busy="true">
        <MaterialIcon name="progress_activity" className="animate-spin text-[32px] text-primary" />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-5">
      <p className="text-body-md text-on-surface-variant">
        Formatos PDF/JPEG/PNG, hasta 5 MB por archivo.
      </p>

      {isVerified ? (
        <p className="flex items-start gap-2 rounded-xl border border-tertiary/30 bg-tertiary-container/20 px-4 py-3 text-body-sm text-on-surface">
          <MaterialIcon name="verified_user" className="mt-0.5 shrink-0 text-[18px] text-tertiary" />
          <span>
            Tu perfil está verificado: puedes <span className="font-semibold">agregar</span> títulos
            y certificaciones nuevos en cualquier momento (quedan disponibles para el equipo de
            verificación). Tu identificación ya fue verificada y quedó bloqueada.
          </span>
        </p>
      ) : (
        <p className="flex items-start gap-2 rounded-xl border border-outline-variant/60 bg-surface-container-low px-4 py-3 text-body-sm text-on-surface-variant">
          <MaterialIcon name="info" className="mt-0.5 shrink-0 text-[18px]" />
          <span>
            Puedes agregar títulos, certificaciones y (si falta) tu identificación mientras tu
            postulación está en revisión o fue rechazada.
          </span>
        </p>
      )}

      {uploadedNotice && (
        <p
          role="status"
          className="flex items-center gap-2 rounded-lg border border-tertiary/30 bg-tertiary-container/20 px-3.5 py-2.5 text-body-md text-on-surface"
        >
          <MaterialIcon name="check_circle" className="text-[18px] text-tertiary" />
          {uploadedNotice}
        </p>
      )}
      {uploadError && (
        <p
          role="alert"
          className="flex items-center gap-2 rounded-lg border border-error/30 bg-error-container/20 px-3.5 py-2.5 text-body-md text-on-error-container"
        >
          <MaterialIcon name="error" className="text-[18px]" />
          {uploadError}
        </p>
      )}

      <div className="flex flex-col gap-4">
        {(["Titulo", "Certificacion", "Identificacion"] as TutorDocumentKindName[]).map((kind) => (
          <DocumentSection
            key={kind}
            kind={kind}
            documents={docs.filter((d) => d.kind === kind)}
            locked={kind === "Identificacion" && isVerified}
            uploading={uploadingKind === kind}
            onUpload={(files) => void handleUpload(kind, files)}
          />
        ))}
      </div>
    </div>
  );
}

function DocumentSection({
  kind,
  documents,
  locked,
  uploading,
  onUpload,
}: {
  kind: TutorDocumentKindName;
  documents: TutorDocumentItem[];
  locked: boolean;
  uploading: boolean;
  onUpload: (files: File[]) => void;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const meta = KIND_META[kind];
  const atCapacity = meta.accumulated && documents.length >= MAX_PER_ACCUMULATED_KIND;
  const hasIdentification = kind === "Identificacion" && documents.length > 0;
  const uploadDisabled = locked || atCapacity;

  const uploadLabel = locked
    ? "Verificada — bloqueada"
    : kind === "Identificacion" && hasIdentification
      ? "Reemplazar archivo"
      : documents.length > 0
        ? "Agregar más archivos"
        : "Subir archivo";

  return (
    <section className="flex flex-col gap-3 rounded-2xl border border-outline-variant/60 bg-surface-container-lowest p-5">
      <div className="flex items-start gap-3">
        <span className="grid h-10 w-10 shrink-0 place-items-center rounded-xl bg-primary-container/30">
          <MaterialIcon name={meta.icon} className="text-[20px] text-primary" />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-title-md font-bold text-on-surface">
            {meta.title}
            {meta.accumulated && (
              <span className="ml-2 text-label-md font-normal text-on-surface-variant">
                {documents.length}/{MAX_PER_ACCUMULATED_KIND}
              </span>
            )}
          </h2>
          <p className="text-body-sm text-on-surface-variant">{meta.description}</p>
        </div>
      </div>

      {documents.length > 0 ? (
        <ul className="flex flex-col gap-1.5">
          {documents.map((doc) => (
            <li
              key={doc.id}
              className="flex items-center gap-2 rounded-lg bg-surface-container-low px-3 py-2 text-label-md text-on-surface"
            >
              <MaterialIcon name="description" className="shrink-0 text-[16px] text-primary" />
              <span className="min-w-0 flex-1 truncate font-semibold">{doc.fileName}</span>
              <span className="shrink-0 text-on-surface-variant">{formatDocumentSize(doc.sizeBytes)}</span>
              <span className="shrink-0 text-on-surface-variant">
                {new Date(doc.uploadedAt).toLocaleDateString("es-CR", {
                  day: "numeric",
                  month: "short",
                })}
              </span>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-label-md text-on-surface-variant">
          {kind === "Identificacion" ? "Sin identificación subida." : "Sin archivos todavía."}
        </p>
      )}

      <input
        ref={inputRef}
        type="file"
        accept={DOCUMENT_ACCEPT_ATTR}
        multiple={meta.accumulated}
        className="sr-only"
        onChange={(e) => {
          onUpload(Array.from(e.target.files ?? []));
          e.target.value = "";
        }}
      />
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        disabled={uploadDisabled || uploading}
        className={
          locked || atCapacity
            ? "inline-flex cursor-not-allowed items-center justify-center gap-2 rounded-xl border border-outline-variant/60 px-4 py-3 text-label-md font-semibold text-on-surface-variant/60"
            : "inline-flex items-center justify-center gap-2 rounded-xl border border-dashed border-outline px-4 py-3 text-label-md font-semibold text-on-surface-variant transition-colors hover:border-primary hover:bg-primary-container/10 hover:text-primary disabled:opacity-60"
        }
      >
        {uploading ? (
          <>
            <MaterialIcon name="progress_activity" className="animate-spin text-[18px]" />
            Subiendo…
          </>
        ) : locked ? (
          <>
            <MaterialIcon name="lock" className="text-[18px]" />
            {uploadLabel}
          </>
        ) : (
          <>
            <MaterialIcon name="upload_file" className="text-[18px]" />
            {uploadLabel}
          </>
        )}
      </button>
    </section>
  );
}

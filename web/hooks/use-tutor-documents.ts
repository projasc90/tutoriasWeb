"use client";

/**
 * Hook de subida de documentos de verificación del tutor.
 *
 * Orquesta la cola de uploads posterior al submit de la postulación (la fila
 * de tutor debe existir antes de subir archivos). Devuelve los uploads que
 * fallaron para permitir el reintento desde la pantalla de confirmación.
 */

import { useState } from "react";
import { uploadTutorDocument } from "@/lib/api";
import type { TutorDocumentKindName } from "@/lib/types/tutor-application";

export type DocumentUpload = { kind: TutorDocumentKindName; file: File };

export type DocumentsUploadState =
  | { status: "idle" }
  | { status: "uploading"; total: number; done: number }
  | { status: "error"; failures: DocumentUpload[] }
  | { status: "success" };

export function useTutorDocuments() {
  const [state, setState] = useState<DocumentsUploadState>({ status: "idle" });

  /**
   * Sube los documentos en secuencia y devuelve los que fallaron
   * (array vacío = todo ok). Con la cola vacía resuelve success directo.
   */
  const uploadAll = async (
    uploads: DocumentUpload[],
    token: string,
  ): Promise<DocumentUpload[]> => {
    if (uploads.length === 0) {
      setState({ status: "success" });
      return [];
    }

    setState({ status: "uploading", total: uploads.length, done: 0 });
    const failures: DocumentUpload[] = [];
    let done = 0;
    for (const upload of uploads) {
      const result = await uploadTutorDocument(token, upload.kind, upload.file);
      if (!result.ok) failures.push(upload);
      done += 1;
      setState({ status: "uploading", total: uploads.length, done });
    }

    setState(failures.length > 0 ? { status: "error", failures } : { status: "success" });
    return failures;
  };

  const reset = () => setState({ status: "idle" });

  return { state, uploadAll, reset };
}

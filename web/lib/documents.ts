/**
 * Validación de documentos de verificación compartida entre el wizard
 * /postular y el gestor de documentos del portal (/tutor/documentos).
 * Reglas espejo del backend: PDF/JPEG/PNG, ≤ 5 MB.
 */

export const MAX_DOCUMENT_SIZE_BYTES = 5 * 1024 * 1024;
export const ACCEPTED_DOCUMENT_EXTENSIONS = [".pdf", ".jpg", ".jpeg", ".png"];
export const DOCUMENT_ACCEPT_ATTR = ".pdf,.jpg,.jpeg,.png";

/** Mensaje de validación o null si el archivo es aceptable. */
export function validateDocumentFile(file: File): string | null {
  const extension = `.${file.name.split(".").pop()?.toLowerCase() ?? ""}`;
  if (!ACCEPTED_DOCUMENT_EXTENSIONS.includes(extension)) {
    return "Formato no permitido: usa PDF, JPG o PNG.";
  }
  if (file.size > MAX_DOCUMENT_SIZE_BYTES) {
    return "El archivo supera el máximo de 5 MB.";
  }
  return null;
}

export function formatDocumentSize(bytes: number): string {
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${Math.max(Math.round(bytes / 1024), 1)} KB`;
}

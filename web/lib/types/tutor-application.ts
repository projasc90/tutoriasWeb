/**
 * Tipos de dominio de la postulación de tutores (onboarding del profesor).
 *
 * Espejo de los DTOs del backend .NET
 * (AuraLearn.Application/Dto/TutorApplicationDto.cs).
 */

export type TutorApplicationPayload = {
  fullName: string;
  credentials: string;
  /** Especialización o énfasis del título (opcional). */
  specialization: string | null;
  university: string;
  bio: string;
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
};

export type TutorApplicationResponse = {
  id: string;
  status: string;
};

export type TutorApplicationStatus = {
  id: string;
  status: "PendingReview" | "UnderReview" | "Verified" | "Rejected";
  rejectionReason: string | null;
  submittedAt: string;
};

export type AuthSession = {
  id: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
};

/**
 * Tipos de documento de verificación (espejo de TutorDocumentKind.cs).
 * Titulo e Identificacion son únicos por tutor (upsert); Certificacion
 * acumula hasta 5.
 */
export type TutorDocumentKindName = "Titulo" | "Certificacion" | "Identificacion";

/** Metadatos de un documento de verificación (espejo de TutorDocumentItemDto). */
export type TutorDocumentItem = {
  id: string;
  kind: TutorDocumentKindName;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedAt: string;
};

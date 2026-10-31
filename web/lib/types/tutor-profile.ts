/**
 * Tipos de dominio del perfil del tutor autenticado.
 *
 * Espejo de los DTOs del backend .NET
 * (AuraLearn.Application/Dto/TutorProfileDto.cs — GET/PUT /api/tutors/me/profile).
 */

export type VerificationStatusName =
  | "PendingReview"
  | "UnderReview"
  | "Verified"
  | "Rejected";

export type TutorProfile = {
  id: string;
  /** Nombre de la cuenta (no editable desde el perfil). */
  name: string;
  credentials: string;
  specialization: string | null;
  university: string;
  bio: string;
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
  verificationStatus: VerificationStatusName;
  rejectionReason: string | null;
  /** URL pública relativa de la foto de perfil (null = sin foto, mostrar iniciales). */
  photoUrl: string | null;
};

/** Cuerpo del PUT /api/tutors/me/profile (solo campos editables por el tutor). */
export type UpdateTutorProfilePayload = {
  credentials: string;
  specialization: string | null;
  university: string;
  bio: string;
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
};

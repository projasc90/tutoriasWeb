/**
 * Tipos de dominio de la postulación de tutores (onboarding del profesor).
 *
 * Espejo de los DTOs del backend .NET
 * (AuraLearn.Application/Dto/TutorApplicationDto.cs).
 */

export type TutorApplicationPayload = {
  fullName: string;
  credentials: string;
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

/**
 * Tipos de los mentores destacados de la landing.
 *
 * Espejo transitorio de los datos mock: pasará a reflejar los DTOs del backend
 * .NET cuando exista la API (ver docs/plans/2026-09-28-extract-mock-data/).
 */

export type Mentor = {
  initials: string;
  name: string;
  credentials: string;
  rating: number;
  reviews: number;
  badges: string[];
  nextSlot: string;
  price: string; // formato presentacional: "₡14,500"
  priceUsd: string; // formato presentacional: "~$28"
  tone: "primary" | "success" | "tertiary" | "secondary";
  avatarBg: string;
};

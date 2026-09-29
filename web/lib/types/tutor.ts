/**
 * Tipos de dominio del directorio de tutores.
 *
 * Espejo del DTO del backend .NET (AuraLearn.Application/Dto/TutorDto.cs).
 * El cliente HTTP tipado vive en web/lib/api.ts; este archivo mantiene el
 * tipo compartido para filtros y componentes.
 */

export type Tutor = {
  id: string;
  name: string;
  credentials: string;
  university: string;
  rating: number;
  reviews: number;
  subjects: string[];
  priceCrc: number; // CRC
  priceUsd: number; // USD
  nextSlot: string;
  featured: boolean;
  bio: string;
};

export type FilterState = {
  query: string;
  university: string;
  level: string;
  availability: string;
  minRating: string;
  priceMin: number;
  priceMax: number;
};

export type SortOption = "rating" | "price_asc" | "price_desc" | "reviews";

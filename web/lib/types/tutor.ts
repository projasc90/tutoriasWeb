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
  /** Especialización o énfasis del título (opcional). */
  specialization: string | null;
  university: string;
  rating: number;
  reviews: number;
  subjects: string[];
  priceCrc: number; // CRC
  priceUsd: number; // USD
  /** Próximo cupo disponible en UTC (ISO 8601). Formateo a zona CR en presentación. */
  nextSlotAt: string | null;
  featured: boolean;
  bio: string;
  /** URL pública relativa de la foto de perfil (null = sin foto, mostrar iniciales). */
  photoUrl: string | null;
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

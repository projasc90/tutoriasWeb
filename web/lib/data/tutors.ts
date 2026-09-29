/**
 * Constantes de filtro del directorio de tutores.
 *
 * Los datos de tutores ya NO viven aquí: el directorio consume la API real
 * (GET /api/tutors vía web/hooks/use-tutors.ts). Este módulo conserva solo
 * las opciones de la UI de filtros.
 */

import type { FilterState } from "@/lib/types/tutor";

export const UNIVERSITIES = ["Todas", "UCR", "TEC", "UNA", "LEAD", "ULACIT", "U Latina"];
export const LEVELS = ["Todos", "Grado", "Bachillerato Internacional", "Examen de Admisión", "Posgrado"];
export const AVAILABILITIES = ["Cualquiera", "Hoy", "Esta semana", "Fines de semana"];
export const RATINGS = ["Cualquiera", "4.5+", "4.0+", "3.5+"];

export const DEFAULT_FILTERS: FilterState = {
  query: "",
  university: "Todas",
  level: "Todos",
  availability: "Cualquiera",
  minRating: "Cualquiera",
  priceMin: 0,
  priceMax: 25000,
};

/**
 * Lógica pura de orden del directorio de tutores.
 *
 * El filtrado de datos es server-side (GET /api/tutors del backend .NET);
 * aquí queda solo el orden client-side sobre la página actual.
 * Funciones puras para poder testear sin renderizar componentes.
 */

import type { SortOption, Tutor } from "@/lib/types/tutor";

/** Ordena el listado según la opción seleccionada; no muta el arreglo de entrada. */
export function applySort(tutors: Tutor[], sort: SortOption): Tutor[] {
  return [...tutors].sort((a, b) => {
    if (sort === "rating") return b.rating - a.rating;
    if (sort === "price_asc") return a.priceCrc - b.priceCrc;
    if (sort === "price_desc") return b.priceCrc - a.priceCrc;
    if (sort === "reviews") return b.reviews - a.reviews;
    return 0;
  });
}

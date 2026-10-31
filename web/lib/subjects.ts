/**
 * Normalización y sugerencias de nombres de materias.
 *
 * Regla de producto: las materias se guardan SIEMPRE con la primera letra
 * mayúscula en cada palabra significativa (conectores en minúscula, salvo
 * la primera palabra). Espejo del SubjectNormalizer del backend .NET.
 */

const LOWER_CASE_WORDS = new Set([
  "de", "del", "la", "las", "el", "los", "y", "e", "o", "u", "para", "con", "en", "al", "a",
]);

/** "cálculo i" → "Cálculo I" · "estadística y probabilidad" → "Estadística y Probabilidad". */
export function normalizeSubject(raw: string): string {
  const trimmed = raw.trim();
  if (!trimmed) return trimmed;

  const words = trimmed.split(/\s+/);
  return words
    .map((word, i) =>
      i > 0 && LOWER_CASE_WORDS.has(word.toLowerCase())
        ? word.toLowerCase()
        : word.charAt(0).toUpperCase() + word.slice(1),
    )
    .join(" ");
}

/** Quita acentos, mayúsculas y espacios redundantes para comparar nombres. */
const looseKey = (value: string) =>
  value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase()
    .replace(/\s+/g, " ")
    .trim();

/**
 * Sugerencia de corrección contra el catálogo: si lo escrito coincide
 * (ignorando acentos/mayúsculas) o es prefijo de una materia canónica,
 * devuelve la forma canónica (null si no hay sugerencia).
 * "calculo i" → "Cálculo I" · "estadistic" → "Estadística".
 */
export function suggestSubjectCorrection(raw: string, catalog: readonly string[]): string | null {
  const target = looseKey(raw);
  if (target.length < 3) return null;

  const exact = catalog.find((c) => looseKey(c) === target);
  if (exact && exact !== raw.trim()) return exact;

  const prefix = catalog.find((c) => {
    const key = looseKey(c);
    return key.startsWith(target) && key !== target;
  });
  return prefix ?? null;
}

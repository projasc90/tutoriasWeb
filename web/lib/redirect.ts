/**
 * Redirect de retorno tras login/registro.
 *
 * Flujo: un estudiante anónimo llega a /tutores/[id], elige un slot y la UI lo
 * envía a /login?redirect=/tutores/<id>&slot=<slotId>. Tras autenticarse,
 * volvemos a esa ruta para retomar el flujo de reserva con el slot preseleccionado.
 */

export const DEFAULT_LOGIN_REDIRECT = "/tutores";

/**
 * Extrae un redirect seguro de una query string. Solo se aceptan rutas
 * internas (empiezan con "/" y no son "//" ni contienen esquema) para
 * evitar open redirects a dominios externos.
 */
export function readInternalRedirect(search: string): string | null {
  const raw = new URLSearchParams(search).get("redirect");
  if (!raw || !raw.startsWith("/") || raw.startsWith("//")) return null;
  return raw;
}

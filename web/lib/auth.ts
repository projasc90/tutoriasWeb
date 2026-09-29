/**
 * Sesión mínima del lado del cliente (token JWT en localStorage).
 *
 * Usada por el wizard /postular (cuenta paso 1) y por el contexto global de
 * sesión (hooks/use-auth.tsx): páginas /login y /registro + header con sesión.
 * La expiración se valida decodificando el JWT; la firma la verifica el backend.
 */

import { AUTH_STORAGE_KEY, type AuthResponse } from "@/lib/api";

export type StoredSession = AuthResponse & {
  tokenExpiresAt: number; // epoch ms
};

/** Decodifica el payload de un JWT (sin verificar firma — eso es del backend). */
export function decodeJwt(token: string): { exp?: number; sub?: string; email?: string; name?: string; role?: string } | null {
  try {
    const payload = token.split(".")[1];
    const json = atob(payload + "=".repeat(-payload.length % 4));
    return JSON.parse(json);
  } catch {
    return null;
  }
}

/** Convierte un AuthResponse del backend en sesión persistible con expiración. */
export function toSession(auth: AuthResponse): StoredSession {
  const claims = decodeJwt(auth.token);
  const tokenExpiresAt = claims?.exp ? claims.exp * 1000 : Date.now() + 8 * 60 * 60 * 1000;
  return { ...auth, tokenExpiresAt };
}

export function isExpired(session: StoredSession): boolean {
  return Date.now() >= session.tokenExpiresAt;
}

export function readSession(): StoredSession | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = window.localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as StoredSession;
    if (typeof parsed?.token !== "string") return null;
    // Sesión expirada → descartar (el usuario deberá iniciar sesión de nuevo).
    if (parsed.tokenExpiresAt && Date.now() >= parsed.tokenExpiresAt) {
      clearSession();
      return null;
    }
    // Sesiones antiguas sin expiración: calcular del token.
    if (!parsed.tokenExpiresAt) {
      const claims = decodeJwt(parsed.token);
      const tokenExpiresAt = claims?.exp ? claims.exp * 1000 : 0;
      if (tokenExpiresAt && Date.now() >= tokenExpiresAt) {
        clearSession();
        return null;
      }
      return { ...parsed, tokenExpiresAt };
    }
    return parsed;
  } catch {
    clearSession();
    return null;
  }
}

export function saveSession(session: StoredSession): void {
  if (typeof window === "undefined") return;
  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  if (typeof window === "undefined") return;
  window.localStorage.removeItem(AUTH_STORAGE_KEY);
}

"use client";

/**
 * Contexto global de sesión: AuthProvider + useAuth.
 *
 * Centraliza login/registro/logout y la restauración de la sesión al montar
 * (localStorage con validación de expiración del JWT, ver lib/auth.ts).
 * El wizard /postular sigue usando readSession/saveSession directamente;
 * este proveedor añade el estado reactivo para header y páginas de auth.
 */

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { clearSession, readSession, saveSession, toSession, type StoredSession } from "@/lib/auth";
import { loginUser, registerUser, type LoginPayload, type RegisterPayload } from "@/lib/api";

type AuthState =
  | { status: "loading" } // restaurando del storage (evita flash del header)
  | { status: "authenticated"; session: StoredSession }
  | { status: "anonymous" };

type AuthContextValue = {
  state: AuthState;
  login: (payload: LoginPayload) => Promise<{ ok: boolean; error?: string }>;
  register: (payload: RegisterPayload) => Promise<{ ok: boolean; error?: string }>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({ status: "loading" });

  // Restauración al montar: lee la sesión y descarta tokens expirados.
  // Se difiere al siguiente tick para no llamar setState sincrónicamente en el efecto.
  useEffect(() => {
    const id = setTimeout(() => {
      const session = readSession();
      setState(session ? { status: "authenticated", session } : { status: "anonymous" });
    }, 0);
    return () => clearTimeout(id);
  }, []);

  const login = useCallback(async (payload: LoginPayload) => {
    const result = await loginUser(payload);
    if (!result.ok) return { ok: false, error: result.error };
    const session = toSession(result.data);
    saveSession(session);
    setState({ status: "authenticated", session });
    return { ok: true };
  }, []);

  const register = useCallback(async (payload: RegisterPayload) => {
    const result = await registerUser(payload);
    if (!result.ok) return { ok: false, error: result.error };
    const session = toSession(result.data);
    saveSession(session);
    setState({ status: "authenticated", session });
    return { ok: true };
  }, []);

  const logout = useCallback(() => {
    clearSession();
    setState({ status: "anonymous" });
  }, []);

  const value = useMemo(
    () => ({ state, login, register, logout }),
    [state, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth debe usarse dentro de <AuthProvider>");
  return ctx;
}

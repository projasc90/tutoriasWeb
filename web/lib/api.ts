/**
 * Cliente HTTP tipado para la API de AuraLearn (.NET).
 *
 * Contrato espejo de `AuraLearn.Application/Dto/TutorDto.cs` (backend).
 * Base URL configurable vía NEXT_PUBLIC_API_URL (ver web/.env.example).
 */

import type {
  TutorApplicationPayload,
  TutorApplicationResponse,
  TutorApplicationStatus,
} from "@/lib/types/tutor-application";

// ── Contratos (espejo de los DTOs del backend) ─────────────────────

export type Tutor = {
  id: string;
  name: string;
  credentials: string;
  university: string;
  rating: number;
  reviews: number;
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
  bio: string;
  featured: boolean;
  nextSlot: string;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type TutorSearchParams = {
  query?: string;
  university?: string;
  minRating?: number;
  priceMin?: number;
  priceMax?: number;
  page?: number;
  pageSize?: number;
};

// ── Cliente ────────────────────────────────────────────────────────

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5037";

/** Resultado de una llamada: éxito con datos o error con mensaje accionable. */
export type ApiResult<T> = { ok: true; data: T } | { ok: false; error: string };

/** Clave de la sesión en localStorage (token JWT del backend). */
export const AUTH_STORAGE_KEY = "auralearn.auth";

function buildQuery(params: TutorSearchParams): string {
  const search = new URLSearchParams();
  if (params.query) search.set("query", params.query);
  if (params.university && params.university !== "Todas") search.set("university", params.university);
  if (params.minRating !== undefined) search.set("minRating", String(params.minRating));
  if (params.priceMin !== undefined) search.set("priceMin", String(params.priceMin));
  if (params.priceMax !== undefined) search.set("priceMax", String(params.priceMax));
  if (params.page !== undefined) search.set("page", String(params.page));
  if (params.pageSize !== undefined) search.set("pageSize", String(params.pageSize));
  const qs = search.toString();
  return qs ? `?${qs}` : "";
}

/** GET /api/tutors — catálogo público de tutores verificados. */
export async function fetchTutors(
  params: TutorSearchParams,
  signal?: AbortSignal,
): Promise<ApiResult<PagedResult<Tutor>>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutors${buildQuery(params)}`, { signal });

    if (!res.ok) {
      return { ok: false, error: `El servidor respondió con el error ${res.status}` };
    }

    const data = (await res.json()) as PagedResult<Tutor>;
    return { ok: true, data };
  } catch (err) {
    if (err instanceof DOMException && err.name === "AbortError") {
      // Request cancelado por un cambio de filtros: no es un error de usuario.
      return { ok: false, error: "aborted" };
    }
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Auth (registro/login, espejo de AuthController) ────────────────

export type RegisterPayload = { email: string; password: string; fullName: string };
export type LoginPayload = { email: string; password: string };

export type AuthResponse = {
  id: string;
  email: string;
  fullName: string;
  role: string;
  token: string;
};

/** Datos de /api/auth/me (el claim email no se mapea en backend — deuda menor). */
export type MeResponse = {
  id: string;
  email: string | null;
  role: string;
  fullName: string;
};

/** POST /api/auth/register — crea la cuenta (rol Estudiante) y devuelve el JWT. */
export async function registerUser(payload: RegisterPayload): Promise<ApiResult<AuthResponse>> {
  try {
    const res = await fetch(`${BASE_URL}/api/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (res.status === 409) {
      return { ok: false, error: "Este correo ya está registrado. Inicia sesión para continuar." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo crear la cuenta. Revisa los datos e inténtalo de nuevo." };
    }

    const data = (await res.json()) as AuthResponse;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** POST /api/auth/login — autentica y devuelve el JWT. */
export async function loginUser(payload: LoginPayload): Promise<ApiResult<AuthResponse>> {
  try {
    const res = await fetch(`${BASE_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (res.status === 401) {
      return { ok: false, error: "Correo o contraseña incorrectos." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo iniciar sesión. Inténtalo de nuevo." };
    }

    const data = (await res.json()) as AuthResponse;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Postulación de tutores (espejo de TutorApplicationsController) ──

/** POST /api/tutor-applications — envía la postulación (queda PendingReview). */
export async function submitTutorApplication(
  payload: TutorApplicationPayload,
  token: string,
): Promise<ApiResult<TutorApplicationResponse>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutor-applications`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    if (res.status === 409) {
      return { ok: false, error: "Ya tienes una postulación activa en revisión." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo enviar la postulación. Revisa los datos e inténtalo de nuevo." };
    }

    const data = (await res.json()) as TutorApplicationResponse;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** GET /api/tutor-applications/status — estado de la postulación del usuario. */
export async function fetchTutorApplicationStatus(
  token: string,
): Promise<ApiResult<TutorApplicationStatus | null>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutor-applications/status`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.status === 404) {
      return { ok: true, data: null };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo consultar el estado de tu postulación." };
    }

    const data = (await res.json()) as TutorApplicationStatus;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** GET /api/auth/me — valida un Bearer token y devuelve los datos del usuario. */
export async function fetchMe(token: string): Promise<ApiResult<MeResponse>> {
  try {
    const res = await fetch(`${BASE_URL}/api/auth/me`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo validar tu sesión." };
    }

    const data = (await res.json()) as MeResponse;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Cola admin de postulaciones (espejo de AdminTutorApplicationsController) ──

/** Postulación pendiente visible para el administrador. */
export type AdminTutorApplication = {
  id: string;
  userId: string;
  name: string;
  credentials: string;
  university: string;
  bio: string;
  subjects: string[];
  priceCrc: number;
  priceUsd: number;
  status: string;
  submittedAt: string;
};

export type VerifyDecision = "approve" | "reject";

/** GET /api/admin/tutor-applications — cola pendiente de revisión (solo Admin). */
export async function fetchAdminApplications(
  token: string,
): Promise<ApiResult<AdminTutorApplication[]>> {
  try {
    const res = await fetch(`${BASE_URL}/api/admin/tutor-applications`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "No tienes permisos de administrador." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo cargar la cola de verificación." };
    }

    const data = (await res.json()) as AdminTutorApplication[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/**
 * PATCH /api/admin/tutor-applications/{id}/verify — aprueba o rechaza una
 * postulación. El rechazo exige motivo (validado también en el backend).
 */
export async function verifyApplication(
  token: string,
  id: string,
  decision: VerifyDecision,
  reason?: string,
): Promise<ApiResult<null>> {
  try {
    const res = await fetch(`${BASE_URL}/api/admin/tutor-applications/${id}/verify`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ decision, reason: decision === "reject" ? reason ?? "" : null }),
    });

    if (res.status === 400) {
      return { ok: false, error: "El rechazo exige un motivo." };
    }
    if (res.status === 404) {
      return { ok: false, error: "La postulación ya no existe." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "No tienes permisos de administrador." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo registrar la decisión." };
    }

    return { ok: true, data: null };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

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
  /** Próximo cupo disponible en UTC (ISO 8601). Formateo a zona CR en presentación. */
  nextSlotAt: string | null;
};

// ── Contratos de slots/reservas/pagos (espejo de ReservationDto.cs) ─

export type Slot = {
  id: string;
  tutorId: string;
  /** Inicio en UTC (ISO 8601). */
  startAt: string;
  /** Fin en UTC (ISO 8601). */
  endAt: string;
};

export type PaymentInfo = {
  receptorPhone: string;
  amountCrc: number;
  currency: string;
  /** ISO 8601 UTC. */
  expiresAt: string;
};

export type ReservationStatus =
  | "PendingPayment"
  | "Confirmed"
  | "Rejected"
  | "Expired"
  | "Cancelled"
  | "Completed";

export type Reservation = {
  id: string;
  slotId: string;
  tutorId: string;
  tutorName: string;
  studentName: string;
  /** Inicio de la sesión en UTC (ISO 8601). */
  startAt: string;
  /** Fin de la sesión en UTC (ISO 8601). */
  endAt: string;
  status: ReservationStatus;
  priceCrc: number;
  /** ISO 8601 UTC. */
  expiresAt: string;
  confirmationNumber: string | null;
  decisionReason: string | null;
  /** ISO 8601 UTC. */
  createdAt: string;
  /** Solo presente en la respuesta 201 de creación. */
  paymentInfo?: PaymentInfo;
};

export type AdminPayment = {
  reservationId: string;
  studentId: string;
  studentName: string;
  tutorId: string;
  tutorName: string;
  startAt: string;
  priceCrc: number;
  confirmationNumber: string;
  comprobanteAmountCrc: number | null;
  comprobantePhone: string | null;
  submittedAt: string;
};

export type WalletBalance = {
  balanceCrc: number;
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

/** GET /api/tutors/{id} — detalle público de un tutor (perfil /tutores/[id]). */
export async function fetchTutor(id: string): Promise<ApiResult<Tutor>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutors/${id}`);
    if (res.status === 404) {
      return { ok: false, error: "Tutor no encontrado." };
    }
    if (!res.ok) {
      return { ok: false, error: `El servidor respondió con el error ${res.status}` };
    }
    const data = (await res.json()) as Tutor;
    return { ok: true, data };
  } catch {
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

// ── Slots disponibles (espejo de SlotsController) ───────────────

/**
 * GET /api/tutors/{id}/slots?from=&to= — slots disponibles del tutor en el
 * rango [from, to) UTC. Por defecto: próximos 14 días desde ahora.
 */
export async function fetchTutorSlots(
  tutorId: string,
  token?: string,
): Promise<ApiResult<Slot[]>> {
  try {
    const headers: HeadersInit = token ? { Authorization: `Bearer ${token}` } : {};
    const res = await fetch(`${BASE_URL}/api/tutors/${tutorId}/slots`, { headers });

    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudieron cargar los horarios del tutor." };
    }

    const data = (await res.json()) as Slot[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Reservas + pago SINPE (espejo de ReservationsController) ──────

export type CreateReservationPayload = { slotId: string; idempotencyKey: string; useWalletBalance?: boolean };
export type SubmitComprobantePayload = { confirmationNumber: string; amountCrc: number; phone?: string | null };

/** POST /api/reservations — crea reserva idempotente; devuelve 201 con PaymentInfo. */
export async function createReservation(
  token: string,
  payload: CreateReservationPayload,
): Promise<ApiResult<Reservation>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    if (res.status === 409) {
      return { ok: false, error: "Este horario ya fue reservado por otro estudiante." };
    }
    if (res.status === 422) {
      return { ok: false, error: "El horario seleccionado ya no está disponible." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo crear la reserva. Inténtalo de nuevo." };
    }

    const data = (await res.json()) as Reservation;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** GET /api/reservations — reservas del estudiante autenticado. */
export async function fetchMyReservations(token: string): Promise<ApiResult<Reservation[]>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudieron cargar tus reservas." };
    }
    const data = (await res.json()) as Reservation[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** GET /api/reservations/{id} — detalle de una reserva (polling del checkout). */
export async function fetchReservation(
  token: string,
  id: string,
): Promise<ApiResult<Reservation>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations/${id}`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 404) {
      return { ok: false, error: "La reserva ya no existe." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo consultar la reserva." };
    }
    const data = (await res.json()) as Reservation;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** POST /api/reservations/{id}/comprobante — sube comprobante SINPE (queda en cola). */
export async function submitComprobante(
  token: string,
  reservationId: string,
  payload: SubmitComprobantePayload,
): Promise<ApiResult<Reservation>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations/${reservationId}/comprobante`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(payload),
    });

    if (res.status === 409) {
      return { ok: false, error: "Este comprobante ya fue usado en otra reserva, o la reserva ya no espera pago." };
    }
    if (res.status === 422) {
      return { ok: false, error: "Los datos del comprobante no son válidos (monto, teléfono o vigencia)." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo subir el comprobante." };
    }

    const data = (await res.json()) as Reservation;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** PATCH /api/reservations/{id}/cancel — cancela la reserva del estudiante. */
export async function cancelReservation(
  token: string,
  reservationId: string,
): Promise<ApiResult<Reservation>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations/${reservationId}/cancel`, {
      method: "PATCH",
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.status === 409) {
      return { ok: false, error: "Esta reserva ya no se puede cancelar." };
    }
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo cancelar la reserva." };
    }

    const data = (await res.json()) as Reservation;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Cola admin de pagos (espejo de AdminPaymentsController) ────────

export type DecidePaymentDecision = "approve" | "reject";

/** GET /api/admin/payments — comprobantes pendientes de revisión. */
export async function fetchAdminPayments(token: string): Promise<ApiResult<AdminPayment[]>> {
  try {
    const res = await fetch(`${BASE_URL}/api/admin/payments`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "No tienes permisos de administrador." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo cargar la cola de pagos." };
    }
    const data = (await res.json()) as AdminPayment[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** PATCH /api/admin/payments/{reservationId} — aprueba o rechaza un comprobante. */
export async function decidePayment(
  token: string,
  reservationId: string,
  decision: DecidePaymentDecision,
  reason?: string,
): Promise<ApiResult<null>> {
  try {
    const res = await fetch(`${BASE_URL}/api/admin/payments/${reservationId}`, {
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
      return { ok: false, error: "La reserva ya no existe." };
    }
    if (res.status === 409) {
      return { ok: false, error: "La reserva ya no está esperando pago." };
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

// ── Disponibilidad del tutor (espejo de AvailabilityController) ────

/** Regla semanal de disponibilidad (franja local CR en un día de la semana). */
export type AvailabilityRule = {
  /** 0=domingo … 6=sábado (igual que Date.getDay). */
  weekday: number;
  /** "HH:mm" hora local CR (incluida). */
  startLocal: string;
  /** "HH:mm" hora local CR (excluida). */
  endLocal: string;
};

/** GET /api/tutors/me/availability — reglas del tutor autenticado. */
export async function fetchAvailability(
  token: string,
): Promise<ApiResult<AvailabilityRule[]>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutors/me/availability`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "Este panel es solo para tutores verificados." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo cargar tu disponibilidad." };
    }
    const data = (await res.json()) as AvailabilityRule[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** PUT /api/tutors/me/availability — reemplazo completo de las reglas. */
export async function replaceAvailability(
  token: string,
  rules: AvailabilityRule[],
): Promise<ApiResult<null>> {
  try {
    const res = await fetch(`${BASE_URL}/api/tutors/me/availability`, {
      method: "PUT",
      headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
      body: JSON.stringify({ rules }),
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "Este panel es solo para tutores verificados." };
    }
    if (res.status === 422) {
      const body = (await res.json().catch(() => null)) as { error?: string } | null;
      if (body?.error === "AVAILABILITY_OVERLAP") {
        return { ok: false, error: "Tienes franjas que se traslapan o duplicadas el mismo día." };
      }
      return { ok: false, error: "Hay franjas inválidas (inicio ≥ fin o minutos distintos de :00)." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo guardar tu disponibilidad." };
    }
    return { ok: true, data: null };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Monedero del estudiante (espejo de WalletController) ───────────

/** GET /api/wallet — saldo actual del monedero del estudiante. */
export async function fetchWallet(token: string): Promise<ApiResult<WalletBalance>> {
  try {
    const res = await fetch(`${BASE_URL}/api/wallet`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo cargar tu monedero." };
    }
    const data = (await res.json()) as WalletBalance;
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

// ── Panel del tutor (espejo de ReservationsController) ─────────────

/** GET /api/reservations/tutor-sessions — sesiones que imparte el tutor autenticado. */
export async function fetchTutorSessions(token: string): Promise<ApiResult<Reservation[]>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations/tutor-sessions`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "Este panel es solo para tutores verificados." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudieron cargar tus sesiones." };
    }
    const data = (await res.json()) as Reservation[];
    return { ok: true, data };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

/** PATCH /api/reservations/{id}/complete — el tutor cierra la sesión (liquidación). */
export async function completeSession(
  token: string,
  reservationId: string,
): Promise<ApiResult<null>> {
  try {
    const res = await fetch(`${BASE_URL}/api/reservations/${reservationId}/complete`, {
      method: "PATCH",
      headers: { Authorization: `Bearer ${token}` },
    });
    if (res.status === 401) {
      return { ok: false, error: "Tu sesión expiró. Vuelve a iniciar sesión." };
    }
    if (res.status === 403) {
      return { ok: false, error: "Solo el tutor de esta sesión puede completarla." };
    }
    if (res.status === 404) {
      return { ok: false, error: "La sesión ya no existe." };
    }
    if (res.status === 409) {
      return { ok: false, error: "La sesión no está confirmada." };
    }
    if (res.status === 422) {
      return { ok: false, error: "La sesión aún no termina." };
    }
    if (!res.ok) {
      return { ok: false, error: "No se pudo completar la sesión." };
    }
    return { ok: true, data: null };
  } catch {
    return { ok: false, error: "No se pudo conectar con el servidor" };
  }
}

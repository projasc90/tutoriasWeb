"use client";

/**
 * Panel del Profesor — Gestor de Disponibilidad (Stitch 05).
 * Matriz semanal de 7 columnas con franjas (hora local en la zona del tutor),
 * tarjeta de zona horaria con sincronización, interruptor maestro del horario
 * semanal, excepciones/feriados y action bar sticky.
 * Guardado = reemplazo completo vía PUT /api/tutors/me/availability.
 */

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useAvailability } from "@/hooks/use-availability";
import { useBookingRules } from "@/hooks/use-booking-rules";
import {
  BOOKING_RULES_DEFAULTS,
  SUPPORTED_TIME_ZONES,
  type AvailabilityException,
  type AvailabilityRule,
  type AvailabilitySettings,
  type BookingRules,
} from "@/lib/api";
import {
  AvailabilityHeader,
  AvailabilityTabs,
} from "@/components/tutor/availability/availability-chrome";
import { DayColumn, type DayRule } from "@/components/tutor/availability/day-column";
import { SaveActionBar } from "@/components/tutor/availability/save-action-bar";

const DIAS = [
  { weekday: 1, name: "Lunes" },
  { weekday: 2, name: "Martes" },
  { weekday: 3, name: "Miércoles" },
  { weekday: 4, name: "Jueves" },
  { weekday: 5, name: "Viernes" },
  { weekday: 6, name: "Sábado" },
  { weekday: 0, name: "Domingo" },
];

let seq = 0;
const newLocalId = () => `r${Date.now()}-${seq++}`;

/** Tarjeta informativa de una regla de reserva (definida por el Admin). */
type ReglaCard = { icon: string; titulo: string; desc: string; valor: string };

const fmtHoras = (n: number) => (n === 1 ? "1 hora" : `${n} horas`);
const fmtMinutos = (n: number) => (n === 1 ? "1 minuto" : `${n} minutos`);
const fmtDias = (n: number) =>
  n % 7 === 0 ? (n / 7 === 1 ? "1 semana" : `${n / 7} semanas`) : `${n} días`;

/**
 * Tarjetas visibles según las reglas definidas por el Admin en BD: una regla
 * sin definir (null) no se muestra. Sin conexión se cae a los defaults.
 */
const reglasCards = (reglas: BookingRules | null): ReglaCard[] => {
  if (!reglas) return [];
  const cards: ReglaCard[] = [];
  if (reglas.cancellationNoticeHours != null) {
    const n = reglas.cancellationNoticeHours;
    cards.push({
      icon: "hourglass_top",
      titulo: "Anticipación Mínima",
      desc: `Las cancelaciones y reprogramaciones del estudiante aplican hasta ${n} h antes de la sesión.`,
      valor: fmtHoras(n),
    });
  }
  if (reglas.slotDurationMinutes != null) {
    const n = reglas.slotDurationMinutes;
    cards.push({
      icon: "snooze",
      titulo: "Duración de Slots",
      desc: `Cada sesión se publica en bloques de ${n} minutos dentro de tus franjas.`,
      valor: fmtMinutos(n),
    });
  }
  if (reglas.bookingHorizonDays != null) {
    cards.push({
      icon: "calendar_month",
      titulo: "Ventana Máxima",
      desc: "Horizonte temporal para que estudiantes reserven por adelantado.",
      valor: fmtDias(reglas.bookingHorizonDays),
    });
  }
  return cards;
};

export default function AvailabilityPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { state, save } = useAvailability(token);
  const { state: reglasState } = useBookingRules();

  const [filas, setFilas] = useState<Record<number, DayRule[]>>({});
  const [baseline, setBaseline] = useState<Record<number, DayRule[]>>({});
  const [timeZoneId, setTimeZoneId] = useState("America/Costa_Rica");
  const [baselineTimeZone, setBaselineTimeZone] = useState("America/Costa_Rica");
  const [weeklyEnabled, setWeeklyEnabled] = useState(true);
  const [baselineWeekly, setBaselineWeekly] = useState(true);
  const [exceptions, setExceptions] = useState<AvailabilityException[]>([]);
  const [baselineExceptions, setBaselineExceptions] = useState<AvailabilityException[]>([]);
  const [tab, setTab] = useState<"semanal" | "excepciones">("semanal");
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (authState.status === "anonymous") router.replace("/login");
  }, [authState.status, router]);

  useEffect(() => {
    if (state.status !== "success") return;
    const raf = requestAnimationFrame(() => {
      const byDay: Record<number, DayRule[]> = {};
      for (const d of DIAS) byDay[d.weekday] = [];
      for (const r of state.data.rules) {
        byDay[r.weekday] = [
          ...(byDay[r.weekday] ?? []),
          { ...r, localId: newLocalId() },
        ];
      }
      setFilas(byDay);
      setBaseline(byDay);
      setTimeZoneId(state.data.timeZoneId);
      setBaselineTimeZone(state.data.timeZoneId);
      setWeeklyEnabled(state.data.weeklyEnabled);
      setBaselineWeekly(state.data.weeklyEnabled);
      setExceptions(state.data.exceptions);
      setBaselineExceptions(state.data.exceptions);
    });
    return () => cancelAnimationFrame(raf);
  }, [state]);

  const esTutor = authState.status === "authenticated" && authState.session.role === "Tutor";

  const dirty = useMemo(
    () =>
      JSON.stringify(filas) !== JSON.stringify(baseline) ||
      timeZoneId !== baselineTimeZone ||
      weeklyEnabled !== baselineWeekly ||
      JSON.stringify(exceptions) !== JSON.stringify(baselineExceptions),
    [filas, baseline, timeZoneId, baselineTimeZone, weeklyEnabled, baselineWeekly, exceptions, baselineExceptions],
  );

  // Métricas: capacidad = suma de horas de franjas (solo si el horario está activo)
  const { weeklyCapacityHours } = useMemo(() => {
    let hours = 0;
    if (weeklyEnabled) {
      for (const rules of Object.values(filas)) {
        for (const r of rules) {
          const [h1] = r.startLocal.split(":").map(Number);
          const [h2] = r.endLocal.split(":").map(Number);
          if (Number.isFinite(h1) && Number.isFinite(h2) && h2 > h1) hours += h2 - h1;
        }
      }
    }
    return { weeklyCapacityHours: hours };
  }, [filas, weeklyEnabled]);

  // Cupos abiertos por franja: horas enteras dentro de [inicio, fin)
  const slotsOpenByRule = useMemo(() => {
    const m = new Map<string, number>();
    for (const rules of Object.values(filas)) {
      for (const r of rules) {
        const [h1] = r.startLocal.split(":").map(Number);
        const [h2] = r.endLocal.split(":").map(Number);
        m.set(r.localId, Number.isFinite(h1) && Number.isFinite(h2) && h2 > h1 ? h2 - h1 : 0);
      }
    }
    return m;
  }, [filas]);

  // Reglas de reserva de BD: null mientras cargan; en error se muestran los defaults
  const reglas: BookingRules | null =
    reglasState.status === "success"
      ? reglasState.data
      : reglasState.status === "loading"
        ? null
        : { ...BOOKING_RULES_DEFAULTS, updatedAt: "" };
  const tarjetasReglas = reglasCards(reglas);

  if (authState.status === "loading" || authState.status === "anonymous") {
    return (
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto max-w-7xl px-6 py-12">
          <p className="text-body-lg text-on-surface-variant">Cargando…</p>
        </div>
      </main>
    );
  }

  if (!esTutor) {
    return (
      <main className="flex min-h-screen flex-1 flex-col bg-background">
        <div className="mx-auto max-w-3xl px-6 py-16 text-center">
          <MaterialIcon name="lock" className="text-[40px] text-on-surface-variant" />
          <p className="mt-3 text-body-lg text-on-surface-variant">
            Este panel es solo para tutores verificados.
          </p>
            <Link href="/estudiante/reservas" className="mt-4 inline-block text-primary hover:underline">
              Volver a mis tutorías
            </Link>
          </div>
        </main>
    );
  }

  const onAdd = (weekday: number) => {
    setSaved(false);
    setFilas((prev) => {
      const existing = prev[weekday] ?? [];
      // La nueva franja empieza después de la última (evita duplicados exactos
      // y solapes que el backend rechaza con 422). Con el horario semanal
      // activo, la franja nace recurrente (se puede deseleccionar por franja).
      let start = 8;
      let end = 12;
      if (existing.length > 0) {
        const last = existing[existing.length - 1];
        const lastEnd = Number(last.endLocal.split(":")[0]);
        if (lastEnd + 4 <= 22) {
          start = lastEnd;
          end = lastEnd + 4;
        }
      }
      return {
        ...prev,
        [weekday]: [...existing, { localId: newLocalId(), weekday, startLocal: `${String(start).padStart(2, "0")}:00`, endLocal: `${String(end).padStart(2, "0")}:00`, isRecurring: true, anchorDate: null }],
      };
    });
  };

  const onRemove = (localId: string) => {
    setSaved(false);
    setFilas((prev) => {
      const next: Record<number, DayRule[]> = {};
      for (const [wd, rules] of Object.entries(prev)) {
        next[Number(wd)] = rules.filter((r) => r.localId !== localId);
      }
      return next;
    });
  };

  const onChange = (
    localId: string,
    field: "startLocal" | "endLocal" | "isRecurring",
    value: string | boolean,
  ) => {
    setSaved(false);
    setFilas((prev) => {
      const next: Record<number, DayRule[]> = {};
      for (const [wd, rules] of Object.entries(prev)) {
        next[Number(wd)] = rules.map((r) =>
          r.localId === localId ? { ...r, [field]: value } : r
        );
      }
      return next;
    });
  };

  const onCopyDay = (weekday: number) => {
    setSaved(false);
    const source = filas[weekday] ?? [];
    setFilas((prev) => {
      const next = { ...prev };
      for (const d of DIAS) {
        if (d.weekday === weekday) continue;
        next[d.weekday] = source.map((r) => ({
          ...r,
          localId: newLocalId(),
          weekday: d.weekday,
        }));
      }
      return next;
    });
  };

  const onAddException = (date: string, reason: string) => {
    setSaved(false);
    if (!date || exceptions.some((e) => e.date === date)) return;
    setExceptions((prev) => [...prev, { date, reason: reason.trim() || null }].sort((a, b) => a.date.localeCompare(b.date)));
  };

  const onRemoveException = (date: string) => {
    setSaved(false);
    setExceptions((prev) => prev.filter((e) => e.date !== date));
  };

  const toSettings = (): AvailabilitySettings => {
    const rules: AvailabilityRule[] = [];
    for (const rulesOfDay of Object.values(filas)) {
      for (const r of rulesOfDay) {
        if (r.startLocal < r.endLocal) {
          rules.push({
            weekday: r.weekday,
            startLocal: r.startLocal,
            endLocal: r.endLocal,
            isRecurring: r.isRecurring ?? true,
            anchorDate: null, // el backend calcula el ancla de las no recurrentes
          });
        }
      }
    }
    return { timeZoneId, weeklyEnabled, rules, exceptions };
  };

  const onSave = async () => {
    setSaving(true);
    setError(null);
    const result = await save(toSettings());
    setSaving(false);
    if (result.ok) {
      setSaved(true);
      setBaseline(filas);
      setBaselineTimeZone(timeZoneId);
      setBaselineWeekly(weeklyEnabled);
      setBaselineExceptions(exceptions);
    } else {
      setError(result.error);
    }
  };

  const onDiscard = () => {
    setSaved(false);
    setFilas(baseline);
    setTimeZoneId(baselineTimeZone);
    setWeeklyEnabled(baselineWeekly);
    setExceptions(baselineExceptions);
  };

  const zonaLabel = SUPPORTED_TIME_ZONES.find((z) => z.id === timeZoneId)?.label ?? timeZoneId;

  return (
    <main className="flex-1 bg-background">
      {/* Header estándar full-width (hijo directo de main, como en las otras páginas) */}
      <AvailabilityHeader weeklyCapacityHours={weeklyCapacityHours} occupiedHours={0} />

      <div className="relative mx-auto w-full max-w-7xl px-6 py-8 lg:px-12">
        {/* Glow decorativo */}
        <div className="pointer-events-none absolute -z-10 left-1/3 top-4 h-96 w-96 rounded-full bg-primary/5 blur-3xl" />

          {/* Zona horaria (separada del header): selector + sincronización */}
          <div className="mb-6 flex flex-col justify-between gap-4 rounded-xl bg-surface-container-lowest p-4 shadow-sm sm:flex-row sm:items-center">
            <div className="flex items-center gap-3">
              <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10 text-primary">
                <MaterialIcon name="schedule" className="text-[20px]" />
              </span>
              <div>
                <p className="text-label-sm text-on-surface-variant">Zona horaria de tu agenda</p>
                <select
                  value={timeZoneId}
                  onChange={(e) => {
                    setTimeZoneId(e.target.value);
                    setSaved(false);
                  }}
                  aria-label="Selecciona tu zona horaria"
                  className="mt-0.5 rounded-lg border border-outline-variant bg-surface-container-lowest px-3 py-1.5 text-label-md font-semibold text-on-surface focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
                >
                  {SUPPORTED_TIME_ZONES.map((z) => (
                    <option key={z.id} value={z.id}>
                      {z.label}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="flex items-center gap-3">
              {timeZoneId !== baselineTimeZone && (
                <span className="inline-flex items-center gap-1 rounded-full bg-primary-fixed px-2.5 py-0.5 text-label-sm font-semibold text-primary">
                  <MaterialIcon name="autorenew" className="text-[14px]" />
                  Se sincronizará al guardar
                </span>
              )}
              <button
                type="button"
                onClick={() => void onSave()}
                disabled={saving || !dirty}
                className="inline-flex items-center gap-2 rounded-lg bg-surface-container-high px-4 py-2.5 text-label-md text-on-surface transition-colors hover:bg-surface-variant disabled:opacity-60"
              >
                {saving ? (
                  <MaterialIcon name="progress_activity" className="animate-spin text-[18px]" />
                ) : (
                  <MaterialIcon name="sync" className="text-[18px]" />
                )}
                <span>Sincronizar ahora</span>
              </button>
            </div>
          </div>

          <AvailabilityTabs
            excepciones={exceptions.length}
            tab={tab}
            onTabChange={setTab}
          />

          {tab === "semanal" ? (
            /* Matriz semanal: cada franja muestra la opción recurrente (deseleccionable) */
            <div className="mb-8">
              <div className="grid grid-cols-1 gap-2 md:grid-cols-2 xl:grid-cols-7">
                {DIAS.map((d) => (
                  <DayColumn
                    key={d.weekday}
                    name={d.name}
                    weekday={d.weekday}
                    rules={filas[d.weekday] ?? []}
                    slotsOpenByRule={slotsOpenByRule}
                    busySlotsByHour={new Map()} // alumnos por franja: módulo de sesiones (posterior)
                    onAdd={onAdd}
                    onRemove={onRemove}
                    onChange={onChange}
                    onCopyDay={onCopyDay}
                  />
                ))}
              </div>
            </div>
          ) : (
            /* Excepciones y feriados: fechas sin cupos */
            <ExceptionsPanel
              exceptions={exceptions}
              onAdd={onAddException}
              onRemove={onRemoveException}
            />
          )}

          {/* Reglas informativas: las define el Admin en booking_rules (BD).
              Una regla sin definir no se muestra (decisión de producto). */}
          <div className="mb-8 grid grid-cols-1 gap-6 lg:grid-cols-3">
            <div className="rounded-xl bg-surface-container-lowest p-6 shadow-sm lg:col-span-2">
              <div className="mb-4 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary/10 text-primary">
                    <MaterialIcon name="policy" className="text-[20px]" />
                  </span>
                  <div>
                    <h2 className="text-headline-sm text-on-surface">Reglas de Reserva e Intervalos</h2>
                    <p className="text-body-md text-on-surface-variant">
                      Protege tus tiempos de preparación docente
                    </p>
                  </div>
                </div>
                <span className="rounded-full bg-surface-container-high px-2.5 py-1 text-label-sm font-semibold text-primary">
                  Política Estricta
                </span>
              </div>
              {tarjetasReglas.length > 0 ? (
                <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                  {tarjetasReglas.map((c) => (
                    <Regla key={c.titulo} icon={c.icon} titulo={c.titulo} desc={c.desc} valor={c.valor} />
                  ))}
                </div>
              ) : (
                <p className="text-body-md text-on-surface-variant">
                  La administración no ha definido reglas de reserva; se aplican los valores por defecto de la plataforma.
                </p>
              )}
              <p className="mt-4 flex items-center gap-1.5 text-label-sm text-outline">
                <MaterialIcon name="lock" style={{ fontSize: 14 }} />
                Definidas por la administración de AuraLearn (solo lectura).
              </p>
            </div>
            <div className="rounded-xl bg-surface-container-lowest p-6 shadow-sm">
              <div className="mb-3 flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-secondary-container text-secondary">
                    <MaterialIcon name="sync_alt" className="text-[20px]" />
                  </span>
                  <h2 className="text-headline-sm text-on-surface">Calendarios</h2>
                </div>
                <span className="inline-flex items-center gap-1 text-label-sm text-primary">
                  <span className="h-2 w-2 rounded-full bg-primary" /> 0 Activos
                </span>
              </div>
              <p className="mb-4 text-body-md text-on-surface-variant">
                La sincronización con Google Calendar / Outlook bloqueará franjas automáticamente. Próximamente.
              </p>
              <div className="flex items-center justify-between rounded-lg bg-surface-container-low/50 p-2.5 text-label-sm text-outline">
                <span>Requiere integración externa (ADR)</span>
                <MaterialIcon name="lock" className="text-[16px]" />
              </div>
            </div>
          </div>

          {error && <p className="mb-4 text-label-md text-error">{error}</p>}

          <SaveActionBar
            dirty={dirty}
            saving={saving}
            saved={saved}
            onSave={onSave}
            onDiscard={onDiscard}
          />
        </div>
    </main>
  );
}

/** Panel de excepciones y feriados: fechas en las que no se generan cupos. */
function ExceptionsPanel({
  exceptions,
  onAdd,
  onRemove,
}: {
  exceptions: AvailabilityException[];
  onAdd: (date: string, reason: string) => void;
  onRemove: (date: string) => void;
}) {
  const [date, setDate] = useState("");
  const [reason, setReason] = useState("");
  const [localError, setLocalError] = useState<string | null>(null);

  const hoy = new Date().toISOString().slice(0, 10);

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!date) {
      setLocalError("Selecciona una fecha.");
      return;
    }
    if (date < hoy) {
      setLocalError("La fecha debe ser hoy o futura.");
      return;
    }
    if (exceptions.some((ex) => ex.date === date)) {
      setLocalError("Esa fecha ya está registrada como excepción.");
      return;
    }
    setLocalError(null);
    onAdd(date, reason);
    setDate("");
    setReason("");
  };

  return (
    <div className="mb-8 flex flex-col gap-4 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <div className="flex items-center gap-2">
        <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-error-container/40 text-error">
          <MaterialIcon name="event_busy" className="text-[20px]" />
        </span>
        <div>
          <h2 className="text-headline-sm text-on-surface">Excepciones y feriados</h2>
          <p className="text-body-md text-on-surface-variant">
            En estas fechas tu horario semanal no genera cupos (feriados, vacaciones, bloqueos).
          </p>
        </div>
      </div>

      <form className="flex flex-col gap-3 sm:flex-row sm:items-end" onSubmit={submit} noValidate>
        <div className="flex flex-col gap-1">
          <label htmlFor="exception-date" className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
            Fecha
          </label>
          <input
            id="exception-date"
            type="date"
            min={hoy}
            value={date}
            onChange={(e) => setDate(e.target.value)}
            className="rounded-lg border border-outline-variant bg-surface-container-lowest px-3.5 py-2.5 text-sm text-on-surface focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
          />
        </div>
        <div className="flex flex-1 flex-col gap-1">
          <label htmlFor="exception-reason" className="text-label-sm font-semibold uppercase tracking-wider text-on-surface-variant">
            Motivo (opcional)
          </label>
          <input
            id="exception-reason"
            type="text"
            placeholder="Ej. Feriado, vacaciones, congreso"
            maxLength={200}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full rounded-lg border border-outline-variant bg-surface-container-lowest px-3.5 py-2.5 text-sm text-on-surface placeholder:text-outline focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/30"
          />
        </div>
        <button
          type="submit"
          className="inline-flex items-center justify-center gap-1.5 rounded-lg bg-primary px-4 py-2.5 text-label-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
        >
          <MaterialIcon name="add" className="text-[16px]" />
          Agregar
        </button>
      </form>

      {localError && (
        <p role="alert" className="text-label-md text-error">
          {localError}
        </p>
      )}

      {exceptions.length > 0 ? (
        <ul className="flex flex-col gap-1.5">
          {exceptions.map((ex) => (
            <li
              key={ex.date}
              className="flex items-center gap-3 rounded-lg bg-surface-container-low px-3 py-2 text-label-md"
            >
              <MaterialIcon name="event_busy" className="shrink-0 text-[16px] text-error" />
              <span className="font-semibold text-on-surface">
                {new Date(`${ex.date}T12:00:00`).toLocaleDateString("es-CR", {
                  weekday: "long",
                  day: "numeric",
                  month: "long",
                })}
              </span>
              {ex.reason && (
                <span className="min-w-0 flex-1 truncate text-on-surface-variant">— {ex.reason}</span>
              )}
              <button
                type="button"
                onClick={() => onRemove(ex.date)}
                aria-label={`Quitar excepción del ${ex.date}`}
                className="shrink-0 rounded p-1 text-on-surface-variant transition-colors hover:bg-error-container/30 hover:text-error"
              >
                <MaterialIcon name="close" style={{ fontSize: 14 }} />
              </button>
            </li>
          ))}
        </ul>
      ) : (
        <p className="text-label-md text-on-surface-variant">
          Sin excepciones registradas. Tu horario semanal aplica todos los días.
        </p>
      )}
    </div>
  );
}

function Regla(props: { icon: string; titulo: string; desc: string; valor: string }) {
  return (
    <div className="flex flex-col justify-between rounded-xl bg-surface-container-low p-4">
      <div>
        <div className="mb-2 flex items-center gap-2 text-primary">
          <MaterialIcon name={props.icon} className="text-[20px]" />
          <span className="text-label-md text-on-surface">{props.titulo}</span>
        </div>
        <p className="mb-2 text-body-md leading-relaxed text-on-surface-variant">{props.desc}</p>
      </div>
      <span className="w-full rounded-lg bg-surface-container-lowest px-3 py-2 text-center shadow-sm">
        {props.valor}
      </span>
    </div>
  );
}

"use client";

/**
 * Panel del Profesor — Gestor de Disponibilidad (Stitch 05).
 * Matriz semanal de 7 columnas con franjas recurrentes (hora local CR),
 * métricas de capacidad, tabs, reglas informativas y action bar sticky.
 * Guardado = reemplazo completo vía PUT /api/tutors/me/availability.
 */

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { SiteHeader } from "@/components/landing/currency-toggle";
import { MaterialIcon } from "@/components/material-icon";
import { useAuth } from "@/hooks/use-auth";
import { useAvailability } from "@/hooks/use-availability";
import type { AvailabilityRule } from "@/lib/api";
import {
  AvailabilityHeader,
  AvailabilityToolbar,
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

export default function AvailabilityPage() {
  const router = useRouter();
  const { state: authState } = useAuth();
  const token = authState.status === "authenticated" ? authState.session.token : null;

  const { state, save } = useAvailability(token);

  const [filas, setFilas] = useState<Record<number, DayRule[]>>({});
  const [baseline, setBaseline] = useState<Record<number, DayRule[]>>({});
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
      for (const r of state.data) {
        byDay[r.weekday] = [
          ...(byDay[r.weekday] ?? []),
          { ...r, localId: newLocalId() },
        ];
      }
      setFilas(byDay);
      setBaseline(byDay);
    });
    return () => cancelAnimationFrame(raf);
  }, [state]);

  const esTutor = authState.status === "authenticated" && authState.session.role === "Tutor";

  const dirty = useMemo(() => JSON.stringify(filas) !== JSON.stringify(baseline), [filas, baseline]);

  // Métricas: capacidad = suma de horas de franjas; ocupación = 0 (sin datos de sesiones por franja aún)
  const { weeklyCapacityHours } = useMemo(() => {
    let hours = 0;
    for (const rules of Object.values(filas)) {
      for (const r of rules) {
        const [h1] = r.startLocal.split(":").map(Number);
        const [h2] = r.endLocal.split(":").map(Number);
        if (Number.isFinite(h1) && Number.isFinite(h2) && h2 > h1) hours += h2 - h1;
      }
    }
    return { weeklyCapacityHours: hours };
  }, [filas]);

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

  if (authState.status === "loading" || authState.status === "anonymous") {
    return (
      <>
        <SiteHeader />
        <main className="flex min-h-screen flex-1 flex-col bg-background">
          <div className="mx-auto max-w-7xl px-6 py-12">
            <p className="text-body-lg text-on-surface-variant">Cargando…</p>
          </div>
        </main>
      </>
    );
  }

  if (!esTutor) {
    return (
      <>
        <SiteHeader />
        <main className="flex min-h-screen flex-1 flex-col bg-background">
          <div className="mx-auto max-w-3xl px-6 py-16 text-center">
            <MaterialIcon name="lock" className="text-[40px] text-on-surface-variant" />
            <p className="mt-3 text-body-lg text-on-surface-variant">
              Este panel es solo para tutores verificados.
            </p>
            <Link href="/mis-tutorias" className="mt-4 inline-block text-primary hover:underline">
              Volver a mis tutorías
            </Link>
          </div>
        </main>
      </>
    );
  }

  const onAdd = (weekday: number) => {
    setSaved(false);
    setFilas((prev) => {
      const existing = prev[weekday] ?? [];
      // La nueva franja empieza después de la última (evita duplicados exactos
      // y solapes que el backend rechaza con 422)
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
        [weekday]: [...existing, { localId: newLocalId(), weekday, startLocal: `${String(start).padStart(2, "0")}:00`, endLocal: `${String(end).padStart(2, "0")}:00` }],
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

  const onChange = (localId: string, field: "startLocal" | "endLocal", value: string) => {
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

  const toApi = (byDay: Record<number, DayRule[]>): AvailabilityRule[] => {
    const rules: AvailabilityRule[] = [];
    for (const rulesOfDay of Object.values(byDay)) {
      for (const r of rulesOfDay) {
        if (r.startLocal < r.endLocal) {
          rules.push({ weekday: r.weekday, startLocal: r.startLocal, endLocal: r.endLocal });
        }
      }
    }
    return rules;
  };

  const onSave = async () => {
    setSaving(true);
    setError(null);
    const result = await save(toApi(filas));
    setSaving(false);
    if (result.ok) {
      setSaved(true);
      setBaseline(filas);
    } else {
      setError(result.error);
    }
  };

  const onDiscard = () => {
    setSaved(false);
    setFilas(baseline);
  };

  return (
    <>
      <SiteHeader />
      <main className="flex-1 bg-background">
        <div className="relative mx-auto w-full max-w-7xl px-6 py-8 lg:px-12">
          {/* Glow decorativo */}
          <div className="pointer-events-none absolute -z-10 left-1/3 top-4 h-96 w-96 rounded-full bg-primary/5 blur-3xl" />

          <AvailabilityHeader weeklyCapacityHours={weeklyCapacityHours} occupiedHours={0} />
          <AvailabilityToolbar />
          <AvailabilityTabs excepciones={0} />

          {/* Matriz semanal */}
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

          {/* Reglas informativas (la política activa del repo: 12 h de anticipación) */}
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
              <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                <Regla
                  icon="hourglass_top"
                  titulo="Anticipación Mínima"
                  desc="Las cancelaciones y reprogramaciones del estudiante aplican hasta 12 h antes de la sesión."
                  valor="12 horas (AuraLearn)"
                />
                <Regla
                  icon="snooze"
                  titulo="Duración de Slots"
                  desc="Cada sesión se publica en bloques de 1 hora dentro de tus franjas."
                  valor="60 minutos"
                />
                <Regla
                  icon="calendar_month"
                  titulo="Ventana Máxima"
                  desc="Horizonte temporal para que estudiantes reserven por adelantado."
                  valor="2 semanas"
                />
              </div>
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
    </>
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

import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";

export function Confirmation({
  fullName,
  email,
}: {
  fullName: string;
  email: string;
}) {
  return (
    <div className="flex flex-col items-center gap-6 py-6 text-center">
      <span className="grid h-20 w-20 place-items-center rounded-full bg-tertiary-fixed/40">
        <MaterialIcon name="check_circle" filled className="text-[44px] text-tertiary" />
      </span>

      <div>
        <h2 className="text-headline-lg font-bold text-on-surface">
          ¡Postulación enviada, {fullName.split(" ")[0]}!
        </h2>
        <p className="mx-auto mt-2 max-w-md text-body-lg text-on-surface-variant">
          Recibimos tu perfil. Nuestro equipo verificará tu título y atestados y te
          notificaremos a <span className="font-semibold text-on-surface">{email}</span> en
          un plazo máximo de 48 horas.
        </p>
      </div>

      <ol className="flex w-full max-w-md flex-col gap-3 text-left">
        <TimelineItem
          icon="mail"
          title="Confirmación por correo"
          body="Te enviamos un resumen de tu postulación."
          state="done"
        />
        <TimelineItem
          icon="verified_user"
          title="Revisión de credenciales"
          body="Verificamos tu título contra el registro universitario (≤48 h)."
          state="current"
        />
        <TimelineItem
          icon="event_available"
          title="Perfil activo"
          body="Al aprobarse, tu perfil queda visible y reservable en el catálogo."
          state="todo"
        />
      </ol>

      <Link
        href="/"
        className="inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-6 py-3.5 text-title-md font-semibold text-on-primary shadow-sm transition-colors hover:bg-on-primary-fixed-variant"
      >
        <span>Volver al inicio</span>
        <MaterialIcon name="arrow_forward" className="text-[18px]" />
      </Link>
    </div>
  );
}

function TimelineItem({
  icon,
  title,
  body,
  state,
}: {
  icon: string;
  title: string;
  body: string;
  state: "done" | "current" | "todo";
}) {
  return (
    <li className="flex items-start gap-3 rounded-xl border border-outline-variant/60 bg-surface-container-lowest p-4">
      <span
        className={
          state === "done"
            ? "grid h-9 w-9 shrink-0 place-items-center rounded-full bg-tertiary-fixed/40 text-tertiary"
            : state === "current"
              ? "grid h-9 w-9 shrink-0 place-items-center rounded-full bg-primary/15 text-primary"
              : "grid h-9 w-9 shrink-0 place-items-center rounded-full bg-surface-container-high text-on-surface-variant"
        }
      >
        <MaterialIcon name={icon} className="text-[18px]" />
      </span>
      <div>
        <p className="text-title-md font-semibold text-on-surface">{title}</p>
        <p className="text-body-md text-on-surface-variant">{body}</p>
      </div>
    </li>
  );
}

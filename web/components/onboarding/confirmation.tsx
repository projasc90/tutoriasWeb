import Link from "next/link";
import { MaterialIcon } from "@/components/material-icon";

export function Confirmation({
  fullName,
  email,
  failedFileNames = null,
  retrying = false,
  onRetryDocuments,
}: {
  fullName: string;
  email: string;
  /** Archivos cuya subida falló (null = todo ok o sin documentos nuevos). */
  failedFileNames?: string[] | null;
  retrying?: boolean;
  onRetryDocuments?: () => void;
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
        <TimelineItem
          icon="edit_calendar"
          title="Configura tus horarios"
          body="Al aprobarse, define tus franjas semanales en el panel del tutor."
          state="todo"
        />
      </ol>

      {failedFileNames && failedFileNames.length > 0 && (
        <div
          role="alert"
          className="flex w-full max-w-md flex-col gap-3 rounded-xl border border-error/30 bg-error-container/20 p-4 text-left"
        >
          <p className="flex items-start gap-2 text-body-md text-on-error-container">
            <MaterialIcon name="error" className="mt-0.5 shrink-0 text-[18px]" />
            <span>
              Tu postulación se envió, pero no pudimos subir:
              <span className="font-semibold"> {failedFileNames.join(", ")}</span>. Puedes
              reintentar ahora o subirlos luego desde tu postulación.
            </span>
          </p>
          {onRetryDocuments && (
            <button
              type="button"
              onClick={onRetryDocuments}
              disabled={retrying}
              className="inline-flex items-center justify-center gap-2 self-start rounded-lg border border-error/40 px-4 py-2 text-label-md font-semibold text-error transition-colors hover:bg-error-container/30 disabled:opacity-60"
            >
              {retrying ? (
                <>
                  <MaterialIcon name="progress_activity" className="animate-spin text-[16px]" />
                  Reintentando…
                </>
              ) : (
                <>
                  <MaterialIcon name="refresh" className="text-[16px]" />
                  Reintentar subida
                </>
              )}
            </button>
          )}
        </div>
      )}

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

"use client";

/**
 * Tarjeta de foto de perfil del tutor (/tutor/perfil): preview circular con
 * fallback a iniciales + botón subir/reemplazar. La foto es pública (aparece
 * en el perfil público y en el directorio); el backend valida JPEG/PNG/WebP
 * ≤ 5 MB y reemplaza la anterior (upsert).
 */

import { useRef, useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { tutorPhotoUrl, uploadTutorPhoto } from "@/lib/api";

const MAX_PHOTO_BYTES = 5 * 1024 * 1024;
const ACCEPTED = [".jpg", ".jpeg", ".png", ".webp"];

function initialsOf(name: string): string {
  return name
    .replace(/^(Dr\.|Dra\.|Ing\.|Prof\.|M\.Sc\.)\s+/, "")
    .split(" ")
    .slice(0, 2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();
}

export function ProfilePhotoCard({
  tutorId,
  name,
  initialPhotoUrl,
  token,
}: {
  tutorId: string;
  name: string;
  initialPhotoUrl: string | null;
  token: string;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  // Tras subir, el backend sirve la misma URL: se añade ?v=timestamp para
  // invalidar la caché del navegador y mostrar la foto nueva al instante.
  const [photoUrl, setPhotoUrl] = useState<string | null>(initialPhotoUrl);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const handleFile = async (file: File | null) => {
    setError(null);
    setNotice(null);
    if (!file) return;

    const extension = `.${file.name.split(".").pop()?.toLowerCase() ?? ""}`;
    if (!ACCEPTED.includes(extension)) {
      setError("Formato no permitido: usa JPEG, PNG o WebP.");
      return;
    }
    if (file.size > MAX_PHOTO_BYTES) {
      setError("La imagen supera el máximo de 5 MB.");
      return;
    }

    setUploading(true);
    const result = await uploadTutorPhoto(token, file);
    setUploading(false);

    if (!result.ok) {
      setError(result.error);
      return;
    }
    setPhotoUrl(`${tutorPhotoUrl(tutorId)}?v=${Date.now()}`);
    setNotice("Foto actualizada. Aparece en tu perfil público y en el directorio.");
  };

  return (
    <section className="flex flex-col gap-3 rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <h2 className="text-title-md font-bold text-on-surface">Foto de perfil</h2>
      <div className="flex items-center gap-4">
        <div className="relative shrink-0">
          {photoUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={photoUrl}
              alt={`Foto de ${name}`}
              className="h-20 w-20 rounded-xl object-cover shadow-sm"
            />
          ) : (
            <div className="grid h-20 w-20 place-items-center rounded-xl bg-gradient-to-br from-primary to-primary-container text-title-lg font-bold text-on-primary shadow-sm">
              {initialsOf(name)}
            </div>
          )}
          {uploading && (
            <span className="absolute inset-0 grid place-items-center rounded-xl bg-surface-container-lowest/70">
              <MaterialIcon name="progress_activity" className="animate-spin text-[24px] text-primary" />
            </span>
          )}
        </div>
        <div className="min-w-0 flex-1">
          <p className="text-body-sm text-on-surface-variant">
            JPEG, PNG o WebP · hasta 5 MB. Visible para los estudiantes en tu perfil
            público y en el directorio.
          </p>
          <input
            ref={inputRef}
            type="file"
            accept=".jpg,.jpeg,.png,.webp"
            className="sr-only"
            onChange={(e) => {
              void handleFile(e.target.files?.[0] ?? null);
              e.target.value = "";
            }}
          />
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            disabled={uploading}
            className="mt-2 inline-flex items-center gap-1.5 rounded-lg border border-outline px-3.5 py-2 text-label-md font-semibold text-on-surface-variant transition-colors hover:border-primary hover:text-primary disabled:opacity-60"
          >
            <MaterialIcon name={photoUrl ? "photo_camera" : "add_a_photo"} className="text-[16px]" />
            {photoUrl ? "Reemplazar foto" : "Subir foto"}
          </button>
        </div>
      </div>
      {error && (
        <p role="alert" className="text-label-md text-error">
          {error}
        </p>
      )}
      {notice && (
        <p role="status" className="flex items-center gap-1.5 text-label-md text-tertiary">
          <MaterialIcon name="check_circle" className="text-[16px]" />
          {notice}
        </p>
      )}
    </section>
  );
}

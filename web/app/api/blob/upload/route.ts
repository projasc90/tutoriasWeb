import { handleUpload, type HandleUploadBody } from "@vercel/blob/client";
import { NextResponse } from "next/server";

/**
 * Emisión de tokens para la subida directa a Vercel Blob (ADR-009).
 *
 * Por qué existe: Vercel Functions rechaza cuerpos > 4.5 MB y nuestros
 * documentos admiten hasta 5 MB. En vez de subir el archivo al API .NET, el
 * navegador sube directo al store y después el API solo recibe la referencia.
 *
 * Seguridad: `onBeforeGenerateToken` es la única puerta de entrada. Verifica el
 * JWT contra el API .NET y exige que el pathname cuelgue del usuario
 * autenticado; sin esa comprobación cualquiera podría escribir en el store.
 *
 * Si el store no está conectado (sin BLOB_READ_WRITE_TOKEN) se responde 501 y
 * el cliente cae al multipart clásico (dev local sin Blob).
 */

export const runtime = "nodejs";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5037";

/** Tope por archivo, espejo de TutorDocumentService/TutorPhotoService. */
const MAX_SIZE_BYTES = 5 * 1024 * 1024;

/**
 * Tipos permitidos por categoría. Las claves son el segundo segmento del
 * pathname y deben coincidir con lo que genera el API .NET en
 * `VercelBlobFileStorage.BuildPathname` (kind en minúsculas) más `photo`.
 */
const ALLOWED_CONTENT_TYPES: Record<string, string[]> = {
  titulo: ["application/pdf", "image/jpeg", "image/png"],
  certificacion: ["application/pdf", "image/jpeg", "image/png"],
  identificacion: ["application/pdf", "image/jpeg", "image/png"],
  photo: ["image/jpeg", "image/png", "image/webp"],
};

/** El backend guarda los ids sin guiones; el pathname usa el mismo formato. */
function compactId(id: string): string {
  return id.replace(/-/g, "").toLowerCase();
}

/**
 * Valida el JWT contra el API .NET (fuente única de verdad de la sesión) y
 * devuelve el id del usuario o null si no es válido.
 */
async function authenticate(token: string | undefined): Promise<string | null> {
  if (!token) return null;

  try {
    const res = await fetch(`${API_URL}/api/auth/me`, {
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    });
    if (!res.ok) return null;

    const me = (await res.json()) as { id?: string };
    return me.id ?? null;
  } catch {
    return null;
  }
}

/** Deriva `documents` | `photo` del pathname `{userId}/<categoria>/...`. */
function categoryFromPathname(pathname: string): string | null {
  const [, category] = pathname.split("/");
  return category && category in ALLOWED_CONTENT_TYPES ? category : null;
}

/**
 * Sonda de capacidad: el cliente la usa para decidir entre subida directa y
 * multipart sin tener que interpretar el error del SDK.
 */
export async function GET(): Promise<NextResponse> {
  return NextResponse.json({ enabled: Boolean(process.env.BLOB_READ_WRITE_TOKEN) });
}

export async function POST(request: Request): Promise<NextResponse> {
  // Store no conectado: el cliente debe usar el endpoint multipart.
  if (!process.env.BLOB_READ_WRITE_TOKEN) {
    return NextResponse.json(
      { disabled: true, reason: "BLOB_READ_WRITE_TOKEN no configurado" },
      { status: 501 },
    );
  }

  const body = (await request.json()) as HandleUploadBody;

  try {
    const jsonResponse = await handleUpload({
      body,
      request,
      onBeforeGenerateToken: async (pathname, clientPayload) => {
        const userId = await authenticate(clientPayload ?? undefined);
        if (!userId) throw new Error("No autenticado");

        // El pathname lo construye el cliente: debe colgar de su propio id
        // para que no pueda escribir (ni adoptar) blobs de otro tutor.
        if (!pathname.startsWith(`${compactId(userId)}/`)) {
          throw new Error("Ruta de subida no autorizada");
        }

        const category = categoryFromPathname(pathname);
        if (!category) throw new Error("Categoría de archivo inválida");

        return {
          allowedContentTypes: ALLOWED_CONTENT_TYPES[category],
          maximumSizeInBytes: MAX_SIZE_BYTES,
          addRandomSuffix: false,
          // El pathname ya incluye un UUID: se guarda tal cual en la BD.
        };
      },
      onUploadCompleted: async () => {
        // El API .NET confirma la subida (POST .../attach) leyendo los metadatos
        // reales del blob. No se depende de este webhook: no se dispara en
        // localhost y añadiría un segundo camino de escritura en BD.
      },
    });

    return NextResponse.json(jsonResponse);
  } catch (error) {
    // 400 para que el SDK del cliente propague el mensaje en vez de reintentar.
    return NextResponse.json(
      { error: error instanceof Error ? error.message : String(error) },
      { status: 400 },
    );
  }
}

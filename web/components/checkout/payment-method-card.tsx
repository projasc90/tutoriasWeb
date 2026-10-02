"use client";

/**
 * Card "Método de Pago" del checkout (Stitch 03): tabs SINPE Móvil / Tarjeta /
 * IBAN. El flujo activo del repo es SINPE (confirmación manual Admin, ADR-004):
 * tarjeta e IBAN se muestran como próximamente (sin pasarela integrada aún).
 * Formulario SINPE: emisor, nº comprobante, botón copiar del receptor y monto.
 */

import { useState } from "react";
import { MaterialIcon } from "@/components/material-icon";
import { formatCrc } from "@/lib/time";

type Tab = "sinpe" | "card" | "iban";

export function PaymentMethodCard(props: {
  receptorPhone: string;
  amountCrc: number;
  confirmationNumber: string;
  setConfirmationNumber: (v: string) => void;
  phone: string;
  setPhone: (v: string) => void;
  submitted: boolean;
}) {
  const [tab, setTab] = useState<Tab>("sinpe");
  const [copied, setCopied] = useState(false);
  const {
    receptorPhone, amountCrc,
    confirmationNumber, setConfirmationNumber,
    phone, setPhone,
    submitted,
  } = props;

  const copyPhone = async () => {
    try {
      await navigator.clipboard.writeText(receptorPhone);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Portapapeles no disponible (http no seguro): no es un error de usuario
    }
  };

  const tabCls = (t: Tab) =>
    `flex items-center justify-center gap-2 rounded-lg px-3 py-2.5 text-label-md transition-all ${
      tab === t
        ? "bg-primary font-bold text-on-primary shadow-sm"
        : "text-on-surface-variant hover:text-on-surface"
    }`;

  return (
    <section className="rounded-xl bg-surface-container-lowest p-6 shadow-sm">
      <div className="mb-3 flex items-center gap-2">
        <MaterialIcon name="payments" className="text-[22px] text-primary" />
        <h2 className="text-headline-sm text-on-surface">Método de Pago</h2>
      </div>
      <div className="mb-4 grid grid-cols-3 gap-2 rounded-lg bg-surface-container-low p-1">
        <button type="button" onClick={() => setTab("sinpe")} className={tabCls("sinpe")}>
          <MaterialIcon name="smartphone" className="text-[16px]" />
          <span className="font-bold">SINPE Móvil</span>
        </button>
        <button type="button" onClick={() => setTab("card")} className={tabCls("card")}>
          <MaterialIcon name="credit_card" className="text-[16px]" />
          <span>Tarjeta</span>
        </button>
        <button type="button" onClick={() => setTab("iban")} className={tabCls("iban")}>
          <MaterialIcon name="account_balance" className="text-[16px]" />
          <span>IBAN</span>
        </button>
      </div>

      {tab === "sinpe" && (
        <div className="space-y-4">
          <div className="flex flex-col justify-between gap-4 rounded-xl bg-surface-container-low p-4 md:flex-row md:items-center">
            <div>
              <span className="text-label-sm font-bold uppercase tracking-wider text-primary">
                Transferencia Rápida Nacional
              </span>
              <p className="mt-0.5 text-title-md text-on-surface">AuraLearn Centroamérica S.A.</p>
              <div className="mt-1 flex items-center gap-2 text-on-surface">
                <span className="rounded bg-surface-container-highest px-2 py-0.5 font-mono font-bold tracking-wider text-primary">
                  {receptorPhone}
                </span>
                <button
                  type="button"
                  onClick={copyPhone}
                  title="Copiar número"
                  className="ml-1 text-primary hover:text-on-surface"
                  aria-label="Copiar número receptor"
                >
                  <MaterialIcon name={copied ? "check" : "content_copy"} className="text-[16px]" />
                </button>
              </div>
            </div>
            <div className="rounded-lg bg-surface-container-lowest p-3 text-left shadow-sm md:text-right">
              <span className="text-label-sm block text-secondary">Monto exacto a transferir:</span>
              <span className="text-headline-md font-extrabold tracking-tight text-primary">
                {formatCrc(amountCrc)}
              </span>
              <span className="block text-label-sm text-on-surface-variant">
                Motivo: &quot;AURALEARN-TUTORIA&quot;
              </span>
            </div>
          </div>

          <div className="grid grid-cols-1 gap-4 pt-2 md:grid-cols-2">
            <div className="space-y-1.5">
              <label htmlFor="sinpe-phone" className="text-label-md text-on-surface">
                Número de Teléfono Emisor
              </label>
              <div className="relative">
                <span className="absolute left-3 top-1/2 -translate-y-1/2 text-outline">+506</span>
                <input
                  id="sinpe-phone"
                  type="tel"
                  maxLength={9}
                  placeholder="8765-4321"
                  disabled={submitted}
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  className="font-numeric w-full rounded-lg bg-surface-container-low py-2.5 pl-12 pl-12 pr-3 text-on-surface placeholder:text-outline focus:outline-none focus:ring-2 focus:ring-primary/40 disabled:opacity-60"
                />
              </div>
              <p className="text-label-sm text-outline">El número desde el que ejecutó el pase SINPE.</p>
            </div>
            <div className="space-y-1.5">
              <label htmlFor="sinpe-ref" className="text-label-md text-on-surface">
                Número de Comprobante / Referencia
              </label>
              <input
                id="sinpe-ref"
                type="text"
                required
                disabled={submitted}
                value={confirmationNumber}
                onChange={(e) => setConfirmationNumber(e.target.value)}
                placeholder="Ej: 20241024098234"
                className="font-numeric w-full rounded-lg bg-surface-container-low px-3 py-2.5 text-on-surface placeholder:text-outline focus:outline-none focus:ring-2 focus:ring-primary/40 disabled:opacity-60"
              />
              <p className="text-label-sm text-outline">
                Código de 6 a 16 dígitos proporcionado por su banco.
              </p>
            </div>
          </div>

          <div className="space-y-1.5 pt-1">
            <span className="text-label-md text-on-surface">
              Subir captura o comprobante bancario (JPG, PNG o PDF)
            </span>
            <div
              className={`flex flex-col items-center justify-center rounded-xl bg-surface-container-low p-6 text-center transition-colors ${
                submitted ? "opacity-50" : "cursor-pointer hover:bg-surface-container"
              }`}
            >
              <div className="mb-2 flex h-12 w-12 items-center justify-center rounded-full bg-surface-container-high text-primary">
                <MaterialIcon name="upload_file" className="text-[26px]" />
              </div>
              <p className="text-title-md text-on-surface">
                Arrastra el comprobante aquí o{" "}
                <span className="text-primary underline">haz clic para examinar</span>
              </p>
              <p className="text-label-sm text-outline">
                El nº de referencia es la validación activa; el archivo adjunto se habilita con el módulo de storage.
              </p>
            </div>
          </div>
        </div>
      )}

      {tab === "card" && (
        <div className="rounded-xl bg-surface-container-low p-4 text-body-md text-on-surface-variant">
          <p className="text-title-md text-on-surface">Tarjeta de crédito / débito</p>
          <p className="mt-1">
            La pasarela de tarjeta (Stripe 3D Secure) aún no está integrada. Usa SINPE Móvil,
            nuestro método activo de pago en colones.
          </p>
          <div className="mt-3 flex items-center gap-2 text-label-sm">
            <span className="rounded bg-surface-container-lowest px-2 py-0.5 font-bold text-on-surface">VISA</span>
            <span className="rounded bg-surface-container-lowest px-2 py-0.5 font-bold text-on-surface">Mastercard</span>
            <span className="rounded bg-surface-container-lowest px-2 py-0.5 font-bold text-on-surface">AMEX</span>
            <span className="text-outline">· próximamente</span>
          </div>
        </div>
      )}

      {tab === "iban" && (
        <div className="rounded-xl bg-surface-container-low p-4 text-body-md text-on-surface-variant">
          <p className="text-title-md text-on-surface">Transferencia bancaria (IBAN)</p>
          <p className="mt-1">
            Las cuentas IBAN de AuraLearn Centroamérica S.A. estarán disponibles próximamente.
            Usa SINPE Móvil, nuestro método activo de pago en colones.
          </p>
        </div>
      )}
    </section>
  );
}

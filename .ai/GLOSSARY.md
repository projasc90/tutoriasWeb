---
id: glossary
priority: 3
loadWhen: duda-de-dominio
maxLines: 100
---

# Glosario — AuraLearn

## Dominio de negocio

| Término | Definición |
|---------|------------|
| **Tutoría** | Sesión 1-a-1 online entre un estudiante y un tutor verificado, con pizarra digital. |
| **Tutor** | Docente universitario o profesional verificado que imparte tutorías. Rol `TUTOR`. |
| **Estudiante** | Usuario que reserva y paga sesiones. Rol `ESTUDIANTE`. |
| **Admin** | Usuario con permisos de gestión de plataforma. Rol `ADMIN`. |
| **Verificación de credenciales** | Proceso de revisión de atestados y títulos del tutor contra el registro universitario (UCR, TEC, UNA, LEAD). |
| **Atestado** | Certificado de antecedentes/acreditación profesional presentado por el tutor. |
| **SINPE Móvil** | Sistema de pagos instantáneos de Costa Rica; método principal de pago en colones. |
| **Comprobante SINPE** | Notificación/receipt de la transferencia; el backend valida su firma digital. |
| **Firma digital** | Validación criptográfica/administrativa del comprobante SINPE para acreditar el pago. |
| **Slot** | Espacio horario reservable en la agenda del tutor; motor de reservas garantiza exclusividad. |
| **Reserva** | Operación que bloquea un slot, valida pago y confirma la sesión. |
| **Pizarra digital** | Lienzo colaborativo en tiempo real durante la sesión. |
| **Garantía 15'** | Si en los primeros 15 minutos el enfoque no encaja, se reasigna o reembolsa. |
| **Reprogramación** | Cambio de horario implementado como cancelar + crear nueva reserva (decisión 2026-10-01): sin PATCH reschedule para no desincronizar el comprobante pagado. Con ≥12 h de anticipación la cancelación acredita el monedero. |
| **Monedero** | Saldo acreditado en la cuenta del estudiante tras cancelaciones. |
| **Institución** | Universidad de procedencia: UCR, TEC, UNA, LEAD (y extendedas: ULACIT, U Latina). |
| **Nivel académico** | Grado, Bachillerato Internacional (IB), Examen de Admisión (PAA), Posgrado. |
| **PAA** | Prueba de Aptitud Académica (admisión UCR/TEC). |

## Técnicos

| Término | Definición |
|---------|------------|
| **Monorepo** | Repositorio único con subproyectos `api/`, `web/`, `portal/`, `design/`. |
| **base-nova** | Estilo del sistema de componentes shadcn usado en `web/`, sobre `@base-ui/react`. |
| **MD3 / Material 3** | Sistema de diseño cuyos tokens (color, tipografía, radio) viven en `web/app/globals.css`. |
| **Material Symbols** | Fuente de iconos de Google cargada por `web/components/material-icon.tsx`. |
| **Testing gate** | Conjunto mínimo de validaciones obligatorias post-cambio (ver `PROTOCOLS.md`). |
| **ADR** | Architecture Decision Record; índice en `.ai/ADR_LOG.md`. |
| **Rollback manual** | Pasos comentados dentro de la migración/script para revertir sin tooling. |
| **docs/plans/** | Metodología por fases: context, plan, verification, summary. |
| **Carga base** | `CONTEXT_INDEX.md` + `CURRENT_STATE.md` + `AI_CONTEXT.md` (siempre). |

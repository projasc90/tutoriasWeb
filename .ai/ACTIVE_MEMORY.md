---
id: active-memory
priority: 2
loadWhen: solicitudes-ambiguas-o-continuidad
maxLines: 120
---

# Active Memory — AuraLearn

> Máximo 5 entradas. La más reciente arriba con detalle completo; las anteriores resumen en tabla compacta.

## Sprint actual — Sistema de gobernanza y base del frontend

**Periodo:** 2026-09-27 → abierto
**Versión:** 0.1.0

### Hecho
- Configurado el sistema de memoria externa y gobernanza completo: `.ai/` (14 archivos + `archive/`), `docs/plans/`, `.github/` (copilot-instructions, instructions/, skills/, agents/, prompts/, errors/, templates/). Registrado como ADR-001.
- Landing completa en `web/` (hero, rigor, disciplinas, mentores, pasos, testimonios, FAQ, CTAs, footer) y directorio `/tutores` con filtros y orden funcionales sobre datos mock.

### En curso
- Nada abierto a la fecha de este documento.

### Siguientes pasos
1. Extraer `TUTORS`/`MENTORS` y tipos (`Tutor`, `Mentor`, `FilterState`) a `web/lib/data/` antes de integrar API.
2. Implementar proyecto .NET en `api/` (primera decisión de estructura → nueva ADR).
3. Configurar Playwright cuando se active E2E.

---

## Entradas anteriores (resumen)

| Fecha | Ámbito | Resumen |
|-------|--------|---------|
| — | — | (sin historial aún) |

# Contexto — Portal del tutor fase 2: documentos acumulables + materias normalizadas

**Fecha:** 2026-10-11 · **Base:** 0.8.0 · **Objetivo:** 0.9.0

## Alcance

1. **Títulos acumulables:** el tutor puede tener varios archivos de título (máx 5, como certificaciones). Migración del índice único parcial (solo Identificacion queda única).
2. **Documentos post-aprobación:** con estado `Verified`, el tutor puede **agregar** títulos y certificaciones desde su portal (`/tutor/documentos`, pestaña nueva en la subnav); la identificación queda bloqueada (ya fue verificada) → 409 `IDENTIFICATION_LOCKED`.
3. **Materias normalizadas:** primera letra mayúscula siempre (conectores en minúscula), con sugerencia de corrección contra el catálogo (ignorando acentos/mayúsculas). Normalización también en backend (regla "siempre").
4. **Análisis de gaps** de la fase de tutores (entregable documental, sin código).

## Decisiones cerradas (con el usuario)

| Decisión | Valor |
|----------|-------|
| Títulos | Acumulables, máx 5 (igual que certificaciones) |
| Post-aprobación | Agregar títulos/certificaciones; identificación bloqueada |
| Ubicación UI | Pestaña nueva "Documentos" en subnav `/tutor/documentos` |

## Restricciones

- El estado `Verified` no cambia al agregar documentos (no re-abre revisión; los docs nuevos quedan disponibles para el equipo).
- El gate de aprobación (3 kinds) no cambia: 1+ título, 1+ certificación, 1 identificación.
- Sin borrado de documentos en esta fase (fuera de alcance).
- Normalizador de materias compartido: backend `SubjectNormalizer` (Application) + frontend `lib/subjects.ts` (misma lógica).

## Fuera de alcance

- Borrar/reemplazar documentos existentes; descarga del propio archivo por el tutor; re-verificación al agregar docs post-aprobación; catálogo maestro de materias en BD (la sugerencia usa el catálogo estático `SUGGESTED`).

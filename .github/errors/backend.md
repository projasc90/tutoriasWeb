# Catálogo de errores — Backend (api/)

> Se llenará al implementar el proyecto .NET. Cada error nuevo resuelto se agrega automáticamente aquí.
> Formato: nombre · mensaje o síntoma observable · causa · solución · contexto/fecha.

---

## ERR-BE-001 — Migración sin rollback manual (reserva preventiva)

- **Nombre:** migración EF sin sección de rollback manual
- **Síntoma:** al necesitar revertir en producción, no hay pasos documentados y solo existe el `Down()` del tooling.
- **Causa:** asumir que el rollback automático de EF es suficiente.
- **Solución:** toda migración incluye al final `// ROLLBACK MANUAL` con los pasos inversos (SQL/comandos); ver template `.github/templates/migration-with-rollback.md`. Nunca asumir rollback automático.
- **Contexto:** 2026-09-27 — regla establecida en la gobernanza inicial; pendiente de primer caso real.

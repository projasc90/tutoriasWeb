---
applyTo: "portal/**"
description: Portal público separado de AuraLearn (reservado — aplica desde el primer archivo)
---

# Portal — AuraLearn (portal/)

## Estado

Carpeta **reservada** (`.gitkeep`). Se prevé un portal público separado de `web/` (marketing institucional, SEO, contenidos), con su propio proyecto Next.js y ciclo de despliegue.

## Reglas al crearlo

1. Proyecto Next.js independiente con su propio `package.json` (versionado SEMVER propio o alineado al monorepo — decidir con ADR).
2. **No reutilizar** componentes de `web/components/` vía imports cruzados: copiar/extraer a una lib compartida solo si hay necesidad real (y registrar en `PROJECT_MAP.md` + ADR).
3. Tokens de diseño propios consistentes con la marca AuraLearn (no acoplar a los MD3 de `web/` salvo decisión de ADR).
4. SEO como prioridad: metadata por página, sitemap, OG tags, contenido estático/ISR preferible a CSR.
5. Sin lógica de negocio ni llamadas directas a BD: el portal consume APIs públicas del backend o contenido estático.
6. Testing gate al existir código: `cd portal && npm run lint && npx tsc --noEmit` (+ build si riesgo).
7. Registrarse en `.ai/PROJECT_MAP.md` y `.ai/STACK.md` en el momento de crear el proyecto.

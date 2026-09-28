# Prompt — Sistema de Memoria Externa y Gobernanza (AuraLearn)

Actua como Principal Software Engineer. Necesito que configures en este repositorio un sistema completo de memoria externa, gobernanza de codigo, instrucciones contextuales, artefactos de planificacion, catalogos operativos y test gate.

Proyecto:
- Nombre: AuraLearn
- Descripcion: Red de tutorias universitarias verificadas en Centroamerica (UCR, TEC, UNA, LEAD): sesiones 1-a-1 online con pizarra digital y pago en colones (SINPE Movil) o dolares

Stack:
- Backend: .NET Core (C#) + Entity Framework + FluentValidation + JWT (carpeta api/ reservada, aun sin implementar)
- Frontend: Next.js 16 (App Router) + React 19 + TypeScript 5 + Tailwind CSS 4 + shadcn/ui (estilo base-nova)
- Adicional: PostgreSQL, Material Symbols, lucide-react; despliegue frontend en Vercel/Netlify y backend en VPS Linux (systemd/nginx), Azure App Service o AWS
- Directorios principales: api/, portal/, web/, design/
- Idioma docs/comentarios: Español
- Idioma codigo: Ingles
- Auth: JWT emitido por el backend .NET
- Roles: ESTUDIANTE, TUTOR, ADMIN
- OS dev: macOS
- OS prod: Linux (VPS con systemd/nginx) o Azure App Service / AWS Elastic Beanstalk
- Versionado: SEMVER en web/package.json (actual 0.1.0)
- Migraciones: Entity Framework Migrations (dotnet ef) en api/Migrations/

## Objetivo

Crear una base versionable y operativa para que el agente trabaje con continuidad de contexto, reglas claras de ejecucion, testing gate, contexto reusable, artefactos de planificacion y mantenimiento automatico del conocimiento del proyecto.

No generes una plantilla generica. Adapta todo al stack, estructura, dominio y comandos reales de este repositorio (monorepo AuraLearn).

---

## 1. Crea la carpeta `.ai/` como memoria externa versionada del proyecto

Debe incluir como minimo:

- `CONTEXT_INDEX.md`
- `CURRENT_STATE.md`
- `AI_CONTEXT.md`
- `STACK.md`
- `CONVENTIONS.md`
- `PROTOCOLS.md`
- `ADR_LOG.md`
- `PROJECT_MAP.md`
- `ACTIVE_MEMORY.md`
- `TASK_PATTERNS.md`
- `GLOSSARY.md`
- `version-changes.md`
- `CONTEXT_BUDGET.md`
- `archive/` vacio para archivado futuro

Cada archivo `.ai/` debe tener frontmatter YAML con `id`, `priority`, `loadWhen` y `maxLines` cuando aplique.

### 1.1 Reglas de contenido para `.ai/`

#### `CONTEXT_INDEX.md`
- Debe ser el primer archivo a leer siempre.
- Debe catalogar los archivos `.ai/` y explicar cuando cargar cada uno.
- Debe dejar explicita la carga base: `CONTEXT_INDEX.md` + `CURRENT_STATE.md` + `AI_CONTEXT.md`.
- Debe apuntar a `docs/plans/` como artefacto operativo fuera de `.ai/`.
- No debe duplicar reglas profundas; solo orientar la carga.
- Debe incluir protocolo corto de cierre de sesion en este orden:
  1. `CURRENT_STATE.md`
  2. `ACTIVE_MEMORY.md`
  3. `version-changes.md`
  4. `ADR_LOG.md` si aplica
  5. `PROJECT_MAP.md` si aplica
  6. `TASK_PATTERNS.md` si aplica
  7. Confirmacion: "He actualizado los archivos de contexto en `.ai/`"

#### `CURRENT_STATE.md`
- Estado vivo del proyecto.
- Se reemplaza completamente al cierre de cada tarea significativa.
- Maximo 60 lineas.
- Debe incluir: version actual, completado recientemente, pendientes inmediatos y contexto rapido de arquitectura.

#### `AI_CONTEXT.md`
- Router corto always-load.
- No debe duplicar reglas profundas.
- Debe apuntar a `STACK.md`, `CONVENTIONS.md`, `PROTOCOLS.md`, `PROJECT_MAP.md`, `ACTIVE_MEMORY.md`, `TASK_PATTERNS.md` y `CONTEXT_BUDGET.md`.
- Debe incluir entre 3 y 5 reglas tier-1, no una lista larga.

#### `STACK.md`
- Versiones exactas reales de backend, frontend e infraestructura.
- Nota explicita: no cambiar versiones sin justificacion.
- Basarse en archivos reales del repo (`web/package.json`, futuro `api/*.csproj`).

#### `CONVENTIONS.md`
- Idioma de codigo vs idioma de docs.
- Nomenclatura por capa.
- Estructura de carpetas.
- Ubicacion de migraciones, scripts temporales y tests.
- Politicas de archivo: limite de lineas, no hardcode, no mocks persistentes fuera de tests.
- Regla de migraciones: toda migracion o script que altere esquema/datos debe incluir rollback manual comentado.

#### `PROTOCOLS.md`
- Testing Gate exacto.
- TDD escalado por riesgo.
- Revision en dos etapas para cambios grandes.
- Auditoria funcional vs logs tecnicos.
- Async/workers/colas si aplica.
- Observabilidad.
- Seguridad.
- Integraciones externas si aplica.
- ORM o reglas equivalentes de acceso a datos.
- Versionado.
- Gobernanza full-stack de testing.

#### `ADR_LOG.md`
- Debe tener indice rapido.
- Debe registrar ADRs vigentes y supersedidas.
- Debe crear una ADR inicial para la configuracion del sistema de memoria y gobernanza.

#### `PROJECT_MAP.md`
- Mapa semantico del proyecto y responsabilidad de modulos/directorios.
- Debe actualizarse si cambia el ownership o la responsabilidad de un modulo.

#### `ACTIVE_MEMORY.md`
- Sprint/corrimiento actual arriba con detalle completo.
- Anteriores resumidos en tabla compacta.
- Maximo 5 entradas totales.

#### `TASK_PATTERNS.md`
- Bugs recurrentes, patrones y gotchas permanentes.
- Formato: contexto, error observado, causa, solucion, aprendido en.

#### `GLOSSARY.md`
- Terminos del dominio de negocio y terminos tecnicos del proyecto.

#### `version-changes.md`
- Changelog por version con clasificacion `feat`, `fix`, `refactor`, `db`, `test`, `docs`.

#### `CONTEXT_BUDGET.md`
- Presupuesto de contexto por tipo de sesion.
- Debe dejar claro que changelogs historicos y archivos de archive no son de carga automatica.

---

## 2. Crea `docs/plans/` como metodologia ligera por fases

Esta parte es obligatoria en la metodologia actual.

Cuando un cambio sea grande, transversal o de riesgo medio/alto, el proyecto debe usar artefactos en `docs/plans/`:

- `YYYY-MM-DD-<tema>-context.md`
- `YYYY-MM-DD-<tema>-plan.md`
- `YYYY-MM-DD-<tema>-verification.md`
- `YYYY-MM-DD-<tema>-summary.md`

### 2.1 Regla de activacion

Activar este flujo cuando ocurra al menos una de estas condiciones:

- toca multiples archivos o modulos relevantes
- impacto transversal entre backend, frontend, workers, contratos o documentacion operativa
- riesgo medio o alto para produccion
- cambio de mas de cinco archivos relevantes

### 2.2 Reglas por artefacto

- `context`: alcance, decisiones, restricciones, dudas cerradas
- `plan`: tareas atomicas, archivos esperados, criterios de aceptacion
- `verification`: comandos ejecutados, evidencia, riesgos residuales, notas de validacion
- `summary`: que cambio, que no cambio, deuda o proximos pasos

### 2.3 Regla de calidad

- `verification` no reemplaza el testing gate
- `summary` se completa antes del cierre de `.ai/`

---

## 3. Crea `.github/copilot-instructions.md` como punto de entrada real del agente

Debe ser el archivo central de trabajo del agente y contener:

### 3.1 Identidad
- rol de Principal Software Engineer
- prioridad: integridad arquitectonica + continuidad del contexto

### 3.2 Herramientas MCP y herramientas automaticas
- que MCPs o herramientas deben usarse automaticamente segun el stack real
- para este repo: `context7` para verificar APIs de Next.js 16 (el repo advierte en `web/AGENTS.md` que difiere de datos de entrenamiento), `shadcn` para componentes de `web/components/ui`, `playwright` solo cuando se configure E2E

### 3.3 Instrucciones por contexto
- tabla de archivos `.instructions.md` con `applyTo` por carpeta o subproyecto

### 3.4 Skills por contexto
- tabla que indique que skills cargar segun el area o tipo de archivo

### 3.5 Protocolo de contexto
- al iniciar cada nueva sesion o chat, leer siempre primero el archivo principal de instrucciones del proyecto (`.github/copilot-instructions.md` o su equivalente) antes de cualquier otra accion
- leer `CONTEXT_INDEX.md` + `CURRENT_STATE.md` antes de tareas complejas
- leer `AI_CONTEXT.md` en tareas de codigo o arquitectura
- comportamiento context-first:
  - si la solicitud es ambigua -> leer `ACTIVE_MEMORY.md`
  - antes de crear utilidades -> revisar `PROJECT_MAP.md`
  - si contradice un ADR vigente -> advertirlo

### 3.6 Protocolo de cierre proactivo
- actualizar `.ai/` al terminar tareas significativas
- si aplica, cerrar tambien `docs/plans/`

### 3.7 Prohibiciones absolutas
- no hardcode de secretos
- no codigo muerto o comentado innecesario
- no mocks persistentes fuera de tests
- no librerias nuevas sin justificacion clara
- no ignorar errores silenciosamente

### 3.8 Reglas universales
- idioma docs vs idioma codigo
- OS dev/prod
- versionado
- migraciones y rollback manual
- scripts temporales y donde deben vivir
- mantenimiento automatico de errores, skills y contexto

---

## 4. Crea `.github/instructions/` con reglas contextuales reales

Crea al menos:

- `backend.instructions.md`
- `frontend.instructions.md`
- `testing.instructions.md`
- `ai-context.instructions.md`

Y agrega instrucciones adicionales si el repo tiene subproyectos o contextos diferenciados, por ejemplo:

- `portal.instructions.md` (portal publico separado en portal/)
- `infra.instructions.md` (despliegue VPS/Azure, si aplica)

Cada archivo debe tener `applyTo` preciso y contenido adaptado, no texto generico.

### 4.1 `backend.instructions.md`
- arquitectura de capas
- reglas async
- workers/colas si existen
- auditoria funcional
- testing gate backend
- seguridad backend
- observabilidad
- migraciones

### 4.2 `frontend.instructions.md`
- testing gate frontend
- stack real y restricciones de version si aplica
- estructura de features
- convenciones de UI
- modales, estados de carga, errores UX
- reglas de accesibilidad o design system si aplica

### 4.3 `testing.instructions.md`
- estructura de tests por capa
- comando exacto de ejecucion
- definicion de planes de prueba unitarios, integrales/integracion, frontend y regresion cuando aplique
- convencion padre/hijos para planes grandes
- protocolo para actualizar planes al cambiar codigo
- regla de ejecucion ligera por cambio: validar primero el subconjunto mas chico y representativo del area tocada antes de escalar a suites mayores
- excepciones validas al testing gate

### 4.4 `ai-context.instructions.md`
Debe ser la fuente canonica para:

- lectura inicial obligatoria del archivo principal de instrucciones del proyecto al abrir una nueva sesion o chat
- protocolo de carga al iniciar sesion
- mapa de carga condicional (que necesito -> que cargar)
- protocolo de cierre de sesion
- reglas de mantenimiento automatico
- criterio para crear nuevas skills de forma dinamica cuando el agente detecte un dominio recurrente, complejo o no bien cubierto por las skills existentes
- reglas al crear nuevas skills, prompts, instrucciones o catalogos de errores
- limites de tamano y politicas de compresion

---

## 5. Crea `.github/skills/` como conocimiento profundo por dominio

Cada skill debe vivir en `.github/skills/<nombre>/SKILL.md`.

### 5.1 Contenido minimo de cada skill
- cuando usarla
- patrones recomendados
- anti-patrones
- checklist final
- ejemplos reales del stack, no pseudocodigo generico

### 5.2 Skills recomendadas base

- arquitectura backend
- mejores practicas frontend
- tipado frontend
- testing backend
- testing e2e si aplica
- base de datos / migraciones
- seguridad
- performance
- infra / CI-CD

### 5.3 Skills contextuales extra

Si el proyecto tiene dominios especiales, crear skills dedicadas y registrarlas en `copilot-instructions.md`. Ejemplos:

- pagos SINPE y validacion de firma digital de comprobantes
- slots y motor de reservas
- verificacion de credenciales de tutores
- realtime/websockets (pizarra digital)
- observabilidad

### 5.4 Regla de governance para skills

- si el agente detecta que una tarea recurrente, un subdominio del proyecto o una familia de errores requiere conocimiento reutilizable y no existe una skill adecuada, debe crear una skill nueva de forma proactiva
- la creacion dinamica de una skill debe ocurrir cuando aporte claridad, reduzca repeticion o evite errores recurrentes en sesiones futuras
- si se crea una skill nueva, agregarla a la tabla de skills por contexto del archivo `copilot-instructions.md`
- si la nueva skill debe cargarse automaticamente por contexto, agregar tambien su disparador o referencia en las instrucciones/contexto que correspondan
- si la skill cambia la orientacion general del sistema, solo dejar referencia breve en `CONTEXT_INDEX.md`; no duplicar tablas enteras ahi

---

## 6. Crea `.github/agents/` para revisiones o tareas especializadas

Crear archivos `.agent.md` cuando el repositorio realmente se beneficie de agentes especializados.

### 6.1 Agentes base sugeridos

- `architecture-reviewer`
- `security-auditor`
- `testing-qa-specialist`
- `performance-optimizer`

### 6.2 Agentes opcionales por dominio

- `ui-engineer`
- `agente-documentador`
- otros del dominio real del proyecto

### 6.3 Formato minimo
- frontmatter con `name`, `description` y configuracion requerida por el entorno
- rol y responsabilidades
- areas de revision
- anti-patrones o errores frecuentes a buscar

---

## 7. Crea `.github/prompts/` como flujos de trabajo reutilizables

Crear al menos:

- `feature-planning.md`
- `debug-session.md`
- `code-review.md`
- `refactor-guide.md`

Y cualquier otro prompt operativo que el dominio necesite.

### 7.1 Regla para prompts
- todos con frontmatter
- todos deben incluir una seccion explicita de contexto a cargar antes de comenzar
- deben reflejar la metodologia de fases y el cierre de contexto cuando aplique

### 7.2 Contenido esperado

#### `feature-planning.md`
- contexto a cargar
- fases de entendimiento, diseño, implementacion, validacion y cierre

#### `debug-session.md`
- contexto a cargar (`CURRENT_STATE`, errores, patrones)
- pasos de reproduccion, aislamiento, correccion, prevencion

#### `code-review.md`
- hallazgos primero
- riesgos, regresiones, faltantes de prueba

#### `refactor-guide.md`
- como dividir el cambio
- como validar sin perder comportamiento

---

## 8. Crea `.github/errors/` como catalogo de errores resueltos

Crear la carpeta y al menos uno o dos archivos semilla del stack principal.

Formato por entrada:

- nombre del error
- mensaje o sintoma observable
- causa
- solucion
- contexto o fecha si aporta valor

Regla obligatoria:

- cada error nuevo resuelto y no documentado debe agregarse automaticamente

---

## 9. Crea `.github/templates/` como biblioteca de patrones reutilizables

Crear templates reales del stack para los patrones mas frecuentes:

- controller/endpoint (ASP.NET Core)
- service/use-case
- componente frontend
- hook de datos
- migracion con rollback manual
- test base

Si el proyecto tiene contratos o integraciones externas, considerar tambien `.github/contracts/` para documentacion de superficies compartidas.

---

## 10. Configura el Testing Gate y la disciplina de validacion

### 10.1 Testing Gate obligatorio

Usa exactamente estos comandos reales:

- Backend: `cd api && dotnet build && dotnet test` (aplicable cuando exista el proyecto .NET; api/ esta reservada con .gitkeep)
- Frontend: `cd web && npm run lint && npx tsc --noEmit`
- E2E: no configurado aun (pendiente Playwright); no aplica por ahora
- Build: `cd web && npm run build`

Reglas:

1. Si fallan tests existentes, se corrige antes de continuar.
2. Si no hay tests para el codigo modificado, se crea al menos uno cuando el cambio sea de codigo y aplique.
3. Si el cambio afecta mas de una capa, se validan todas las capas tocadas.
4. Si el cambio es solo `.md`, CSS puro, `.env`, `docker-compose` o monitoreo sin logica, el test gate puede no aplicar.

### 10.2 Planes de prueba que debe mantener el sistema

El sistema debe dejar explicito que los planes de prueba no son solo funcionales generales; deben cubrir como minimo:

- escenarios unitarios del modulo o servicio afectado
- escenarios de integracion/integrales para flujos entre capas o dependencias reales
- escenarios de frontend si hay UI o componentes interactivos
- escenarios de regresion cuando se corrige un bug o se evita que un fallo vuelva a ocurrir

Reglas:

- si el proyecto usa carpeta `test/` para planes `.md`, el agente debe crear o actualizar el plan padre del modulo afectado
- si el cambio es grande, los planes pueden segmentarse en padre/hijos por foco (`unit`, `integration`, `frontend`, `regression`, etc.)
- si se corrige un bug productivo o un error repetible, debe agregarse un caso de regresion en el plan correspondiente y, cuando aplique, un test automatizado de regresion

### 10.3 Ejecucion ligera por cambio

El test gate no debe ejecutarse siempre como suite total si existe una validacion mas chica que cubra el cambio con mejor costo/beneficio.

Regla de ejecucion:

1. correr primero la validacion mas acotada y falsable del area tocada
2. si existe test puntual o suite de modulo, usarla antes que toda la suite
3. si el cambio toca frontend con TypeScript, ejecutar al menos `lint` y `type-check` (en este repo: `npm run lint` y `npx tsc --noEmit`; no hay script type-check dedicado)
4. si el cambio toca backend, ejecutar al menos unit o integracion segun la capa afectada
5. escalar a validaciones mayores solo si el riesgo o el alcance lo justifican

Esto debe quedar reflejado tambien en `testing.instructions.md` y en los prompts operativos.

### 10.4 TDD escalado por riesgo

Documenta tres niveles:

- obligatorio test-first para auth/seguridad, pagos SINPE y validacion de firma digital, motor de slots/reservas, migraciones criticas y reglas de negocio sensibles
- recomendado test-first para servicios compartidos, schemas complejos y helpers nucleares
- test-after permitido para documentacion, refactors de bajo riesgo y ajustes visuales menores

### 10.5 Gate frontend para React o frameworks similares

Si el proyecto usa React, Vite, Next.js, Remix, Angular, Vue con TypeScript u otro stack frontend equivalente, el documento debe dejar explicito que despues de cambiar archivos de UI o TypeScript se ejecuta el gate de frontend correspondiente.

Regla minima para stacks React/TypeScript y similares:

- `lint` obligatorio despues de cambios en `.ts`, `.tsx`, `.js`, `.jsx` o archivos equivalentes del framework
- `type-check` obligatorio si el stack usa TypeScript
- `build` cuando el framework o el riesgo del cambio lo ameriten, especialmente en cambios estructurales, de rutas, SSR, bundling o configuracion

Estos comandos ya estan adaptados al framework real de este repositorio (Next.js 16 + TypeScript); no asumir que son iguales en otros repositorios.

### 10.6 Revision en dos etapas para cambios grandes

Cuando un cambio active `docs/plans/`, exigir y documentar:

1. Gate 1: cumplimiento de especificacion
2. Gate 2: calidad tecnica

El Gate 2 debe revisar arquitectura, seguridad, performance si aplica, tests ejecutados y deuda tecnica.

---

## 11. Reglas de mantenimiento automatico

Configura el sistema para que el agente haga esto sin que el usuario lo pida expresamente:

- error nuevo resuelto -> agregar a `.github/errors/`
- error comun o patron repetido -> consolidarlo en el catalogo de errores o en `TASK_PATTERNS.md`, segun corresponda
- termino de negocio nuevo -> agregar a `GLOSSARY.md`
- patron recurrente descubierto -> agregar a `TASK_PATTERNS.md`
- necesidad recurrente de conocimiento especializado no cubierto -> crear nueva skill y registrarla en `copilot-instructions.md`
- skill desactualizada -> actualizar `SKILL.md`
- cambio grande terminado -> cerrar `docs/plans/` y luego actualizar `.ai/`

---

## 12. Reglas de migraciones y rollback manual

Toda migracion o script que altere datos, indices, constraints o esquema debe incluir una seccion comentada de rollback manual dentro del mismo archivo.

Debe cubrir los pasos inversos necesarios para revertir la operacion de forma manual si hace falta.

En Entity Framework, ademas del metodo `Down()`, incluir en comentarios los pasos manuales de rollback (SQL o comandos) para revertir sin depender del tooling.

No asumir rollback automatico.

---

## 13. Reglas de salida

Al terminar:

1. confirmar los archivos creados
2. explicar que partes quedaron adaptadas al stack real
3. ejecutar el test gate que aplique si el repositorio ya tiene comandos funcionales
4. no dejar placeholders `{{...}}` sin reemplazar en los archivos finales

La confirmacion final debe ser:

"Sistema de memoria y gobernanza configurado. Archivos creados: [...]"



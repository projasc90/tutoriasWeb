# Skill — Infra / CI-CD / Despliegue

## Cuándo usarla
Al configurar workflows, Dockerfiles, entornos de despliegue (Vercel/Netlify, VPS systemd/nginx, Azure, AWS) o pipelines de migraciones.

## Topología

| Componente | Destino | Nota |
|------------|---------|------|
| `web/` | Vercel/Netlify | preview por PR, prod por merge a main |
| `api/` (futuro) | VPS Linux (systemd + nginx) o Azure App Service o AWS EB | elegir con ADR |
| PostgreSQL | host del VPS o Azure/RDS managed | backups programados |
| `portal/` (futuro) | Vercel/Netlify | independiente de web |

## Patrones recomendados

### Workflow CI frontend (`.github/workflows/web-ci.yml`)
```yaml
name: web-ci
on:
  pull_request:
    paths: ["web/**"]
jobs:
  verify:
    runs-on: ubuntu-latest
    defaults: { run: { working-directory: web } }
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: 22, cache: npm, cache-dependency-path: web/package-lock.json }
      - run: npm ci
      - run: npm run lint
      - run: npx tsc --noEmit
      - run: npm run build
```

### Backend en VPS (cuando exista)
- `auralearn-api.service` (systemd): `Restart=on-failure`, secretos vía `EnvironmentFile` 600.
- `nginx` reverse proxy: TLS (Let's Encrypt/certbot), HSTS, proxy a Kestrel.
- Despliegue: publicar artefacto (`dotnet publish -c Release`), swap del servicio, migración (`dotnet ef database update`), healthcheck.
- Rollback: conservar la versión anterior del artefacto y revertir migraciones **con los ROLLBACK MANUAL documentados**.

### Order de despliegue con BD
1. Backup de BD.
2. Desplegar código retrocompatible.
3. Migrar BD.
4. Healthcheck + smoke.

## Anti-patrones
- Secretos en YAML del workflow (usar `${{ secrets.* }}`).
- Migrar sin backup y sin plan de rollback.
- `latest` como tag de imagen/versión en prod.
- Cache de dependencias ausente (CI lento).
- Deploy del viernes sin verificación post-deploy.
- Configurar E2E/Playwright en CI antes de que exista la suite.

## Checklist final
- [ ] CI corre lint + type-check (+ build) en PR del área tocada
- [ ] Secretos solo vía secrets del proveedor
- [ ] Migraciones con backup + rollback manual a mano
- [ ] Healthcheck post-deploy verificado
- [ ] Cambio de infra relevante → ADR + `.ai/STACK.md` actualizado

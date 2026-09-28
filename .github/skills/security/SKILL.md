# Skill — Seguridad

## Cuándo usarla
Al implementar o revisar autenticación/autorización, manejo de secretos, validación de entrada, PII o cualquier superficie sensible.

## Principios del repo

- **Backend es la frontera:** JWT emitido por .NET, roles `ESTUDIANTE`|`TUTOR`|`ADMIN` como claims. El frontend nunca valida permisos por su cuenta (solo los refleja).
- **Validación en backend siempre** (FluentValidation); la del frontend es UX.
- **Secretos:** nunca en el repo. Dev: `.env.local` (web) / `appsettings.Development.json` (api, ignorado por git). Prod: variables de entorno / Key Vault.
- **PII sensible:** comprobantes SINPE, atestados, datos de contacto → logs ofuscados, storage con URLs firmadas, acceso por rol.

## Patrones recomendados

### JWT + roles
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* TokenValidationParameters desde config */ });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole(nameof(Role.Admin)))
    .AddPolicy("TutorOrAdmin", p => p.RequireRole(nameof(Role.Tutor), nameof(Role.Admin)));
```

### Cabeceras de seguridad (nginx / edge)
```
Strict-Transport-Security: max-age=63072000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Content-Security-Policy: default-src 'self' https://fonts.googleapis.com https://fonts.gstatic.com
```

### Validación de entrada
- DTOs + FluentValidation en cada endpoint público; rechazar campos no esperados (whitelist).
- Uploads: validar tipo/tamaño; nombres de archivo generados por servidor.

## Anti-patrones
- Secretos commiteados o en `appsettings.json` / `.env` versionados.
- Autorización solo en UI (menús ocultos sin gate backend).
- `catch {}` silencioso que oculta fallos de seguridad.
- JWT en `localStorage` sin criterio (evaluar cookies httpOnly con ADR al implementar auth).
- Mensajes de error que revelan estructura interna (stack traces al cliente).
- CORS en `*` con credenciales.

## Checklist final
- [ ] Endpoint protegido con política de roles explícita
- [ ] Validación backend de todo input
- [ ] Sin secretos en el repo (revisar diff antes de commit)
- [ ] Logs sin PII financiera/documental
- [ ] Cabeceras de seguridad configuradas en el despliegue
- [ ] Revisión con `security-auditor` (`.github/agents/`) en cambios sensibles

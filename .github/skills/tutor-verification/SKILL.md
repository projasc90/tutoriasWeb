# Skill — Verificación de tutores

## Cuándo usarla
Al implementar o revisar el onboarding de tutores, la revisión de atestados y títulos, y el estado de verificación que condiciona su visibilidad en la plataforma.

## Flujo de negocio

1. Tutor postula (`Postular como Docente`) con datos, título y atestados.
2. Estado inicial: `PENDING_REVIEW` (no visible en el catálogo público).
3. Equipo/admin revisa: título contra el registro universitario (UCR, TEC, UNA, LEAD) + atestados ante el colegio profesional.
4. Aprobación en ≤48 h (promesa comercial mostrada en la landing).
5. Estado `VERIFIED` → visible y reservable; `REJECTED` → motivo registrado; re-postulación permitida con documento corregido.

## Patrones recomendados

### Estado explícito y trazable
```csharp
public enum TutorVerificationStatus { PendingReview, UnderReview, Verified, Rejected }
```
- Cada transición registra: quién, cuándo, motivo (`TutorVerified`, `TutorRejected` como eventos de auditoría funcional).
- El catálogo público (`/tutores`, `Mentors`) filtra **en backend** por `Verified`; jamás enviar no-verificados al cliente con un flag oculto.

### Documentos
- Almacenar en storage con URLs firmadas de corta duración; no en la BD ni en el repo.
- Metadatos (tipo, emisor, fecha de emisión) como columnas estructuradas para auditoría.

### API sugerida
```
POST /api/tutor-applications            → postulación (202)
GET  /api/admin/tutor-applications?status=UnderReview
PATCH /api/admin/tutor-applications/{id}/verify   → { decision: "approve"|"reject", reason }
GET  /api/tutor-applications/{id}/status          → progreso del tutor
```

## Anti-patrones
- Hacer visible un tutor sin `Verified` "temporalmente" (la promesa de rigor es el core del producto: "solo admitimos al 8%").
- Borrar/reemplazar documentos rechazados sin historial.
- Decisión de verificación sin motivo persistido (auditoría funcional incompleta).
- Auto-verificación o check manual sin registro.
- Filtrar visibilidad en el frontend con datos no verificados ya cargados.

## Reglas del repo
- TDD recomendado/obligatorio para las transiciones de estado y visibilidad.
- La revisión es humana en primera instancia; si luego se automatiza (OCR/APIs), mantener el registro de decisión.
- Integración con el gate de seguridad: los documentos son PII → acceso restringido por rol `ADMIN`.

## Checklist final
- [ ] Estados de verificación explícitos con historial inmutable
- [ ] Catálogo público solo muestra `Verified`
- [ ] Motivo de rechazo persistido y comunicable
- [ ] Documentos en storage con URLs firmadas (no PII en BD plana)
- [ ] Tests de transición + auditoría de decisión
- [ ] `dotnet build && dotnet test` verde

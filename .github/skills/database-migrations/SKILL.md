# Skill — Base de datos y migraciones (EF Core + PostgreSQL)

## Cuándo usarla
Al crear/modificar migraciones, esquema, índices, constraints o scripts de datos en `api/`.

## Patrones recomendados

### Crear migración
```bash
cd api
dotnet ef migrations add AddReservationsTable
dotnet ef database update
```
Ubicación: `api/Migrations/`.

### Migración con rollback manual (OBLIGATORIO)
```csharp
public partial class AddReservationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "reservations",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                slot_id = table.Column<Guid>(nullable: false),
                student_id = table.Column<Guid>(nullable: false),
                status = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_reservations", x => x.id);
                table.ForeignKey("fk_reservations_slots", x => x.slot_id, "slots", "id");
            });
        migrationBuilder.CreateIndex(
            name: "ix_reservations_slot_id", table: "reservations", column: "slot_id", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "reservations");
    }

    // ROLLBACK MANUAL — pasos inversos sin depender del tooling EF:
    // psql -U auralearn -d auralearn
    // BEGIN;
    //   DROP INDEX IF EXISTS ix_reservations_slot_id;
    //   DROP TABLE IF EXISTS reservations;
    // COMMIT;
    // ⚠️ Si hay datos que preservar: hacer backup previo
    //   pg_dump -U auralearn -t reservations auralearn > reservations_backup.sql
}
```

### Configuración de entidades (OnModelCreating / IEntityTypeConfiguration)
- Nombres en snake_case si la convención de BD lo decide (ADR).
- Índices únicos para invariantes de negocio: slot único por reserva (`unique: true` en `slot_id`).
- `decimal` para precios (nunca `double`); moneda explícita (`CRC`/`USD`).

### Migración de datos (no solo esquema)
```csharp
migrationBuilder.Sql(@"UPDATE tutors SET verification_status = 'PENDING' WHERE verification_status IS NULL;");
// Con su ROLLBACK MANUAL comentado: UPDATE tutors SET verification_status = NULL WHERE verification_status = 'PENDING';
```

## Anti-patrones
- `migrationBuilder.Sql()` sin rollback manual comentado.
- Cambios destructivos (drop column, truncate) sin plan en `docs/plans/`.
- Queries N+1: usar `Include`/`projection` y revisar SQL generado.
- Datos sensibles en columnas planas (pagos, cédulas) — cifrar o tokenizar.
- Asumir que `Down()` es suficiente: **nunca asumir rollback automático**.
- Migrar sin backup en producción.

## Checklist final
- [ ] Migración en `api/Migrations/` con nombre descriptivo
- [ ] `Down()` correcto + sección `// ROLLBACK MANUAL` completa
- [ ] Índices para queries de listado y unicidad de invariantes
- [ ] Cambio destructivo → plan en `docs/plans/` + backup
- [ ] `dotnet ef database update` probado en local desde cero
- [ ] Registrar en `.ai/version-changes.md` con clasificación `db`

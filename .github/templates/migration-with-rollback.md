# Template — Migración con rollback manual

> Regla obligatoria del repo (ver `CONVENTIONS.md` y `PROTOCOLS.md` §11): toda migración o script que altere esquema/datos incluye rollback manual comentado en el mismo archivo, además del `Down()` de EF. **No asumir rollback automático.**

```csharp
// api/Migrations/20260927000000_AddReservationsTable.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddReservationsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "reservations",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                student_id = table.Column<Guid>(type: "uuid", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_reservations", x => x.id);
                table.ForeignKey(
                    name: "fk_reservations_slots_slot_id",
                    column: x => x.slot_id,
                    principalTable: "slots",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        // Invariante: un slot solo puede tener una reserva activa (exclusividad)
        migrationBuilder.CreateIndex(
            name: "uq_reservations_slot_id",
            table: "reservations",
            column: "slot_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "reservations");
    }

    // ROLLBACK MANUAL — pasos inversos sin depender del tooling EF:
    //
    //   1. Backup previo (si hay datos que preservar):
    //        pg_dump -U auralearn -t reservations auralearn > reservations_backup_$(date +%F).sql
    //
    //   2. Revertir el esquema:
    //        psql -U auralearn -d auralearn
    //        BEGIN;
    //          DROP INDEX IF EXISTS uq_reservations_slot_id;
    //          DROP TABLE IF EXISTS reservations;
    //        COMMIT;
    //
    //   3. Confirmar en __EFMigrationsHistory:
    //        DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260927000000_AddReservationsTable';
    //
    // ⚠️ Si la migración incluyó UPDATEs de datos, listar aquí el UPDATE inverso equivalente.
}
```

## Para scripts SQL sueltos (no EF)

```sql
-- script: 2026-09-27_backfill_verification_status.sql
-- Descripción: ...

BEGIN;

UPDATE tutors
SET    verification_status = 'PENDING'
WHERE  verification_status IS NULL;

COMMIT;

-- ROLLBACK MANUAL:
--   UPDATE tutors SET verification_status = NULL WHERE verification_status = 'PENDING';
--   (verificar con SELECT count(*) FROM tutors WHERE verification_status = 'PENDING';)
```

## Checklist
- [ ] `Up` y `Down` correctos y simétricos
- [ ] Sección `// ROLLBACK MANUAL` completa (incluye UPDATEs inversos si hay migración de datos)
- [ ] Índices/unique para invariantes de negocio
- [ ] Probado contra BD vacía (`dotnet ef database update` desde cero)
- [ ] Backup en producción antes de aplicar
- [ ] Registrado en `.ai/version-changes.md` con clasificación `db`

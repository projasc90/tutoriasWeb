using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "tutors",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "tutors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tutors_user_id",
                table: "tutors",
                column: "user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_tutors_users_user_id",
                table: "tutors",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_tutors_users_user_id",
                table: "tutors");

            migrationBuilder.DropIndex(
                name: "ix_tutors_user_id",
                table: "tutors");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "tutors");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "tutors");
        }
    }
}

// ROLLBACK MANUAL (psql contra auralearn_dev):
//   ALTER TABLE tutors DROP CONSTRAINT IF EXISTS fk_tutors_users_user_id;
//   DROP INDEX IF EXISTS ix_tutors_user_id;
//   ALTER TABLE tutors DROP COLUMN IF EXISTS rejection_reason;
//   ALTER TABLE tutors DROP COLUMN IF EXISTS user_id;
// Nota: los 12 tutores seed quedan con user_id NULL (catálogo demo sin dueño).
// Si se necesita revertir datos de postulaciones reales, exportarlas antes:
//   SELECT * FROM tutors WHERE user_id IS NOT NULL;

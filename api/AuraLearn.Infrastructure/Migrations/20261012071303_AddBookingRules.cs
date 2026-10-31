using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cancellation_notice_hours = table.Column<int>(type: "integer", nullable: true),
                    payment_window_minutes = table.Column<int>(type: "integer", nullable: true),
                    slot_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    booking_horizon_days = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_rules", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "booking_rules",
                columns: new[] { "id", "booking_horizon_days", "cancellation_notice_hours", "payment_window_minutes", "slot_duration_minutes", "updated_at", "updated_by_user_id" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 14, 12, 60, 60, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                column: "subjects",
                value: new List<string> { "Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                column: "subjects",
                value: new List<string> { "Python", "Algoritmos", "Estructuras de Datos", "IA" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3303"),
                column: "subjects",
                value: new List<string> { "Física I", "Física II", "Termodinámica", "Mecánica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3304"),
                column: "subjects",
                value: new List<string> { "Química Orgánica", "Bioquímica", "Farmacología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3305"),
                column: "subjects",
                value: new List<string> { "Econometría", "Microeconomía", "Macroeconomía", "Finanzas" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3306"),
                column: "subjects",
                value: new List<string> { "Biología Celular", "Genética", "Microbiología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3307"),
                column: "subjects",
                value: new List<string> { "Circuitos", "Electrónica", "Señales", "Control" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3308"),
                column: "subjects",
                value: new List<string> { "Estadística", "Probabilidad", "R", "Análisis de Datos" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3309"),
                column: "subjects",
                value: new List<string> { "Cálculo III", "Variable Compleja", "Topología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3310"),
                column: "subjects",
                value: new List<string> { "Inglés Académico", "TOEFL", "Redacción", "Español" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3311"),
                column: "subjects",
                value: new List<string> { "Física Médica", "Radiología", "Protección Radiológica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3312"),
                column: "subjects",
                value: new List<string> { "Estática", "Resistencia", "Hormigón", "Diseño Estructural" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_rules");

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                column: "subjects",
                value: new List<string> { "Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                column: "subjects",
                value: new List<string> { "Python", "Algoritmos", "Estructuras de Datos", "IA" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3303"),
                column: "subjects",
                value: new List<string> { "Física I", "Física II", "Termodinámica", "Mecánica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3304"),
                column: "subjects",
                value: new List<string> { "Química Orgánica", "Bioquímica", "Farmacología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3305"),
                column: "subjects",
                value: new List<string> { "Econometría", "Microeconomía", "Macroeconomía", "Finanzas" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3306"),
                column: "subjects",
                value: new List<string> { "Biología Celular", "Genética", "Microbiología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3307"),
                column: "subjects",
                value: new List<string> { "Circuitos", "Electrónica", "Señales", "Control" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3308"),
                column: "subjects",
                value: new List<string> { "Estadística", "Probabilidad", "R", "Análisis de Datos" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3309"),
                column: "subjects",
                value: new List<string> { "Cálculo III", "Variable Compleja", "Topología" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3310"),
                column: "subjects",
                value: new List<string> { "Inglés Académico", "TOEFL", "Redacción", "Español" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3311"),
                column: "subjects",
                value: new List<string> { "Física Médica", "Radiología", "Protección Radiológica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3312"),
                column: "subjects",
                value: new List<string> { "Estática", "Resistencia", "Hormigón", "Diseño Estructural" });
        }
    }
}
/*
// ROLLBACK MANUAL (2026-10-12 · AddBookingRules)
// Contexto: crea la tabla booking_rules (fila única de reglas de reserva
// definidas por el Admin) con seed 12/60/60/14. El Down() ya hace DropTable;
// los UpdateData de tutors son ruido idempotente del snapshot del seed.
//
// Pasos inversos con psql (auralearn_dev):
//   DELETE FROM booking_rules;            -- elimina la fila única (y el seed)
//   DROP TABLE IF EXISTS booking_rules;
//
// O con EF (reversible automáticamente porque solo crea una tabla):
//   cd api && dotnet ef database update AddAvailabilitySettings \
//     --project AuraLearn.Infrastructure --startup-project AuraLearn.Api
//
// Verificación del rollback:
//   psql -d auralearn_dev -c "\dt booking_rules"  → no debe existir.
*/

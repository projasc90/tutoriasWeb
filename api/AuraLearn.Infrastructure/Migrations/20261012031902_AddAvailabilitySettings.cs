using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilitySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "timezone_id",
                table: "tutors",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "weekly_enabled",
                table: "tutor_availability",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "tutor_availability_exceptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutor_availability_exceptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_tutor_availability_exceptions_tutors_tutor_id",
                        column: x => x.tutor_id,
                        principalTable: "tutors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Python", "Algoritmos", "Estructuras de Datos", "IA" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3303"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Física I", "Física II", "Termodinámica", "Mecánica" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3304"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Química Orgánica", "Bioquímica", "Farmacología" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3305"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Econometría", "Microeconomía", "Macroeconomía", "Finanzas" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3306"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Biología Celular", "Genética", "Microbiología" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3307"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Circuitos", "Electrónica", "Señales", "Control" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3308"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Estadística", "Probabilidad", "R", "Análisis de Datos" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3309"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Cálculo III", "Variable Compleja", "Topología" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3310"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Inglés Académico", "TOEFL", "Redacción", "Español" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3311"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Física Médica", "Radiología", "Protección Radiológica" }, "America/Costa_Rica" });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3312"),
                columns: new[] { "subjects", "timezone_id" },
                values: new object[] { new List<string> { "Estática", "Resistencia", "Hormigón", "Diseño Estructural" }, "America/Costa_Rica" });

            migrationBuilder.CreateIndex(
                name: "ux_tutor_availability_exceptions_tutor_date",
                table: "tutor_availability_exceptions",
                columns: new[] { "tutor_id", "date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tutor_availability_exceptions");

            migrationBuilder.DropColumn(
                name: "timezone_id",
                table: "tutors");

            migrationBuilder.DropColumn(
                name: "weekly_enabled",
                table: "tutor_availability");

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

// ROLLBACK MANUAL (psql contra auralearn_dev):
//   DROP TABLE IF EXISTS tutor_availability_exceptions;
//   ALTER TABLE tutor_availability DROP COLUMN IF EXISTS weekly_enabled;
//   ALTER TABLE tutors DROP COLUMN IF EXISTS timezone_id;
//   DELETE FROM __EFMigrationsHistory WHERE "MigrationId" LIKE '%AddAvailabilitySettings%';
// ⚠️ No asumir rollback automático.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TutorDocumentsAccumulateTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_tutor_documents_tutor_kind",
                table: "tutor_documents");

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

            migrationBuilder.CreateIndex(
                name: "ux_tutor_documents_tutor_kind",
                table: "tutor_documents",
                columns: new[] { "tutor_id", "kind" },
                unique: true,
                filter: "kind = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_tutor_documents_tutor_kind",
                table: "tutor_documents");

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

            migrationBuilder.CreateIndex(
                name: "ux_tutor_documents_tutor_kind",
                table: "tutor_documents",
                columns: new[] { "tutor_id", "kind" },
                unique: true,
                filter: "kind IN (1, 3)");
        }
    }
}

// ROLLBACK MANUAL (psql contra auralearn_dev):
//   DROP INDEX IF EXISTS ux_tutor_documents_tutor_kind;
//   CREATE UNIQUE INDEX ux_tutor_documents_tutor_kind ON tutor_documents (tutor_id, kind) WHERE kind IN (1, 3);
//   DELETE FROM __EFMigrationsHistory WHERE "MigrationId" LIKE '%TutorDocumentsAccumulateTitles%';
//   (si hay >1 título por tutor, el CREATE fallará: consolidar filas antes)
// ⚠️ No asumir rollback automático.

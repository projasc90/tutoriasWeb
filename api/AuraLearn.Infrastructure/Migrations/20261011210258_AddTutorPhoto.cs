using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "photo_path",
                table: "tutors",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Python", "Algoritmos", "Estructuras de Datos", "IA" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3303"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Física I", "Física II", "Termodinámica", "Mecánica" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3304"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Química Orgánica", "Bioquímica", "Farmacología" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3305"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Econometría", "Microeconomía", "Macroeconomía", "Finanzas" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3306"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Biología Celular", "Genética", "Microbiología" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3307"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Circuitos", "Electrónica", "Señales", "Control" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3308"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Estadística", "Probabilidad", "R", "Análisis de Datos" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3309"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Cálculo III", "Variable Compleja", "Topología" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3310"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Inglés Académico", "TOEFL", "Redacción", "Español" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3311"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Física Médica", "Radiología", "Protección Radiológica" } });

            migrationBuilder.UpdateData(
                table: "tutors",
                keyColumn: "id",
                keyValue: new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3312"),
                columns: new[] { "photo_path", "subjects" },
                values: new object[] { null, new List<string> { "Estática", "Resistencia", "Hormigón", "Diseño Estructural" } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photo_path",
                table: "tutors");

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
//   ALTER TABLE tutors DROP COLUMN IF EXISTS photo_path;
//   DELETE FROM __EFMigrationsHistory WHERE "MigrationId" LIKE '%AddTutorPhoto%';
//   (los archivos físicos en Storage:DocumentsRoot/{tutorId}/photo/ quedan huérfanos)
// ⚠️ No asumir rollback automático.

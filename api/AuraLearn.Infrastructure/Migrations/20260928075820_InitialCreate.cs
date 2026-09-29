using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tutors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    credentials = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    university = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    rating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    reviews = table.Column<int>(type: "integer", nullable: false),
                    subjects = table.Column<List<string>>(type: "text[]", nullable: false),
                    price_crc = table.Column<int>(type: "integer", nullable: false),
                    price_usd = table.Column<int>(type: "integer", nullable: false),
                    bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    featured = table.Column<bool>(type: "boolean", nullable: false),
                    verification_status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tutors", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "tutors",
                columns: new[] { "id", "bio", "created_at", "credentials", "featured", "name", "price_crc", "price_usd", "rating", "reviews", "subjects", "university", "verification_status" },
                values: new object[,]
                {
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), "8 años de experiencia en tutoría universitaria. Metodología orientada a resultados.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PhD Matemáticas", true, "Dr. Carlos Solano", 14500, 28, 4.98m, 184, new List<string> { "Cálculo I", "Cálculo II", "Álgebra Lineal", "EDOs" }, "UCR", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3302"), "Especialista en preparación técnica para entrevistas en big tech.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Computación", true, "Ing. Sofía Hernández", 16000, 31, 5.00m, 92, new List<string> { "Python", "Algoritmos", "Estructuras de Datos", "IA" }, "TEC", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3303"), "Profesor agregado UCR. Enfoque práctico con más de 500 estudiantes ayudados.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Física", false, "Prof. David Morales", 12000, 23, 4.90m, 115, new List<string> { "Física I", "Física II", "Termodinámica", "Mecánica" }, "UCR", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3304"), "Investigadora postdoctoral. Especialista en química orgánica para carreras de salud.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PhD Química", false, "Dra. Marcela Vargas", 15000, 29, 4.95m, 140, new List<string> { "Química Orgánica", "Bioquímica", "Farmacología" }, "UNA", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3305"), "Economista senior con experiencia en organismos internacionales.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Economía", false, "M.Sc. Javier Rodríguez", 13500, 26, 4.85m, 78, new List<string> { "Econometría", "Microeconomía", "Macroeconomía", "Finanzas" }, "TEC", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3306"), "Docente-investigadora con énfasis en biología molecular y genética.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PhD Biología", false, "Dra. Laura Prop", 14000, 27, 4.92m, 103, new List<string> { "Biología Celular", "Genética", "Microbiología" }, "UCR", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3307"), "Ingeniero electricista con Maestría en TEC. 6 años de experiencia.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Ing. Eléctrica", false, "Ing. Andrés González", 13000, 25, 4.78m, 56, new List<string> { "Circuitos", "Electrónica", "Señales", "Control" }, "TEC", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3308"), "Profesora jubilada UCR. 20 años en estadística aplicada.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PhD Estadística", false, "Dra. Carolina Morales", 15500, 30, 4.88m, 67, new List<string> { "Estadística", "Probabilidad", "R", "Análisis de Datos" }, "UCR", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3309"), "Matemático puro. Doctorado en vías. Dominio completo del cálculo avanzado.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Matemáticas", true, "M.Sc. Ricardo Fernández", 12500, 24, 4.97m, 201, new List<string> { "Cálculo III", "Variable Compleja", "Topología" }, "UNA", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3310"), "Preparadora certificada TOEFL. Metodología inmersiva con materiales auténticos.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "M.Sc. Lingüística", false, "M.Sc. María Pérez", 11000, 21, 4.93m, 88, new List<string> { "Inglés Académico", "TOEFL", "Redacción", "Español" }, "UCR", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3311"), "Físico médico hospitalario. Prepara para exámenes de boards profesionales.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PhD Física Médica", false, "Dr. Jorge Torres", 17000, 33, 4.81m, 44, new List<string> { "Física Médica", "Radiología", "Protección Radiológica" }, "TEC", 3 },
                    { new Guid("3f2504e0-4f89-11d3-9a0c-0305e82c3312"), "Ingeniera civil con maestría en estructuras. Experiencia en proyecto y supervisión.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ing. Civil", false, "Ing. Ana Salas", 14000, 27, 4.76m, 39, new List<string> { "Estática", "Resistencia", "Hormigón", "Diseño Estructural" }, "TEC", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "ix_tutors_university_rating",
                table: "tutors",
                columns: new[] { "university", "rating" });

            migrationBuilder.CreateIndex(
                name: "ix_tutors_verification_status",
                table: "tutors",
                column: "verification_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tutors");
        }
    }
}

// ROLLBACK MANUAL — pasos inversos sin depender del tooling EF:
//
//   1. Backup previo (si hay datos que preservar):
//        pg_dump -U auralearn -h localhost -t tutors auralearn_dev > tutors_backup_$(date +%F).sql
//
//   2. Revertir el esquema:
//        psql -U auralearn -h localhost -d auralearn_dev
//        BEGIN;
//          DROP INDEX IF EXISTS ix_tutors_verification_status;
//          DROP INDEX IF EXISTS ix_tutors_university_rating;
//          DROP TABLE IF EXISTS tutors;
//        COMMIT;
//
//   3. Confirmar en __EFMigrationsHistory:
//        DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260928075820_InitialCreate';
//
//   4. Alternativa con tooling (solo si el historial está consistente):
//        dotnet ef database update 0 --project AuraLearn.Infrastructure --startup-project AuraLearn.Api
//
// ⚠️ No asumir rollback automático: verificar siempre el estado de la BD tras revertir.

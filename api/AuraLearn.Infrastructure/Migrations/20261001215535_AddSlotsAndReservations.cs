using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuraLearn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotsAndReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slots", x => x.id);
                    table.ForeignKey(
                        name: "fk_slots_tutors_tutor_id",
                        column: x => x.tutor_id,
                        principalTable: "tutors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    price_crc = table.Column<int>(type: "integer", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmation_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    comprobante_amount_crc = table.Column<int>(type: "integer", nullable: true),
                    comprobante_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    comprobante_submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                    table.ForeignKey(
                        name: "fk_reservations_slots_slot_id",
                        column: x => x.slot_id,
                        principalTable: "slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reservations_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_crc = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_entries_reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "reservations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_wallet_entries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "ix_reservations_student_created",
                table: "reservations",
                columns: new[] { "student_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_reservations_active_slot",
                table: "reservations",
                column: "slot_id",
                unique: true,
                filter: "status IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "ux_reservations_confirmation_number",
                table: "reservations",
                column: "confirmation_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_reservations_idempotency_key",
                table: "reservations",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_slots_tutor_start",
                table: "slots",
                columns: new[] { "tutor_id", "start_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_entries_reservation_id",
                table: "wallet_entries",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_entries_user_created",
                table: "wallet_entries",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallet_entries");

            migrationBuilder.DropTable(
                name: "reservations");

            migrationBuilder.DropTable(
                name: "slots");

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

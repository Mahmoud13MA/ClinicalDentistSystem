using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceEHRWithChangeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClinicalNotes",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalAlerts",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendations",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XRayFindings",
                table: "EHRs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EHRChangeLogs",
                columns: table => new
                {
                    ChangeLog_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByDoctorId = table.Column<int>(type: "int", nullable: false),
                    ChangedByDoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    EHR_ID = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    Appointment_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EHRChangeLogs", x => x.ChangeLog_ID);
                    table.ForeignKey(
                        name: "FK_EHRChangeLogs_Appointments_Appointment_ID",
                        column: x => x.Appointment_ID,
                        principalTable: "Appointments",
                        principalColumn: "Appointment_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EHRChangeLogs_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EHRChangeLogs_EHRs_EHR_ID",
                        column: x => x.EHR_ID,
                        principalTable: "EHRs",
                        principalColumn: "EHR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EHRChangeLogs_Appointment_ID",
                table: "EHRChangeLogs",
                column: "Appointment_ID");

            migrationBuilder.CreateIndex(
                name: "IX_EHRChangeLogs_DoctorId",
                table: "EHRChangeLogs",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_EHRChangeLogs_EHR_ID",
                table: "EHRChangeLogs",
                column: "EHR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EHRChangeLogs");

            migrationBuilder.DropColumn(
                name: "ClinicalNotes",
                table: "EHRs");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "EHRs");

            migrationBuilder.DropColumn(
                name: "MedicalAlerts",
                table: "EHRs");

            migrationBuilder.DropColumn(
                name: "Recommendations",
                table: "EHRs");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "EHRs");

            migrationBuilder.DropColumn(
                name: "XRayFindings",
                table: "EHRs");
        }
    }
}

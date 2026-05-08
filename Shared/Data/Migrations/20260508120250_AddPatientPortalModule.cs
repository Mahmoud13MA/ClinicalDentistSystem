using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPortalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsentRequests",
                columns: table => new
                {
                    Consent_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Patient_ID = table.Column<int>(type: "int", nullable: false),
                    Doctor_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentRequests", x => x.Consent_ID);
                    table.ForeignKey(
                        name: "FK_ConsentRequests_Doctors_Doctor_ID",
                        column: x => x.Doctor_ID,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsentRequests_Patients_Patient_ID",
                        column: x => x.Patient_ID,
                        principalTable: "Patients",
                        principalColumn: "Patient_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRequests_Doctor_ID",
                table: "ConsentRequests",
                column: "Doctor_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRequests_Patient_ID",
                table: "ConsentRequests",
                column: "Patient_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsentRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Patients");
        }
    }
}

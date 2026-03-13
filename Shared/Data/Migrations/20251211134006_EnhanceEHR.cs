using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceEHR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Medications",
                table: "EHRs",
                newName: "PeriodontalStatus");

            migrationBuilder.RenameColumn(
                name: "Last_Updated",
                table: "EHRs",
                newName: "UpdatedAt");

            migrationBuilder.CreateTable(
                name: "MedicationRecords",
                columns: table => new
                {
                    Medication_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EHR_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationRecords", x => x.Medication_ID);
                    table.ForeignKey(
                        name: "FK_MedicationRecords_EHRs_EHR_ID",
                        column: x => x.EHR_ID,
                        principalTable: "EHRs",
                        principalColumn: "EHR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureRecords",
                columns: table => new
                {
                    Procedure_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToothNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EHR_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureRecords", x => x.Procedure_ID);
                    table.ForeignKey(
                        name: "FK_ProcedureRecords_EHRs_EHR_ID",
                        column: x => x.EHR_ID,
                        principalTable: "EHRs",
                        principalColumn: "EHR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToothRecords",
                columns: table => new
                {
                    ToothRecord_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ToothNumber = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentPlanned = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TreatmentCompleted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Surfaces = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EHR_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToothRecords", x => x.ToothRecord_ID);
                    table.ForeignKey(
                        name: "FK_ToothRecords_EHRs_EHR_ID",
                        column: x => x.EHR_ID,
                        principalTable: "EHRs",
                        principalColumn: "EHR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XRayRecords",
                columns: table => new
                {
                    XRay_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TakenBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EHR_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XRayRecords", x => x.XRay_ID);
                    table.ForeignKey(
                        name: "FK_XRayRecords_EHRs_EHR_ID",
                        column: x => x.EHR_ID,
                        principalTable: "EHRs",
                        principalColumn: "EHR_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRecords_EHR_ID",
                table: "MedicationRecords",
                column: "EHR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureRecords_EHR_ID",
                table: "ProcedureRecords",
                column: "EHR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ToothRecords_EHR_ID",
                table: "ToothRecords",
                column: "EHR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_XRayRecords_EHR_ID",
                table: "XRayRecords",
                column: "EHR_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicationRecords");

            migrationBuilder.DropTable(
                name: "ProcedureRecords");

            migrationBuilder.DropTable(
                name: "ToothRecords");

            migrationBuilder.DropTable(
                name: "XRayRecords");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "EHRs",
                newName: "Last_Updated");

            migrationBuilder.RenameColumn(
                name: "PeriodontalStatus",
                table: "EHRs",
                newName: "Medications");
        }
    }
}

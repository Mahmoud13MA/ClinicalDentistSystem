using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Admin_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Admin_ID);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Nurses",
                columns: table => new
                {
                    NURSE_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nurses", x => x.NURSE_ID);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Patient_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Middle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Last = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Patient_ID);
                });

            migrationBuilder.CreateTable(
                name: "Supplies",
                columns: table => new
                {
                    Supply_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Supply_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplies", x => x.Supply_ID);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Appointment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Ref_Num = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Patient_ID = table.Column<int>(type: "int", nullable: false),
                    Doctor_ID = table.Column<int>(type: "int", nullable: false),
                    Nurse_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Appointment_ID);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_Doctor_ID",
                        column: x => x.Doctor_ID,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Nurses_Nurse_ID",
                        column: x => x.Nurse_ID,
                        principalTable: "Nurses",
                        principalColumn: "NURSE_ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_Patient_ID",
                        column: x => x.Patient_ID,
                        principalTable: "Patients",
                        principalColumn: "Patient_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    T_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<TimeSpan>(type: "time", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Doctor_ID = table.Column<int>(type: "int", nullable: false),
                    Supply_ID = table.Column<int>(type: "int", nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.T_ID);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_StockTransactions_Doctors_Doctor_ID",
                        column: x => x.Doctor_ID,
                        principalTable: "Doctors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Supplies_Supply_ID",
                        column: x => x.Supply_ID,
                        principalTable: "Supplies",
                        principalColumn: "Supply_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EHRs",
                columns: table => new
                {
                    EHR_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalAlerts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    XRayFindings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodontalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClinicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    History = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Patient_ID = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EHRs", x => x.EHR_ID);
                    table.ForeignKey(
                        name: "FK_EHRs_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Appointment_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EHRs_Patients_Patient_ID",
                        column: x => x.Patient_ID,
                        principalTable: "Patients",
                        principalColumn: "Patient_ID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Appointments_Doctor_ID",
                table: "Appointments",
                column: "Doctor_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Nurse_ID",
                table: "Appointments",
                column: "Nurse_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Patient_ID",
                table: "Appointments",
                column: "Patient_ID");

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

            migrationBuilder.CreateIndex(
                name: "IX_EHRs_AppointmentId",
                table: "EHRs",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EHRs_Patient_ID",
                table: "EHRs",
                column: "Patient_ID");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationRecords_EHR_ID",
                table: "MedicationRecords",
                column: "EHR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureRecords_EHR_ID",
                table: "ProcedureRecords",
                column: "EHR_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Doctor_ID",
                table: "StockTransactions",
                column: "Doctor_ID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_DoctorID",
                table: "StockTransactions",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_Supply_ID",
                table: "StockTransactions",
                column: "Supply_ID");

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
                name: "Admins");

            migrationBuilder.DropTable(
                name: "EHRChangeLogs");

            migrationBuilder.DropTable(
                name: "MedicationRecords");

            migrationBuilder.DropTable(
                name: "ProcedureRecords");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "ToothRecords");

            migrationBuilder.DropTable(
                name: "XRayRecords");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "EHRs");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Nurses");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}

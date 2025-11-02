using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    First = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Middle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    Quantity = table.Column<int>(type: "int", nullable: false)
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
                    Medications = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    History = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Treatments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Updated = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                name: "IX_EHRs_AppointmentId",
                table: "EHRs",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EHRs_Patient_ID",
                table: "EHRs",
                column: "Patient_ID");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EHRs");

            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Nurses");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}

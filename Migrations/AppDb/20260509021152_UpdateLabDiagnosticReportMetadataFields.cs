using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class UpdateLabDiagnosticReportMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "LabDiagnosticReportMetadata",
                newName: "ProstheticType");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "LabDiagnosticReportMetadata",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "ReportDate",
                table: "LabDiagnosticReportMetadata",
                newName: "CompletedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProstheticType",
                table: "LabDiagnosticReportMetadata",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "LabDiagnosticReportMetadata",
                newName: "ReportId");

            migrationBuilder.RenameColumn(
                name: "CompletedDate",
                table: "LabDiagnosticReportMetadata",
                newName: "ReportDate");
        }
    }
}

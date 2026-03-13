using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace clinical.APIs.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToSupplyAndPhoneToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Supplies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Supplies");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Patients");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    /// <inheritdoc />
    public partial class dbv11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicenseVerificationReason",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxVerificationReason",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseVerificationReason",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TaxVerificationReason",
                table: "Users");
        }
    }
}

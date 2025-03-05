using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    /// <inheritdoc />
    public partial class dbv12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyWebsite",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyWebsite",
                table: "Users");
        }
    }
}

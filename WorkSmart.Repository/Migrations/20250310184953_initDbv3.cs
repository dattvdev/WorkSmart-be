using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    /// <inheritdoc />
    public partial class initDbv3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHidden",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHidden",
                table: "Jobs");
        }
    }
}

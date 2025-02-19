using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCVTemplateRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CVTemplateId",
                table: "CVs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CVTemplates",
                columns: table => new
                {
                    CVTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Thumbnail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVTemplates", x => x.CVTemplateId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVs_CVTemplateId",
                table: "CVs",
                column: "CVTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_CVs_CVTemplates_CVTemplateId",
                table: "CVs",
                column: "CVTemplateId",
                principalTable: "CVTemplates",
                principalColumn: "CVTemplateId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CVs_CVTemplates_CVTemplateId",
                table: "CVs");

            migrationBuilder.DropTable(
                name: "CVTemplates");

            migrationBuilder.DropIndex(
                name: "IX_CVs_CVTemplateId",
                table: "CVs");

            migrationBuilder.DropColumn(
                name: "CVTemplateId",
                table: "CVs");
        }
    }
}

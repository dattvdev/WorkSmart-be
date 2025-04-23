using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkSmart.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddJobAlertFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkType",
                table: "JobAlerts",
                newName: "SalaryRange");

            migrationBuilder.RenameColumn(
                name: "SalaryLevel",
                table: "JobAlerts",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "NotifyBy",
                table: "JobAlerts",
                newName: "NotificationMethod");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "JobAlerts",
                newName: "JobType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SalaryRange",
                table: "JobAlerts",
                newName: "WorkType");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "JobAlerts",
                newName: "SalaryLevel");

            migrationBuilder.RenameColumn(
                name: "NotificationMethod",
                table: "JobAlerts",
                newName: "NotifyBy");

            migrationBuilder.RenameColumn(
                name: "JobType",
                table: "JobAlerts",
                newName: "City");
        }
    }
}

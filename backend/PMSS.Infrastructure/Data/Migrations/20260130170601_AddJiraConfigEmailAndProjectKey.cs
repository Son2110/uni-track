using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddJiraConfigEmailAndProjectKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "JiraConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectKey",
                table: "JiraConfigs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "JiraConfigs");

            migrationBuilder.DropColumn(
                name: "ProjectKey",
                table: "JiraConfigs");
        }
    }
}

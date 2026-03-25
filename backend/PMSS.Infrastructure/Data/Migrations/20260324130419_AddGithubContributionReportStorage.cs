using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGithubContributionReportStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GithubContributionReports",
                columns: table => new
                {
                    GithubContributionReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCommits = table.Column<int>(type: "int", nullable: false),
                    TotalAdditions = table.Column<int>(type: "int", nullable: false),
                    TotalDeletions = table.Column<int>(type: "int", nullable: false),
                    ContributorCount = table.Column<int>(type: "int", nullable: false),
                    ActiveContributorCount = table.Column<int>(type: "int", nullable: false),
                    ModelProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    InsightsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarkdownContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GithubContributionReports", x => x.GithubContributionReportId);
                    table.ForeignKey(
                        name: "FK_GithubContributionReports_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GithubContributionReports_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GithubContributionReports_GeneratedByUserId",
                table: "GithubContributionReports",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GithubContributionReports_ProjectId",
                table: "GithubContributionReports",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_GithubContributionReports_ProjectId_CreatedAt",
                table: "GithubContributionReports",
                columns: new[] { "ProjectId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GithubContributionReports");
        }
    }
}

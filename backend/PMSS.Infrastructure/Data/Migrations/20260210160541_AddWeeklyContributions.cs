using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyContributions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "GithubRepos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalAdditions",
                table: "GithubRepos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCommits",
                table: "GithubRepos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDeletions",
                table: "GithubRepos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WeeklyContributions",
                columns: table => new
                {
                    WeeklyContributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GithubRepoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeekTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    WeekStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCommits = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalAdditions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalDeletions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyContributions", x => x.WeeklyContributionId);
                    table.ForeignKey(
                        name: "FK_WeeklyContributions_GithubRepos_GithubRepoId",
                        column: x => x.GithubRepoId,
                        principalTable: "GithubRepos",
                        principalColumn: "GithubRepoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWeeklyContributions",
                columns: table => new
                {
                    UserWeeklyContributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeeklyContributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GithubUsername = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Commits = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Additions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Deletions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWeeklyContributions", x => x.UserWeeklyContributionId);
                    table.ForeignKey(
                        name: "FK_UserWeeklyContributions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserWeeklyContributions_WeeklyContributions_WeeklyContributionId",
                        column: x => x.WeeklyContributionId,
                        principalTable: "WeeklyContributions",
                        principalColumn: "WeeklyContributionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyContributions_GithubUsername",
                table: "UserWeeklyContributions",
                column: "GithubUsername");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyContributions_UserId",
                table: "UserWeeklyContributions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyContributions_WeeklyContributionId",
                table: "UserWeeklyContributions",
                column: "WeeklyContributionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyContributions_WeeklyContributionId_GithubUsername",
                table: "UserWeeklyContributions",
                columns: new[] { "WeeklyContributionId", "GithubUsername" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyContributions_GithubRepoId",
                table: "WeeklyContributions",
                column: "GithubRepoId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyContributions_GithubRepoId_WeekTimestamp",
                table: "WeeklyContributions",
                columns: new[] { "GithubRepoId", "WeekTimestamp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyContributions_WeekStart_WeekEnd",
                table: "WeeklyContributions",
                columns: new[] { "WeekStart", "WeekEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWeeklyContributions");

            migrationBuilder.DropTable(
                name: "WeeklyContributions");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "GithubRepos");

            migrationBuilder.DropColumn(
                name: "TotalAdditions",
                table: "GithubRepos");

            migrationBuilder.DropColumn(
                name: "TotalCommits",
                table: "GithubRepos");

            migrationBuilder.DropColumn(
                name: "TotalDeletions",
                table: "GithubRepos");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PMSS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameClassSectionToClassCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Section",
                table: "Classes",
                newName: "ClassCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClassCode",
                table: "Classes",
                newName: "Section");
        }
    }
}

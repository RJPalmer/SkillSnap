using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class FixModelRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUserProjects_PortfolioUsers_PortfolioUserId1",
                table: "PortfolioUserProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUserProjects_Projects_ProjectId1",
                table: "PortfolioUserProjects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUserProjects_PortfolioUserId1",
                table: "PortfolioUserProjects");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUserProjects_ProjectId1",
                table: "PortfolioUserProjects");

            migrationBuilder.DropColumn(
                name: "PortfolioUserId1",
                table: "PortfolioUserProjects");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                table: "PortfolioUserProjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortfolioUserId1",
                table: "PortfolioUserProjects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId1",
                table: "PortfolioUserProjects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserProjects_PortfolioUserId1",
                table: "PortfolioUserProjects",
                column: "PortfolioUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserProjects_ProjectId1",
                table: "PortfolioUserProjects",
                column: "ProjectId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUserProjects_PortfolioUsers_PortfolioUserId1",
                table: "PortfolioUserProjects",
                column: "PortfolioUserId1",
                principalTable: "PortfolioUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUserProjects_Projects_ProjectId1",
                table: "PortfolioUserProjects",
                column: "ProjectId1",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}

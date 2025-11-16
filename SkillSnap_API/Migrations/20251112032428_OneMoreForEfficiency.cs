using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class OneMoreForEfficiency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortfolioUserId1",
                table: "PortfolioUserSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkillId1",
                table: "PortfolioUserSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserSkills_PortfolioUserId1",
                table: "PortfolioUserSkills",
                column: "PortfolioUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserSkills_SkillId1",
                table: "PortfolioUserSkills",
                column: "SkillId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUserSkills_PortfolioUsers_PortfolioUserId1",
                table: "PortfolioUserSkills",
                column: "PortfolioUserId1",
                principalTable: "PortfolioUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUserSkills_Skills_SkillId1",
                table: "PortfolioUserSkills",
                column: "SkillId1",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUserSkills_PortfolioUsers_PortfolioUserId1",
                table: "PortfolioUserSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUserSkills_Skills_SkillId1",
                table: "PortfolioUserSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUserSkills_PortfolioUserId1",
                table: "PortfolioUserSkills");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUserSkills_SkillId1",
                table: "PortfolioUserSkills");

            migrationBuilder.DropColumn(
                name: "PortfolioUserId1",
                table: "PortfolioUserSkills");

            migrationBuilder.DropColumn(
                name: "SkillId1",
                table: "PortfolioUserSkills");
        }
    }
}

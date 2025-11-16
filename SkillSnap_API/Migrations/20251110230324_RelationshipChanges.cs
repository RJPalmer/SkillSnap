using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skills_PortfolioUsers_PortfolioUserId",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_Skills_PortfolioUserId",
                table: "Skills");

            migrationBuilder.CreateTable(
                name: "PortfolioUserSkills",
                columns: table => new
                {
                    PortfolioUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    SkillId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioUserSkills", x => new { x.PortfolioUserId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_PortfolioUserSkills_PortfolioUsers_PortfolioUserId",
                        column: x => x.PortfolioUserId,
                        principalTable: "PortfolioUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioUserSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserSkills_SkillId",
                table: "PortfolioUserSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioUserSkills");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_PortfolioUserId",
                table: "Skills",
                column: "PortfolioUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skills_PortfolioUsers_PortfolioUserId",
                table: "Skills",
                column: "PortfolioUserId",
                principalTable: "PortfolioUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

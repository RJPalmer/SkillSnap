using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialProjectNormalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_PortfolioUsers_PortfolioUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PortfolioUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PortfolioUserId",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "PortfolioUserProjects",
                columns: table => new
                {
                    PortfolioUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioUserId1 = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId1 = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioUserProjects", x => new { x.PortfolioUserId, x.ProjectId });
                    table.ForeignKey(
                        name: "FK_PortfolioUserProjects_PortfolioUsers_PortfolioUserId",
                        column: x => x.PortfolioUserId,
                        principalTable: "PortfolioUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioUserProjects_PortfolioUsers_PortfolioUserId1",
                        column: x => x.PortfolioUserId1,
                        principalTable: "PortfolioUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioUserProjects_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PortfolioUserProjects_Projects_ProjectId1",
                        column: x => x.ProjectId1,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserProjects_PortfolioUserId1",
                table: "PortfolioUserProjects",
                column: "PortfolioUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserProjects_ProjectId",
                table: "PortfolioUserProjects",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUserProjects_ProjectId1",
                table: "PortfolioUserProjects",
                column: "ProjectId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioUserProjects");

            migrationBuilder.AddColumn<int>(
                name: "PortfolioUserId",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PortfolioUserId",
                table: "Projects",
                column: "PortfolioUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_PortfolioUsers_PortfolioUserId",
                table: "Projects",
                column: "PortfolioUserId",
                principalTable: "PortfolioUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

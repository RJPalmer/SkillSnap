using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioUserIdToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId1",
                table: "PortfolioUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioUsers_ApplicationUserId1",
                table: "PortfolioUsers",
                column: "ApplicationUserId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId1",
                table: "PortfolioUsers",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId1",
                table: "PortfolioUsers");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUsers_ApplicationUserId1",
                table: "PortfolioUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "PortfolioUsers");
        }
    }
}

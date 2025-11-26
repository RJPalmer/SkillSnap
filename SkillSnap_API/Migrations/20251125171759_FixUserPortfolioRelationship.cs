using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class FixUserPortfolioRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId1",
                table: "PortfolioUsers");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioUsers_ApplicationUserId1",
                table: "PortfolioUsers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId1",
                table: "PortfolioUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers");

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
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId",
                table: "PortfolioUsers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioUsers_AspNetUsers_ApplicationUserId1",
                table: "PortfolioUsers",
                column: "ApplicationUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

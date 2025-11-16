using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class MoreRelationshipChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PortfolioUserId",
                table: "Skills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PortfolioUserId",
                table: "Skills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

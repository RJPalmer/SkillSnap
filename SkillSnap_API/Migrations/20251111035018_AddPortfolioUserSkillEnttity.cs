using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillSnap_API.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioUserSkillEnttity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Proficiency",
                table: "PortfolioUserSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Proficiency",
                table: "PortfolioUserSkills");
        }
    }
}

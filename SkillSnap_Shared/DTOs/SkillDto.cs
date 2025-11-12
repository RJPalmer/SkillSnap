using SkillSnap.Shared.Models;
namespace SkillSnap.Shared.DTOs;


public class SkillDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Level { get; set; }
    public required ICollection<PortfolioUser> PortfolioUsers { get; set; }
}
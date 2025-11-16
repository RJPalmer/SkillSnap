using SkillSnap.Shared.Models;
namespace SkillSnap.Shared.DTOs;

public class SkillCreateDto
{
    public required string Name { get; set; }
    public required string Level { get; set; }

    public List<PortfolioUserSkillCreateDto>? PortfolioUsers { get; set; } = new();
}
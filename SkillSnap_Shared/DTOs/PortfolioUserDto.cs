
namespace SkillSnap.Shared.DTOs;

public class PortfolioUserDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Bio { get; set; }
    public required string ProfileImageUrl { get; set; }

    public List<ProjectDto>? Projects { get; set; } = new();
    public List<PortfolioUserSkillDto>? PortfolioUserSkills { get; set; } = new List<PortfolioUserSkillDto>();
}

namespace SkillSnap.Shared.DTOs;

public class PortfolioUserDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Bio { get; set; }
    public required string ProfileImageUrl { get; set; }

    public List<PortfolioUserProjectDto>? Projects { get; set; } = new List<PortfolioUserProjectDto>();
    public List<PortfolioUserSkillDto>? PortfolioUserSkills { get; set; } = new List<PortfolioUserSkillDto>();
}
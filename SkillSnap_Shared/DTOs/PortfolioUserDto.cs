using SkillSnap.Shared.DTOs;

namespace SkillSnap.Shared.DTOs;

public class PortfolioUserDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Bio { get; set; }
    public required string ProfileImageUrl { get; set; }

    public ICollection<PortfolioUserSkillDto> portfolioUserSkills { get; set; } = new List<PortfolioUserSkillDto>();
}
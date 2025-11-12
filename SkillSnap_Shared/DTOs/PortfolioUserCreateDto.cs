using System.Collections.Generic;

namespace SkillSnap.Shared.DTOs;

public class PortfolioUserCreateDto
{
    public required string Name { get; set; }
    public required string Bio { get; set; }
    public required string ProfileImageUrl { get; set; }
    public List<ProjectCreateDto> Projects { get; set; } = new List<ProjectCreateDto>();
    public ICollection<PortfolioUserDto> portfolioUserSkills { get; set; } = new List<PortfolioUserDto>();
}
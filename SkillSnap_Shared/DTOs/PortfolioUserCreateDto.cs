using System.ComponentModel.DataAnnotations;
namespace SkillSnap.Shared.DTOs;

public class PortfolioUserCreateDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Bio { get; set; } = string.Empty;

    [Url]
    public string ProfileImageUrl { get; set; } = string.Empty;
    public List<PortfolioUserProjectDto> Projects { get; set; } = new();
    public ICollection<PortfolioUserSkillDto> PortfolioUserSkills { get; set; } = new List<PortfolioUserSkillDto>();
}
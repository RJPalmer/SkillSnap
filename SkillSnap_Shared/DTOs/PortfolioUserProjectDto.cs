namespace SkillSnap.Shared.DTOs;

public class PortfolioUserProjectDto
{
    public int PortfolioUserId { get; set; }
    public int ProjectId { get; set; }

    // Optional: Include nested DTOs for richer detail
    public ProjectDto? Project { get; set; }
    public PortfolioUserDto? PortfolioUser { get; set; }
}
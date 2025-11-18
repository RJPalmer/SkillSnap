namespace SkillSnap.Shared.Models;

public class PortfolioUserProject
{
    public int PortfolioUserId { get; set; }
    public PortfolioUser PortfolioUser { get; set; } = default!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = default!;
}
namespace SkillSnap.Shared.Models;

public class PortfolioUserProject
{
    public int Id {get; set; }

    public int PortfolioUserId {get; set;}

    public int projectId {get; set;}
    
    public required PortfolioUser portfolioUser {get; set;}

    public required Project project{get; set;}
}
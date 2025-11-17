using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Shared.Models;

public class Project
{
    [Key]
    public int Id { get; set; }

    public required string Title { get; set; }

    // Keep a Name property alias for compatibility (some pages referenced Name)
    public string Name => Title;

    public required string Description { get; set; }

    public required string ImageUrl { get; set; }

    // public int PortfolioUserId { get; set; }
    public ICollection<PortfolioUserProject> portfolioUserProjects { get; set; }
}

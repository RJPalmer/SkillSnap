using SkillSnap.Shared.DTOs;

namespace SkillSnap.Shared.Models;

public class PortfolioUserProject
{

    // Foreign key to PortfolioUser
    public int PortfolioUserId { get; set; }

    public PortfolioUser PortfolioUser { get; set; } = default!;

    // Foreign key to Project
    public int ProjectId { get; set; }

    public Project Project { get; set; } = default!;

    /// <summary>
    /// Converts the PortfolioUserProject model to its corresponding DTO.
    /// </summary>
    /// <returns></returns>

    internal PortfolioUserProjectDto ToDto()
    {
        return new PortfolioUserProjectDto
        {
            Project = this.Project != null ? new ProjectDto
            {
                Id = this.Project.Id,
                Title = this.Project.Title,
                Description = this.Project.Description,
                ImageUrl = this.Project.ImageUrl
            } : null
        };
    }
}
using SkillSnap.Shared.DTOs;

namespace SkillSnap.Shared.Models;

/// <summary>
/// Join table linking PortfolioUser and Project in a many-to-many relationship.
/// EF Core will configure the composite key (PortfolioUserId, ProjectId)
/// in the SkillSnapDbContext.
/// </summary>
public class PortfolioUserProject
{
    /// <summary>
    /// Foreign key: the PortfolioUser this link belongs to.
    /// </summary>
    public int PortfolioUserId { get; set; }

    /// <summary>
    /// Navigation property: the associated PortfolioUser.
    /// This will only be populated if explicitly included in queries.
    /// </summary>
    public PortfolioUser? PortfolioUser { get; set; }

    /// <summary>
    /// Foreign key: the Project this link belongs to.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Navigation property: the associated Project.
    /// This will only be populated if explicitly included in queries.
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Converts this join entity into a DTO-safe form.
    /// This prevents circular serialization issues and ensures the API
    /// only returns required DTO-safe structures.
    /// </summary>
    public PortfolioUserProjectDto ToDto()
    {
        return new PortfolioUserProjectDto
        {
            PortfolioUserId = PortfolioUserId,
            ProjectId = ProjectId,

            // Convert nested navigation models only if they were included
            PortfolioUser = PortfolioUser?.ToDto(),
            Project = Project?.ToDto()
        };
    }
}
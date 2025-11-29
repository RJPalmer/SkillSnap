using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SkillSnap.Shared.Models;

/// <summary>
/// Represents a project that can be linked to PortfolioUsers through the
/// PortfolioUserProject join table. This model is EF- and JSON-safe and
/// designed for DTO-based serialization.
/// </summary>
public class Project
{
    /// <summary>
    /// Primary key for the Project entity.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The display title of the project.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Convenience alias for Title (maintains backward compatibility).
    /// </summary>
    public string Name => Title;

    /// <summary>
    /// A short description of the project.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// URL of the project's representative image.
    /// </summary>
    public required string ImageUrl { get; set; }

    /// <summary>
    /// Many-to-many navigation property linking PortfolioUsers and Projects.
    /// EF Core populates this only when included explicitly.
    /// </summary>
    public ICollection<PortfolioUserProject> PortfolioUserProjects { get; set; } = new List<PortfolioUserProject>();
    /// <summary>
    /// Converts this Project entity into a DTO-safe representation.
    /// This avoids exposing EF navigation properties directly.
    /// </summary>
    public SkillSnap.Shared.DTOs.ProjectDto ToDto()
    {
        return new SkillSnap.Shared.DTOs.ProjectDto
        {
            Id = this.Id,
            Title = this.Title,
            Description = this.Description,
            ImageUrl = this.ImageUrl
        };
    }
}

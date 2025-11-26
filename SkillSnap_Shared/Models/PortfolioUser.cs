using System.ComponentModel.DataAnnotations;
using SkillSnap.Shared.Models;
using System.Collections.Generic;
using System.Collections;

namespace SkillSnap.Shared.Models;

public class PortfolioUser
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Bio { get; set; }

    // Keep both property names to be compatible with existing client markup
    public required string ProfileImageUrl { get; set; }

    public string? ProfilePictureUrl => ProfileImageUrl;

    public string? ApplicationUserId { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<PortfolioUserProject> portfolioUserProjects { get; set; } = new List<PortfolioUserProject>();

    public ICollection<PortfolioUserSkill> PortfolioUserSkills { get; set; } = new List<PortfolioUserSkill>();

    /// <summary>
    /// Converts PortfolioUser to PortfolioUserDto
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object ToDto()
    {
        return new DTOs.PortfolioUserDto
        {
            Id = this.Id,
            Name = this.Name,
            Bio = this.Bio,
            ProfileImageUrl = this.ProfileImageUrl,
            Projects = this.portfolioUserProjects != null ? 
                new List<DTOs.PortfolioUserProjectDto>(
                    System.Linq.Enumerable.Select(this.portfolioUserProjects, pup => pup.ToDto())
                ) : [],
            PortfolioUserSkills = this.PortfolioUserSkills != null ? 
                new List<DTOs.PortfolioUserSkillDto>(
                    System.Linq.Enumerable.Select(this.PortfolioUserSkills, pus => pus.ToDto())
                ) : []
        };
    }
}

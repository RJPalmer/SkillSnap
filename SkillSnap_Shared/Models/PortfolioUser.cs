using System.ComponentModel.DataAnnotations;
using SkillSnap.Shared.Models;
using System.Collections.Generic;

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

    public required List<Project> Projects { get; set; }

   public ICollection<PortfolioUserSkill> PortfolioUserSkills { get; set; } = new List<PortfolioUserSkill>();
}

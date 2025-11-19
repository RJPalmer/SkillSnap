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

    public string? ApplicationUserId {get; set;} = string.Empty;

    public ApplicationUser? ApplicationUser{get; set;} = new ApplicationUser();

    public ICollection<PortfolioUserProject> portfolioUserProjects{ get; set; } = new List<PortfolioUserProject>();

   public ICollection<PortfolioUserSkill> PortfolioUserSkills { get; set; } = new List<PortfolioUserSkill>();
}

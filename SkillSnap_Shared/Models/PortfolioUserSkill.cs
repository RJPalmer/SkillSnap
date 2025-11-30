using System.ComponentModel.DataAnnotations;
using SkillSnap.Shared.DTOs;

namespace SkillSnap.Shared.Models;

public class PortfolioUserSkill
{
    // Foreign key to PortfolioUser
    public int PortfolioUserId { get; set; }

    public required PortfolioUser PortfolioUser { get; set; }

    // Foreign key to Skill
    public int SkillId { get; set; }

    public required Skill Skill { get; set; }

    public string Proficiency { get; set; } = string.Empty; // optional metadata

    /// <summary>
    /// Converts PortfolioUserSkill to PortfolioUserSkillDto
    /// </summary>
    /// <returns></returns>
   
    internal PortfolioUserSkillDto ToDto()
    {
        return new PortfolioUserSkillDto
        {
            PortfolioUser = this.PortfolioUser != null ? new PortfolioUserDto
            {
                Id = this.PortfolioUser.Id,
                Name = this.PortfolioUser.Name,
                Bio = this.PortfolioUser.Bio,
                ProfileImageUrl = this.PortfolioUser.ProfileImageUrl
            } : null,
            ProficiencyLevel = this.Proficiency,
            Skill = this.Skill != null ? new SkillDto
            {
                Id = this.Skill.Id,
                Name = this.Skill.Name,
                Level = this.Skill.Level
            } : null
        };     
    }
}
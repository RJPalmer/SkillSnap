using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Shared.Models;

public class PortfolioUserSkill
{
    public required int PortfolioUserId { get; set; }
    public required PortfolioUser PortfolioUser { get; set; }

    public required int SkillId { get; set; }
    public required Skill Skill { get; set; }

    public string Proficiency { get; set; } = string.Empty; // optional metadata
}
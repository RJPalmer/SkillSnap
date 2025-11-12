using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Shared.Models;

public class Skill
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Level { get; set; }

    // public int PortfolioUserId { get; set; }

    public ICollection<PortfolioUserSkill> SkillPortfolioUsers { get; set; } = new List<PortfolioUserSkill>();
}

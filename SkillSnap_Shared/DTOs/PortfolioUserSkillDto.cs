namespace SkillSnap.Shared.DTOs;

/// <summary>
/// DTO representing the relationship between a PortfolioUser and a Skill.
/// </summary>
public class PortfolioUserSkillDto
{
    /// <summary>
    /// The ID of the PortfolioUser.
    /// </summary>
    public int PortfolioUserId { get; set; }

    /// <summary>
    /// The ID of the Skill.
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    /// The user's proficiency level for this skill (optional).
    /// </summary>
    public string? ProficiencyLevel { get; set; }

    /// <summary>
    /// Number of years of experience with this skill (optional).
    /// </summary>
    public int? YearsExperience { get; set; }

    /// <summary>
    /// Basic details about the Skill itself (optional nested DTO).
    /// </summary>
    public SkillDto? Skill { get; set; }

    /// <summary>
    /// Basic details about the PortfolioUser (optional nested DTO).
    /// </summary>
    public PortfolioUserDto? PortfolioUser { get; set; }
}

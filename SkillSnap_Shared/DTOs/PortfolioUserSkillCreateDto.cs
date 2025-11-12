namespace SkillSnap_Shared.DTOs;

/// <summary>
/// DTO for creating or updating a PortfolioUserSkill relationship.
/// </summary>
public class PortfolioUserSkillCreateDto
{
    /// <summary>
    /// The ID of the PortfolioUser to associate the skill with.
    /// </summary>
    public int PortfolioUserId { get; set; }

    /// <summary>
    /// The ID of the Skill being associated.
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    /// (Optional) User's proficiency level or experience with the skill.
    /// </summary>
    public string? ProficiencyLevel { get; set; }

    /// <summary>
    /// (Optional) Number of years of experience the user has with the skill.
    /// </summary>
    public int? YearsExperience { get; set; }
}

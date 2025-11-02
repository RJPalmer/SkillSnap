using System.ComponentModel.DataAnnotations;

namespace SkillSnap_API.models;

public class PortfolioUser
{
    [Key]
    public int Id { get; set; }

    public required string /* The `Name` property in the `PortfolioUser` class is a string property that represents the name of the user. It is used to store and retrieve the name of the user associated with the portfolio. */
    Name { get; set; }

    public required string Bio { get; set; }

    public required string ProfileImageUrl { get; set; }

    public required List<Project> Projects { get; set; }

    public required List<Skill> Skills { get; set; }
}
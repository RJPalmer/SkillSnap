using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap_API.models;

public class Skill
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Level { get; set; }

    [ForeignKey("PortfolioUser")]
    public int PortfolioUserId { get; set; }
}


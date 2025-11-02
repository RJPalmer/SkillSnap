using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap_API.models;

public class Project
{
    [Key]
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    public required string ImageUrl { get; set; }

    [ForeignKey("PortfolioUser")]
    public int PortfolioUserId { get; set; }
}
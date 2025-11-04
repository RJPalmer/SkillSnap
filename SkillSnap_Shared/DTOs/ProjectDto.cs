using System;
namespace SkillSnap.Shared.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string ImageUrl { get; set; }
    public int PortfolioUserId { get; set; }
}
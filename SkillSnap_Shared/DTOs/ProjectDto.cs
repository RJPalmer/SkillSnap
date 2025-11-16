using System;
namespace SkillSnap.Shared.DTOs;

public class ProjectDto
{
    public ProjectDto()
    {
        Id = 0;
        Title = string.Empty;
        Description = string.Empty;
        ImageUrl = string.Empty;
    }
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string ImageUrl { get; set; }
    public int PortfolioUserId { get; set; }
}
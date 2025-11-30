using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using SkillSnap.Shared.DTOs;

namespace SkillSnap.Shared.Models
{
    /// <summary>
    /// Represents a user portfolio. A PortfolioUser can have multiple projects and skills.
    /// </summary>
    public class PortfolioUser
    {
        /// <summary>
        /// Primary key for the PortfolioUser entity.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Bio of the user.
        /// </summary>
        [Required]
        public string Bio { get; set; } = null!;

        /// <summary>
        /// URL of the profile image.
        /// </summary>
        [Required]
        public string? ProfileImageUrl { get; set; } = null!;

        /// <summary>
        /// Convenience property alias for ProfileImageUrl (backward compatibility).
        /// </summary>
        public string? ProfilePictureUrl => ProfileImageUrl;

        /// <summary>
        /// Foreign key to associated ApplicationUser (nullable until linked).
        /// </summary>
        public string? ApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property for the associated ApplicationUser.
        /// </summary>
        public ApplicationUser? ApplicationUser { get; set; }

        /// <summary>
        /// Many-to-many navigation property linking this PortfolioUser to Projects.
        /// Initialized to avoid null reference issues.
        /// </summary>
        public ICollection<PortfolioUserProject> PortfolioUserProjects { get; set; } = new List<PortfolioUserProject>();

        /// <summary>
        /// Many-to-many navigation property linking this PortfolioUser to Skills.
        /// Initialized to avoid null reference issues.
        /// </summary>
        public ICollection<PortfolioUserSkill> PortfolioUserSkills { get; set; } = new List<PortfolioUserSkill>();

        /// <summary>
        /// Converts this PortfolioUser into a DTO-safe representation for API responses.
        /// </summary>
        /// <returns>A PortfolioUserDto containing the mapped properties and nested collections.</returns>
        public PortfolioUserDto ToDto()
        {
            return new PortfolioUserDto
            {
                Id = this.Id,
                Name = this.Name,
                Bio = this.Bio,
                ProfileImageUrl = this.ProfileImageUrl,
                Projects = this.PortfolioUserProjects
                            .Select(pup => pup.ToDto())
                            .ToList(),
                PortfolioUserSkills = this.PortfolioUserSkills
                            .Select(pus => pus.ToDto())
                            .ToList()
            };
        }
    }
}

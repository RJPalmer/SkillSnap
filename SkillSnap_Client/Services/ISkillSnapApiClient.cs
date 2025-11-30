using SkillSnap.Shared.DTOs;
using SkillSnap_Shared.DTOs.Account;



namespace SkillSnap_Client.Services;

public interface ISkillSnapApiClient
{
    // User Profile
    Task<PortfolioUserDto?> GetUserProfileAsync();
    Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId);
    Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile);
    Task<bool> UpdateUserProfileAsync(string userId, PortfolioUserDto userProfile);
    Task<bool> DeleteUserProfileAsync(string userId);
    Task<PortfolioUserDto?> CreatePortfolioUserAsync(PortfolioUserCreateDto userProfile);
    Task<IEnumerable<PortfolioUserDto>> GetUnlinkedPortfolioUsersAsync();
    Task<bool> LinkPortfolioUserAsync(int portfolioUserId);

    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();

    // Projects
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync();
    Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto?> CreateProjectAsync(ProjectCreateDto project);
    Task<bool> UpdateProjectAsync(string projectId, ProjectDto dto);
    Task<bool> DeleteProjectAsync(string projectId);
    Task<ProjectDto?> RemoveProjectById(string projectId);

    // Skills
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync();
    Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId);
    Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
    Task<SkillDto?> GetSkillByIdAsync(string skillId);
    Task<SkillDto?> AddSkillAsync(SkillDto skill);
    Task<bool> UpdateSkillAsync(string skillId, SkillDto skill);
    Task<bool> RemoveSkillAsync(string skillId);

    // Patch Support
    Task<bool> PatchSkillAsync(string skillId, SkillDto skill);
    Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile);

    // User Skills
    Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills);

    Task<bool> DeleteUserAsync(string userId);

    Task<bool> ResetUserPasswordAsync(string userId);

    // Role Management
    Task<IEnumerable<string>?> GetAvailableRolesAsync();
    Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles);
}
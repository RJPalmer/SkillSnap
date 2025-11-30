using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.DTOs.Account;
using SkillSnap.Shared.Models;
using SkillSnap_Shared.DTOs.Account;

namespace SkillSnap_Client.Services;

public class SkillSnapApiClient : ISkillSnapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly UserContext _userContext;
    private readonly ILogger<SkillSnapApiClient> _logger;

    public SkillSnapApiClient(HttpClient httpClient, ILogger<SkillSnapApiClient> logger, UserContext userContext)
    {
        _httpClient = httpClient;
        _logger = logger;
        _userContext = userContext;
    }

    // ---------------------------
    // USER PROFILES
    //---------------------------

    public async Task<PortfolioUserDto?> GetUserProfileAsync()
    {
        try
        {
            var id = _userContext.CurrentPortfolioUser?.Id;
            if (id is null)
            {
                _logger.LogWarning("GetUserProfileAsync called without a loaded PortfolioUser.");
                return null;
            }

            var response = await _httpClient.GetAsync($"api/portfoliouser/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserProfileAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            return null;
        }
    }

    public async Task<PortfolioUserDto?> CreatePortfolioUserAsync(PortfolioUserCreateDto userProfile)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/portfoliouser", userProfile);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error creating portfolio user: {Status}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio user");
            return null;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/portfoliouser/{userId}", userProfile);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserProfileAsync(string userId)
    {
        var response = await _httpClient.DeleteAsync($"api/portfoliouser/{userId}");
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // PROJECTS
    //---------------------------

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync()
    {
        int? userId = _userContext.CurrentPortfolioUser?.Id;
        if (userId is null)
        {
            _logger.LogWarning("GetUserProjectsAsync called without a loaded user.");
            return Array.Empty<ProjectDto>();
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/portfoliouser/{userId}/projects");
            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserProjectsAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<ProjectDto>();
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user projects");
            return Array.Empty<ProjectDto>();
        }
    }

    public async Task<ProjectDto?> CreateProjectAsync(ProjectCreateDto project)
    {
        var response = await _httpClient.PostAsJsonAsync("api/project", project);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProjectDto>()
            : null;
    }

    public async Task<bool> UpdateProjectAsync(string projectId, ProjectDto dto)
    {
        if (!int.TryParse(projectId, out var projectIdInt))
        {
            _logger.LogWarning("UpdateProjectAsync called with invalid projectId format: {ProjectId}", projectId);
            return false;
        }

        var response = await _httpClient.PutAsJsonAsync($"api/project/{projectIdInt}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        if (!int.TryParse(projectId, out var projectIdInt))
        {
            _logger.LogWarning("DeleteProjectAsync called with invalid projectId format: {ProjectId}", projectId);
            return false;
        }

        var response = await _httpClient.DeleteAsync($"api/project/{projectIdInt}");
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // SKILLS
    //---------------------------

    public async Task<IEnumerable<SkillDto>> GetUserSkillsAsync()
    {
        int? id = _userContext.CurrentPortfolioUser?.Id;
        if (id is null)
        {
            _logger.LogWarning("GetUserSkillsAsync called without a loaded PortfolioUser.");
            return Array.Empty<SkillDto>();
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/portfoliouser/{id}/skills");
            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserSkillsAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<SkillDto>();
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user skills");
            return Array.Empty<SkillDto>();
        }
    }

    public async Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills)
    {
        if (skills is null) throw new ArgumentNullException(nameof(skills));

        int? id = _userContext.CurrentPortfolioUser?.Id;
        if (id is null)
        {
            _logger.LogWarning("UpdateUserSkillsAsync called without current user.");
            return false;
        }

        var response = await _httpClient.PutAsJsonAsync($"api/portfoliouser/{id}/skills", skills);
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // ADMIN USER MANAGEMENT
    //---------------------------

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/admin/users");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllUsersAsync returned {Status}", response.StatusCode);
                return Array.Empty<AdminUserDto>();
            }

            return await response.Content.ReadFromJsonAsync<IEnumerable<AdminUserDto>>() ?? Array.Empty<AdminUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all users");
            return Array.Empty<AdminUserDto>();
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var response = await _httpClient.DeleteAsync($"api/admin/user/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("DeleteUserAsync failed with status {Status}", response.StatusCode);
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetUserPasswordAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return false;

        var response = await _httpClient.PostAsync($"api/admin/user/{userId}/reset-password", null);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("ResetUserPasswordAsync failed with status {Status}", response.StatusCode);
        }
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // LINK PORTFOLIO USER
    //---------------------------

    public async Task<bool> LinkPortfolioUserAsync(int portfolioUserId)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/link-portfolio-user", portfolioUserId);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to link PortfolioUser. Server returned {Status}", response.StatusCode);
            return false;
        }
        return true;
    }

    public async Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetUserProfileByIdAsync called with empty userId.");
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync($"api/portfoliouser/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserProfileByIdAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile by ID");
            return null;
        }
    }

    public async Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile)
    {
        if (userProfile == null) return null;
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/portfoliouser", userProfile);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CreateUserProfileAsync failed with status {Status}", response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user profile");
            return null;
        }
    }

    public async Task<IEnumerable<PortfolioUserDto>> GetUnlinkedPortfolioUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/portfoliouser/unlinked");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUnlinkedPortfolioUsersAsync failed with status {Status}", response.StatusCode);
                return Array.Empty<PortfolioUserDto>();
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<PortfolioUserDto>>() ?? Array.Empty<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching unlinked PortfolioUsers");
            return Array.Empty<PortfolioUserDto>();
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Array.Empty<ProjectDto>();
        try
        {
            var response = await _httpClient.GetAsync($"api/portfoliouser/{userId}/projects");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetProjectsByUserIdAsync failed with status {Status}", response.StatusCode);
                return Array.Empty<ProjectDto>();
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching projects by userId");
            return Array.Empty<ProjectDto>();
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/project");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllProjectsAsync failed with status {Status}", response.StatusCode);
                return Array.Empty<ProjectDto>();
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects");
            return Array.Empty<ProjectDto>();
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId)) return null;
        var response = await _httpClient.GetAsync($"api/project/{projectId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    public async Task<ProjectDto?> RemoveProjectById(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId)) return null;
        var response = await _httpClient.DeleteAsync($"api/project/{projectId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    public async Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Array.Empty<SkillDto>();
        try
        {
            var response = await _httpClient.GetAsync($"api/portfoliouser/{userId}/skills");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetSkillsByUserIdAsync failed with status {Status}", response.StatusCode);
                return Array.Empty<SkillDto>();
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching skills by userId");
            return Array.Empty<SkillDto>();
        }
    }

    public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/skill");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllSkillsAsync failed with status {Status}", response.StatusCode);
                return Array.Empty<SkillDto>();
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all skills");
            return Array.Empty<SkillDto>();
        }
    }

    public async Task<SkillDto?> GetSkillByIdAsync(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return null;
        var response = await _httpClient.GetAsync($"api/skill/{skillId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SkillDto>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="skill"></param>
    public async Task<SkillDto?> AddSkillAsync(SkillDto skill)
    {
        if (skill == null) return null;
        var response = await _httpClient.PostAsJsonAsync("api/skill", skill);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SkillDto>();
    }

    public async Task<bool> UpdateSkillAsync(string skillId, SkillDto skill)
    {
        if (string.IsNullOrWhiteSpace(skillId) || skill == null) return false;
        var response = await _httpClient.PutAsJsonAsync($"api/skill/{skillId}", skill);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveSkillAsync(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return false;
        var response = await _httpClient.DeleteAsync($"api/skill/{skillId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchSkillAsync(string skillId, SkillDto skill)
    {
        if (string.IsNullOrWhiteSpace(skillId) || skill == null) return false;
        var response = await _httpClient.PatchAsync($"api/skill/{skillId}", JsonContent.Create(skill));
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        if (string.IsNullOrWhiteSpace(userId) || userProfile == null) return false;
        var response = await _httpClient.PatchAsync($"api/portfoliouser/{userId}", JsonContent.Create(userProfile));
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // ROLE MANAGEMENT
    // ---------------------------

    public async Task<IEnumerable<string>?> GetAvailableRolesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/roleassignment/all-roles");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAvailableRolesAsync failed with status {Status}", response.StatusCode);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available roles");
            return null;
        }
    }

    public async Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles)
    {
        if (string.IsNullOrWhiteSpace(userId) || roles == null) return false;

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/roleassignment/update-roles?userId={userId}", roles);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UpdateUserRolesAsync failed with status {Status}", response.StatusCode);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user roles");
            return false;
        }
    }
}

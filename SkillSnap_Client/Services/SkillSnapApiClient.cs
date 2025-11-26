using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.DTOs.Account;
using SkillSnap.Shared.Models;

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

    // Projects
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync();
    Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto?> CreateProjectAsync(ProjectDto project);
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
    Task<bool> PatchProjectAsync(string projectId, string userId, ProjectDto project);
    Task<bool> PatchSkillAsync(string skillId, SkillDto skill);
    Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile);

    // User Skills
    Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills);
}

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

            try
            {
                return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize PortfolioUserDto. Response body: {Body}", body);
                return null;
            }
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            throw;
        }
    }

    public async Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId)
    {
        try
        {
            // Ensure userId is parsed as int; if not valid, return null early
            if (!int.TryParse(userId, out var userIdInt))
            {
                _logger.LogWarning("GetUserProfileByIdAsync called with invalid userId format: {UserId}", userId);
                return null;
            }

            var response = await _httpClient.GetAsync($"api/portfoliouser/{userIdInt}");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserProfileByIdAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return null;
            }

            // Try to parse JSON; if parsing fails, log the body for diagnosis and return null
            try
            {
                return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize PortfolioUserDto. Response body: {Body}", body);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile by ID");
            throw;
        }
    }

    public async Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/portfoliouser", userProfile);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PortfolioUserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user profile");
            throw;
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
            throw;
        }
    }

    // ---------------------------
    // PROJECTS
    //---------------------------

    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync()
    {
        try
        {
            int? userId = _userContext.CurrentPortfolioUser?.Id;
            if (userId is null)
            {
                _logger.LogWarning("GetUserProjectsAsync called without a loaded user.");
                return Array.Empty<ProjectDto>();
            }
            var response = await _httpClient.GetAsync($"api/portfoliouser/{userId}/projects");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUserProjectsAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<ProjectDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize ProjectDto list. Response body: {Body}", body);
                return Array.Empty<ProjectDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user projects");
            throw;
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        if (!int.TryParse(projectId, out var projectIdInt))
        {
            _logger.LogWarning("GetProjectByIdAsync called with invalid projectId format: {ProjectId}", projectId);
            return null;
        }

        var response = await _httpClient.GetAsync($"api/project/{projectIdInt}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProjectDto>()
            : null;
    }

    public async Task<ProjectDto?> CreateProjectAsync(ProjectDto project)
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

    public Task<ProjectDto?> RemoveProjectById(string projectId)
        => GetProjectByIdAsync(projectId);

    public async Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId)
    {
        try
        {
            if (!int.TryParse(userId, out var userIdInt))
            {
                _logger.LogWarning("GetProjectsByUserIdAsync called with invalid userId format: {UserId}", userId);
                return Array.Empty<ProjectDto>();
            }

            var response = await _httpClient.GetAsync($"api/portfoliouser/{userIdInt}/projects");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetProjectsByUserIdAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<ProjectDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize ProjectDto list. Response body: {Body}", body);
                return Array.Empty<ProjectDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching projects by user id");
            throw;
        }
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/project");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetAllProjectsAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<ProjectDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<ProjectDto>>() ?? Array.Empty<ProjectDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize ProjectDto list. Response body: {Body}", body);
                return Array.Empty<ProjectDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects");
            throw;
        }
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

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize SkillDto list. Response body: {Body}", body);
                return Array.Empty<SkillDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user skills");
            return Array.Empty<SkillDto>();
        }
    }

    public async Task<SkillDto?> GetSkillByIdAsync(string skillId)
    {
        if (!int.TryParse(skillId, out var skillIdInt))
        {
            _logger.LogWarning("GetSkillByIdAsync called with invalid skillId format: {SkillId}", skillId);
            return null;
        }

        var response = await _httpClient.GetAsync($"api/skill/{skillIdInt}");
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SkillDto>()
            : null;
    }

    public async Task<SkillDto?> AddSkillAsync(SkillDto skill)
    {
        var response = await _httpClient.PostAsJsonAsync("api/skill", skill);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SkillDto>()
            : null;
    }

    public async Task<bool> UpdateSkillAsync(string skillId, SkillDto skill)
    {
        if (!int.TryParse(skillId, out var skillIdInt))
        {
            _logger.LogWarning("UpdateSkillAsync called with invalid skillId format: {SkillId}", skillId);
            return false;
        }

        var response = await _httpClient.PutAsJsonAsync($"api/skill/{skillIdInt}", skill);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveSkillAsync(string skillId)
    {
        if (!int.TryParse(skillId, out var skillIdInt))
        {
            _logger.LogWarning("RemoveSkillAsync called with invalid skillId format: {SkillId}", skillId);
            return false;
        }

        var response = await _httpClient.DeleteAsync($"api/skill/{skillIdInt}");
        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId)
    {
        try
        {
            if (!int.TryParse(userId, out var userIdInt))
            {
                _logger.LogWarning("GetSkillsByUserIdAsync called with invalid userId format: {UserId}", userId);
                return Array.Empty<SkillDto>();
            }

            var response = await _httpClient.GetAsync($"api/portfoliouser/{userIdInt}/skills");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetSkillsByUserIdAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<SkillDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize SkillDto list. Response body: {Body}", body);
                return Array.Empty<SkillDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching skills by user id");
            throw;
        }
    }

    public async Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/skill");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetAllSkillsAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<SkillDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<SkillDto>>() ?? Array.Empty<SkillDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize SkillDto list. Response body: {Body}", body);
                return Array.Empty<SkillDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all skills");
            throw;
        }
    }

    // ---------------------------
    // PATCH SUPPORT
    //---------------------------

    public async Task<bool> PatchProjectAsync(string projectId, string userId, ProjectDto project)
    {
        int? resolvedUserId = null;

        if (int.TryParse(userId, out var uid))
            resolvedUserId = uid;
        else if (_userContext.CurrentPortfolioUser?.Id is int current)
            resolvedUserId = current;

        if (resolvedUserId is null)
        {
            _logger.LogWarning("PatchProjectAsync called without user id.");
            return false;
        }

        var joinEntry = new PortfolioUserProjectCreateDto
        {
            PortfolioUserId = resolvedUserId.Value,
            ProjectId = int.Parse(projectId)
        };

        var response = await _httpClient.PostAsJsonAsync($"api/project/attach", joinEntry);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchSkillAsync(string skillId, SkillDto skill)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/skill/{skillId}", skill);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        string target = !string.IsNullOrWhiteSpace(userId)
            ? userId
            : _userContext.CurrentPortfolioUser?.Id.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(target))
        {
            _logger.LogWarning("PatchUserProfileAsync called without valid user id.");
            return false;
        }

        var response = await _httpClient.PatchAsJsonAsync($"api/portfoliouser/{target}", userProfile);
        return response.IsSuccessStatusCode;
    }

    // ---------------------------
    // USER SKILL UPDATES
    //---------------------------

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

    public async Task<IEnumerable<PortfolioUserDto>> GetUnlinkedPortfolioUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/portfoliouser/unlinked");

            if (!response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("GetUnlinkedPortfolioUsersAsync returned {Status}. Body: {Body}", response.StatusCode, text);
                return Array.Empty<PortfolioUserDto>();
            }

            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<PortfolioUserDto>>() ?? Array.Empty<PortfolioUserDto>();
            }
            catch (System.Text.Json.JsonException jex)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(jex, "Failed to deserialize unlinked users. Response body: {Body}", body);
                return Array.Empty<PortfolioUserDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching unlinked portfolio users");
            throw;
        }
    }

    /// <summary>
    /// LinkPortfolioUserAsync - link the authenticated user to a PortfolioUser
    /// </summary>
    /// <param name="portfolioUserId"></param>
    /// <returns></returns>
    public async Task<bool> LinkPortfolioUserAsync(int portfolioUserId)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/account/link-portfolio-user", portfolioUserId);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to link PortfolioUser. Server returned {Status}", response.StatusCode);
            return false;
        }

        return true;
    }


}
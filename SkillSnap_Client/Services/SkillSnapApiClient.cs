using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.Models;

namespace SkillSnap_Client.Services;

/// <summary>
/// SkillSnap API Client Interface
/// </summary>
public interface ISkillSnapApiClient
{
    Task<PortfolioUserDto?> GetUserProfileAsync();
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync();
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync();

    Task<ProjectDto?> RemoveProjectById(string projectId);
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);
    Task<ProjectDto?> CreateProjectAsync(ProjectDto project);
    Task<bool> UpdateProjectAsync(string projectId, ProjectDto project);
    Task<bool> DeleteProjectAsync(string projectId);

    Task<SkillDto?> AddSkillAsync(SkillDto skill);
    Task<bool> RemoveSkillAsync(string skillId);
    Task<bool> UpdateSkillAsync(string skillId, SkillDto skill);
    Task<SkillDto?> GetSkillByIdAsync(string skillId);

    Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile);
    Task<bool> UpdateUserProfileAsync(string userId, PortfolioUserDto userProfile);
    Task<bool> DeleteUserProfileAsync(string userId);
    Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId);

    Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId);
    Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId);
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<IEnumerable<SkillDto>> GetAllSkillsAsync();

    Task<bool> PatchProjectAsync(string projectId, ProjectDto project);
    Task<bool> PatchSkillAsync(string skillId, SkillDto skill);
    Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile);
    Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills);
}

/// <summary>
/// SkillSnap API Client Implementation
/// </summary>
public class SkillSnapApiClient : ISkillSnapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SkillSnapApiClient> _logger;

    public SkillSnapApiClient(HttpClient httpClient, ILogger<SkillSnapApiClient> logger)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7271/");
        _logger = logger;
    }

    // ---------- USER PROFILE ----------
    public async Task<PortfolioUserDto?> GetUserProfileAsync()
    {
        try
        {
            var userProfile = await _httpClient.GetFromJsonAsync<PortfolioUserDto>("api/portfoliouser/1");
            return userProfile ?? throw new Exception("User profile not found");
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
            return await _httpClient.GetFromJsonAsync<PortfolioUserDto>($"api/portfoliouser/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching user profile {userId}");
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

    // ---------- PROJECTS ----------
    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProjectDto>>("api/portfoliouser/1/projects")
                   ?? Array.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user projects");
            throw;
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        var response = await _httpClient.GetAsync($"api/project/{projectId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    public async Task<ProjectDto?> CreateProjectAsync(ProjectDto newProject)
    {
        var response = await _httpClient.PostAsJsonAsync("api/project", newProject);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProjectDto>() : null;
    }

    public async Task<bool> UpdateProjectAsync(string projectId, ProjectDto project)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/project/{projectId}", project);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        var response = await _httpClient.DeleteAsync($"api/project/{projectId}");
        return response.IsSuccessStatusCode;
    }

    public Task<ProjectDto?> RemoveProjectById(string projectId)
        => GetProjectByIdAsync(projectId); // placeholder (can call DELETE if preferred)

    public Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId)
        => _httpClient.GetFromJsonAsync<IEnumerable<ProjectDto>>($"api/portfoliouser/{userId}/projects")!;

    public Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
        => _httpClient.GetFromJsonAsync<IEnumerable<ProjectDto>>("api/project")!;

    // ---------- SKILLS ----------
    public async Task<IEnumerable<SkillDto>> GetUserSkillsAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<SkillDto>>("api/portfoliouser/1/skills")
               ?? Array.Empty<SkillDto>();
    }

    public async Task<SkillDto?> GetSkillByIdAsync(string skillId)
    {
        var response = await _httpClient.GetAsync($"api/skill/{skillId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SkillDto>();
    }

    public async Task<SkillDto?> AddSkillAsync(SkillDto skill)
    {
        var response = await _httpClient.PostAsJsonAsync("api/skill", skill);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SkillDto>() : null;
    }

    public async Task<bool> UpdateSkillAsync(string skillId, SkillDto skill)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/skill/{skillId}", skill);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveSkillAsync(string skillId)
    {
        var response = await _httpClient.DeleteAsync($"api/skill/{skillId}");
        return response.IsSuccessStatusCode;
    }

    public Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId)
        => _httpClient.GetFromJsonAsync<IEnumerable<SkillDto>>($"api/portfoliouser/{userId}/skills")!;

    public Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
        => _httpClient.GetFromJsonAsync<IEnumerable<SkillDto>>("api/skill")!;

    // ---------- PATCH SUPPORT ----------
    public async Task<bool> PatchProjectAsync(string projectId, ProjectDto project)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/project/{projectId}", project);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchSkillAsync(string skillId, SkillDto skill)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/skill/{skillId}", skill);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        var response = await _httpClient.PatchAsJsonAsync($"api/portfoliouser/{userId}", userProfile);
        return response.IsSuccessStatusCode;
    }

    // ---------- USER SKILL UPDATES ----------
    public async Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills)
    {
        if (skills == null) throw new ArgumentNullException(nameof(skills));

        var response = await _httpClient.PutAsJsonAsync("api/portfoliouser/1/skills", skills);
        return response.IsSuccessStatusCode;
    }
}
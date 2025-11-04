using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SkillSnap.Shared.DTOs;
using SkillSnap.Shared.Models;

namespace SkillSnap_Client.Services;

public interface ISkillSnapApiClient
{
    Task<PortfolioUserDto?> GetUserProfileAsync();
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync();
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync();
}

public class SkillSnapApiClient : ISkillSnapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SkillSnapApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SkillSnapApiClient"/> class.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="logger"></param>
    public SkillSnapApiClient(HttpClient httpClient, ILogger<SkillSnapApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the user profile.
    /// </summary>
    /// <returns></returns>
    public async Task<PortfolioUserDto?> GetUserProfileAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PortfolioUserDto>("api/user/profile");
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            throw;
        }
    }

    /// <summary>
    /// Gets the user projects.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProjectDto>>("api/user/projects")
                   ?? Array.Empty<ProjectDto>();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return Array.Empty<ProjectDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user projects");
            throw;
        }
    }

    /// <summary>
    /// Gets the user skills.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<SkillDto>> GetUserSkillsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SkillDto>>("api/user/skills")
                   ?? Array.Empty<SkillDto>();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return Array.Empty<SkillDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user skills");
            throw;
        }
    }
}
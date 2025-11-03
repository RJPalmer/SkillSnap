using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SkillSnap.Shared.Models;

namespace SkillSnap_Client.Services;

public interface ISkillSnapApiClient
{
    Task<PortfolioUser?> GetUserProfileAsync();
    Task<IEnumerable<Project>> GetUserProjectsAsync();
    Task<IEnumerable<Skill>> GetUserSkillsAsync();
}

public class SkillSnapApiClient : ISkillSnapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SkillSnapApiClient> _logger;

    public SkillSnapApiClient(HttpClient httpClient, ILogger<SkillSnapApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PortfolioUser?> GetUserProfileAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PortfolioUser>("api/user/profile");
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

    public async Task<IEnumerable<Project>> GetUserProjectsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Project>>("api/user/projects") 
                   ?? Array.Empty<Project>();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return Array.Empty<Project>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user projects");
            throw;
        }
    }

    public async Task<IEnumerable<Skill>> GetUserSkillsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Skill>>("api/user/skills") 
                   ?? Array.Empty<Skill>();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return Array.Empty<Skill>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user skills");
            throw;
        }
    }
}
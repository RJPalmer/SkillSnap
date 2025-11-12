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

    /// <summary>
    /// Gets the user profile.
    /// </summary>
    /// <returns></returns>
    Task<PortfolioUserDto?> GetUserProfileAsync();

    /// <summary>
    /// Gets the user projects.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ProjectDto>> GetUserProjectsAsync();

    /// <summary>
    /// Gets the user skills.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SkillDto>> GetUserSkillsAsync();

    ///<summary>
    /// Removes a project from the user's project list
    /// </summary>
    ///
    public Task<ProjectDto?> RemoveProjectById(string projectId);

    /// Gets the project by identifier.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Task<ProjectDto?> GetProjectByIdAsync(string projectId);

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    Task<ProjectDto?> CreateProjectAsync(ProjectDto project);

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    Task<bool> UpdateProjectAsync(string projectId, ProjectDto project);

    /// <summary>    
    /// Deletes an existing project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    Task<bool> DeleteProjectAsync(string projectId);

    /// <summary>
    /// Adds a skill to the user.
    /// </summary>
    /// <param name="skill"></param>
    /// <returns></returns>
    Task<SkillDto?> AddSkillAsync(SkillDto skill);

    /// <summary>
    /// Removes a skill from the user.
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    Task<bool> RemoveSkillAsync(string skillId);

    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    Task<bool> UpdateSkillAsync(string skillId, SkillDto skill);

    /// <summary>
    /// Gets the skill by identifier.
    /// </summary>
    /// <param name="skillId"></param>
    /// <returns></returns>
    Task<SkillDto?> GetSkillByIdAsync(string skillId);

    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    /// <param name="userProfile"></param>
    /// <returns></returns>
    Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile);

    /// <summary>
    /// Updates an existing user profile.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userProfile"></param>
    /// <returns></returns>
    Task<bool> UpdateUserProfileAsync(string userId, PortfolioUserDto userProfile);

    /// <summary>
    /// Deletes an existing user profile.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> DeleteUserProfileAsync(string userId);

    /// <summary>
    /// Gets the user profile by identifier.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId);

    /// <summary>
    /// Gets the projects for a specific user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId);

    /// <summary>
    /// Gets the skills for a specific user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId);

    /// <summary>
    /// Gets all projects. 
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();

    /// <summary>
    /// Gets all skills.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<SkillDto>> GetAllSkillsAsync();

    /// <summary>
    /// Update project details.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    Task<bool> PatchProjectAsync(string projectId, ProjectDto project);

    /// <summary>
    /// Update skill details.
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="skill"></param>
    /// <returns></returns>
    Task<bool> PatchSkillAsync(string skillId, SkillDto skill);

    /// <summary>
    /// Update user profile details.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userProfile"></param>
    /// <returns></returns>
    Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile);

    Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills);
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
        _httpClient.BaseAddress = new Uri("https://localhost:7271/");
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


            var userProfile = await _httpClient.GetFromJsonAsync<PortfolioUserDto>("api/portfoliouser/1");
            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found");
                return null;
            }
            return userProfile;
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
            //
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProjectDto>>("api/portfoliouser/1/projects")
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
            return await _httpClient.GetFromJsonAsync<IEnumerable<SkillDto>>("api/portfoliouser/1/skills")
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

    /// <summary>
    /// Gets the project by identifier.
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<ProjectDto?> GetProjectByIdAsync(string projectId)
    {
        try
        {
            _logger.LogInformation($"Fetching project with id: {projectId}");
            var response = await _httpClient.GetAsync($"api/project/{projectId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Project with id {projectId} not found. Status Code: {response.StatusCode}");
                return null;
            }
            var project = await response.Content.ReadFromJsonAsync<ProjectDto>();
            return project;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login");
            exception.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project by id");
            throw;
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public async Task<ProjectDto?> CreateProjectAsync(ProjectDto newProject)
    {
        if (newProject == null)
        {
            _logger.LogWarning("Attempted to add a null project.");
            throw new ArgumentNullException(nameof(newProject));
        }

        try
        {
            _logger.LogInformation("Adding a new project...");

            var response = await _httpClient.PostAsJsonAsync("api/project", newProject);

            if (response.IsSuccessStatusCode)
            {
                var createdProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
                _logger.LogInformation($"Project '{createdProject?.Title}' created successfully.");
                return createdProject;
            }
            else
            {
                _logger.LogWarning($"Failed to create project. Status: {response.StatusCode}");
                return null;
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login.");
            exception.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new project.");
            throw;
        }
        // throw new NotImplemen   tedException();
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="project"></param>
    /// <returns></returns>
    public Task<bool> UpdateProjectAsync(string projectId, ProjectDto project)
    {
        //validate the projectId
        if (projectId != project.Id.ToString())
        {
            _logger.LogError("Project ID mismatch");
            return Task.FromResult(false);
        }
        else
        {
            var response = _httpClient.PutAsJsonAsync($"api/project/{projectId}", project);
            return response.ContinueWith(task => task.Result.IsSuccessStatusCode);
        }
    }

    /// <summary>
    /// Deletes the project with the given projectId
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>

    public async Task<bool> DeleteProjectAsync(string projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            _logger.LogWarning("Attempted to delete a project with an invalid or empty ID.");
            throw new ArgumentNullException(nameof(projectId));
        }

        try
        {
            _logger.LogInformation($"Attempting to delete project with ID: {projectId}");
            var response = await _httpClient.DeleteAsync($"api/project/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Project with ID {projectId} deleted successfully.");
                return true;
            }
            else
            {
                _logger.LogWarning($"Failed to delete project with ID {projectId}. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login.");
            exception.Redirect();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting project with ID {projectId}");
            throw;
        }
    }

    public Task<SkillDto?> AddSkillAsync(SkillDto skill)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveSkillAsync(string skillId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateSkillAsync(string skillId, SkillDto skill)
    {
        throw new NotImplementedException();
    }


    public Task<SkillDto?> GetSkillByIdAsync(string skillId)
    {
        throw new NotImplementedException();
    }

    public Task<PortfolioUserDto?> CreateUserProfileAsync(PortfolioUserDto userProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the user profile asynchronous.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="userProfile">The user profile.</param>
    /// <returns></returns>
    public Task<bool> UpdateUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUserProfileAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<PortfolioUserDto?> GetUserProfileByIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProjectDto>> GetProjectsByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SkillDto>> GetSkillsByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<SkillDto>> GetAllSkillsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> PatchProjectAsync(string projectId, ProjectDto project)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PatchSkillAsync(string skillId, SkillDto skill)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PatchUserProfileAsync(string userId, PortfolioUserDto userProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds a new project.
    /// </summary>
    /// <param name="newProject"></param>
    /// <returns>The created ProjectDto, or null if creation failed.</returns>
    public async Task<ProjectDto?> AddProjectAsync(ProjectDto newProject)
    {
        if (newProject == null)
        {
            _logger.LogWarning("Attempted to add a null project.");
            throw new ArgumentNullException(nameof(newProject));
        }

        try
        {
            _logger.LogInformation("Adding a new project...");

            var response = await _httpClient.PostAsJsonAsync("api/project", newProject);

            if (response.IsSuccessStatusCode)
            {
                var createdProject = await response.Content.ReadFromJsonAsync<ProjectDto>();
                _logger.LogInformation($"Project '{createdProject?.Title}' created successfully.");
                return createdProject;
            }
            else
            {
                _logger.LogWarning($"Failed to create project. Status: {response.StatusCode}");
                return null;
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login.");
            exception.Redirect();
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new project.");
            throw;
        }
    }

    public Task<ProjectDto?> RemoveProjectById(string projectId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the user's skills.
    /// </summary>
    /// <param name="skills">A collection of updated skill names.</param>
    /// <returns>True if update succeeded, false otherwise.</returns>
    public async Task<bool> UpdateUserSkillsAsync(IEnumerable<string> skills)
    {
        if (skills == null)
        {
            _logger.LogWarning("Attempted to update with a null skill list.");
            throw new ArgumentNullException(nameof(skills));
        }

        try
        {
            _logger.LogInformation("Updating user skills...");
            
            var response = await _httpClient.PutAsJsonAsync("api/portfoliouser/1/skills", skills);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User skills updated successfully.");
                return true;
            }
            else
            {
                _logger.LogWarning($"Failed to update skills. Status: {response.StatusCode}");
                return false;
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            _logger.LogWarning("Token not available, redirecting to login.");
            exception.Redirect();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user skills.");
            throw;
        }
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SkillSnap.Shared.DTOs;
using SkillSnap_Client.Services;
using SkillSnap_Client.Services.Authentication;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace SkillSnap_Client.Services;
public class UserContext
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService; // your token storage service
    private readonly ILogger<UserContext> _logger;

    public PortfolioUserDto? CurrentPortfolioUser { get; private set; }
    public ClaimsPrincipal? CurrentClaimsPrincipal { get; private set; }

    public event Action? OnChange;

    public UserContext(AuthenticationStateProvider authProvider,
                       HttpClient httpClient,
                       ITokenService tokenService,
                       ILogger<UserContext> logger)
    {
        _authProvider = authProvider;
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task LoadAsync()
    {
        _logger?.LogInformation("UserContext.LoadAsync: starting");

        // Get current auth state
        var state = await _authProvider.GetAuthenticationStateAsync();
        CurrentClaimsPrincipal = state.User;
        _logger?.LogInformation("UserContext.LoadAsync: auth state retrieved; authenticated={Authenticated}", state.User?.Identity?.IsAuthenticated);

        // Try to read claim first
        var portfolioClaim = state.User.FindFirst("portfolioUserId")?.Value;

        if (!string.IsNullOrWhiteSpace(portfolioClaim) && int.TryParse(portfolioClaim, out var pid))
        {
            // fetch portfolio user from API using HttpClient (avoid circular DI)
            try
            {
                _logger?.LogInformation("UserContext.LoadAsync: found portfolio claim {Pid}; calling API", pid);
                var resp = await _httpClient.GetAsync($"api/portfoliouser/{pid}");
                if (resp.IsSuccessStatusCode)
                {
                    CurrentPortfolioUser = await resp.Content.ReadFromJsonAsync<PortfolioUserDto>();
                    _logger?.LogInformation("UserContext.LoadAsync: loaded portfolio user from claim id: {Id}", CurrentPortfolioUser?.Id);
                    NotifyStateChanged();
                    return;
                }
            }
            catch
            {
                // ignore and continue to token fallback
            }
        }

        // If claim is missing, but we might still have a token with embedded data; try token parse fallback
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            // optionally decode token to extract portfolioUserId (or reuse AuthenticationState)
            var portfolioId = await _tokenService.GetPortfolioUserIdFromSpecificTokenAsync(token);
            _logger?.LogInformation("UserContext.LoadAsync: parsed portfolioId from token: {PortfolioId}", portfolioId);
            if (portfolioId.HasValue)
            {
                try
                {
                    _logger?.LogInformation("UserContext.LoadAsync: fetching portfolio user by token id {Pid}", portfolioId.Value);
                    var resp = await _httpClient.GetAsync($"api/portfoliouser/{portfolioId.Value}");
                    if (resp.IsSuccessStatusCode)
                    {
                        CurrentPortfolioUser = await resp.Content.ReadFromJsonAsync<PortfolioUserDto>();
                        _logger?.LogInformation("UserContext.LoadAsync: loaded portfolio user from token id: {Id}", CurrentPortfolioUser?.Id);
                        NotifyStateChanged();
                        return;
                    }
                }
                catch
                {
                    // ignore and fall through
                }
            }
        }

        // No portfolio user found
        CurrentPortfolioUser = null;
        NotifyStateChanged();
    }

    public async Task ApplyNewTokenAsync(string token)
    {
        // persist the token and refresh context
        await _tokenService.SaveTokenAsync(token);
        // notify auth provider if necessary (depends on your setup)
        await LoadAsync();
    }

    public void SetPortfolioUser(PortfolioUserDto dto)
    {
        CurrentPortfolioUser = dto;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
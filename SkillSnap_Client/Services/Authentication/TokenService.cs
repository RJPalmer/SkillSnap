using Microsoft.JSInterop;
using SkillSnap_Client.Services.Authentication;

namespace SkillSnap_Client.Services.Authentication;

/// <summary>
/// Implementation of ITokenService for managing JWT tokens using browser's local storage.
/// </summary>
/// <remarks>This implementation uses JavaScript interop to interact with local storage.</remarks>
/// <seealso cref="ITokenService" />
/// <author>Robert Palmer</author>
/// <date>2024-06-15</date>
public class TokenService : ITokenService
{
    private readonly IJSRuntime _js;

    public TokenService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>
    /// Saves the JWT token securely.
    /// </summary>
    /// <param name="token">The JWT token to be saved.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveTokenAsync(string token) =>
        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);

    /// <summary>
    /// Retrieves the saved JWT token.
    /// </summary>
    /// <returns>A task that represents the asynchronous retrieval operation. The task result contains the JWT token if found; otherwise, null.</returns>
    public async Task<string?> GetTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

    /// <summary>
    /// Removes the saved JWT token.
    /// </summary>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    public async Task RemoveTokenAsync() =>
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");

    /// <summary>
    /// Retrieves the portfolio user ID from the JWT token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the portfolio user ID if found; otherwise, null.</returns>
    public async Task<int?> GetPortfolioUserIdFromTokenAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var payload = await DecodeTokenPayloadAsync();
        // Try case-insensitive key lookup for portfolioUserId
        foreach (var kv in payload)
        {
            if (string.Equals(kv.Key, "portfolioUserId", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(kv.Key, "PortfolioUserId", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(kv.Value?.ToString(), out var userId))
                    return userId;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if the saved JWT token is valid.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if the token is valid; otherwise, false.</returns>
    public async Task<bool> IsTokenValidAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var payload = await DecodeTokenPayloadAsync();

            // EXP claim is Unix time (seconds)
            if (payload.TryGetValue("exp", out var expObj))
            {
                long expUnix;
                if (long.TryParse(expObj.ToString(), out expUnix))
                {
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                    return expiry > DateTimeOffset.UtcNow; // token still valid
                }
            }

            return false;
        }
        catch
        {
            // Token is corrupt or decode failed
            return false;
        }
    }

    /// <summary>
    /// Decodes the payload of the saved JWT token.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of the token payload if found; otherwise, an empty dictionary.</returns>
    public async Task<Dictionary<string, object>> DecodeTokenPayloadAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new Dictionary<string, object>();

        var parts = token.Split('.');
        if (parts.Length < 2)
            return new Dictionary<string, object>();

        var payload = parts[1];

        // Fix base64 padding
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        var json = System.Text.Encoding.UTF8.GetString(bytes);

        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json)
            ?? new Dictionary<string, object>();
    }

    public Task<int?> GetPortfolioUserIdFromSpecificTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult<int?>(null);

        var parts = token.Split('.');
        if (parts.Length < 2)
            return Task.FromResult<int?>(null);

        var payload = parts[1];

        // Fix base64 padding
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        var json = System.Text.Encoding.UTF8.GetString(bytes);

        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json)
            ?? new Dictionary<string, object>();

        // Case-insensitive lookup for portfolioUserId
        foreach (var kv in dict)
        {
            if (string.Equals(kv.Key, "portfolioUserId", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(kv.Key, "PortfolioUserId", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(kv.Value?.ToString(), out var userId))
                    return Task.FromResult<int?>(userId);
            }
        }

        return Task.FromResult<int?>(null);
    }
}
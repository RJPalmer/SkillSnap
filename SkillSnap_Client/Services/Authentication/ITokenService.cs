namespace SkillSnap_Client.Services.Authentication;

/// <summary>
/// Interface for managing JWT tokens in the client application.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Saves the JWT token securely.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task SaveTokenAsync(string token);
    
    /// <summary>
    /// Retrieves the stored JWT token.
    /// </summary>
    /// <returns>The JWT token if available; otherwise, null.</returns> 
    Task<string?> GetTokenAsync();
    
    /// <summary>
    /// Removes the stored JWT token.
    /// </summary>
    /// <returns></returns>
    Task RemoveTokenAsync();

    /// <summary>
    /// Extracts the PortfolioUserId from the stored JWT token.
    /// </summary>
    /// <returns>The PortfolioUserId if available; otherwise, null.</returns>
    Task<int?> GetPortfolioUserIdFromSpecificTokenAsync(string token);

    /// <summary>
    /// Checks if a valid JWT token is stored.
    /// </summary>
    /// <returns>True if a valid token is stored; otherwise, false.</returns>
    /// <remarks> A valid token is one that exists and has not expired.</remarks>
    Task<bool> IsTokenValidAsync();

    /// <summary>
    /// Decodes the JWT token and returns its payload as a dictionary.
    /// </summary>
    /// <returns>A dictionary containing the JWT payload claims.</returns>
    /// <remarks>This method does not validate the token; it only decodes it.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if no token is stored.</exception>
    /// <exception cref="FormatException">Thrown if the token format is invalid.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown if the payload cannot be deserialized.</exception>
    /// <remarks>This method is intended for debugging and informational purposes only.</remarks>
    /// <returns></returns>
    Task<Dictionary<string, object>> DecodeTokenPayloadAsync();
}

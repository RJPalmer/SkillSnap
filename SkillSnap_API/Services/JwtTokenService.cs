using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SkillSnap_API.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
    {
        _config = config;
        _userManager = userManager;
    }

    /// <summary>
    /// Generate JWT token for the specified user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> GenerateToken(ApplicationUser user)
    {
        var rawKey = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(rawKey))
            throw new InvalidOperationException("JWT signing key is missing from configuration (Jwt:Key).");

        var keyBytes = Encoding.UTF8.GetBytes(rawKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException($"JWT signing key is too short. HS256 requires at least 256 bits (32 characters). Current key length: {keyBytes.Length} bytes.");
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };

        // FIX: Use FK instead of navigation property
        if (user.PortfolioUser != null)
        {
            claims.Add(new Claim("portfolioUserId", user.PortfolioUser.Id.ToString()));
        }

         // ---- ADD IDENTITY ROLES HERE ----
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        // ----------------------------------

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "60")
            ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
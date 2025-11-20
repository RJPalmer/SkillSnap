using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillSnap.Shared.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SkillSnap_API.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(ApplicationUser user)
    {
                var rawKey = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(rawKey))
            throw new Exception("JWT signing key is missing from configuration (Jwt:Key).");
        var keyBytes = Encoding.UTF8.GetBytes(rawKey);
        if (keyBytes.Length < 32)
            throw new Exception($"JWT signing key is too short. HS256 requires at least 256 bits (32 characters). Current key length: {keyBytes.Length} bytes.");

        var key = new SymmetricSecurityKey(keyBytes);

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

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
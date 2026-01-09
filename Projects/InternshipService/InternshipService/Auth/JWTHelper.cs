using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InternshipService.Auth.models;
using Microsoft.Extensions.Logging;

namespace InternshipService.Auth;

public class JWTHelper(
    IOptions<JWTConfiguration> config,
    ILogger<JWTHelper> logger
) : IJWTHelper
{
    public string GenerateToken(int userId, string role)
    {
        if (!RolePermissions.Map.TryGetValue(role, out var permissions))
        {
            logger.LogWarning("Role {role} not found in RolePermissions.Map, using Candidate permissions", role);
            permissions = RolePermissions.Map[Roles.Candidate];
        }

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("role", role)
        };

        claims.AddRange(permissions.Select(p =>
            new Claim("permissions", p)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config.Value.Key));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config.Value.Issuer,
            audience: config.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(config.Value.ExpiryMinutes),
            signingCredentials: creds
        );
        
        logger.LogInformation("JWT Token for User with ID {userId} created successfully", userId);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


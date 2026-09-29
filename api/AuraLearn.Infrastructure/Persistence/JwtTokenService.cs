using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuraLearn.Application.Interfaces;
using AuraLearn.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuraLearn.Infrastructure.Persistence;

/// <summary>
/// Emisión de tokens JWT con los claims sub (id), email y role.
/// La clave y el issuer provienen de la configuración (Jwt:Key / Jwt:Issuer).
/// </summary>
public class JwtTokenService(string secretKey, string issuer) : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("name", user.FullName),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

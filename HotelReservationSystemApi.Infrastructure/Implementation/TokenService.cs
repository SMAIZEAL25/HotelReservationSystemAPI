using HotelReservationSystemAPI.Application.Interface;
using HotelReservationSystemAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace HotelReservationSystemAPI.Infrastructure.Implementation;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public TokenService(IConfiguration config, UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<string> GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        var roleName = userRoles.FirstOrDefault();
        ;
        if (!string.IsNullOrEmpty(roleName))
        {
            var roleEntity = await _roleManager.FindByNameAsync(roleName);
            if (roleEntity?.Permissions != null)
            {
                foreach (var permission in roleEntity.Permissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
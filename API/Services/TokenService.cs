using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using InvalidOperationException = System.InvalidOperationException;

namespace API.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<AppUserLogin> _userManager;

    public TokenService(IConfiguration config, UserManager<AppUserLogin> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<string> CreateToken(AppUserLogin user)
    {
        var tokenKey = _config["TokenKey"];
        Console.WriteLine($"🔹 Loaded TokenKey from appsettings.json: {tokenKey}");

        if (string.IsNullOrWhiteSpace(tokenKey))
            throw new InvalidOperationException("TokenKey is missing in configuration.");

        byte[] keyBytes;
    
        try
        {
            keyBytes = Convert.FromBase64String(tokenKey);
        }
        catch (System.FormatException)
        {
            throw new InvalidOperationException("TokenKey is not a valid Base64-encoded string.");
        }

        if (keyBytes.Length < 64)
            throw new InvalidOperationException($"TokenKey must be at least 64 bytes (512 bits) for HMAC-SHA512. Current length: {keyBytes.Length} bytes");

        Console.WriteLine($"Decoded TokenKey Length: {keyBytes.Length} bytes"); // Ensure this prints 64

        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private async Task<int> GetPercentageAllowedForRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.PercentageAllowed ?? 0;
    }

}
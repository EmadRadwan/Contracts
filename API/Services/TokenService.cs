using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity; // Added for clarity
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
        // REFACTOR: Added null checks for dependencies to improve robustness.
        // This prevents NullReferenceException if dependencies are not injected properly.
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public async Task<string> CreateToken(AppUserLogin user)
    {
        // REFACTOR: Extracted token key validation into a separate method for clarity and reusability.
        // This isolates configuration logic and makes the method easier to maintain.
        var key = GetTokenKey();

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // REFACTOR: Added role claims to the JWT token to ensure frontend can parse them.
        // This fixes the issue where roles are null in the frontend.
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // REFACTOR: Added logging for claims to aid debugging.
        // This helps verify which claims, including roles, are included in the token.
        Console.WriteLine($"🔹 Creating token for user: {user.UserName}, Roles: {JsonConvert.SerializeObject(roles)}");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // REFACTOR: Added logging for token creation success.
        // This helps confirm the token was generated successfully.
        Console.WriteLine($"🔹 Token created for user: {user.UserName}");
        return tokenString;
    }

    // REFACTOR: Extracted token key validation logic into a separate method.
    // This improves readability and isolates configuration validation.
    private SymmetricSecurityKey GetTokenKey()
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
        catch (Exception)
        {
            throw new InvalidOperationException("TokenKey is not a valid Base64-encoded string.");
        }

        if (keyBytes.Length < 64)
            throw new InvalidOperationException($"TokenKey must be at least 64 bytes (512 bits) for HMAC-SHA512. Current length: {keyBytes.Length} bytes");

        Console.WriteLine($"🔹 Decoded TokenKey Length: {keyBytes.Length} bytes");
        return new SymmetricSecurityKey(keyBytes);
    }

    // REFACTOR: Kept GetPercentageAllowedForRole but added null check for role.
    // This ensures robustness when the role is not found.
    private async Task<int> GetPercentageAllowedForRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            Console.WriteLine($"🔹 Role not found: {roleName}");
            return 0;
        }
        return (int)role.PercentageAllowed;
    }
}
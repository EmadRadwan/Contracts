using System.Security.Claims;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly SignInManager<AppUserLogin> _signInManager;
    private readonly TokenService _tokenService;
    private readonly UserManager<AppUserLogin> _userManager;
    private readonly DataContext _context;


    public AccountController(UserManager<AppUserLogin> userManager,
        SignInManager<AppUserLogin> signInManager, TokenService tokenService, DataContext context)
    {
        _tokenService = tokenService;
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (result.Succeeded) return await CreateUserObject(user);

        return Unauthorized();
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await _userManager.Users.AnyAsync(x => x.Email == registerDto.Email))
        {
            ModelState.AddModelError("email", "Email taken");
            return ValidationProblem();
        }

        if (await _userManager.Users.AnyAsync(x => x.UserName == registerDto.Username))
        {
            ModelState.AddModelError("username", "Username taken");
            return ValidationProblem();
        }

        var user = new AppUserLogin
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Username
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded) return await CreateUserObject(user);

        return BadRequest("Problem registering user");
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.Users.Include(p => p.Photos)
            .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        return await CreateUserObject(user);
    }

    private async Task<UserDto> CreateUserObject(AppUserLogin user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        string organizationPartyName = string.Empty;

        if (user.OrganizationPartyId != null)
        {
            var party = await _context.Parties
                .Where(p => p.PartyId == user.OrganizationPartyId)
                .Select(p => p.Description)           // Only select the column we need
                .FirstOrDefaultAsync();

            organizationPartyName = party ?? string.Empty;
        }

        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Image = null, //user?.Files?.FirstOrDefault(x => x.IsMain)?.Url,
            Token = await _tokenService.CreateToken(user),
            Username = user.UserName,
            OrganizationPartyId = user.OrganizationPartyId,
            OrganizationPartyName = organizationPartyName,
            DualLanguage = user.DualLanguage,
            Roles = roles.ToArray()
        };
    }
    
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "The new password and confirmation password do not match.");
            return BadRequest(ModelState);
        }

        var user = await _userManager.Users
            .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));

        if (user == null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

        if (result.Succeeded)
        {
            return Ok("Password changed successfully.");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }
}
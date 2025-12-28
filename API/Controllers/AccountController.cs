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
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly DataContext _context;


    public AccountController(UserManager<AppUserLogin> userManager,
        SignInManager<AppUserLogin> signInManager, TokenService tokenService,
        RoleManager<ApplicationRole> roleManager, DataContext context)
    {
        _tokenService = tokenService;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
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

         var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);

        var user = new AppUserLogin
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Username,
            CreatedStamp = nowDateTime,
            LastUpdatedStamp = nowDateTime,
        };

        var result = await _userManager.CreateAsync(user, "Pa$$w0rd");

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

    [Authorize]
    [HttpGet("listUsers")]
    public async Task<ActionResult<List<UserListDto>>> ListUsers()
    {
        var users = await _userManager.Users
            .Select(u => new UserListDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName,
                Email = u.Email,
                OrganizationPartyId = u.OrganizationPartyId
            })
            .ToListAsync();

        return Ok(users);
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
            Roles = roles.ToArray(),
            MustChangePassword = user.MustChangePassword
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
         var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);

        if (result.Succeeded)
        {
            user.MustChangePassword = false;
            user.LastUpdatedStamp = nowDateTime;
            await _userManager.UpdateAsync(user);
            return Ok("Password changed successfully.");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return BadRequest(ModelState);
    }

    [Authorize]
    [HttpPost("assignRole")]
    public async Task<IActionResult> AssignRole(AssignRoleDto assignRoleDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
        if (user == null)
            return NotFound($"User with ID {assignRoleDto.UserId} not found.");

        var roleExists = await _roleManager.RoleExistsAsync(assignRoleDto.Role);
        if (!roleExists)
            return BadRequest($"Role '{assignRoleDto.Role}' does not exist.");

        var existingRoles = await _userManager.GetRolesAsync(user);
        if (existingRoles.Contains(assignRoleDto.Role))
            return BadRequest($"User already has role '{assignRoleDto.Role}'.");

        var result = await _userManager.AddToRoleAsync(user, assignRoleDto.Role);
        if (result.Succeeded)
            return Ok($"Role '{assignRoleDto.Role}' assigned to user successfully.");

        return BadRequest(result.Errors.Select(e => e.Description));
    }

    [Authorize]
    [HttpPost("removeRole")]
    public async Task<IActionResult> RemoveRole(AssignRoleDto assignRoleDto)
    {
        var user = await _userManager.FindByIdAsync(assignRoleDto.UserId);
        if (user == null)
            return NotFound($"User with ID {assignRoleDto.UserId} not found.");

        var existingRoles = await _userManager.GetRolesAsync(user);
        if (!existingRoles.Contains(assignRoleDto.Role))
            return BadRequest($"User does not have role '{assignRoleDto.Role}'.");

        var result = await _userManager.RemoveFromRoleAsync(user, assignRoleDto.Role);
        if (result.Succeeded)
            return Ok($"Role '{assignRoleDto.Role}' removed from user successfully.");

        return BadRequest(result.Errors.Select(e => e.Description));
    }

    [Authorize]
    [HttpPost("createRole")]
    public async Task<IActionResult> CreateRole(CreateRoleDto createRoleDto)
    {
        if (string.IsNullOrWhiteSpace(createRoleDto.RoleName))
            return BadRequest("Role name cannot be empty.");

        var roleExists = await _roleManager.RoleExistsAsync(createRoleDto.RoleName);
        if (roleExists)
            return BadRequest($"Role '{createRoleDto.RoleName}' already exists.");

        var result = await _roleManager.CreateAsync(new ApplicationRole { Name = createRoleDto.RoleName });
        if (result.Succeeded)
            return Ok($"Role '{createRoleDto.RoleName}' created successfully.");

        return BadRequest(result.Errors.Select(e => e.Description));
    }

    [Authorize]
    [HttpPost("createUser")]
    public async Task<ActionResult<UserListDto>> CreateUser(CreateUserDto createUserDto)
    {
        if (await _userManager.Users.AnyAsync(x => x.Email == createUserDto.Email))
        {
            ModelState.AddModelError("email", "Email already taken.");
            return ValidationProblem();
        }

        if (await _userManager.Users.AnyAsync(x => x.UserName == createUserDto.UserName))
        {
            ModelState.AddModelError("username", "Username already taken.");
            return ValidationProblem();
        }

         var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);

        var user = new AppUserLogin
        {
            DisplayName = createUserDto.DisplayName,
            Email = createUserDto.Email,
            UserName = createUserDto.UserName,
            OrganizationPartyId = string.IsNullOrWhiteSpace(createUserDto.OrganizationPartyId) ? null : createUserDto.OrganizationPartyId,
            EmailConfirmed = true,
            CreatedStamp = nowDateTime,
            LastUpdatedStamp = nowDateTime
        };

        var result = await _userManager.CreateAsync(user, "Pa$$w0rd");

        if (result.Succeeded)
        {
            // Assign roles if provided
            if (createUserDto.Roles != null && createUserDto.Roles.Length > 0)
            {
                var validRoles = new List<string>();
                foreach (var role in createUserDto.Roles)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                        validRoles.Add(role);
                }

                if (validRoles.Count > 0)
                    await _userManager.AddToRolesAsync(user, validRoles);
            }

            return Ok(new UserListDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                OrganizationPartyId = user.OrganizationPartyId
            });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return ValidationProblem();
    }

    [Authorize]
    [HttpGet("listRoles")]
    public async Task<ActionResult<List<RoleDto>>> ListRoles()
    {
        var roles = await _roleManager.Roles
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }

    [Authorize]
    [HttpGet("userRoles/{userId}")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound($"User with ID {userId} not found.");

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(roles.ToList());
    }

    [Authorize]
    [HttpGet("usersByRole/{roleName}")]
    public async Task<ActionResult<List<UserListDto>>> GetUsersByRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return NotFound($"Role '{roleName}' not found.");

        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

        var result = usersInRole.Select(u => new UserListDto
        {
            Id = u.Id,
            UserName = u.UserName,
            DisplayName = u.DisplayName,
            Email = u.Email,
            OrganizationPartyId = u.OrganizationPartyId
        }).ToList();

        return Ok(result);
    }

    [Authorize]
    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound($"User with ID {dto.UserId} not found.");

        // Check if email is taken by another user
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await _userManager.Users.AnyAsync(x => x.Email == dto.Email && x.Id != dto.UserId))
            {
                ModelState.AddModelError("email", "Email already taken by another user.");
                return ValidationProblem();
            }
        }

        // Check if username is taken by another user
        if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
        {
            if (await _userManager.Users.AnyAsync(x => x.UserName == dto.UserName && x.Id != dto.UserId))
            {
                ModelState.AddModelError("username", "Username already taken by another user.");
                return ValidationProblem();
            }
        }

         var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);

        // Update user fields
        user.UserName = dto.UserName ?? user.UserName;
        user.DisplayName = dto.DisplayName ?? user.DisplayName;
        user.Email = dto.Email ?? user.Email;
        user.OrganizationPartyId = string.IsNullOrWhiteSpace(dto.OrganizationPartyId) ? null : dto.OrganizationPartyId;
        user.LastUpdatedStamp = nowDateTime;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return ValidationProblem();
        }

        // Handle roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        var targetRoles = dto.Roles ?? Array.Empty<string>();

        // Validate all target roles exist
        var validRoles = new List<string>();
        foreach (var role in targetRoles)
        {
            if (await _roleManager.RoleExistsAsync(role))
                validRoles.Add(role);
        }

        // Roles to remove (in current but not in target)
        var rolesToRemove = currentRoles.Except(validRoles).ToList();
        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
                return BadRequest(removeResult.Errors.Select(e => e.Description));
        }

        // Roles to add (in target but not in current)
        var rolesToAdd = validRoles.Except(currentRoles).ToList();
        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
                return BadRequest(addResult.Errors.Select(e => e.Description));
        }

        return Ok(new
        {
            Message = "User updated successfully.",
            RolesAdded = rolesToAdd,
            RolesRemoved = rolesToRemove,
            CurrentRoles = validRoles
        });
    }
}
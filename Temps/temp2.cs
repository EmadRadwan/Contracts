using Microsoft.AspNetCore.Identity; // For IdentityResult
using System.Linq; // For Errors.Select

// ... existing code ...

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
// REFACTOR: Added the "ReviewCertificate" role to user role assignments
// Purpose: Enables the new review menu items ("Mark as Ready for Approval" and "Mark as Requires Editing") to appear in the UI
// Improvement: Grants the reviewer permission to perform the intermediate review step in the certificate workflow
// Context: This role must exist in the database (created via seeding or manually) and be checked in the frontend (user?.roles?.includes('ReviewCertificate'))
static async Task AssignRoles(UserManager<AppUserLogin> userManager, DateTime nowDateTime)
{
    // REFACTOR: Added error handling and validation for role assignments.
    // This ensures roles are assigned only to existing users and reports failures.
    var userRoles = new Dictionary<string, string[]>
    {
        { "eradwan1967@gmail.com", new[] { "CreateCertificate", "ApproveCertificate", "CompleteCertificate", "viewCrm", "ReviewCertificate" } },
        { "aagiba@gmail.com", new[] { "CreateCertificate", "ApproveCertificate", "CompleteCertificate", "viewCrm", "ReviewCertificate" } }
        // Add more users here if needed, e.g.:
        // { "reviewer@example.com", new[] { "ReviewCertificate" } }
    };

    foreach (var (email, roles) in userRoles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new InvalidOperationException($"User with email {email} not found for role assignment.");
        }

        // REFACTOR: Check existing roles to avoid duplicate assignments.
        // This improves performance by skipping unnecessary database operations.
        var existingRoles = await userManager.GetRolesAsync(user);
        var rolesToAdd = roles.Except(existingRoles).ToArray();

        if (rolesToAdd.Any())
        {
            var roleResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign roles to {email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
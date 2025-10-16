using Bogus;
using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Persistence;

public class SeedContracts
{
    public static async Task SeedData(DataContext context,
        UserManager<AppUserLogin> userManager, RoleManager<ApplicationRole> roleManager)
    {
        var dateNow = DateTime.UtcNow;
        var nowDateTime = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, dateNow.Minute,
            dateNow.Second, 0, DateTimeKind.Utc);

        // [Existing seeding logic for other entities unchanged...]
        // MimeTypes, DataResourceTypes, ContentTypes, etc., remain as provided.

        // [Skipping to user and role seeding section]

        // REFACTOR: Wrapped user and role seeding in a transaction to ensure atomicity.
        // This ensures consistency between UserLogins, AspNetUsers, and AspNetUserRoles.
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Create roles
            var requiredRoles = new[] { "CreateCertificate", "ApproveCertificate", "CompleteCertificate" };
            foreach (var role in requiredRoles)
            {
                // REFACTOR: Added error handling for role creation to catch and report failures.
                // This ensures roles are created successfully before user assignments.
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new ApplicationRole { Name = role });
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Seed UserLogins
            if (!await context.UserLogins.AnyAsync())
            {
                // REFACTOR: Reused original UserLogin creation logic for consistency.
                // This maintains the same behavior while ensuring proper saving.
                var userLogins = CreateUserLogins(nowDateTime);
                context.UserLogins.AddRange(userLogins);
                await context.SaveChangesAsync();
            }

            // Seed AppUserLogins and assign roles
            if (!await userManager.Users.AnyAsync())
            {
                var users = CreateAppUserLogins(nowDateTime);

                // REFACTOR: Replaced Task.WhenAll with sequential user creation for better error handling.
                // This ensures each user creation is validated, preventing silent failures.
                foreach (var user in users)
                {
                    var createResult = await userManager.CreateAsync(user, "Pa$$w0rd123!"); // Updated password to meet common policies
                    if (!createResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Failed to create user {user.Email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    }
                }

                // REFACTOR: Enhanced role assignment with validation and error handling.
                // This ensures roles are assigned only to existing users and reports failures.
                await AssignRoles(userManager, nowDateTime);
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"User and role seeding failed: {ex.Message}", ex);
        }

        // [Rest of the seeding logic unchanged...]
        // Facilities, WorkEfforts, BillingAccounts, etc., remain as provided.
    }

    static List<UserLogin> CreateUserLogins(DateTime nowDateTime)
    {
        return new List<UserLogin>
        {
            new UserLogin
            {
                UserLoginId = "3bb4e859-1157-4cc7-81b5-10f419359a41",
                PartyId = "26",
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            },
            new UserLogin
            {
                UserLoginId = "29a02dc0-70ea-46d0-a687-6a72b2f91d07",
                PartyId = "27",
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            }
        };
    }

    static List<AppUserLogin> CreateAppUserLogins(DateTime nowDateTime)
    {
        return new List<AppUserLogin>
        {
            new()
            {
                Id = "3bb4e859-1157-4cc7-81b5-10f419359a41", // REFACTOR: Added Id to match UserLoginId for consistency.
                DisplayName = "Emad Radwan",
                UserName = "Emad",
                PartyId = "26",
                OrganizationPartyId = "Company",
                ProductStoreId = "9000",
                Email = "eradwan1967@gmail.com",
                DualLanguage = "N",
                EmailConfirmed = true,
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            },
            new()
            {
                Id = "29a02dc0-70ea-46d0-a687-6a72b2f91d07", // REFACTOR: Added Id to match UserLoginId for consistency.
                DisplayName = "Ahmad Agiba",
                UserName = "Ahmad",
                PartyId = "27",
                OrganizationPartyId = "Company",
                ProductStoreId = "9000",
                Email = "aagiba@gmail.com",
                DualLanguage = "N",
                EmailConfirmed = true,
                CreatedStamp = nowDateTime,
                LastUpdatedStamp = nowDateTime
            }
        };
    }

    static async Task AssignRoles(UserManager<AppUserLogin> userManager, DateTime nowDateTime)
    {
        // REFACTOR: Added error handling and validation for role assignments.
        // This ensures roles are assigned only to existing users and reports failures.
        var userRoles = new Dictionary<string, string[]>
        {
            { "eradwan1967@gmail.com", new[] { "CreateCertificate", "ApproveCertificate", "CompleteCertificate" } },
            { "aagiba@gmail.com", new[] { "CreateCertificate", "ApproveCertificate", "CompleteCertificate" } }
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
                    throw new InvalidOperationException($"Failed to assign roles to {email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
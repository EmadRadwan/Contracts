namespace Application.Interfaces;

public interface IUserAccessor
{
    string GetUsername();

    /// <summary>
    /// AspNetUsers.Id of the signed-in user, taken straight from the JWT's
    /// NameIdentifier claim (see TokenService). Null when unauthenticated.
    /// </summary>
    string? GetUserId();

    /// <summary>
    /// True when the signed-in user holds the given security role.
    /// Roles are carried as claims on the JWT (see TokenService).
    /// </summary>
    bool IsInRole(string role);
}

namespace API.DTOs;

public class UpdateUserDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string OrganizationPartyId { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

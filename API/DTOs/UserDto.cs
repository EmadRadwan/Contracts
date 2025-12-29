namespace API.DTOs;

public class UserDto
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Token { get; set; }
    public string Username { get; set; }
    public string OrganizationPartyId { get; set; }
    public string OrganizationPartyName { get; set; }
    public string Image { get; set; }

    public string DualLanguage { get; set; }
    public string[] Roles { get; set; }
    public bool MustChangePassword { get; set; }
}
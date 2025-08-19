using Microsoft.AspNetCore.Identity;

namespace Domain;

public class AppUserLogin : IdentityUser
{
    public string DisplayName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    
    public string? PartyId { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? DualLanguage { get; set; }
    public string? ProductStoreId { get; set; }
    public string? UserLoginId { get; set; }
    public ProductStore? ProductStore { get; set; }


    // Navigation property to Party (for PartyId)
    public Party? Party { get; set; }
    
    // New navigation property for OrganizationPartyId
    public Party? OrganizationParty { get; set; }
    public UserLogin? UserLogin { get; set; }
    
    public ICollection<Photo> Photos { get; set; }
}


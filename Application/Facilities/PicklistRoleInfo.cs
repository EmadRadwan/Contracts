using Domain;

namespace Application.Facilities;

public class PicklistRoleInfo
{
    public PicklistRole PicklistRole { get; set; }
    public PartyNameViewDto PartyNameViewDto { get; set; } // Replacing PartyNameView
    public RoleType RoleType { get; set; }
}

using Microsoft.AspNetCore.Identity;

namespace Domain;

public class ApplicationRole : IdentityRole
{
    public int? PercentageAllowed { get; set; }
}
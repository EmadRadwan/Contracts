namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Holds two lists: one for Income/Revenues, one for Expenses
/// (mirroring your "glAccountTotalsMap" in OFBiz).
/// </summary>
public class GlAccountTotalsMap
{
    public List<GlAccountTotal> Income { get; set; }
    public List<GlAccountTotal> Expenses { get; set; }
}

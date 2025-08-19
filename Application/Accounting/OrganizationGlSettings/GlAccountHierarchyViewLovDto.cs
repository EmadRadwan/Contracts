namespace Application.Shipments.OrganizationGlSettings;

public class GlAccountHierarchyViewLovDto
{
    public string? GlAccountId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public int? Level { get; set; }
    public string? Text { get; set; }
    public bool IsLeaf { get; set; }

    public List<GlAccountHierarchyViewLovDto> Items { get; set; } = new();
}
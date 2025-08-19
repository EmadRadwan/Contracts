namespace Domain;

public class GlAccountHierarchyView
{
    public string? GlAccountId { get; set; }
    public string? GlAccountTypeId { get; set; }
    public string? GlAccountClassId { get; set; }
    public string? GlResourceTypeId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public int? Level { get; set; }
}
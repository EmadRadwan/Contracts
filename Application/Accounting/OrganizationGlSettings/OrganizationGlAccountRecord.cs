using System.ComponentModel.DataAnnotations;

namespace Application.Shipments.OrganizationGlSettings;

public class OrganizationGlAccountRecord
{
    [Key] public string GlAccountId { get; set; }

    [Key] public string OrganizationPartyId { get; set; }

    public string GlAccountTypeId { get; set; }
    public string GlAccountTypeDescription { get; set; }
    public string GlAccountClassId { get; set; }
    public string GlResourceTypeId { get; set; }
    public string GlResourceTypeDescription { get; set; }
    public string GlXbrlClassId { get; set; }
    public string ParentGlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public string ParentAccountName { get; set; }
    public string Description { get; set; }
    public string ProductId { get; set; }
    public string ExternalId { get; set; }
    public string? GlReportId { get; set; }
    public string? GlReportDescription { get; set; }
    public string? GlClassCourseId { get; set; }
    public string? GlClassCourseDescription { get; set; }
    public string? GlSubClassId { get; set; }
    public string? GlSubClassDescription { get; set; }
    public string? GlSubClass2Id { get; set; }
    public string? GlSubClass2Description { get; set; }
    public string? GlAccountCourseLabelId { get; set; }
    public string? GlAccountCourseLabelDescription { get; set; }
    public string? RoleTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
}
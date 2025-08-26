using System.ComponentModel.DataAnnotations;

namespace Application.Projects;

public class ProjectCertificateRecord
{
    [Key]
    public string WorkEffortId { get; set; }
    public string ProjectNum { get; set; }
    public string ProjectName { get; set; }
    public string PartyId { get; set; }
    public string PartyName { get; set; }
    public string Description { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public string StatusDescription { get; set; }
}
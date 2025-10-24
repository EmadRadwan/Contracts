using System.ComponentModel.DataAnnotations;

namespace Application.Projects;

public class MultiPaymentCertificateRecord
{
    [Key] public string WorkEffortId { get; init; }
    public string Code { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; }
    public string StatusDescription { get; init; }
    public string CurrentStatusId { get; init; }

    public string PartyIdEmployee { get; init; }
    public string PartyEmployeeName { get; set; }
}
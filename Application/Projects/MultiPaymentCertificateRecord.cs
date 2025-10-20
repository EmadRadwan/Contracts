using System.ComponentModel.DataAnnotations;

namespace Application.Projects;

public class MultiPaymentCertificateRecord
{
    [Key]
    public string WorkEffortId { get; init; }
    public string Code { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; }
    public string PaymentMethodId { get; init; }
    public string PaymentMethodDescription { get; init; }
    public string StatusDescription { get; init; }
    public string CurrentStatusId { get; init; } 
    public string? ChequeNumber { get; set; }
    public DateTime? ChequeDate { get; set; }
}
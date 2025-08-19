using Application.Core;

namespace Application.Accounting.Payments;


public class PaymentParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? Types { get; set; }
    public string? Categories { get; set; }
}
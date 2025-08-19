using Application.Core;

namespace Application.Shipments.BillingAccounts;

public class BillingAccountParams : PaginationParams
{
    public string OrderBy { get; set; }
    public string SearchTerm { get; set; }
    public string PartyId { get; set; }
}
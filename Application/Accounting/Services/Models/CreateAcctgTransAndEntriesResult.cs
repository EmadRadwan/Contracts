using Domain;

namespace Application.Accounting.Services.Models;

public class CreateAcctgTransAndEntriesResult
{
    // Additional properties if needed

    // Constructor
    public CreateAcctgTransAndEntriesResult()
    {
        // Initialize the list if needed
        AcctgTransEntries = new List<AcctgTransEntry>();
    }

    public string? AcctgTransId { get; set; }
    public string? GlFiscalTypeId { get; set; }
    public string? AcctgTransTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? PaymentId { get; set; }
    public string? PartyId { get; set; }
    public string? ShipmentId { get; set; }
    public string? RoleTypeId { get; set; }
    public List<AcctgTransEntry>? AcctgTransEntries { get; set; }
    public DateTime? TransactionDate { get; set; }
}
using Domain;

namespace Application.Accounting.Services.Models;

public class CreateAcctgTransAndEntriesParams
{
    // Additional properties if needed

    // Constructor
    public CreateAcctgTransAndEntriesParams()
    {
        // Initialize the list if needed
        AcctgTransEntries = new List<AcctgTransEntry>();
    }

    // Properties representing the input parameters
    public string? GlFiscalTypeId { get; set; }
    public string? AcctgTransTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? InventoryItemId { get; set; }
    public string? PaymentId { get; set; }
    public string? PartyId { get; set; }
    public string? ShipmentId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? PhysicalInventoryId { get; set; }
    public string? RoleTypeId { get; set; }
    public string? Description { get; set; }
    public string? FinAccountTransId { get; set; }
    public string IsPosted { get; set; }

    public string OrganizationPartyId { get; set; }
    public decimal Amount { get; set; }
    public string? AcctgTransEntryTypeId { get; set; }

    public string DebitGlAccountId { get; set; }
    public string CreditGlAccountId { get; set; }
    public List<AcctgTransEntry>? AcctgTransEntries { get; set; }
    public DateTime? TransactionDate { get; set; }
}
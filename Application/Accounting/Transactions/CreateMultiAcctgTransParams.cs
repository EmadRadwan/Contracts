namespace Application.Accounting.Transactions
{
    public class CreateMultiAcctgTransParams
    {
        public string AcctgTransTypeId { get; set; }
        public DateOnly? TransactionDate { get; set; }
        public string OrganizationPartyId { get; set; }
        public string HeaderDescription { get; set; } // REFACTOR: Added HeaderDescription for transaction header
        public string Description { get; set; } // REFACTOR: Retained for backward compatibility or alternative use
        public string IsPosted { get; set; }
        public string? PartyId { get; set; }
        public string GlFiscalTypeId { get; set; }
    }
    
}
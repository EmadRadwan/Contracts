namespace Application.Accounting.Transactions
{
    // REFACTOR: Define parameters for transaction entries
    // This class structures each entry's data, ensuring exactly one of debit or credit GL account
    // is provided, along with amount and other details
    public class MultiAcctgTransEntryParams
    {
        public string? DebitGlAccountId { get; set; } // Optional: GL account ID for debit, mutually exclusive with credit
        public string? CreditGlAccountId { get; set; } // Optional: GL account ID for credit, mutually exclusive with debit
        public string? AcctgTransEntrySeqId { get; set; } 
        
        public decimal Amount { get; set; } // Required: Transaction amount, must be positive
        public string Description { get; set; } // Optional: Entry description
        public string DebitCreditFlag { get; set; } // Required: "D" for debit, "C" for credit
    }
}
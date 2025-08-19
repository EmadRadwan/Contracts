using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TransactionTypeAccountRule
{
    public TransactionTypeAccountRule()
    {
        // Initialize collections if needed for future relationships
    }

    public string RuleId { get; set; } = null!; // Unique identifier, e.g., "AMORTIZATION_DEBIT_EXPENSE_001"
    public string TransactionTypeId { get; set; } = null!; // Foreign key to AcctgTransType, e.g., "AMORTIZATION"
    public string EntryType { get; set; } = null!; // "DEBIT" or "CREDIT"
    public string? GlAccountClassId { get; set; } // Foreign key to GlAccountClass, e.g., "EXPENSE"
    public string? GlAccountTypeId { get; set; } // Foreign key to GlAccountType, e.g., "EXPENSE"
    public string? Notes { get; set; } // Purpose of the rule, e.g., "Records expense for amortization"
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation properties
    public AcctgTransType? TransactionType { get; set; }
    public GlAccountClass? GlAccountClass { get; set; }
    public GlAccountType? GlAccountType { get; set; }
}
namespace Domain;

public class GlAccountCategoryMember
{
    public string GlAccountId { get; set; } = null!;
    public string GlAccountCategoryId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? AmountPercentage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount GlAccount { get; set; } = null!;
    public GlAccountCategory GlAccountCategory { get; set; } = null!;
}
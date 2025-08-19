namespace Domain;

public class TemporalExpressionAssoc
{
    public string FromTempExprId { get; set; } = null!;
    public string ToTempExprId { get; set; } = null!;
    public string? ExprAssocType { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public TemporalExpression FromTempExpr { get; set; } = null!;
    public TemporalExpression ToTempExpr { get; set; } = null!;
}
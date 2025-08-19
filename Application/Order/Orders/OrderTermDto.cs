namespace Application.Order.Orders;

public class OrderTermDto
{

    public string TermTypeId { get; set; } = null!;
    public string TermTypeName { get; set; }
    public string OrderId { get; set; } = null!;
    public string OrderItemSeqId { get; set; } = null!;
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public string? DescriptionTurkish { get; set; }
    public string? UomId { get; set; }
    public bool IsTermDeleted { get; set; }
    public string isNewTerm { get; set; }
}
namespace Application.Shipments.Transactions;

public class AcctgTransTypeDto
{
    public string AcctgTransTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
}
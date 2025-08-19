using Application.Catalog.Products;

namespace Application.Order.CustomerRequests;

public class CustRequestItemDto
{
    public string CustRequestId { get; set; }
    public string CustRequestItemSeqId { get; set; }
    public string? CustRequestResolutionId { get; set; }
    public string StatusId { get; set; }
    public decimal? Priority { get; set; }
    public decimal? SequenceNum { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public ProductLovDto ProductId { get; set; }
    public string ProductName { get; set; }
    public bool IsProductDeleted { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? SelectedAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public string? ConfigId { get; set; }
    public string? Description { get; set; }
    public string? Story { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
}
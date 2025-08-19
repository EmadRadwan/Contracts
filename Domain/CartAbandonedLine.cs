namespace Domain;

public class CartAbandonedLine
{
    public string VisitId { get; set; } = null!;
    public string CartAbandonedLineSeqId { get; set; } = null!;
    public string? ProductId { get; set; }
    public string? ProdCatalogId { get; set; }
    public decimal? Quantity { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Reserv2ndPPPerc { get; set; }
    public decimal? ReservNthPPPerc { get; set; }
    public string? ConfigId { get; set; }
    public decimal? TotalWithAdjustments { get; set; }
    public string? WasReserved { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdCatalog? ProdCatalog { get; set; }
    public Product? Product { get; set; }
}
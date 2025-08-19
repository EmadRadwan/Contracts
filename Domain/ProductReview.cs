namespace Domain;

public class ProductReview
{
    public string ProductReviewId { get; set; } = null!;
    public string? ProductStoreId { get; set; }
    public string? ProductId { get; set; }
    public string? UserLoginId { get; set; }
    public string? StatusId { get; set; }
    public string? PostedAnonymous { get; set; }
    public DateTime? PostedDateTime { get; set; }
    public decimal? ProductRating { get; set; }
    public string? ProductReview1 { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Product? Product { get; set; }
    public ProductStore? ProductStore { get; set; }
    public StatusItem? Status { get; set; }
    public UserLogin? UserLogin { get; set; }
}
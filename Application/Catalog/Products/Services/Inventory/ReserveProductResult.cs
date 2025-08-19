namespace Application.Catalog.Products.Services.Inventory;

public class ReserveProductResult
{
    public decimal QuantityNotReserved { get; set; }

    // Default constructor
    public ReserveProductResult() { }

    // Optional constructor to set QuantityNotReserved during initialization
    public ReserveProductResult(decimal quantityNotReserved)
    {
        QuantityNotReserved = quantityNotReserved;
    }
}


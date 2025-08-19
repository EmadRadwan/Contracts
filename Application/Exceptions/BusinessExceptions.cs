namespace API.Middleware;

public class ProductNotFoundException : ApplicationException
{
    public ProductNotFoundException(string productId)
        : base($"Product with ID {productId} not found.") { }
}

public class ProductTypeNotFoundException : ApplicationException
{
    public ProductTypeNotFoundException(string productTypeId)
        : base($"Product type with ID {productTypeId} not found.") { }
}

public class InsufficientInventoryException : ApplicationException
{
    public InsufficientInventoryException(string productId, decimal requested, decimal available)
        : base($"Insufficient inventory for product {productId}. Requested: {requested}, Available: {available}.") { }
}

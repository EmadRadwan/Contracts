namespace Application.Facilities;

public class PackBulkItemsResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public static PackBulkItemsResult Success()
    {
        return new PackBulkItemsResult { IsSuccess = true };
    }

    public static PackBulkItemsResult Error(string message)
    {
        return new PackBulkItemsResult { IsSuccess = false, ErrorMessage = message };
    }
}

using Application.Core;

namespace Application.Order.CustomerRequests;

public class CustomerRequestParams : PaginationParams
{
    public string OrderBy { get; set; }
    public string SearchTerm { get; set; }
    public string Types { get; set; }
    public string Categories { get; set; }
}
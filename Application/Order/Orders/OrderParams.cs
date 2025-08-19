using Application.Core;

namespace Application.Order.Orders;

public class OrderParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? OrderTypes { get; set; }
    public string? Filter { get; set; }
    public string? Group { get; set; }
    public string? Sort { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
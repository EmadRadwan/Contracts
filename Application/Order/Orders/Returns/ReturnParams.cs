using Application.Core;

namespace Application.Order.Orders.Returns;

public class ReturnParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? SearchTerm { get; set; }
    public string? ReturnTypes { get; set; }
}
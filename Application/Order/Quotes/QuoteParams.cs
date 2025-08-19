using Application.Core;

namespace Application.Order.Quotes;

public class QuoteParams : PaginationParams
{
    public string OrderBy { get; set; }
    public string SearchTerm { get; set; }
    public string Types { get; set; }
    public string Categories { get; set; }
}
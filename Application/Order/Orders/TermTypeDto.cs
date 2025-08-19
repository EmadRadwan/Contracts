namespace Application.Order.Orders;

public class TermTypeDto
{
    public string TermTypeId { get; set; }
    public string Text { get; set;}
    public List<TermTypeDto> Items { get; set; }
}
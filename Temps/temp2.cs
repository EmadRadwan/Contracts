[EnableQuery]
[HttpGet]
public IQueryable<InvoiceView> Get(ODataQueryOptions<InvoiceView> options)
{
    return _mediator.Send(new ListInvoices.Query { Options = options }).Result;
}
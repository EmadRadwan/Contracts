[EnableQuery]  // You can keep this, but it will now work on IEnumerable
public async Task<IActionResult> Get(ODataQueryOptions<PartyRecord> options)
{
    var query = new PartiesList.Query { Options = options };
    var parties = await _mediator.Send(query);

    var result = options.ApplyTo(parties.AsQueryable());  // ApplyTo works on IEnumerable too
    return Ok(result);
}
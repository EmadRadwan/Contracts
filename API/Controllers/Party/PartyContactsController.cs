using Application.Catalog.ProductPrices;
using Application.Parties.PartyContacts;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PartyContactsController : BaseApiController
{
    [HttpGet("{partyId}")]
    public async Task<IActionResult> GetPartyContacts(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListPartyContacts.Query { PartyId = partyId }));
    }

    [HttpPut("updatePartyContact")]
    public async Task<IActionResult> UpdateProductPrice(ProductPrice productPrice)
    {
        return HandleResult(await Mediator.Send(new UpdateProductPrice.Command { ProductPrice = productPrice }));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProductPrice(ProductPrice productPrice)
    {
        return HandleResult(await Mediator.Send(new CreateProductPrice.Command { ProductPrice = productPrice }));
    }
}
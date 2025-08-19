using Application.Accounting.Taxes;
using Application.Shipments.Taxes;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class TaxesController : BaseApiController
{
    [HttpPost("calculateTaxAdjustmentsForSalesOrder", Name = "CalculateTaxAdjustmentsForSalesOrder")]
    public async Task<IActionResult> CalculateTaxAdjustmentsForSalesOrder(
        OrderItemsAndAdjustmentsDto orderItemsAndAdjustments)
    {
        return HandleResult(await Mediator.Send(new CalculateTaxAdjustmentsForSalesOrder.Command
            { OrderItemsAndAdjustments = orderItemsAndAdjustments }));
    }

    [HttpPost("calculateTaxAdjustments", Name = "CalculateTaxAdjustments")]
    public async Task<IActionResult> CalculateTaxAdjustments(
        OrderItemsToBeTaxedDto orderItems)
    {
        return HandleResult(await Mediator.Send(new CalculateTaxAdjustments.Command
            { OrderItems = orderItems }));
    }
    
    [HttpPost("calculateTax", Name = "calculateTax")]
    public async Task<IActionResult> CalculateTax([FromBody] CalculateTaxRequestDto request)
    {
        return HandleResult(await Mediator.Send(new CalculateTax.Command
        {
            OrderItems = request.OrderItems,
            OrderAdjustments = request.OrderAdjustments
        }));
    }
   

    [HttpPost("calculateTaxAdjustmentsForQuote", Name = "CalculateTaxAdjustmentsForQuote")]
    public async Task<IActionResult> CalculateTaxAdjustmentsForQuote(
        QuoteItemsToBeTaxedDto quoteItems)
    {
        return HandleResult(await Mediator.Send(new CalculateTaxAdjustmentsForQuote.Command
            { QuoteItems = quoteItems }));
    }


    [HttpGet("{taxAuthGeoId}/{taxAuthPartyId}/getTaxAuthorityRateProducts")]
    public async Task<IActionResult> GetTaxAuthorityRateProducts(string taxAuthGeoId, string taxAuthPartyId)
    {
        return HandleResult(await Mediator.Send(new GetTaxAuthorityRateProducts.Query
        {
            TaxAuthGeoId = taxAuthGeoId,
            TaxAuthPartyId = taxAuthPartyId
        }));
    }
    
    [HttpGet("{taxAuthGeoId}/{taxAuthPartyId}/getTaxAuthorityCategories")]
    public async Task<IActionResult> GetTaxAuthorityCategories(string taxAuthGeoId, string taxAuthPartyId)
    {
        return HandleResult(await Mediator.Send(new GetTaxAuthorityCategories.Query
        {
            TaxAuthGeoId = taxAuthGeoId,
            TaxAuthPartyId = taxAuthPartyId
        }));
    }
}
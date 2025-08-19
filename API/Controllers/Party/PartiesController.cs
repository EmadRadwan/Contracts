#nullable enable
using Application.Accounting.Services;
using Application.Parties.Parties;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PartiesController : BaseApiController
{
    [HttpGet("getCustomersLov", Name = "GetCustomersLov")]
    public async Task<IActionResult> GetCustomersLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetCustomersLov.Query { Params = param }));
    }

    [HttpGet("getPartiesLov", Name = "GetPartiesLov")]
    public async Task<IActionResult> GetPartiesLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetPartiesLov.Query { Params = param }));
    }

    [HttpGet("getAllPartiesLov", Name = "GetAllPartiesLov")]
    public async Task<IActionResult> GetAllPartiesLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetAllPartiesLov.Query { Params = param }));
    }

    [HttpGet("getPartiesWithEmployeesLov", Name = "GetPartiesWithEmployeesLov")]
    public async Task<IActionResult> GetPartiesWithEmployeesLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetPartiesWithEmployeesLov.Query { Params = param }));
    }

    [HttpGet("getSuppliersLov", Name = "GetSuppliersLov")]
    public async Task<IActionResult> GetSuppliersLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetSuppliersLov.Query { Params = param }));
    }

    [HttpGet("{customerId}/getCustomerTaxStatus", Name = "GetCustomerTaxStatus")]
    public async Task<IActionResult> GetCustomerTaxStatus(string customerId)
    {
        return HandleResult(await Mediator.Send(new GetCustomerTaxStatus.Query { CustomerId = customerId }));
    }

    [HttpGet("{partyId}/getCustomer", Name = "GetCustomer")]
    public async Task<IActionResult> GetCustomer(string partyId)
    {
        return HandleResult(await Mediator.Send(new GetCustomer.Query { PartyId = partyId }));
    }

    [HttpGet("{partyId}/getSupplier", Name = "GetSupplier")]
    public async Task<IActionResult> GetSupplier(string partyId)
    {
        return HandleResult(await Mediator.Send(new GetSupplier.Query { PartyId = partyId }));
    }

    [HttpGet("getSuppliers", Name = "GetSuppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        return HandleResult(await Mediator.Send(new GetSuppliers.Query()));
    }

    [HttpPost("createCustomer", Name = "CreateCustomer")]
    public async Task<IActionResult> CreateCustomer(PartyDto partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateCustomer.Command { PartyDto = partyDto }));
    }

    [HttpPost("createSupplier", Name = "CreateSupplier")]
    public async Task<IActionResult> CreateSupplier(PartyDto partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateSupplier.Command { PartyDto = partyDto }));
    }

    [HttpPut("updateCustomer", Name = "UpdateCustomer")]
    public async Task<IActionResult> UpdateCustomer(PartyDto partyDto)
    {
        return HandleResult(await Mediator.Send(new UpdateCustomer.Command { PartyDto = partyDto }));
    }

    [HttpPut("updateSupplier", Name = "UpdateSupplier")]
    public async Task<IActionResult> UpdateSupplier(PartyDto partyDto)
    {
        return HandleResult(await Mediator.Send(new UpdateSupplier.Command { PartyDto = partyDto }));
    }

    [HttpGet("getCompanies")]
    public async Task<IActionResult> GetCompanies()
    {
        return HandleResult(await Mediator.Send(new ListCompanies.Query()));
    }

    [HttpGet("listRoles")]
    public async Task<IActionResult> ListRoles()
    {
        return HandleResult(await Mediator.Send(new ListRoles.Query()));
    }
    
    [HttpGet("{partyId}/getPartyFinancialHistory")]
    public async Task<IActionResult> GetPartyFinancialHistory(
        string partyId,
        [FromQuery] string? organizationPartyId = null,
        [FromQuery] string? defaultCurrencyUomId = null)
    {
        if (string.IsNullOrWhiteSpace(partyId))
        {
            return BadRequest("PartyId is required.");
        }

        var query = new GetPartyFinancialHistory.Query
        {
            PartyId = partyId,
            OrganizationPartyId = organizationPartyId,
            DefaultCurrencyUomId = defaultCurrencyUomId,
        };

        return HandleResult(await Mediator.Send(query));
    }
}
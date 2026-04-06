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

    [HttpGet("getPartiesEmployeesLov", Name = "GetPartiesEmployeesLov")]
    public async Task<IActionResult> GetPartiesEmployeesLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetPartiesEmployeesLov.Query { Params = param }));
    }

    [HttpGet("getSuppliersLov", Name = "GetSuppliersLov")]
    public async Task<IActionResult> GetSuppliersLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetSuppliersLov.Query { Params = param }));
    }

    [HttpGet("getContractorsLov", Name = "GetContractorsLov")]
    public async Task<IActionResult> GetContractorsLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetContractorsLov.Query { Params = param }));
    }

    [HttpGet("getContractorsAndSuppliersLov", Name = "GetContractorsAndSuppliersLov")]
    public async Task<IActionResult> GetContractorsAndSuppliersLov([FromQuery] PartyLovParams param)
    {
        return HandleResult(await Mediator.Send(new GetContractorsAndSuppliersLov.Query { Params = param }));
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

    [HttpGet("{partyId}/getEmployee", Name = "GetEmployee")]
    public async Task<IActionResult> GetEmployee(string partyId)
    {
        return HandleResult(await Mediator.Send(new GetEmployee.Query { PartyId = partyId }));
    }

    [HttpGet("{partyId}/getSupplier", Name = "GetSupplier")]
    public async Task<IActionResult> GetSupplier(string partyId)
    {
        return HandleResult(await Mediator.Send(new GetSupplier.Query { PartyId = partyId }));
    }

    [HttpGet("{partyId}/getContractor", Name = "GetContractor")]
    public async Task<IActionResult> GetContractor(string partyId)
    {
        return HandleResult(await Mediator.Send(new GetContractor.Query { PartyId = partyId }));
    }

    [HttpPut("updateMainRole/{partyId}")]
    public async Task<IActionResult> UpdateMainRole(string partyId, [FromBody] UpdateMainRoleDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateMainRole.Command
            { PartyId = partyId, MainRole = dto.MainRole }));
    }

    [HttpGet("listEmployeesWithSalary", Name = "ListEmployeesWithSalary")]
    public async Task<IActionResult> ListEmployeesWithSalary()
    {
        return HandleResult(await Mediator.Send(new ListEmployeesWithSalary.Query()));
    }

    [HttpGet("getSuppliers", Name = "GetSuppliers")]
    public async Task<IActionResult> GetSuppliers()
    {
        return HandleResult(await Mediator.Send(new GetSuppliers.Query()));
    }

    [HttpPost("createCustomer", Name = "CreateCustomer")]
    public async Task<IActionResult> CreateCustomer(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateCustomer.Command { PartyDto = partyDto }));
    }

    [HttpPost("createParty", Name = "CreateParty")]
    public async Task<IActionResult> CreateParty(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateParty.Command { PartyDto = partyDto }));
    }

    [HttpPost("createContractor", Name = "CreateContractor")]
    public async Task<IActionResult> CreateContractor(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateContractor.Command { PartyDto = partyDto }));
    }

    [HttpPost("createSupplier", Name = "CreateSupplier")]
    public async Task<IActionResult> CreateSupplier(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateSupplier.Command { PartyDto = partyDto }));
    }

    [HttpPost("createLead", Name = "CreateLead")]
    public async Task<IActionResult> CreateLead(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateLead.Command { PartyDto = partyDto }));
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

    [HttpPut("updateContractor", Name = "UpdateContractor")]
    public async Task<IActionResult> UpdateContractor(PartyDto partyDto)
    {
        return HandleResult(await Mediator.Send(new UpdateContractor.Command { PartyDto = partyDto }));
    }

    [HttpDelete("deleteParty/{partyId}")]
    public async Task<IActionResult> DeleteParty(string partyId)
    {
        return HandleResult(await Mediator.Send(new DeleteParty.Command { PartyId = partyId }));
    }

    [HttpGet("getCompanies")]
    public async Task<IActionResult> GetCompanies()
    {
        return HandleResult(await Mediator.Send(new ListCompanies.Query()));
    }

    [HttpGet("listAllParties")]
    public async Task<IActionResult> ListAllParties()
    {
        return HandleResult(await Mediator.Send(new ListAllParties.Query()));
    }

    [HttpGet("listRoles")]
    public async Task<IActionResult> ListRoles()
    {
        return HandleResult(await Mediator.Send(new ListRoles.Query()));
    }

    [HttpGet("listRoleTypes")]
    public async Task<IActionResult> ListRoleTypes()
    {
        return HandleResult(await Mediator.Send(new ListRoleTypes.Query()));
    }

    [HttpGet("{partyId}/listPartyRoles")]
    public async Task<IActionResult> ListPartyRoles(string partyId)
    {
        return HandleResult(await Mediator.Send(new ListPartyRoles.Query { PartyId = partyId }));
    }

    [HttpPost("addPartyRole")]
    public async Task<IActionResult> AddPartyRole(AddPartyRole.Command command)
    {
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("deletePartyRole")]
    public async Task<IActionResult> DeletePartyRole([FromQuery] string partyId, [FromQuery] string roleTypeId)
    {
        return HandleResult(await Mediator.Send(new DeletePartyRole.Command { PartyId = partyId, RoleTypeId = roleTypeId }));
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
    
    [HttpGet("subledger/{partyId}")]
    public async Task<IActionResult> GetPartySubLedger(
        string partyId,
        [FromQuery] string? organizationPartyId = null,
        [FromQuery] string? defaultCurrencyUomId = null)
    {
        var query = new GetPartySubLedgerDetails.Query 
        { 
            PartyId = partyId, 
            OrganizationPartyId = organizationPartyId, 
            DefaultCurrencyUomId = defaultCurrencyUomId 
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost("createEmployee", Name = "CreateEmployee")]
    public async Task<IActionResult> CreateEmployee(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new CreateEmployee.Command { PartyDto = partyDto }));
    }

    [HttpPut("updateEmployee", Name = "UpdateEmployee")]
    public async Task<IActionResult> UpdateEmployee(PartyDto2 partyDto)
    {
        return HandleResult(await Mediator.Send(new UpdateEmployee.Command { PartyDto = partyDto }));
    }
}
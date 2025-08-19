using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class OrganizationGlReportsController : BaseApiController
{
    [HttpGet("{companyId}/getPartyAccountingPreferences")]
    public async Task<IActionResult> GetPartyAccountingPreferences(string companyId)
    {
        return HandleResult(await Mediator.Send(new GetPartyAccountingPreferences.Query { CompanyId = companyId }));
    }

    [HttpGet("getInventoryValuationReport")]
    public async Task<IActionResult> GetInventoryValuationReport(
        [FromQuery] string? organizationPartyId, 
        [FromQuery] string? facilityId, 
        [FromQuery] string? productId, 
        [FromQuery] DateTime? thruDate)
    {
        return HandleResult(await Mediator.Send(new GetInventoryValuationReport.Query
        {
            OrganizationPartyId = organizationPartyId,
            FacilityId = facilityId,
            ProductId = productId,
            ThruDate = thruDate
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getTransactionTotalsReport")]
    public async Task<IActionResult> GetTransactionTotalsReport(
        string selectedAccountingCompanyId, 
        [FromQuery] string? glFiscalTypeId, 
        [FromQuery] int? selectedMonth, 
        [FromQuery] DateTime? fromDate, 
        [FromQuery] DateTime? thruDate
    )
    {
        return HandleResult(await Mediator.Send(new GetTransactionTotalsReport.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            FromDate = fromDate, 
            ThruDate = thruDate, 
            GlFiscalTypeId = glFiscalTypeId, 
            SelectedMonth = selectedMonth 
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getIncomeStatementReport")]
    public async Task<IActionResult> GetIncomeStatementReport(
        string selectedAccountingCompanyId, 
        [FromQuery] string glFiscalTypeId,
        [FromQuery] int? selectedMonth,  
        [FromQuery] DateTime? fromDate, 
        [FromQuery] DateTime? thruDate
    )
    {
        return HandleResult(await Mediator.Send(new GetIncomeStatementReport.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            FromDate = fromDate, 
            ThruDate = thruDate, 
            GlFiscalTypeId = glFiscalTypeId, 
            SelectedMonth = selectedMonth 
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getCashFlowStatementReport")]
    public async Task<IActionResult> GetCashFlowStatementReport(
        string selectedAccountingCompanyId, 
        [FromQuery] string glFiscalTypeId, 
        [FromQuery] int? selectedMonth, 
        [FromQuery] DateTime? fromDate, 
        [FromQuery] DateTime? thruDate
    )
    {
        return HandleResult(await Mediator.Send(new GetCashFlowStatementReport.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            FromDate = fromDate, 
            ThruDate = thruDate, 
            GlFiscalTypeId = glFiscalTypeId, 
            SelectedMonth = selectedMonth 
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getGlAccountTrialBalanceReport")]
    public async Task<IActionResult> GetGlAccountTrialBalanceReport(
        string selectedAccountingCompanyId, 
        [FromQuery] string timePeriodId, 
        [FromQuery] string glAccountId, 
        [FromQuery] string? isPosted
    )
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTrialBalanceReport.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            TimePeriodId = timePeriodId,
            GlAccountId = glAccountId,
            IsPosted = isPosted
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getBalanceSheetReport")]
    public async Task<IActionResult> GetBalanceSheetReport(
        string selectedAccountingCompanyId, 
        [FromQuery] string glFiscalTypeId, 
        [FromQuery] DateTime? thruDate 
    )
    {
        return HandleResult(await Mediator.Send(new GetBalanceSheetReport.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            GlFiscalTypeId = glFiscalTypeId,
            ThruDate = thruDate
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/generateComparativeBalanceSheet")]
    public async Task<IActionResult> GenerateComparativeBalanceSheet(
        string selectedAccountingCompanyId, 
        [FromQuery] DateTime? period1ThruDate,
        [FromQuery] string period1GlFiscalTypeId,
        [FromQuery] DateTime? period2ThruDate,
        [FromQuery] string period2GlFiscalTypeId
    )
    {
        return HandleResult(await Mediator.Send(new GenerateComparativeBalanceSheet.Query { 
            OrganizationPartyId = selectedAccountingCompanyId, 
            Period1GlFiscalTypeId = period1GlFiscalTypeId,
            Period2GlFiscalTypeId = period2GlFiscalTypeId,
            Period1ThruDate = period1ThruDate,
            Period2ThruDate = period2ThruDate
        }));
    }
}
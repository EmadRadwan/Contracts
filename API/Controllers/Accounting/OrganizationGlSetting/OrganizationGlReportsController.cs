using System.Globalization;
using System.Text.RegularExpressions;
using Application.Accounting.OrganizationGlSettings;
using Application.Accounting.Services.Models;
using Application.Interfaces;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting.OrganizationGlSetting;

public class OrganizationGlReportsController : BaseApiController
{
    private readonly IGlAccountTransactionsReportService _glAccountTransactionsReportService;

    public OrganizationGlReportsController(IGlAccountTransactionsReportService glAccountTransactionsReportService)
    {
        _glAccountTransactionsReportService = glAccountTransactionsReportService;
    }

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

    [HttpGet("{selectedAccountingCompanyId}/getBalanceSheetGlAccountTransactionDetails")]
    public async Task<IActionResult> GetBalanceSheetGlAccountTransactionDetails(
        string selectedAccountingCompanyId,
        [FromQuery] DateTime? thruDate,
        [FromQuery] string glFiscalTypeId,
        [FromQuery] string glAccountId,
        [FromQuery] bool includePrePeriodTransactions
    )
    {
        return HandleResult(await Mediator.Send(new GetBalanceSheetGlAccountTransactionDetails.Query
        {
            OrganizationPartyId = selectedAccountingCompanyId,
            ThruDate = thruDate,
            GlFiscalTypeId = glFiscalTypeId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/getIncomeStatementGlAccountTransactionDetails")]
    public async Task<IActionResult> GetIncomeStatementGlAccountTransactionDetails(
        string selectedAccountingCompanyId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? thruDate,
        [FromQuery] int? selectedMonth,
        [FromQuery] string glFiscalTypeId,
        [FromQuery] string glAccountId,
        [FromQuery] bool includePrePeriodTransactions,
        [FromQuery] string? isPosted = null
    )
    {
        return HandleResult(await Mediator.Send(new GetIncomeStatementGlAccountTransactionDetails.Query
        {
            OrganizationPartyId = selectedAccountingCompanyId,
            FromDate = fromDate,
            ThruDate = thruDate,
            SelectedMonth = selectedMonth,
            GlFiscalTypeId = glFiscalTypeId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions,
            IsPosted = isPosted
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
    [HttpGet("{selectedAccountingCompanyId}/getComparativeIncomeStatementReport")]
    public async Task<IActionResult> GetComparativeIncomeStatementReport(
        string selectedAccountingCompanyId,
        [FromQuery] DateTime? fromDate1, [FromQuery] DateTime? thruDate1,
        [FromQuery] string glFiscalTypeId1, [FromQuery] int? selectedMonth1,
        [FromQuery] DateTime? fromDate2, [FromQuery] DateTime? thruDate2,
        [FromQuery] string glFiscalTypeId2, [FromQuery] int? selectedMonth2
    )
    {
        return HandleResult(await Mediator.Send(new GetComparativeIncomeStatementReport.Query
        {
            OrganizationPartyId = selectedAccountingCompanyId,
            FromDate1 = fromDate1,
            ThruDate1 = thruDate1,
            GlFiscalTypeId1 = glFiscalTypeId1,
            SelectedMonth1 = selectedMonth1,
            FromDate2 = fromDate2,
            ThruDate2 = thruDate2,
            GlFiscalTypeId2 = glFiscalTypeId2,
            SelectedMonth2 = selectedMonth2
        }));
    }

    // PDF export of the balance sheet's transaction-details modal. Same query the modal itself
    // uses, rendered server-side with Telerik so Arabic shapes correctly — the client-side
    // kendo-drawing PDF does not (see the project report). Mirrors
    // TrialBalanceController.GetGlAccountTransactionsPdf; the only difference is the query.
    [HttpGet("{selectedAccountingCompanyId}/balanceSheetGlAccountTransactionsPdf")]
    public async Task<IActionResult> GetBalanceSheetGlAccountTransactionsPdf(
        string selectedAccountingCompanyId,
        [FromQuery] DateTime? thruDate,
        [FromQuery] string glFiscalTypeId,
        [FromQuery] string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = false,
        [FromQuery] string format = "PDF")
    {
        var result = await Mediator.Send(new GetBalanceSheetGlAccountTransactionDetails.Query
        {
            OrganizationPartyId = selectedAccountingCompanyId,
            ThruDate = thruDate,
            GlFiscalTypeId = glFiscalTypeId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions
        });

        return RenderGlAccountTransactions(result, format);
    }

    // PDF export of the income statement's transaction-details modal. See the balance sheet
    // action above.
    [HttpGet("{selectedAccountingCompanyId}/incomeStatementGlAccountTransactionsPdf")]
    public async Task<IActionResult> GetIncomeStatementGlAccountTransactionsPdf(
        string selectedAccountingCompanyId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? thruDate,
        [FromQuery] int? selectedMonth,
        [FromQuery] string glFiscalTypeId,
        [FromQuery] string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = false,
        [FromQuery] string? isPosted = null,
        [FromQuery] string format = "PDF")
    {
        var result = await Mediator.Send(new GetIncomeStatementGlAccountTransactionDetails.Query
        {
            OrganizationPartyId = selectedAccountingCompanyId,
            FromDate = fromDate,
            ThruDate = thruDate,
            SelectedMonth = selectedMonth,
            GlFiscalTypeId = glFiscalTypeId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions,
            IsPosted = isPosted
        });

        return RenderGlAccountTransactions(result, format);
    }

    // Shared rendering + download plumbing for the two actions above. The period suffix comes from
    // the dates the handler resolved, because neither drill-down has a named CustomTimePeriod the
    // way the trial balance does.
    private IActionResult RenderGlAccountTransactions(Result<GlAccountTransactionDetails>? result, string format)
    {
        if (result == null || !result.IsSuccess || result.Value == null)
            return BadRequest(result?.Error ?? "Could not load GL account transaction details.");

        var bytes = _glAccountTransactionsReportService.Render(result.Value, format);

        var normalized = string.IsNullOrWhiteSpace(format) ? "PDF" : format.Trim().ToUpperInvariant();
        var (contentType, extension) = normalized switch
        {
            "XLSX" => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            "DOCX" => ("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx"),
            _ => ("application/pdf", "pdf")
        };

        var period = result.Value.PeriodFromDate.HasValue && result.Value.PeriodThruDate.HasValue
            ? $"{result.Value.PeriodFromDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-{result.Value.PeriodThruDate.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}"
            : "period";
        var safePeriod = Regex.Replace(period, @"[^a-zA-Z0-9\u0600-\u06FF\s-]", "_").Trim();

        return File(bytes, contentType, $"GL_Transactions_{result.Value.AccountCode}_{safePeriod}.{extension}");
    }
}

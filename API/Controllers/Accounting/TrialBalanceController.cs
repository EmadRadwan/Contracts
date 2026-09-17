using System.Text.RegularExpressions;
using Application.Accounting.Services;
using Application.Interfaces;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class TrialBalanceController : BaseApiController
{
    private readonly IGlAccountTransactionsReportService _glAccountTransactionsReportService;

    public TrialBalanceController(IGlAccountTransactionsReportService glAccountTransactionsReportService)
    {
        _glAccountTransactionsReportService = glAccountTransactionsReportService;
    }

    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/getTrialBalanceReport")]
    public async Task<IActionResult> GetTrialBalanceReport(string selectedAccountingCompanyId, string customTimePeriodId)
    {
        return HandleResult(await Mediator.Send(new GetTrialBalanceReport.Query { CustomTimePeriodId = customTimePeriodId, OrganizationPartyId = selectedAccountingCompanyId }));
    }
    
    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/generateTrialBalanceReport")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateTrialBalanceReport(string selectedAccountingCompanyId, string customTimePeriodId)
    {
        return HandleResult(await Mediator.Send(new GetTrialBalanceReport.Query { CustomTimePeriodId = customTimePeriodId, OrganizationPartyId = selectedAccountingCompanyId }));
    }

    // New COA-hierarchy-level version of the trial balance. Additive endpoint — does not replace
    // getTrialBalanceReport above.
    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/getTrialBalanceByLevelReport")]
    public async Task<IActionResult> GetTrialBalanceByLevelReport(string selectedAccountingCompanyId, string customTimePeriodId)
    {
        return HandleResult(await Mediator.Send(new GetTrialBalanceByLevel.Query { CustomTimePeriodId = customTimePeriodId, OrganizationPartyId = selectedAccountingCompanyId }));
    }

    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/{glAccountId}/getGlAccountTransactionDetails")]
    public async Task<IActionResult> GetGlAccountTransactionDetails(
        string selectedAccountingCompanyId,
        string customTimePeriodId,
        string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = true,
        [FromQuery] bool showCorrectedDuplicatePairs = false)
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTransactionDetails.Query
        {
            CustomTimePeriodId = customTimePeriodId,
            OrganizationPartyId = selectedAccountingCompanyId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions,
            ShowCorrectedDuplicatePairs = showCorrectedDuplicatePairs
        }));
    }

    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/{glAccountId}/generateGlAccountTransactionDetails")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateGlAccountTransactionDetails(
        string selectedAccountingCompanyId,
        string customTimePeriodId,
        string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = true,
        [FromQuery] bool showCorrectedDuplicatePairs = false)
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTransactionDetails.Query
        {
            CustomTimePeriodId = customTimePeriodId,
            OrganizationPartyId = selectedAccountingCompanyId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions,
            ShowCorrectedDuplicatePairs = showCorrectedDuplicatePairs
        }));
    }

    // PDF export of the transaction-details modal (shared by the classic and by-level trial balance).
    // Same query as getGlAccountTransactionDetails above; rendered server-side with Telerik Reporting
    // so Arabic shapes correctly (client-side kendo-drawing PDF does not — see the project report).
    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/{glAccountId}/glAccountTransactionsPdf")]
    public async Task<IActionResult> GetGlAccountTransactionsPdf(
        string selectedAccountingCompanyId,
        string customTimePeriodId,
        string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = false,
        [FromQuery] bool showCorrectedDuplicatePairs = false,
        [FromQuery] string format = "PDF")
    {
        var result = await Mediator.Send(new GetGlAccountTransactionDetails.Query
        {
            CustomTimePeriodId = customTimePeriodId,
            OrganizationPartyId = selectedAccountingCompanyId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions,
            ShowCorrectedDuplicatePairs = showCorrectedDuplicatePairs
        });

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

        var safePeriod = Regex.Replace(result.Value.PeriodName ?? customTimePeriodId, @"[^a-zA-Z0-9\u0600-\u06FF\s-]", "_").Trim();
        return File(bytes, contentType, $"GL_Transactions_{result.Value.AccountCode}_{safePeriod}.{extension}");
    }
}
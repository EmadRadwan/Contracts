using Application.Accounting.Services;
using Application.Shipments.OrganizationGlSettings;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class TrialBalanceController : BaseApiController
{
    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/getTrialBalanceReport")]
    public async Task<IActionResult> GetTrialBalanceReport(string selectedAccountingCompanyId, string customTimePeriodId)
    {
        return HandleResult(await Mediator.Send(new GetTrialBalanceReport.Query { CustomTimePeriodId = customTimePeriodId, OrganizationPartyId = selectedAccountingCompanyId }));
    }
    
    [HttpGet("{selectedAccountingCompanyId}/{customTimePeriodId}/{glAccountId}/getGlAccountTransactionDetails")]
    public async Task<IActionResult> GetGlAccountTransactionDetails(
        string selectedAccountingCompanyId,
        string customTimePeriodId,
        string glAccountId,
        [FromQuery] bool includePrePeriodTransactions = true)
    {
        return HandleResult(await Mediator.Send(new GetGlAccountTransactionDetails.Query
        {
            CustomTimePeriodId = customTimePeriodId,
            OrganizationPartyId = selectedAccountingCompanyId,
            GlAccountId = glAccountId,
            IncludePrePeriodTransactions = includePrePeriodTransactions
        }));
    }
}
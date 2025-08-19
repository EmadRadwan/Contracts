using Application.Shipments.Agreement;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class AgreementsController : BaseApiController
{
    [HttpGet("{agreementId}/getAgreementItems", Name = "GetAgreementItems")]
    public async Task<IActionResult> GetAgreementItems(string agreementId)
    {
        return HandleResult(await Mediator.Send(new GetAgreementItems.Query{ AgreementId = agreementId}));
    }

    [HttpGet("{agreementId}/getAgreementTerms", Name = "GetAgreementTerms")]
    public async Task<IActionResult> GetAgreementTerms(string agreementId)
    {
        return HandleResult(await Mediator.Send(new GetAgreementTerms.Query{ AgreementId = agreementId}));
    }

    [HttpGet("{partyId}/{orderType}/getAgreementsByPartyId", Name = "GetAgreementsByPartyId")]
    public async Task<IActionResult> GetAgreementsByPartyId(string partyId, string orderType)
    {
        return HandleResult(await Mediator.Send(new GetAgreementsByPartyId.Query{ PartyId = partyId, OrderType = orderType}));
    }
}
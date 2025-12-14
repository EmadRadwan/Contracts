using Microsoft.AspNetCore.Mvc;
using Application.Shipments.GlobalGlSettings;
using System.Threading.Tasks;
using Application.GlAccounts;
using Application.Accounting.GlobalGlSettings;

namespace API.Controllers.Accounting
{
    public class GlAccountsController : BaseApiController
    {
        [HttpGet("getGlAccountsLov")]
        public async Task<IActionResult> GetGlAccountsLov()
        {
            var language = GetLanguage();
            return HandleResult(await Mediator.Send(new GetGlAccountsLov.Query { Language = language }));
        }

        [HttpGet("{parentGlAccountId}/getChildGlAccounts")]
        public async Task<IActionResult> GetChildGlAccounts(string parentGlAccountId)
        {
            return HandleResult(await Mediator.Send(new GetChildGlAccounts.Query { ParentGlAccountId = parentGlAccountId }));
        }

        [HttpGet("getAdvancePaymentGlAccounts")]
        public async Task<IActionResult> GetAdvancePaymentGlAccounts()
        {
            var lang = GetLanguage(); // Assuming GetLanguage is defined elsewhere
            return HandleResult(await Mediator.Send(new ListAdvancePaymentGlAccounts.Query { Language = lang }));
        }

        [HttpPost]
        public async Task<IActionResult> CreateGlAccount([FromBody] CreateGlAccountRequest request)
        {
            return HandleResults(await Mediator.Send(new CreateGlAccount.Command { Request = request }));
        }

        [HttpPut("{glAccountId}")]
        public async Task<IActionResult> UpdateGlAccount(string glAccountId, [FromBody] UpdateGlAccountRequest request)
        {
            var updateRequest = new UpdateGlAccountRequest
            {
                GlAccountId = glAccountId,
                AccountName = request.AccountName,
                Description = request.Description,
                ParentGlAccountId = request.ParentGlAccountId
            };
            return HandleResults(await Mediator.Send(new UpdateGlAccount.Command { Request = updateRequest }));
        }
    }
}
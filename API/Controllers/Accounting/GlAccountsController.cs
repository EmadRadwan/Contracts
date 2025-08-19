using Microsoft.AspNetCore.Mvc;
using Application.Shipments.GlobalGlSettings;
using System.Threading.Tasks;

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
    }
}
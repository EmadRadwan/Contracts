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
            request.GlAccountId = glAccountId;
            return HandleResults(await Mediator.Send(new UpdateGlAccount.Command { Request = request }));
        }
        
        [HttpGet("getAllGlAccountTypes")]
        public async Task<IActionResult> GetAllGlAccountTypes([FromQuery] GlAccountTypeParams param)
        {
            return HandleResult(await Mediator.Send(new GetAllGlAccountTypes.Query { Params = param }));
        }
        
        [HttpGet("getAllGlAccountClasses")]
        public async Task<IActionResult> GetAllGlAccountClasses([FromQuery] GlAccountClassParams param)
        {
            return HandleResult(await Mediator.Send(new GetAllGlAccountClasses.Query { Params = param }));
        }

        [HttpGet("getGlReports")]
        public async Task<IActionResult> GetGlReports([FromQuery] GlReportParams param)
        {
            return HandleResult(await Mediator.Send(new GetGlReports.Query { Params = param }));
        }

        [HttpGet("getGlClassCourses")]
        public async Task<IActionResult> GetGlClassCourses([FromQuery] GlClassCourseParams param)
        {
            return HandleResult(await Mediator.Send(new GetGlClassCourses.Query { Params = param }));
        }

        [HttpGet("getGlSubClasses")]
        public async Task<IActionResult> GetGlSubClasses([FromQuery] GlSubClassParams param)
        {
            return HandleResult(await Mediator.Send(new GetGlSubClasses.Query { Params = param }));
        }

        [HttpGet("getGlSubClasses2")]
        public async Task<IActionResult> GetGlSubClasses2([FromQuery] GlSubClass2Params param)
        {
            return HandleResult(await Mediator.Send(new GetGlSubClasses2.Query { Params = param }));
        }

        [HttpGet("getGlAccountCourseLabels")]
        public async Task<IActionResult> GetGlAccountCourseLabels([FromQuery] GlAccountCourseLabelParams param)
        {
            return HandleResult(await Mediator.Send(new GetGlAccountCourseLabels.Query { Params = param }));
        }
    }
}
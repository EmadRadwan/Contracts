using Application.Accounting.FinAccounts;
using Application.Accounting.Payments;
using Application.Shipments.Payments;
using Application.Order.Orders;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Accounting;

public class PaymentApplicationsController : BaseApiController
{
    [HttpDelete("{paymentApplicationId}")]
    public async Task<IActionResult> RemovePaymentApplication(string paymentApplicationId)
    {
        var result = await Mediator.Send(new RemovePaymentApplication.Command { PaymentApplicationId = paymentApplicationId });
        return HandleResults(result);
    }
    
}
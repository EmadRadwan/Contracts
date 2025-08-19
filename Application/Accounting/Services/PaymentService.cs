using Application.Accounting.Payments;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Manufacturing;
using Application.Order.Orders;
using Application.Shipments.Payments;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPaymentService
{
    Task<OrderPaymentPreference> CreatePurchaseOrderPaymentPreference(OrderDto purchaseOrder);
    Task<List<Payment>> ProcessSalesOrderPaymentsForQuickShip(OrderDto salesOrder);
    Task CreatePaymentApplicationsForQuickShipSalesOrder(string orderId, string invoiceId);

    Task<Payment> CreatePurchaseOrderPayment(OrderDto purchaseOrder, OrderPaymentPreference paymentPreference);
    
}

public class PaymentService : IPaymentService
{
    private readonly DataContext _context;
    private readonly IInvoiceService _invoiceService;
    private readonly IUtilityService _utilityService;
    private readonly IPaymentApplicationService _paymentApplicationService;
    private readonly ILogger _logger;
    private readonly IOrderHelperService _orderHelperService;


    public PaymentService(DataContext context, IUtilityService utilityService,
        IInvoiceService invoiceService, 
        IPaymentApplicationService paymentApplicationService, IOrderHelperService orderHelperService, ILogger<PaymentService> logger)
    {
        _context = context;
        _utilityService = utilityService;
        _invoiceService = invoiceService;
        _paymentApplicationService = paymentApplicationService;
        _orderHelperService = orderHelperService;
        _logger = logger;
    }


    
    public async Task<OrderPaymentPreference> CreatePurchaseOrderPaymentPreference(OrderDto purchaseOrder)
    {
        var stamp = DateTime.UtcNow;
        // Get the next sequence for the OrderPaymentPreferenceId
        var newOrderPaymentPreferenceSequence = await _utilityService.GetNextSequence("OrderPaymentPreference");
        var paymentPreference = new OrderPaymentPreference
        {
            OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
            OrderId = purchaseOrder.OrderId,
            PaymentMethodTypeId = "COMPANY_ACCOUNT",
            StatusId = "PMNT_NOT_PAID",
            MaxAmount = (decimal)purchaseOrder.GrandTotal,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };

        await _context.OrderPaymentPreferences.AddAsync(paymentPreference);

        return paymentPreference;
    }

    public async Task<List<Payment>> ProcessSalesOrderPaymentsForQuickShip(OrderDto salesOrder)
    {
        var payments = new List<Payment>();
        //todo: consider adjusting this to use the payment method type from from the order
        var stamp = DateTime.UtcNow;

        var orderRoleBillToCustomer = await _utilityService.GetOrderRole(salesOrder.OrderId, "BILL_TO_CUSTOMER");
        var orderRoleBillFromVendor = await _utilityService.GetOrderRole(salesOrder.OrderId, "BILL_FROM_VENDOR");
        var newOrderPaymentPreferenceSequence = "";
        var newPaymentSequence = "";
        var paymentPreference = new OrderPaymentPreference();
        var orderPaymentReceivedStatus = new OrderStatus();


        // loop through payments and create payment records
        foreach (var payment in salesOrder.OrderPayments)
        {
            newOrderPaymentPreferenceSequence = await _utilityService.GetNextSequence("OrderPaymentPreference");
            paymentPreference = new OrderPaymentPreference
            {
                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                OrderId = salesOrder.OrderId,
                PaymentMethodTypeId = payment.PaymentMethodTypeId,
                MaxAmount = payment.Amount,
                StatusId = "PAYMENT_RECEIVED",
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };

            await _context.OrderPaymentPreferences.AddAsync(paymentPreference);

            orderPaymentReceivedStatus = new OrderStatus
            {
                OrderStatusId = Guid.NewGuid().ToString(),
                StatusId = "PAYMENT_RECEIVED",
                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                OrderId = salesOrder.OrderId,
                StatusDatetime = stamp,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.OrderStatuses.Add(orderPaymentReceivedStatus);

            newPaymentSequence = await _utilityService.GetNextSequence("Payment");

            var newPayment = new Payment
            {
                PaymentId = newPaymentSequence,
                PaymentTypeId = "CUSTOMER_PAYMENT",
                PaymentMethodTypeId = payment.PaymentMethodTypeId,
                PaymentPreferenceId = newOrderPaymentPreferenceSequence,
                PaymentMethodId = null,
                PaymentGatewayResponseId = null,
                StatusId = "PMNT_RECEIVED",
                PartyIdFrom = orderRoleBillToCustomer.PartyId,
                PartyIdTo = orderRoleBillFromVendor.PartyId,
                RoleTypeIdTo = orderRoleBillToCustomer.RoleTypeId,
                PaymentRefNum = null,
                EffectiveDate = stamp,
                Amount = payment.Amount,
                CurrencyUomId = salesOrder.CurrencyUomId,
                Comments = null,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.Payments.Add(newPayment);
            payments.Add(newPayment);
        }

        // check if a billing account has been used
        if (salesOrder.BillingAccountId != null)
        {
            newOrderPaymentPreferenceSequence = await _utilityService.GetNextSequence("OrderPaymentPreference");
            paymentPreference = new OrderPaymentPreference
            {
                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                OrderId = salesOrder.OrderId,
                PaymentMethodTypeId = "EXT_BILLACT",
                MaxAmount = (decimal)salesOrder.UseUpToFromBillingAccount,
                StatusId = "PAYMENT_NOT_RECEIVED",
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };

            await _context.OrderPaymentPreferences.AddAsync(paymentPreference);

            orderPaymentReceivedStatus = new OrderStatus
            {
                OrderStatusId = Guid.NewGuid().ToString(),
                StatusId = "PMNT_NOT_PAID",
                OrderPaymentPreferenceId = newOrderPaymentPreferenceSequence,
                OrderId = salesOrder.OrderId,
                StatusDatetime = stamp,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.OrderStatuses.Add(orderPaymentReceivedStatus);

            newPaymentSequence = await _utilityService.GetNextSequence("Payment");
            var newPayment = new Payment
            {
                PaymentId = newPaymentSequence,
                PaymentTypeId = "CUSTOMER_PAYMENT",
                PaymentMethodTypeId = "EXT_BILLACT",
                PaymentPreferenceId = newOrderPaymentPreferenceSequence,
                PaymentMethodId = null,
                PaymentGatewayResponseId = null,
                StatusId = "PMNT_NOT_PAID",
                PartyIdFrom = orderRoleBillToCustomer.PartyId,
                PartyIdTo = orderRoleBillFromVendor.PartyId,
                RoleTypeIdTo = orderRoleBillToCustomer.RoleTypeId,
                PaymentRefNum = null,
                EffectiveDate = stamp,
                Amount = (decimal)salesOrder.UseUpToFromBillingAccount,
                CurrencyUomId = salesOrder.CurrencyUomId,
                Comments = null,
                LastUpdatedStamp = stamp,
                CreatedStamp = stamp
            };
            _context.Payments.Add(newPayment);
            payments.Add(newPayment);
        }


        return payments;
    }

    public async Task<Payment> CreatePurchaseOrderPayment(OrderDto purchaseOrder,
        OrderPaymentPreference paymentPreference)
    {
        var stamp = DateTime.UtcNow;
        // Get the next sequence for the PaymentId
        var newPaymentSequence = await _utilityService.GetNextSequence("Payment");
        var payment = new Payment
        {
            PaymentId = newPaymentSequence,
            PaymentTypeId = "VENDOR_PAYMENT",
            PaymentMethodTypeId = "COMPANY_ACCOUNT",
            PartyIdFrom = "Company",
            PartyIdTo = purchaseOrder.FromPartyId,
            StatusId = "PMNT_NOT_PAID",
            Amount = (decimal)purchaseOrder.GrandTotal,
            PaymentPreferenceId = paymentPreference.OrderPaymentPreferenceId,
            EffectiveDate = stamp,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };

        await _context.Payments.AddAsync(payment);

        return payment;
    }


    public async Task CreatePaymentApplicationsForQuickShipSalesOrder(string orderId, string invoiceId)
    {
        var stamp = DateTime.UtcNow;

        // get the payments for the order
        var payments = await GetSalesOrderPayments(orderId);

        // loop through payments and create payment application records
        foreach (var payment in payments)
        {
            // get the next sequence for the payment application
            var newPaymentApplicationSequence = await _utilityService.GetNextSequence("PaymentApplication");

            var paymentApplication = new PaymentApplication
            {
                PaymentApplicationId = newPaymentApplicationSequence,
                PaymentId = payment.PaymentId,
                InvoiceId = invoiceId,
                AmountApplied = payment.Amount,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            _context.PaymentApplications.Add(paymentApplication);
        }
    }

    
    private async Task<List<Payment>> GetSalesOrderPayments(string orderId)
    {
        var payments = await (from p in _context.Payments
            join op in _context.OrderPaymentPreferences on p.PaymentPreferenceId equals op.OrderPaymentPreferenceId
            where op.OrderId == orderId
            select p).ToListAsync();

        return payments;
    }

   
    
    public async Task ProcessPayment(
        string orderId,
        decimal orderTotal,
        UserLogin userLogin,
        bool faceToFace,
        List<OrderPaymentPreference> allPaymentPreferences)
    {
        // Filter for alternative payment methods
        var alternativePaymentPreferences = allPaymentPreferences
            .Where(p => p.PaymentMethodTypeId == "CASH" ||
                        p.PaymentMethodTypeId == "EXT_COD" ||
                        p.PaymentMethodTypeId == "PERSONAL_CHECK" ||
                        p.PaymentMethodTypeId == "EXT_BILLACT")
            .ToList();

        // Check if all payment preferences match the alternative payment methods
        if (alternativePaymentPreferences.Any() &&
            alternativePaymentPreferences.Count == allPaymentPreferences.Count)
        {
            // Check for personal check preferences
            var personalCheckPreferences = alternativePaymentPreferences
                .Where(p => p.PaymentMethodTypeId == "PERSONAL_CHECK")
                .ToList();

            // Approve the order based on payment method and face-to-face flag
            bool isOrderApproved = await _orderHelperService.ApproveOrder(orderId, false);

            if (!isOrderApproved)
            {
                throw new Exception("Problem with order change; see above error");
            }
        }
    }

    private AuthResult ProcessAuthResult(AuthContext context)
    {
        var result = new AuthResult();

        // Retrieve decision from context
        string decision = GetDecision(context);

        // Assign a default value for checkModeStatus
        string checkModeStatus = "Y"; // Hardcoded default value

        // Handle decision-based processing
        if (string.Equals(decision, "ACCEPT", StringComparison.OrdinalIgnoreCase))
        {
            result.AuthCode = context.AuthCode;
            result.AuthResultFlag = true;
        }
        else
        {
            result.AuthCode = decision;
            result.AuthResultFlag =
                string.Equals(checkModeStatus, "N", StringComparison.OrdinalIgnoreCase) ? false : true;

            // TODO: Based on reasonCode populate the following flags as applicable:
            // resultDeclined, resultNsf, resultBadExpire, resultBadCardNumber
        }

        // Process amount if available
        result.ProcessAmount = context.ProcessAmount > 0 ? context.ProcessAmount : 0;

        // Set additional result properties from context
        result.AuthRefNum = context.AuthRefNum;
        result.AuthFlag = context.AuthFlag;
        result.AuthMessage = context.AuthMessage;
        result.CvCode = context.CvCode;
        result.AvsCode = context.AvsCode;
        result.ScoreCode = context.ScoreCode;
        result.CaptureRefNum = context.AuthRefNum;

        // Handle capture result and related flags
        if (!string.IsNullOrEmpty(context.CaptureRefNum))
        {
            result.CaptureResult = string.Equals(decision, "ACCEPT", StringComparison.OrdinalIgnoreCase);
            result.CaptureCode = context.CaptureRefNum;
            result.CaptureFlag = context.AuthFlag; // Assuming capture flag is the same as auth flag
            result.CaptureMessage = decision;
        }

        // Log the result if debug info is enabled
        if (IsDebugInfoOn())
        {
            DebugLogInfo("CC [Cybersource] authorization result: " + result.ToString(), "module");
        }

        // Return the populated AuthResult object
        return result;
    }

    string GetDecision(AuthContext context)
    {
        try
        {
            // Assuming AuthContext has properties similar to ProcessAuthResult
            string decision = context.AuthCode; // Modify to get the decision in the context
            string reasonCode = context.AuthFlag; // Modify to get the reason code if applicable

            if (!string.Equals(decision, "ACCEPT", StringComparison.OrdinalIgnoreCase))
            {
                // Log the decision and reasonCode if needed
            }

            return decision;
        }
        catch (Exception ex)
        {
            // Log the error
            throw; // Re-throw the exception
        }
    }

    private static void DebugLogInfo(string message, string module)
    {
        // Implement logging logic here
        Console.WriteLine($"{module}: {message}");
    }

    private static bool IsDebugInfoOn()
    {
        // You would implement this according to your logging configuration
        return true;
    }

    public async Task ProcessCaptureResult(CaptureResultContext resultContext)
    {
        if (resultContext == null)
        {
            throw new Exception("Null capture result sent to ProcessCaptureResult; fatal error");
        }

        var captureResult = resultContext.CaptureResult;
        decimal? amount = resultContext.CaptureAmount ?? resultContext.ProcessAmount;

        if (!amount.HasValue)
        {
            throw new Exception("Unable to process null capture amount");
        }

        // Setup the amount decimal
        amount = decimal.Round(amount.Value, 2, MidpointRounding.AwayFromZero); // Assuming 2 decimal places

        // Fetch the payment preference from the database
        var paymentPreference = await _context.OrderPaymentPreferences
            .FirstOrDefaultAsync(p =>
                p.OrderPaymentPreferenceId == resultContext.OrderPaymentPreference.OrderPaymentPreferenceId);

        if (paymentPreference == null)
        {
            throw new Exception("Payment preference not found.");
        }

        // Set up the context for the capture result processing
        var context = new CaptureResultContext
        {
            OrderPaymentPreference = paymentPreference,
            ServiceTypeEnum = resultContext.ServiceTypeEnum,
            PayToPartyId = resultContext.PayToPartyId,
            InvoiceId = resultContext.InvoiceId,
            CaptureAmount = amount,
            CurrencyUomId = resultContext.CurrencyUomId,
            CaptureResult = captureResult,
            CaptureAltRefNum = resultContext.CaptureAltRefNum,
            CaptureRefNum = resultContext.CaptureRefNum,
            CaptureCode = resultContext.CaptureCode,
            CaptureFlag = resultContext.CaptureFlag,
            CaptureMessage = resultContext.CaptureMessage,
            InternalRespMsgs = resultContext.InternalRespMsgs
        };

        // Simulating the call to an external service or another method
        //var captureResponse = await ProcessCaptureResult(context);

        /*if (captureResponse.ContainsKey("error"))
        {
            throw new Exception($"Error processing capture result: {captureResponse["error"]}");
        }
    
        // If capture failed, process re-auth
        if (!captureResult)
        {
            try
            {
                await ProcessReAuthFromCaptureFailureAsync(resultContext, amount.Value, locale);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PaymentService");
            }
        }*/
    }
}
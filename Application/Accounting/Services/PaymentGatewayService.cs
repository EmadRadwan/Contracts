/*using System.Globalization;
using Application.Accounting.Services.Models;using Application.Core;
using Application.Order.Orders;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPaymentGatewayService
{
    Task<CapturePaymentsByInvoiceResult> CapturePaymentsByInvoice(string invoiceId);
}

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly DataContext _context;
    private readonly IPaymentWorkerService _paymentWorkerService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly ILogger<PaymentGatewayService> _logger;

    private const int DECIMALS = 2;
    private const MidpointRounding ROUNDING = MidpointRounding.AwayFromZero;
    private const decimal ZERO = 0m;
    private const string AUTH_SERVICE_TYPE = "AUTH";
    private const string REAUTH_SERVICE_TYPE = "REAUTH";


    public PaymentGatewayService(DataContext context, IPaymentWorkerService paymentWorkerService,
        IInvoiceUtilityService invoiceUtilityService, ILogger<PaymentGatewayService> logger)
    {
        _context = context;
        _paymentWorkerService = paymentWorkerService;
        _invoiceUtilityService = invoiceUtilityService;
        _logger = logger;
    }

    /// <summary>
    /// Captures payments through service calls to the defined processing service for the ProductStore/PaymentMethodType
    /// </summary>
    /// <param name="userLogin">The user executing the service</param>
    /// <param name="invoiceId">The ID of the invoice to process payment for</param>
    /// <param name="locale">The locale for error message localization</param>
    /// <returns>COMPLETE|FAILED|ERROR for complete processing of ALL payment methods</returns>
    public async Task<CapturePaymentsByInvoiceResult> CapturePaymentsByInvoice(string invoiceId)
    {
        // Wrap the function in a try-catch for unexpected errors
        // Business intent: Ensure all failures are caught and logged
        // Technical: Per your requirement
        try
        {
            // Initialize invoice variable
            // Business intent: Store the invoice for payment processing
            // Technical: Matches GenericValue invoice = null
            Invoice invoice = null;
            // Try-catch for invoice lookup
            // Business intent: Safely query the invoice
            // Technical: Replaces try-catch with EntityQuery
            try
            {
                // Query the invoice using LINQ
                // Business intent: Verify the invoice exists
                // Technical: Replaces EntityQuery.use(delegator).from("Invoice").where("invoiceId", invoiceId).queryOne()
                invoice = await _context.Invoices
                    // Include related OrderItemBillings
                    // Filter by invoiceId
                    .FindAsync(invoiceId);
            }
            // Catch database errors
            catch (Exception e)
            {
                // Log the error
                // Business intent: Track query failures for support
                // Technical: Matches Debug.logError(e, ...)
                _logger.LogError(e, "Trouble looking up Invoice #{invoiceId}", invoiceId);
                // Return ERROR with localized message
                // Business intent: Inform caller of failure
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Trouble looking up Invoice",
                };
            }

            // Check if invoice was found
            // Business intent: Prevent processing invalid invoices
            // Technical: Matches if (invoice == null)
            if (invoice == null)
            {
                // Log the error
                // Business intent: Track missing invoices
                // Technical: Matches Debug.logError("Could not locate invoice ...")
                _logger.LogError("Could not locate invoice #{invoiceId}", invoiceId);
                // Return ERROR with localized message
                // Business intent: Inform caller
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Invoice not found. Please check the invoice ID.",
                };
            }

            // Initialize OrderItemBilling list
            // Business intent: Link invoice to order
            // Technical: Matches List<GenericValue> orderItemBillings = null
            List<OrderItemBilling> orderItemBillings = null;
            // Try-catch for retrieving OrderItemBillings
            // Business intent: Safely fetch order linkages
            // Technical: Replaces try-catch with getRelated
            try
            {
                // Get OrderItemBillings from navigation property
                // Business intent: Ensure invoice is tied to order items
                // Technical: Replaces invoice.getRelated("OrderItemBilling", ...)
                orderItemBillings = await _context.OrderItemBillings
                    .Where(oib => oib.InvoiceId == invoiceId)
                    .ToListAsync();
            }
            // Catch errors
            catch (Exception e)
            {
                // Log the error
                // Business intent: Track data issues
                // Technical: Matches Debug.logError("Trouble getting OrderItemBilling(s) ...")
                _logger.LogError(e, "Trouble getting OrderItemBilling(s) from Invoice #{invoiceId}", invoiceId);
                // Return ERROR with localized message
                // Business intent: Inform caller
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Trouble getting OrderItemBilling(s) from Invoice",
                };
            }

            // Get billing account ID
            // Business intent: Support COMPANY_ACCOUNT payments
            // Technical: Matches String billingAccountId = invoice.getString("billingAccountId")
            string billingAccountId = invoice.BillingAccountId;

            // Initialize order ID tracker
            // Business intent: Ensure single order linkage
            // Technical: Matches String testOrderId = null
            string testOrderId = null;
            // Flag for order consistency
            // Business intent: Prevent multi-order invoices
            // Technical: Matches boolean allSameOrder = true
            bool allSameOrder = true;
            // Check if OrderItemBillings exist
            // Business intent: Avoid processing empty invoices
            // Technical: Matches if (orderItemBillings != null)
            if (orderItemBillings != null)
            {
                // Iterate through OrderItemBillings
                // Business intent: Verify order consistency
                // Technical: Matches Iterator<GenericValue> oii = orderItemBillings.iterator()
                foreach (var oib in orderItemBillings)
                {
                    // Get order ID
                    // Business intent: Identify order for payment
                    // Technical: Matches String orderId = oib.getString("orderId")
                    string orderId = oib.OrderId;
                    // Set reference order ID if first
                    // Business intent: Establish baseline order
                    // Technical: Matches if (testOrderId == null)
                    if (testOrderId == null)
                    {
                        // Set reference
                        // Technical: Matches testOrderId = orderId
                        testOrderId = orderId;
                    }
                    // Compare with reference
                    // Business intent: Ensure single order
                    // Technical: Matches if (!orderId.equals(testOrderId))
                    else
                    {
                        if (orderId != testOrderId)
                        {
                            // Mark as inconsistent
                            // Technical: Matches allSameOrder = false
                            allSameOrder = false;
                            // Exit loop
                            // Technical: Matches break
                            break;
                        }
                    }
                }
            }

            // Check for no order or multiple orders
            // Business intent: Prevent invalid processing
            // Technical: Matches if (testOrderId == null || !allSameOrder)
            if (string.IsNullOrEmpty(testOrderId) || !allSameOrder)
            {
                // Log warning
                // Business intent: Alert support
                // Technical: Matches Debug.logWarning(...)
                _logger.LogWarning("Attempt to settle Invoice #{invoiceId} which contained none/multiple orders",
                    invoiceId);
                // Return FAILURE
                // Business intent: Indicate unprocessable invoice
                // Technical: Matches ServiceUtil.returnFailure(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "FAILED",
                    ErrorMessage = "Invoice cannot be processed due to multiple orders or no orders found."
                };
            }

            // Get unpaid invoice amount
            // Business intent: Determine charge amount
            // Technical: Matches BigDecimal invoiceTotal = InvoiceWorker.getInvoiceNotApplied(invoice)
            decimal invoiceTotal = await _invoiceUtilityService.GetInvoiceNotApplied(invoice.InvoiceId);
            // Log amount if info logging enabled
            // Business intent: Track payment amount
            // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                // Log capture amount
                // Technical: Matches Debug.logInfo("(Capture) Invoice ...")
                _logger.LogInformation("(Capture) Invoice #{invoiceId} total: {invoiceTotal}", invoiceId, invoiceTotal);
            }

            // Prepare input for captureOrderPayments
            // Business intent: Process payment
            // Technical: Matches Map<String, Object> serviceContext = UtilMisc.toMap(...)
            var serviceContext = new CaptureOrderPaymentsInput
            {
                // Order ID
                // Technical: Matches "orderId", testOrderId
                OrderId = testOrderId,
                // Invoice ID
                // Technical: Matches "invoiceId", invoiceId
                InvoiceId = invoiceId,
                // Capture amount
                // Technical: Matches "captureAmount", invoiceTotal
                CaptureAmount = invoiceTotal,
                // Billing account ID (set below)
                BillingAccountId = null
            };
            // Check billingAccountId
            // Business intent: Include for COMPANY_ACCOUNT
            // Technical: Matches if (UtilValidate.isNotEmpty(billingAccountId))
            if (!string.IsNullOrEmpty(billingAccountId))
            {
                // Set billingAccountId
                // Technical: Matches serviceContext.put("billingAccountId", ...)
                serviceContext.BillingAccountId = billingAccountId;
            }

            // Try-catch for captureOrderPayments
            // Business intent: Safely process payment
            // Technical: Replaces try-catch with runSync
            try
            {
                // Call captureOrderPayments
                // Business intent: Capture payment
                // Technical: Matches dispatcher.runSync("captureOrderPayments", ...)
                CapturePaymentsByInvoiceResult result = await CaptureOrderPayments(serviceContext);
                // Check for error
                // Business intent: Propagate payment errors
                // Technical: Matches if (ServiceUtil.isError(result))
                if (result.Status == "ERROR")
                {
                    // Return ERROR
                    // Technical: Matches return ServiceUtil.returnError(...)
                    return new CapturePaymentsByInvoiceResult
                    {
                        Status = "ERROR",
                        ErrorMessage = result.ErrorMessage
                    };
                }

                // Return result
                // Business intent: Pass payment outcome
                // Technical: Matches return result
                return result;
            }
            // Catch service errors
            catch (Exception e)
            {
                // Log error
                // Business intent: Track payment failures
                // Technical: Matches Debug.logError(e, ...)
                _logger.LogError(e, "Trouble running captureOrderPayments service");
                // Return ERROR
                // Business intent: Inform caller
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Trouble running captureOrderPayments service"
                };
            }
        }
        // Catch unexpected errors
        catch (Exception e)
        {
            // Log error
            // Business intent: Track all failures
            // Technical: Per your requirement
            _logger.LogError(e, "Unexpected error in capturePaymentsByInvoice for Invoice #{invoiceId}", invoiceId);
            // Return ERROR
            return new CapturePaymentsByInvoiceResult
            {
                Status = "ERROR",
                ErrorMessage = "An unexpected error occurred while processing the payment."
            };
        }
    }

    public async Task<CapturePaymentsByInvoiceResult> CaptureOrderPayments(CaptureOrderPaymentsInput input)
    {
        // Wrap in try-catch
        // Business intent: Handle payment errors
        // Technical: Per your requirement
        try
        {
            string orderId = input.OrderId;
            string invoiceId = input.InvoiceId;
            string billingAccountId = input.BillingAccountId;
            decimal amountToCapture = input.CaptureAmount;

            // Round amountToCapture
            // Business intent: Ensure consistent precision
            // Technical: Matches amountToCapture.setScale(DECIMALS, ROUNDING)
            amountToCapture = Math.Round(amountToCapture, DECIMALS, ROUNDING);

            // Initialize variables
            // Business intent: Store order and payment preferences
            // Technical: Matches GenericValue orderHeader = null
            OrderHeader orderHeader = null;
            // List for regular payment preferences
            // Technical: Matches List<GenericValue> paymentPrefs = null
            List<OrderPaymentPreference> paymentPrefs = null;
            // List for billing account payment preferences
            // Technical: Matches List<GenericValue> paymentPrefsBa = null
            List<OrderPaymentPreference> paymentPrefsBa = null;

            // Try-catch for querying order and preferences
            // Business intent: Safely fetch data
            // Technical: Replaces try-catch with EntityQuery
            try
            {
                // Query OrderHeader
                // Business intent: Verify order exists
                // Technical: Matches EntityQuery.use(delegator).from("OrderHeader")...
                orderHeader = _context.OrderHeaders
                    .FirstOrDefault(oh => oh.OrderId == orderId);

                // Query payment preferences
                // Business intent: Identify authorized payments
                // Technical: Matches EntityQuery.use(delegator).from("OrderPaymentPreference")...
                paymentPrefs = _context.OrderPaymentPreferences
                    .Where(opp => opp.OrderId == orderId && opp.StatusId == "PAYMENT_AUTHORIZED")
                    .OrderByDescending(opp => opp.MaxAmount)
                    .ToList();

                // Query billing account preferences if billingAccountId exists
                // Business intent: Support COMPANY_ACCOUNT payments
                // Technical: Matches if (UtilValidate.isNotEmpty(billingAccountId)) { ... }
                if (!string.IsNullOrEmpty(billingAccountId))
                {
                    paymentPrefsBa = _context.OrderPaymentPreferences
                        .Where(opp => opp.OrderId == orderId &&
                                      opp.PaymentMethodTypeId == "EXT_BILLACT" &&
                                      opp.StatusId == "PAYMENT_NOT_RECEIVED")
                        .OrderByDescending(opp => opp.MaxAmount)
                        .ToList();
                }
            }
            // Catch database errors
            catch (Exception gee)
            {
                // Log error
                // Business intent: Track query failures
                // Technical: Matches Debug.logError(gee, ...)
                _logger.LogError(gee, "Problems getting entity record(s) for Order #{orderId}", orderId);
                // Return ERROR
                // Business intent: Inform caller
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Trouble getting entity record(s) for Order " + orderId
                };
            }

            // Check if order was found
            // Business intent: Prevent processing invalid orders
            // Technical: Matches if (orderHeader == null)
            if (orderHeader == null)
            {
                // Return ERROR
                // Business intent: Inform caller
                // Technical: Matches ServiceUtil.returnError(...)
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "Order not found. Please check the order ID."
                };
            }

            // Initialize OrderReadHelper (simplified)
            // Business intent: Access order details
            // Technical: Matches OrderReadHelper orh = new OrderReadHelper(orderHeader)
            var orh = new OrderReadHelper(orderHeader.OrderId)
            {
                Context = _context
            };
            orh.InitializeOrder();

            // Get order grand total
            // Business intent: Calculate total order value
            // Technical: Matches BigDecimal orderGrandTotal = orh.getOrderGrandTotal()
            decimal orderGrandTotal = await orh.GetOrderGrandTotal();
            // Round total
            // Technical: Matches orderGrandTotal.setScale(...)
            orderGrandTotal = Math.Round(orderGrandTotal, DECIMALS, ROUNDING);

            // Get total payments
            // Business intent: Determine paid amount
            // Technical: Matches BigDecimal totalPayments = PaymentWorker.getPaymentsTotal(...)
            decimal totalPayments = _paymentWorkerService.GetPaymentsTotal(await orh.GetOrderPayments());
            // Round total
            // Technical: Matches totalPayments.setScale(...)
            totalPayments = Math.Round(totalPayments, DECIMALS, ROUNDING);

            // Calculate remaining total
            // Business intent: Determine unpaid amount
            // Technical: Matches BigDecimal remainingTotal = orderGrandTotal.subtract(totalPayments)
            decimal remainingTotal = orderGrandTotal - totalPayments;

            // Log remaining total
            // Business intent: Track order status
            // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("The Remaining Total for order: {orderId} is: {remainingTotal}", orderId,
                    remainingTotal);
            }

            // Limit amountToCapture
            // Business intent: Prevent over-capturing
            // Technical: Matches amountToCapture = amountToCapture.min(remainingTotal)
            amountToCapture = Math.Min(amountToCapture, remainingTotal);

            // Log actual capture amount
            // Business intent: Track capture amount
            // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Actual Expected Capture Amount: {amountToCapture}", amountToCapture);
            }

            // Process billing account payments
            // Business intent: Handle COMPANY_ACCOUNT payments first
            // Technical: Matches if (UtilValidate.isNotEmpty(paymentPrefsBa)) { ... }
            if (paymentPrefsBa != null && paymentPrefsBa.Any())
            {
                // Iterate billing account preferences
                // Business intent: Process each preference
                // Technical: Matches Iterator<GenericValue> paymentsBa = paymentPrefsBa.iterator()
                foreach (var paymentPref in paymentPrefsBa)
                {
                    // Get authorized amount
                    // Business intent: Determine capturable amount
                    // Technical: Matches BigDecimal authAmount = paymentPref.getBigDecimal("maxAmount")
                    decimal authAmount = paymentPref.MaxAmount;
                    // Use 0 if null
                    // Technical: Matches if (authAmount == null) { authAmount = ZERO }
                    authAmount = authAmount != null ? authAmount : ZERO;
                    // Round amount
                    // Technical: Matches authAmount.setScale(...)
                    authAmount = Math.Round(authAmount, DECIMALS, ROUNDING);

                    // Check if nothing to capture
                    // Business intent: Skip empty preferences
                    // Technical: Matches if (authAmount.compareTo(ZERO) == 0)
                    if (authAmount == ZERO)
                    {
                        // Log info
                        // Business intent: Track skipped preferences
                        // Technical: Matches Debug.logInfo("Nothing to capture...")
                        _logger.LogInformation("Nothing to capture; authAmount = 0");
                        // Skip to next
                        // Technical: Matches continue
                        continue;
                    }

                    // Calculate capture amount for this preference
                    // Business intent: Capture up to authorized amount
                    // Technical: Matches BigDecimal amountThisCapture = amountToCapture.min(authAmount)
                    decimal amountThisCapture = Math.Min(amountToCapture, authAmount);

                    // Decrease remaining amountToCapture
                    // Business intent: Track remaining amount
                    // Technical: Matches amountToCapture = amountToCapture.subtract(amountThisCapture)
                    amountToCapture -= amountThisCapture;

                    // Process capture if invoiceId exists
                    // Business intent: Apply billing account payments
                    // Technical: Matches if (UtilValidate.isNotEmpty(invoiceId)) { ... }
                    if (!string.IsNullOrEmpty(invoiceId))
                    {
                        // Initialize capture result
                        // Business intent: Store capture outcome
                        // Technical: Matches Map<String, Object> captureResult = null
                        CapturePaymentsByInvoiceResult captureResult = null;
                        // Try-catch for captureBillingAccountPayments
                        // Business intent: Safely process payment
                        // Technical: Replaces try-catch with runSync
                        try
                        {
                            // Call captureBillingAccountPayments
                            // Business intent: Capture from billing account
                            // Technical: Matches dispatcher.runSync("captureBillingAccountPayments", ...)
                            captureResult = CaptureBillingAccountPayments(new CaptureBillingAccountPaymentsInput
                            {
                                InvoiceId = invoiceId,
                                BillingAccountId = billingAccountId,
                                CaptureAmount = amountThisCapture,
                                OrderId = orderId,
                            });
                            
                            // Check for error
                            // Business intent: Propagate errors
                            // Technical: Matches if (ServiceUtil.isError(captureResult))
                            if (captureResult.Status == "ERROR")
                            {
                                // Return ERROR
                                // Technical: Matches return ServiceUtil.returnError(...)
                                return new CapturePaymentsByInvoiceResult
                                {
                                    Status = "ERROR",
                                    ErrorMessage = captureResult.ErrorMessage
                                };
                            }
                        }
                        // Catch service errors
                        catch (Exception ex)
                        {
                            // Return ERROR
                            // Business intent: Inform caller
                            // Technical: Matches return ServiceUtil.returnError(ex.getMessage())
                            return new CapturePaymentsByInvoiceResult
                            {
                                Status = "ERROR",
                                ErrorMessage = ex.Message
                            };
                        }

                        // Check if captureResult exists
                        // Business intent: Process successful capture
                        // Technical: Matches if (captureResult != null)
                        if (captureResult != null)
                        {
                            // Initialize captured amount
                            // Business intent: Track captured amount
                            // Technical: Matches BigDecimal amountCaptured = BigDecimal.ZERO
                            decimal amountCaptured = ZERO;
                            // Try-catch for converting capture amount
                            // Business intent: Safely parse result
                            // Technical: Replaces try-catch with ObjectType.simpleTypeOrObjectConvert
                            try
                            {
                                // Get capture amount from result
                                // Technical: Matches ObjectType.simpleTypeOrObjectConvert(captureResult.get("captureAmount"), ...)
                                amountCaptured = Convert.ToDecimal(captureResult.Data);
                            }
                            // Catch conversion errors
                            catch (Exception e)
                            {
                                // Log error
                                // Business intent: Track parsing issues
                                // Technical: Matches Debug.logError(e, ...)
                                _logger.LogError(e, "Trouble processing the result; captureResult: {captureResult}",
                                    captureResult);
                                // Return ERROR
                                // Technical: Matches ServiceUtil.returnError(...)
                                return new CapturePaymentsByInvoiceResult
                                {
                                    Status = "ERROR",
                                    ErrorMessage = "Trouble processing the result; captureResult: " +
                                        captureResult
                                };
                            }

                            // Round captured amount
                            // Technical: Matches amountCaptured.setScale(...)
                            amountCaptured = Math.Round(amountCaptured, DECIMALS, ROUNDING);

                            // Check if nothing captured
                            // Business intent: Skip if no payment
                            // Technical: Matches if (amountCaptured.compareTo(BigDecimal.ZERO) == 0)
                            if (amountCaptured == ZERO)
                            {
                                // Skip to next
                                // Technical: Matches continue
                                continue;
                            }

                            // Log captured amount
                            // Business intent: Track payment
                            // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation(
                                    "Amount captured for order [{orderId}] from unapplied payments associated to billing account [{billingAccountId}] is: {amountCaptured}",
                                    orderId, billingAccountId, amountCaptured);
                            }

                            // Prepare processResult input
                            // Business intent: Update payment preference
                            // Technical: Matches captureResult.put(...) statements
                            var processResultInput = new ProcessResultInput
                            {
                                CaptureAmount = amountCaptured,
                                InvoiceId = invoiceId,
                                CaptureResult = true,
                                OrderPaymentPreference = paymentPref,
                                //CaptureRefNum = input.CaptureRefNum ?? ""
                            };

                            // Try-catch for processResult
                            // Business intent: Finalize capture
                            // Technical: Replaces try-catch with processResult
                            try
                            {
                                // Process capture result
                                // Business intent: Update preference status
                                // Technical: Matches processResult(dctx, captureResult, ...)
                                ProcessResult(processResultInput, userLogin, paymentPref, locale);
                            }
                            // Catch processing errors
                            catch (Exception e)
                            {
                                // Log error
                                // Business intent: Track issues
                                // Technical: Matches Debug.logError(e, ...)
                                _logger.LogError(e, "Trouble processing the result; captureResult: {captureResult}",
                                    captureResult);
                                // Return ERROR
                                // Technical: Matches ServiceUtil.returnError(...)
                                return new CapturePaymentsByInvoiceResult
                                {
                                    Status = "ERROR",
                                    ErrorMessage =
                                        GetLocalizedMessage("AccountingPaymentCannotBeCaptured", null, locale) + " " +
                                        captureResult
                                };
                            }

                            // Check for split payment
                            // Business intent: Handle partial captures
                            // Technical: Matches if (authAmount.compareTo(amountCaptured) > 0)
                            if (authAmount > amountCaptured)
                            {
                                // Calculate split amount
                                // Business intent: Track remaining authorization
                                // Technical: Matches BigDecimal splitAmount = authAmount.subtract(amountCaptured)
                                decimal splitAmount = authAmount - amountCaptured;
                                // Try-catch for split payment
                                // Business intent: Process split
                                // Technical: Replaces try-catch with addCommitService
                                try
                                {
                                    // Process split payment
                                    // Business intent: Create new preference
                                    // Technical: Matches dispatcher.addCommitService("processCaptureSplitPayment", ...)
                                    ProcessCaptureSplitPayment(new ProcessCaptureSplitPaymentInput
                                    {
                                        UserLogin = userLogin,
                                        OrderPaymentPreference = paymentPref,
                                        SplitAmount = splitAmount
                                    });
                                }
                                // Catch split errors
                                catch (Exception e)
                                {
                                    // Log warning
                                    // Business intent: Track split issues
                                    // Technical: Matches Debug.logWarning(e, ...)
                                    _logger.LogWarning(e, "Problem processing the capture split payment");
                                }

                                // Log split info
                                // Business intent: Track split
                                // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
                                if (_logger.IsEnabled(LogLevel.Information))
                                {
                                    _logger.LogInformation(
                                        "Captured: {amountThisCapture} Remaining (re-auth): {splitAmount}",
                                        amountThisCapture, splitAmount);
                                }
                            }
                        }
                        // Log failure if captureResult is null
                        // Business intent: Track failed captures
                        // Technical: Matches else { Debug.logError(...) }
                        else
                        {
                            _logger.LogError(
                                "Payment not captured for order [{orderId}] from billing account [{billingAccountId}]",
                                orderId, billingAccountId);
                        }
                    }
                }
            }

            // Process regular payment preferences
            // Business intent: Handle non-billing account payments
            // Technical: Matches if (UtilValidate.isNotEmpty(paymentPrefs)) { ... }
            if (paymentPrefs != null && paymentPrefs.Any())
            {
                // Iterate payment preferences
                // Business intent: Process each preference
                // Technical: Matches Iterator<GenericValue> payments = paymentPrefs.iterator()
                foreach (var paymentPref in paymentPrefs)
                {
                    // Get authorization transaction
                    // Business intent: Verify authorization
                    // Technical: Matches GenericValue authTrans = getAuthTransaction(paymentPref)
                    PaymentGatewayResponse authTrans = GetAuthTransaction(paymentPref);
                    // Check if authorization exists
                    // Business intent: Ensure valid payment
                    // Technical: Matches if (authTrans == null)
                    if (authTrans == null)
                    {
                        // Log warning
                        // Business intent: Track invalid preferences
                        // Technical: Matches Debug.logWarning(...)
                        _logger.LogWarning(
                            "Authorized OrderPaymentPreference has no corresponding PaymentGatewayResponse, cannot capture payment: {paymentPref}",
                            paymentPref);
                        // Skip to next
                        // Technical: Matches continue
                        continue;
                    }

                    // Check for existing capture
                    // Business intent: Prevent duplicate captures
                    // Technical: Matches GenericValue captureTrans = getCaptureTransaction(paymentPref)
                    PaymentGatewayResponse captureTrans = GetCaptureTransaction(paymentPref);
                    // Check if already captured
                    // Business intent: Avoid re-processing
                    // Technical: Matches if (captureTrans != null)
                    if (captureTrans != null)
                    {
                        // Log warning
                        // Business intent: Track duplicates
                        // Technical: Matches Debug.logWarning(...)
                        _logger.LogWarning("Attempt to capture an already captured preference: {captureTrans}",
                            captureTrans);
                        // Skip to next
                        // Technical: Matches continue
                        continue;
                    }

                    // Get authorized amount
                    // Business intent: Determine capturable amount
                    // Technical: Matches BigDecimal authAmount = authTrans.getBigDecimal("amount")
                    decimal authAmount = authTrans.Amount;
                    // Use 0 if null
                    // Technical: Matches if (authAmount == null) { authAmount = ZERO }
                    authAmount = authAmount != null ? authAmount : ZERO;
                    // Round amount
                    // Technical: Matches authAmount.setScale(...)
                    authAmount = Math.Round(authAmount, DECIMALS, ROUNDING);

                    // Check if nothing to capture
                    // Business intent: Skip empty authorizations
                    // Technical: Matches if (authAmount.compareTo(ZERO) == 0)
                    if (authAmount == ZERO)
                    {
                        // Log info
                        // Business intent: Track skipped preferences
                        // Technical: Matches Debug.logInfo("Nothing to capture...")
                        _logger.LogInformation("Nothing to capture; authAmount = 0");
                        // Skip to next
                        // Technical: Matches continue
                        continue;
                    }

                    // Initialize capture amount
                    // Business intent: Determine amount for this capture
                    // Technical: Matches BigDecimal amountThisCapture
                    decimal amountThisCapture;

                    // Determine capture amount based on conditions
                    // Business intent: Handle different scenarios
                    // Technical: Matches if (isReplacementOrder(orderHeader)) { ... }
                    if (IsReplacementOrder(orderHeader))
                    {
                        // Capture full authorized amount for replacement orders
                        // Business intent: Simplify replacement processing
                        // Technical: Matches amountThisCapture = authAmount
                        amountThisCapture = authAmount;
                    }
                    // Check if authorized amount is sufficient
                    // Technical: Matches else if (authAmount.compareTo(amountToCapture) >= 0)
                    else if (authAmount >= amountToCapture)
                    {
                        // Capture requested amount
                        // Business intent: Use available authorization
                        // Technical: Matches amountThisCapture = amountToCapture
                        amountThisCapture = amountToCapture;
                    }
                    // Check if more preferences exist
                    // Technical: Matches else if (payments.hasNext())
                    else if (paymentPrefs.IndexOf(paymentPref) < paymentPrefs.Count - 1)
                    {
                        // Capture authorized amount
                        // Business intent: Use current authorization
                        // Technical: Matches amountThisCapture = authAmount
                        amountThisCapture = authAmount;
                    }
                    // Handle insufficient authorization
                    // Technical: Matches else { ... }
                    else
                    {
                        // Log error (re-auth not implemented)
                        // Business intent: Track under-authorization
                        // Technical: Matches Debug.logError(...)
                        _logger.LogError(
                            "The amount to capture was more then what was authorized; we only captured the authorized amount: {paymentPref}",
                            paymentPref);
                        // Capture authorized amount
                        // Business intent: Proceed with available amount
                        // Technical: Matches amountThisCapture = authAmount
                        amountThisCapture = authAmount;
                    }

                    // Capture payment
                    // Business intent: Process payment
                    // Technical: Matches Map<String, Object> captureResult = capturePayment(...)
                    CapturePaymentsByInvoiceResult captureResult = CapturePayment(new CapturePaymentInput
                    {
                        UserLogin = userLogin,
                        OrderPaymentPreference = paymentPref,
                        CaptureAmount = amountThisCapture,
                        Locale = locale
                    });
                    // Check for success
                    // Business intent: Process successful capture
                    // Technical: Matches if (captureResult != null && ServiceUtil.isSuccess(captureResult))
                    if (captureResult != null && captureResult.Status == "SUCCESS")
                    {
                        // Initialize captured amount
                        // Business intent: Track captured amount
                        // Technical: Matches BigDecimal amountCaptured = null
                        decimal amountCaptured = 0;
                        // Try-catch for converting capture amount
                        // Business intent: Safely parse result
                        // Technical: Replaces try-catch with ObjectType.simpleTypeOrObjectConvert
                        try
                        {
                            // Get capture amount
                            // Technical: Matches captureResult.get("captureAmount")
                            amountCaptured = Convert.ToDecimal(captureResult.Data);
                            // Fallback to processAmount
                            // Technical: Matches if (amountCaptured == null) { ... }
                            if (amountCaptured == 0)
                            {
                                // Assume Data contains processAmount
                                amountCaptured = Convert.ToDecimal(captureResult.Data);
                            }

                            // Round captured amount
                            // Technical: Matches amountCaptured.setScale(...)
                            amountCaptured = Math.Round(amountCaptured, DECIMALS, ROUNDING);

                            // Decrease remaining amountToCapture
                            // Business intent: Track remaining amount
                            // Technical: Matches amountToCapture = amountToCapture.subtract(amountCaptured)
                            amountToCapture -= amountCaptured;

                            // Add invoiceId for non-replacement orders
                            // Business intent: Link payment to invoice
                            // Technical: Matches if (!isReplacementOrder(orderHeader)) { ... }
                            if (!IsReplacementOrder(orderHeader))
                            {
                                // Update result (simulated)
                                // Technical: Matches captureResult.put("invoiceId", invoiceId)
                                captureResult.Data = new { InvoiceId = invoiceId, AmountCaptured = amountCaptured };
                            }

                            // Process capture result
                            // Business intent: Update preference
                            // Technical: Matches processResult(dctx, captureResult, ...)
                            ProcessResult(new ProcessResultInput
                            {
                                CaptureAmount = amountCaptured,
                                InvoiceId = invoiceId,
                                CaptureResult = true,
                                OrderPaymentPreference = paymentPref,
                                CaptureRefNum = input.CaptureRefNum ?? ""
                            }, userLogin, paymentPref, locale);
                        }
                        // Catch processing errors
                        catch (Exception e)
                        {
                            // Log error
                            // Business intent: Track issues
                            // Technical: Matches Debug.logError(e, ...)
                            _logger.LogError(e, "Trouble processing the result; captureResult: {captureResult}",
                                captureResult);
                            // Return ERROR
                            // Technical: Matches ServiceUtil.returnError(...)
                            return new CapturePaymentsByInvoiceResult
                            {
                                Status = "ERROR",
                                ErrorMessage = GetLocalizedMessage("AccountingPaymentCannotBeCaptured", null, locale) +
                                               " " + captureResult
                            };
                        }

                        // Check for split payment
                        // Business intent: Handle partial captures
                        // Technical: Matches if (authAmount.compareTo(amountCaptured) > 0)
                        if (authAmount > amountCaptured)
                        {
                            // Calculate split amount
                            // Business intent: Track remaining authorization
                            // Technical: Matches BigDecimal splitAmount = authAmount.subtract(amountCaptured)
                            decimal splitAmount = authAmount - amountCaptured;
                            // Try-catch for split payment
                            // Business intent: Process split
                            // Technical: Replaces try-catch with addCommitService
                            try
                            {
                                // Process split payment
                                // Business intent: Create new preference
                                // Technical: Matches dispatcher.addCommitService("processCaptureSplitPayment", ...)
                                ProcessCaptureSplitPayment(new ProcessCaptureSplitPaymentInput
                                {
                                    UserLogin = userLogin,
                                    OrderPaymentPreference = paymentPref,
                                    SplitAmount = splitAmount
                                });
                            }
                            // Catch split errors
                            catch (Exception e)
                            {
                                // Log warning
                                // Business intent: Track issues
                                // Technical: Matches Debug.logWarning(e, ...)
                                _logger.LogWarning(e, "Problem processing the capture split payment");
                            }

                            // Log split info
                            // Business intent: Track split
                            // Technical: Matches if (Debug.infoOn()) { Debug.logInfo(...) }
                            if (_logger.IsEnabled(LogLevel.Information))
                            {
                                _logger.LogInformation(
                                    "Captured: {amountThisCapture} Remaining (re-auth): {splitAmount}",
                                    amountThisCapture, splitAmount);
                            }
                        }
                    }
                    // Log failure
                    // Business intent: Track failed captures
                    // Technical: Matches else { Debug.logError(...) }
                    else
                    {
                        _logger.LogError("Payment not captured");
                    }
                }
            }

            // Check if amount remains to capture
            // Business intent: Handle incomplete captures
            // Technical: Matches if (amountToCapture.compareTo(ZERO) > 0)
            if (amountToCapture > ZERO)
            {
                // Get ProductStore
                // Business intent: Check store settings
                // Technical: Matches GenericValue productStore = orh.getProductStore()
                ProductStore productStore = await orh.GetProductStore();
                // Check if store exists
                // Business intent: Apply store-specific rules
                // Technical: Matches if (UtilValidate.isNotEmpty(productStore))
                if (productStore != null)
                {
                    // Check shipIfCaptureFails setting
                    // Business intent: Allow shipping despite failure
                    // Technical: Matches boolean shipIfCaptureFails = ...
                    bool shipIfCaptureFails = string.IsNullOrEmpty(productStore.ShipIfCaptureFails) ||
                                              productStore.ShipIfCaptureFails.Equals("Y",
                                                  StringComparison.OrdinalIgnoreCase);
                    // Check if shipping is disallowed
                    // Business intent: Enforce payment requirement
                    // Technical: Matches if (!shipIfCaptureFails)
                    if (!shipIfCaptureFails)
                    {
                        // Return ERROR
                        // Business intent: Prevent shipping
                        // Technical: Matches ServiceUtil.returnError(...)
                        return new CapturePaymentsByInvoiceResult
                        {
                            Status = "ERROR",
                            ErrorMessage = ""
                        };
                    }
                    // Log warning
                    // Business intent: Track exception case
                    // Technical: Matches Debug.logWarning(...)
                    else
                    {
                        _logger.LogWarning(
                            "Payment capture failed, shipping order anyway as per ProductStore setting (shipIfCaptureFails)");
                    }
                }

                // Return SUCCESS with FAILED result
                // Business intent: Allow shipping
                // Technical: Matches Map<String, Object> result = ServiceUtil.returnSuccess()
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "SUCCESS",
                    Data = new { processResult = "FAILED" }
                };
            }
            // Return SUCCESS with COMPLETE result
            // Business intent: Indicate full capture
            // Technical: Matches else { Map<String, Object> result = ServiceUtil.returnSuccess() ... }
            else
            {
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "SUCCESS",
                    Data = new { processResult = "COMPLETE" }
                };
            }
        }
        // Catch unexpected errors
        catch (Exception e)
        {
            // Log error
            // Business intent: Track all failures
            // Technical: Per your requirement
            _logger.LogError(e, "Unexpected error in captureOrderPayments for Order #{orderId}", orderId);
            // Return ERROR
            return new CapturePaymentsByInvoiceResult
            {
                Status = "ERROR",
                ErrorMessage = "An unexpected error occurred while capturing payments."
            };
        }
    }
    
    /// <summary>
        /// Captures payments from unapplied payment applications associated with a billing account and applies them to the specified invoice.
        /// </summary>
        /// <param name="input">Input containing invoiceId, billingAccountId, captureAmount, orderId, and userLogin</param>
        /// <returns>SUCCESS with captured amount or ERROR with message</returns>
        public CapturePaymentsByInvoiceResult CaptureBillingAccountPayments(CaptureBillingAccountPaymentsInput input)
        {
            // Wrap the function in a try-catch for unexpected errors
            // Business intent: Ensure all failures are caught and logged
            // Technical: Per your requirement
            try
            {
                // Extract input parameters
                // Business intent: Prepare data for processing
                // Technical: Matches context.get(...)
                string invoiceId = input.InvoiceId;
                string billingAccountId = input.BillingAccountId;
                decimal captureAmount = input.CaptureAmount;

                // Round captureAmount
                // Business intent: Ensure consistent precision for financial calculations
                // Technical: Matches captureAmount.setScale(DECIMALS, ROUNDING)
                captureAmount = Math.Round(captureAmount, DECIMALS, ROUNDING);

                // Initialize captured amount
                // Business intent: Track total amount applied to the invoice
                // Technical: Matches BigDecimal capturedAmount = BigDecimal.ZERO
                decimal capturedAmount = ZERO;

                // Try-catch for querying and processing payment applications
                // Business intent: Safely fetch and apply unapplied payments
                // Technical: Replaces try-catch with EntityQuery
                try
                {
                    // Query unapplied payment applications for the billing account
                    // Business intent: Identify available funds to apply to the invoice
                    // Technical: Matches EntityQuery.use(delegator).from("PaymentApplication")...
                    List<PaymentApplication> paymentApplications = _context.PaymentApplications
                        .Include(pa => pa.Payment) // Eagerly load related Payment
                        .Where(pa => pa.BillingAccountId == billingAccountId && pa.InvoiceId == null)
                        .OrderByDescending(pa => pa.AmountApplied)
                        .ToList();

                    // Check if payment applications exist
                    // Business intent: Proceed only if there are unapplied payments
                    // Technical: Matches if (UtilValidate.isNotEmpty(paymentApplications))
                    if (paymentApplications != null && paymentApplications.Any())
                    {
                        // Iterate through payment applications
                        // Business intent: Apply each payment until captureAmount is met
                        // Technical: Matches Iterator<GenericValue> paymentApplicationsIt = paymentApplications.iterator()
                        foreach (var paymentApplication in paymentApplications)
                        {
                            // Check if captured amount meets or exceeds target
                            // Business intent: Stop if required amount is captured
                            // Technical: Matches if (capturedAmount.compareTo(captureAmount) >= 0)
                            if (capturedAmount >= captureAmount)
                            {
                                // Exit loop
                                // Business intent: Avoid over-capturing
                                // Technical: Matches break
                                break;
                            }

                            // Get related payment
                            // Business intent: Verify payment details
                            // Technical: Matches GenericValue payment = paymentApplication.getRelatedOne("Payment", false)
                            Payment payment = paymentApplication.Payment;

                            // Check if payment is reserved for an OrderPaymentPreference
                            // Business intent: Skip payments allocated to specific preferences
                            // Technical: Matches if (payment.getString("paymentPreferenceId") != null)
                            if (!string.IsNullOrEmpty(payment.PaymentPreferenceId))
                            {
                                // Skip to next payment application
                                // Business intent: Respect payment reservations
                                // Technical: Matches continue
                                continue;
                            }

                            // TODO: Check payment status (not implemented in original)
                            // Business intent: Ensure payment is valid (e.g., PMNT_RECEIVED)
                            // Technical: Matches TODO comment in Java code
                            // Note: Original code lacks status check; we omit for fidelity

                            // Get payment application amount
                            // Business intent: Determine available amount to apply
                            // Technical: Matches BigDecimal paymentApplicationAmount = paymentApplication.getBigDecimal("amountApplied")
                            decimal paymentApplicationAmount = (decimal)paymentApplication.AmountApplied;

                            // Calculate amount to capture from this application
                            // Business intent: Apply up to the remaining needed amount
                            // Technical: Matches BigDecimal amountToCapture = paymentApplicationAmount.min(captureAmount.subtract(capturedAmount))
                            decimal amountToCapture = Math.Min(paymentApplicationAmount, captureAmount - capturedAmount);

                            // Round amount to capture
                            // Business intent: Maintain precision for financial records
                            // Technical: Matches amountToCapture.setScale(DECIMALS, ROUNDING)
                            amountToCapture = Math.Round(amountToCapture, DECIMALS, ROUNDING);

                            // Check if entire application amount is used
                            // Business intent: Decide whether to update or split the application
                            // Technical: Matches if (amountToCapture.compareTo(paymentApplicationAmount) == 0)
                            if (amountToCapture == paymentApplicationAmount)
                            {
                                // Apply the entire payment application to the invoice
                                // Business intent: Fully utilize the payment application
                                // Technical: Matches paymentApplication.set("invoiceId", invoiceId)
                                paymentApplication.InvoiceId = invoiceId;
                                // Update the record
                                // Business intent: Persist the application to the invoice
                                // Technical: Matches paymentApplication.store()
                                _context.PaymentApplications.Update(paymentApplication);
                            }
                            // Handle partial application
                            // Business intent: Split the application if only part is used
                            // Technical: Matches else { ... }
                            else
                            {
                                // Update existing application with amount to capture
                                // Business intent: Apply portion to the invoice
                                // Technical: Matches paymentApplication.set("invoiceId", invoiceId)
                                paymentApplication.InvoiceId = invoiceId;
                                // Set applied amount
                                // Technical: Matches paymentApplication.set("amountApplied", amountToCapture)
                                paymentApplication.AmountApplied = amountToCapture;
                                // Update the record
                                // Business intent: Persist the partial application
                                // Technical: Matches paymentApplication.store()
                                _context.PaymentApplications.Update(paymentApplication);

                                // Create new payment application for remaining amount
                                // Business intent: Preserve unapplied amount
                                // Technical: Matches GenericValue newPaymentApplication = delegator.makeValue("PaymentApplication", ...)
                                var newPaymentApplication = new PaymentApplication
                                {
                                    // Generate new ID
                                    // Business intent: Ensure unique record
                                    // Technical: Matches delegator.getNextSeqId("PaymentApplication")
                                    PaymentApplicationId = Guid.NewGuid().ToString(),
                                    // Copy fields from original
                                    PaymentId = paymentApplication.PaymentId,
                                    BillingAccountId = paymentApplication.BillingAccountId,
                                    // Set remaining amount
                                    // Technical: Matches newPaymentApplication.set("amountApplied", paymentApplicationAmount.subtract(amountToCapture))
                                    AmountApplied = paymentApplicationAmount - amountToCapture,
                                    // InvoiceId remains null (unapplied)
                                    InvoiceId = null
                                };
                                // Add new record
                                // Business intent: Persist remaining amount
                                // Technical: Matches newPaymentApplication.create()
                                _context.PaymentApplications.Add(newPaymentApplication);
                            }

                            // Add to captured amount
                            // Business intent: Track total captured
                            // Technical: Matches capturedAmount = capturedAmount.add(amountToCapture)
                            capturedAmount += amountToCapture;
                        }
                    }
                    
                }
                // Catch database or entity errors
                catch (Exception ex)
                {
                    // Log error
                    // Business intent: Track query or update failures
                    // Technical: Matches return ServiceUtil.returnError(ex.getMessage())
                    _logger.LogError(ex, "Error processing payment applications for Invoice #{invoiceId}, BillingAccount #{billingAccountId}", invoiceId, billingAccountId);
                    // Return ERROR
                    // Business intent: Inform caller of failure
                    // Technical: Matches ServiceUtil.returnError(...)
                    return new CapturePaymentsByInvoiceResult
                    {
                        Status = "ERROR",
                        ErrorMessage = ex.Message
                    };
                }

                // Round captured amount
                // Business intent: Ensure precision in result
                // Technical: Matches capturedAmount.setScale(DECIMALS, ROUNDING)
                capturedAmount = Math.Round(capturedAmount, DECIMALS, ROUNDING);

                // Prepare success result
                // Business intent: Return captured amount
                // Technical: Matches Map<String, Object> results = ServiceUtil.returnSuccess()
                var results = new CapturePaymentsByInvoiceResult
                {
                    Status = "SUCCESS",
                    // Return captured amount
                    // Technical: Matches results.put("captureAmount", capturedAmount)
                    Data = new { captureAmount = capturedAmount }
                };

                // Return result
                // Business intent: Indicate successful processing
                // Technical: Matches return results
                return results;
            }
            // Catch unexpected errors
            catch (Exception e)
            {
                // Log error
                // Business intent: Track all failures for support
                // Technical: Per your requirement for try-catch
                _logger.LogError(e, "Unexpected error in captureBillingAccountPayments for Invoice #{invoiceId}", input.InvoiceId);
                // Return ERROR
                // Business intent: Provide a fallback error message
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "An unexpected error occurred while capturing billing account payments."
                };
            }
        }
    
    /// <summary>
        /// Processes the result of an authorization or capture operation, updating the payment preference accordingly.
        /// </summary>
        /// <param name="input">Input containing authResult, captureResult, captureAmount, invoiceId, orderPaymentPreference, and captureRefNum</param>
        /// <param name="userLogin">The user executing the service</param>
        /// <param name="paymentPreference">The payment preference to update</param>
        /// <param name="locale">The locale for error message localization</param>
        /// <returns>True if the auth or capture result is successful, false otherwise</returns>
        public bool ProcessResult(ProcessResultInput input, UserLogin userLogin, OrderPaymentPreference paymentPreference, CultureInfo locale)
        {
            // Wrap the function in a try-catch for unexpected errors
            // Business intent: Handle all exceptions and log them
            // Technical: Matches throws GeneralException in Java
            try
            {
                // Extract input parameters
                // Business intent: Determine which result to process
                // Technical: Matches result.get("authResult") and result.get("captureResult")
                bool? authResult = input.AuthResult;
                bool? captureResult = input.CaptureResult;

                // Initialize result flag
                // Business intent: Track whether the operation succeeded
                // Technical: Matches boolean resultPassed = false
                bool resultPassed = false;

                // Get initial status
                // Business intent: Determine current state of payment preference
                // Technical: Matches String initialStatus = paymentPreference.getString("statusId")
                string initialStatus = paymentPreference.StatusId;

                // Initialize auth service type
                // Business intent: Track whether this is an initial auth or re-auth
                // Technical: Matches String authServiceType = null
                string authServiceType = null;

                // Process authorization result if present
                // Business intent: Handle authorization updates
                // Technical: Matches if (authResult != null) { ... }
                if (authResult.HasValue)
                {
                    // Process auth result
                    // Business intent: Update preference based on auth outcome
                    // Technical: Matches processAuthResult(dctx, result, userLogin, paymentPreference)
                    ProcessAuthResult(input, userLogin, paymentPreference);
                    // Set result flag
                    // Business intent: Record auth success/failure
                    // Technical: Matches resultPassed = authResult
                    resultPassed = authResult.Value;
                    // Determine auth service type
                    // Business intent: Differentiate initial vs. re-authorization
                    // Technical: Matches authServiceType = ("PAYMENT_NOT_AUTH".equals(initialStatus)) ? AUTH_SERVICE_TYPE : REAUTH_SERVICE_TYPE
                    authServiceType = initialStatus == "PAYMENT_NOT_AUTH" ? AUTH_SERVICE_TYPE : REAUTH_SERVICE_TYPE;
                }

                // Process capture result if present
                // Business intent: Handle capture updates
                // Technical: Matches if (captureResult != null) { ... }
                if (captureResult.HasValue)
                {
                    // Process capture result
                    // Business intent: Update preference based on capture outcome
                    // Technical: Matches processCaptureResult(dctx, result, userLogin, paymentPreference, authServiceType, locale)
                    ProcessCaptureResult(input, userLogin, paymentPreference, authServiceType, locale);
                    // Update result flag if not already set
                    // Business intent: Ensure capture success is recorded if auth didn’t set it
                    // Technical: Matches if (!resultPassed) { resultPassed = captureResult }
                    if (!resultPassed)
                    {
                        resultPassed = captureResult.Value;
                    }
                }

                // Return result
                // Business intent: Indicate overall success or failure
                // Technical: Matches return resultPassed
                return resultPassed;
            }
            // Catch unexpected errors
            catch (Exception e)
            {
                // Log error
                // Business intent: Track all failures for support
                // Technical: Matches throws GeneralException, per your try-catch requirement
                _logger.LogError(e, "Unexpected error in ProcessResult for PaymentPreference #{orderPaymentPreferenceId}", paymentPreference.OrderPaymentPreferenceId);
                // Throw exception to maintain original behavior
                // Business intent: Propagate error to caller
                // Technical: Matches throws GeneralException
                throw;
            }
        }

    /// <summary>
        /// Processes an authorization result by invoking the public ProcessAuthResult service.
        /// </summary>
        /// <param name="input">Input containing authResult and other authorization details</param>
        /// <param name="userLogin">The user executing the service</param>
        /// <param name="paymentPreference">The payment preference to update</param>
        public void ProcessAuthResult(ProcessResultInput input, UserLogin userLogin, OrderPaymentPreference paymentPreference)
        {
            // Wrap in try-catch
            // Business intent: Handle errors during auth processing
            // Technical: Matches throws GeneralException
            try
            {
                // Prepare input for public ProcessAuthResult
                // Business intent: Pass necessary data
                // Technical: Matches result.put("userLogin", ...) and result.put("orderPaymentPreference", ...)
                var authInput = new ProcessAuthResultInput
                {
                    OrderPaymentPreference = paymentPreference,
                    UserLogin = userLogin,
                    AuthResult = input.AuthResult,
                    ServiceTypeEnum = input.ServiceTypeEnum,
                    CurrencyUomId = input.CurrencyUomId,
                    AvsCode = input.AvsCode,
                    CvCode = input.CvCode,
                    ScoreCode = input.ScoreCode,
                    ProcessAmount = input.ProcessAmount,
                    AuthRefNum = input.AuthRefNum,
                    AuthAltRefNum = input.AuthAltRefNum,
                    AuthCode = input.AuthCode,
                    AuthFlag = input.AuthFlag,
                    AuthMessage = input.AuthMessage,
                    ResultDeclined = input.ResultDeclined,
                    ResultNsf = input.ResultNsf,
                    ResultBadExpire = input.ResultBadExpire,
                    ResultBadCardNumber = input.ResultBadCardNumber,
                    InternalRespMsgs = input.InternalRespMsgs,
                    Locale = CultureInfo.CurrentCulture
                };

                // Simulate ModelService for validation
                // Business intent: Ensure input is valid
                // Technical: Matches ModelService model = dctx.getModelService("processAuthResult")
                // Note: We call directly, as C# doesn’t need a model definition

                // TODO: Implement rollback service
                // Business intent: Ensure rollback on failure
                // Technical: Matches dispatcher.addRollbackService(model.getName(), ...)
                // Note: EF Core transactions can handle this in production

                // Invoke the public ProcessAuthResult
                // Business intent: Process authorization
                // Technical: Matches dispatcher.runSync(model.getName(), context)
                var resResp = ProcessAuthResult(authInput);

                // Check for error
                // Business intent: Propagate failures
                // Technical: Matches if (ServiceUtil.isError(resResp))
                if (resResp.Status == "ERROR")
                {
                    // Throw exception
                    // Business intent: Stop processing
                    // Technical: Matches throw new GeneralException(...)
                    throw new Exception(resResp.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                // Log error
                // Business intent: Track failures
                // Technical: Matches Debug.logError(e, MODULE)
                _logger.LogError(e, "Error in ProcessAuthResult for PaymentPreference #{orderPaymentPreferenceId}", paymentPreference.OrderPaymentPreferenceId);
                // Rethrow
                // Technical: Matches throw e
                throw;
            }
        }

        /// <summary>
        /// Processes the result of an authorization, updating payment preference and related entities.
        /// </summary>
        /// <param name="input">Input containing authorization details</param>
        /// <returns>SUCCESS or ERROR with message</returns>
        public CapturePaymentsByInvoiceResult ProcessAuthResult(ProcessAuthResultInput input)
        {
            // Wrap in try-catch
            // Business intent: Handle all errors
            // Technical: Matches try-catch in Java
            try
            {
                // Extract input parameters
                // Business intent: Prepare data
                // Technical: Matches context.get(...)
                OrderPaymentPreference orderPaymentPreference = input.OrderPaymentPreference;
                bool? authResult = input.AuthResult;
                string authType = input.ServiceTypeEnum;
                string currencyUomId = input.CurrencyUomId;
                CultureInfo locale = input.Locale ?? CultureInfo.CurrentCulture;
                DateTime nowTimestamp = DateTime.UtcNow;

                // Validate authResult
                // Business intent: Ensure result is provided
                // Technical: Matches if (authResult == null)
                if (!authResult.HasValue)
                {
                    // Log error
                    // Business intent: Track invalid input
                    // Technical: Matches Debug.logError(...)
                    _logger.LogError("No authentification result available. Payment preference can't be checked.");
                    // Return ERROR
                    // Technical: Matches ServiceUtil.returnError(...)
                    return new CapturePaymentsByInvoiceResult
                    {
                        Status = "ERROR",
                        ErrorMessage = GetLocalizedMessage("AccountingProcessingAuthResultEmpty", null, locale)
                    };
                }

                // Refresh payment preference
                // Business intent: Ensure latest data
                // Technical: Matches orderPaymentPreference.refresh()
                try
                {
                    // Reload from database
                    // Technical: Matches refresh()
                    orderPaymentPreference = _context.OrderPaymentPreferences
                        .FirstOrDefault(opp => opp.OrderPaymentPreferenceId == orderPaymentPreference.OrderPaymentPreferenceId)
                        ?? orderPaymentPreference;
                }
                catch (Exception e)
                {
                    // Log error
                    // Business intent: Track database issues
                    // Technical: Matches Debug.logError(e, MODULE)
                    _logger.LogError(e, "Error refreshing PaymentPreference #{orderPaymentPreferenceId}", orderPaymentPreference.OrderPaymentPreferenceId);
                    // Return ERROR
                    // Technical: Matches ServiceUtil.returnError(...)
                    return new CapturePaymentsByInvoiceResult
                    {
                        Status = "ERROR",
                        ErrorMessage = e.Message
                    };
                }

                // Determine auth type
                // Business intent: Identify initial or re-auth
                // Technical: Matches if (UtilValidate.isEmpty(authType)) { ... }
                if (string.IsNullOrEmpty(authType))
                {
                    // Set based on status
                    // Technical: Matches authType = ("PAYMENT_NOT_AUTH".equals(...)) ? ...
                    authType = orderPaymentPreference.StatusId == "PAYMENT_NOT_AUTH" ? AUTH_SERVICE_TYPE : REAUTH_SERVICE_TYPE;
                }

                // Try-catch for main processing
                // Business intent: Update payment records safely
                // Technical: Matches try-catch in Java
                try
                {
                    // Get payment method
                    // Business intent: Retrieve payment details
                    // Technical: Matches GenericValue paymentMethod = EntityQuery.use(delegator)...
                    string paymentMethodId = orderPaymentPreference.PaymentMethodId;
                    PaymentMethod paymentMethod = _context.PaymentMethods
                        .Include(pm => pm.CreditCard)
                        .FirstOrDefault(pm => pm.PaymentMethodId == paymentMethodId);

                    // Get credit card if applicable
                    // Business intent: Handle credit card-specific logic
                    // Technical: Matches GenericValue creditCard = paymentMethod.getRelatedOne("CreditCard", false)
                    CreditCard creditCard = paymentMethod?.PaymentMethodTypeId == "CREDIT_CARD" ? paymentMethod.CreditCard : null;

                    // Create PaymentGatewayResponse
                    // Business intent: Record authorization details
                    // Technical: Matches GenericValue response = delegator.makeValue("PaymentGatewayResponse")
                    var response = new PaymentGatewayResponse
                    {
                        // Generate ID
                        // Technical: Matches delegator.getNextSeqId("PaymentGatewayResponse")
                        PaymentGatewayResponseId = Guid.NewGuid().ToString(),
                        // Set fields
                        // Technical: Matches response.set(...)
                        PaymentServiceTypeEnumId = authType,
                        OrderPaymentPreferenceId = orderPaymentPreference.OrderPaymentPreferenceId,
                        PaymentMethodTypeId = orderPaymentPreference.PaymentMethodTypeId,
                        PaymentMethodId = orderPaymentPreference.PaymentMethodId,
                        TransCodeEnumId = "PGT_AUTHORIZE",
                        CurrencyUomId = currencyUomId,
                        // AVS/fraud results
                        GatewayAvsResult = input.AvsCode,
                        GatewayCvResult = input.CvCode,
                        GatewayScoreResult = input.ScoreCode,
                        // Auth info
                        Amount = input.ProcessAmount,
                        ReferenceNum = input.AuthRefNum,
                        AltReference = input.AuthAltRefNum,
                        GatewayCode = input.AuthCode,
                        GatewayFlag = input.AuthFlag,
                        GatewayMessage = input.AuthMessage,
                        TransactionDate = nowTimestamp,
                        // Result flags
                        ResultDeclined = input.ResultDeclined == true ? "Y" : null,
                        ResultNsf = input.ResultNsf == true ? "Y" : null,
                        ResultBadExpire = input.ResultBadExpire == true ? "Y" : null,
                        ResultBadCardNumber = input.ResultBadCardNumber == true ? "Y" : null
                    };

                    // Create response messages
                    // Business intent: Log gateway messages
                    // Technical: Matches List<GenericValue> messageEntities = new LinkedList<>()
                    var messageEntities = new List<PaymentGatewayRespMsg>();
                    // Process messages
                    // Technical: Matches if (UtilValidate.isNotEmpty(messages)) { ... }
                    if (input.InternalRespMsgs != null && input.InternalRespMsgs.Any())
                    {
                        // Iterate messages
                        // Technical: Matches Iterator<String> i = messages.iterator()
                        foreach (var message in input.InternalRespMsgs)
                        {
                            // Create message entity
                            // Technical: Matches GenericValue respMsg = delegator.makeValue("PaymentGatewayRespMsg")
                            var respMsg = new PaymentGatewayRespMsg
                            {
                                // Generate ID
                                // Technical: Matches delegator.getNextSeqId("PaymentGatewayRespMsg")
                                PaymentGatewayRespMsgId = Guid.NewGuid().ToString(),
                                PaymentGatewayResponseId = response.PaymentGatewayResponseId,
                                PgrMessage = message
                            };
                            // Add to list
                            // Technical: Matches messageEntities.add(respMsg)
                            messageEntities.Add(respMsg);
                        }
                    }

                    // Save response and messages
                    // Business intent: Persist authorization records
                    // Technical: Matches savePgrAndMsgs(dctx, response, messageEntities)
                    SavePgrAndMsgs(response, messageEntities);

                    // Validate amount
                    // Business intent: Ensure authorized amount matches
                    // Technical: Matches if (response.getBigDecimal("amount").compareTo(...) != 0)
                    if (response.Amount != input.ProcessAmount)
                    {
                        // Log warning
                        // Business intent: Track discrepancies
                        // Technical: Matches Debug.logWarning(...)
                        _logger.LogWarning("The authorized amount does not match the max amount: Response - {response}, Input - {input}",
                            response, input);
                    }

                    // Set payment preference status
                    // Business intent: Update based on auth result
                    // Technical: Matches if (authResultOk) { ... } else { ... }
                    bool authResultOk = authResult.Value;
                    orderPaymentPreference.StatusId = authResultOk ? "PAYMENT_AUTHORIZED" : "PAYMENT_DECLINED";

                    // Clear sensitive data
                    // Business intent: Enhance security
                    // Technical: Matches orderPaymentPreference.set("securityCode", null)
                    orderPaymentPreference.SecurityCode = null;
                    orderPaymentPreference.Track2 = null;

                    // Check NSF retry
                    // Business intent: Flag for retry on NSF
                    // Technical: Matches boolean needsNsfRetry = needsNsfRetry(...)
                    bool needsNsfRetry = NeedsNsfRetry(orderPaymentPreference, input);
                    // Set flag
                    // Technical: Matches orderPaymentPreference.set("needsNsfRetry", ...)
                    orderPaymentPreference.NeedsNsfRetry = needsNsfRetry ? "Y" : "N";

                    // Save preference
                    // Business intent: Persist status
                    // Technical: Matches orderPaymentPreference.store()
                    _context.OrderPaymentPreferences.Update(orderPaymentPreference);

                    // Handle declined payment for credit card
                    // Business intent: Track failed attempts
                    // Technical: Matches if (!authResultOk)
                    if (!authResultOk && creditCard != null)
                    {
                        // Update failed auths
                        // Technical: Matches creditCard.set("consecutiveFailedAuths", ...)
                        creditCard.ConsecutiveFailedAuths = (creditCard.ConsecutiveFailedAuths ?? 0) + 1;
                        creditCard.LastFailedAuthDate = nowTimestamp;

                        // Update NSF if applicable
                        // Technical: Matches if (Boolean.TRUE.equals(context.get("resultNsf")))
                        if (input.ResultNsf == true)
                        {
                            creditCard.ConsecutiveFailedNsf = (creditCard.ConsecutiveFailedNsf ?? 0) + 1;
                            creditCard.LastFailedNsfDate = nowTimestamp;
                        }

                        // Save credit card
                        // Technical: Matches creditCard.store()
                        _context.CreditCards.Update(creditCard);
                    }

                    // Clear failed auth info on success
                    // Business intent: Reset failure counters
                    // Technical: Matches if (authResultOk)
                    if (authResultOk && creditCard != null && creditCard.LastFailedAuthDate != null)
                    {
                        // Reset fields
                        // Technical: Matches creditCard.set("consecutiveFailedAuths", 0L)
                        creditCard.ConsecutiveFailedAuths = 0;
                        creditCard.LastFailedAuthDate = null;
                        creditCard.ConsecutiveFailedNsf = 0;
                        creditCard.LastFailedNsfDate = null;
                        // Save credit card
                        // Technical: Matches creditCard.store()
                        _context.CreditCards.Update(creditCard);
                    }
                    
                }
                catch (Exception e)
                {
                    // Log error
                    // Business intent: Track update failures
                    // Technical: Matches Debug.logError(e, ...)
                    _logger.LogError(e, "Error updating payment status information for PaymentPreference #{orderPaymentPreferenceId}",
                        orderPaymentPreference.OrderPaymentPreferenceId);
                    // Return ERROR
                    // Technical: Matches ServiceUtil.returnError(...)
                    return new CapturePaymentsByInvoiceResult
                    {
                        Status = "ERROR",
                        ErrorMessage = GetLocalizedMessage("AccountingPaymentStatusUpdatingError",
                            new { errorString = e.ToString() }, locale)
                    };
                }

                // Return SUCCESS
                // Business intent: Indicate successful processing
                // Technical: Matches ServiceUtil.returnSuccess()
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "SUCCESS",
                    Data = null
                };
            }
            catch (Exception e)
            {
                // Log error
                // Business intent: Track unexpected failures
                // Technical: Per your try-catch requirement
                _logger.LogError(e, "Unexpected error in ProcessAuthResult for PaymentPreference #{orderPaymentPreferenceId}",
                    input.OrderPaymentPreference.OrderPaymentPreferenceId);
                // Return ERROR
                return new CapturePaymentsByInvoiceResult
                {
                    Status = "ERROR",
                    ErrorMessage = "An unexpected error occurred while processing authorization result."
                };
            }
            
            /// <summary>
        /// Saves a PaymentGatewayResponse and its associated messages to the database.
        /// </summary>
        /// <param name="response">The PaymentGatewayResponse to save</param>
        /// <param name="messageEntities">List of PaymentGatewayRespMsg entities to save</param>
        public void SavePgrAndMsgs(PaymentGatewayResponse response, List<PaymentGatewayRespMsg> messageEntities)
        {
            // Wrap in try-catch
            // Business intent: Handle database errors
            // Technical: Matches try-catch for GenericEntityException | GenericServiceException
            try
            {
                // Prepare context for rollback
                // Business intent: Ensure rollback on failure
                // Technical: Matches Map<String, GenericValue> context = UtilMisc.toMap(...)
                var rollbackContext = new { PaymentGatewayResponse = response, Messages = messageEntities };

                // Register rollback service
                // Business intent: Undo changes on transaction failure
                // Technical: Matches dispatcher.addRollbackService("savePaymentGatewayResponseAndMessages", ...)
                // Note: Use EF Core transaction in production
                try
                {
                    // TODO: Implement rollback with EF Core transaction
                    // Business intent: Simulate addRollbackService
                    // Technical: Placeholder for SavePaymentGatewayResponseAndMessages
                    // Begin transaction
                    using var transaction = _context.Database.BeginTransaction();

                    // Save PaymentGatewayResponse
                    // Business intent: Persist authorization record
                    // Technical: Matches delegator.create(pgr)
                    _context.PaymentGatewayResponses.Add(response);

                    // Save messages
                    // Business intent: Persist gateway messages
                    // Technical: Matches for (GenericValue message : messages) { delegator.create(message) }
                    if (messageEntities != null && messageEntities.Any())
                    {
                        _context.PaymentGatewayRespMsgs.AddRange(messageEntities);
                    }
                    
                }
                catch (Exception)
                {
                    // Rollback transaction
                    // Business intent: Undo changes
                    // Technical: Matches rollback service
                    SavePaymentGatewayResponseAndMessages(response, messageEntities);
                    throw;
                }
            }
            catch (Exception ge)
            {
                // Log error
                // Business intent: Track save failures
                // Technical: Matches Debug.logError(ge, MODULE)
                _logger.LogError(ge, "Error saving PaymentGatewayResponse #{paymentGatewayResponseId}", response.PaymentGatewayResponseId);
                // Swallow exception
                // Business intent: Allow processing to continue
                // Technical: Matches catch without rethrow
            }
        }

        }


}*/
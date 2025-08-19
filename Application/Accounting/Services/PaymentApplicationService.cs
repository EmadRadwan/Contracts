using Application.Accounting.Payments;
using Application.Accounting.Services.Models;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPaymentApplicationService
{
    Task<decimal> GetPaymentApplied(Payment payment, bool actual);
    Task<decimal> GetPaymentNotApplied(Payment payment, bool actual);
    Task<PaymentApplicationParam> CreatePaymentApplication(PaymentApplicationParam paymentApplicationParam);
    Task<GeneralServiceResult<RemovePaymentApplicationResult>> RemovePaymentApplication(string paymentApplicationId);
}

public class PaymentApplicationService : IPaymentApplicationService
{
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly ILogger<PaymentApplicationService> _logger;


    public PaymentApplicationService(DataContext context, IUtilityService utilityService,
        IInvoiceUtilityService invoiceUtilityService, ILogger<PaymentApplicationService> logger)
    {
        _context = context;
        _utilityService = utilityService;
        _invoiceUtilityService = invoiceUtilityService;
        _logger = logger;
    }

    public async Task<decimal> GetPaymentApplied(Payment payment, bool actual)
    {
        if (payment == null)
        {
            throw new ArgumentException("The provided payment is null or does not exist.");
        }

        decimal paymentApplied = 0;

        try
        {
            // Retrieve payment applications related to the payment
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == payment.PaymentId || pa.ToPaymentId == payment.PaymentId)
                .ToListAsync();

            // Iterate through payment applications and calculate the total applied amount
            foreach (var paymentApplication in paymentApplications)
            {
                var amountApplied = (decimal)paymentApplication.AmountApplied;

                // Check if currency conversion is needed (when 'actual' is false)
                if (!actual && paymentApplication.InvoiceId != null && payment.ActualCurrencyAmount != null &&
                    payment.ActualCurrencyUomId != null)
                {
                    // Retrieve the related invoice
                    var invoice = await _context.Invoices.FindAsync(paymentApplication.InvoiceId);

                    if (invoice != null && payment.ActualCurrencyUomId == invoice.CurrencyUomId)
                    {
                        // Adjust the amount applied based on the currency exchange rate
                        amountApplied = (decimal)(amountApplied * payment.Amount / payment.ActualCurrencyAmount);
                    }
                }

                // Add to total applied amount
                paymentApplied += amountApplied;
            }

            return paymentApplied;
        }
        catch (Exception ex)
        {
            // Log the exception (you can use your logging framework here)
            // Log.Error(ex, "Error calculating payment applied");

            // Rethrow or handle the error as needed
            throw new Exception("An error occurred while calculating the payment applied.", ex);
        }
    }

    public async Task<decimal> GetPaymentNotApplied(Payment payment, bool actual)
    {
        if (payment == null)
        {
            throw new ArgumentException("The provided payment is null or does not exist.");
        }

        try
        {
            // Call the GetPaymentApplied method to calculate the total amount applied for the payment
            var appliedAmount = await GetPaymentApplied(payment, actual);

            // Subtract the total applied amount from the total payment amount
            var notAppliedAmount = payment.Amount - appliedAmount;

            // Return the result rounded to 2 decimal places
            return Math.Round(notAppliedAmount, 2);
        }
        catch (Exception ex)
        {
            // Log the exception (assuming you have a logger like Serilog)
            // Log.Error(ex, "Error calculating not applied amount for payment");

            // Rethrow or provide meaningful error
            throw new Exception("Error calculating not applied amount for payment.", ex);
        }
    }


    public async Task<PaymentApplicationParam> CreatePaymentApplication(PaymentApplicationParam paymentApplicationParam)
    {
        try
        {
            // Validate that at least one of invoiceId, billingAccountId, taxAuthGeoId, or toPaymentId is provided
            if (string.IsNullOrEmpty(paymentApplicationParam.InvoiceId))
            {
                if (string.IsNullOrEmpty(paymentApplicationParam.BillingAccountId))
                {
                    if (string.IsNullOrEmpty(paymentApplicationParam.TaxAuthGeoId))
                    {
                        if (string.IsNullOrEmpty(paymentApplicationParam.ToPaymentId))
                        {
                            throw new Exception("AccountingPaymentApplicationParameterMissing");
                        }
                    }
                }
            }

            // Retrieve payment details from the database
            var payment = await _context.Payments.FindAsync(paymentApplicationParam.PaymentId);
            if (payment == null)
            {
                throw new Exception("PaymentNotFound");
            }

            // Calculate not applied payment amount
            var notAppliedPayment = await GetPaymentNotApplied(payment, true);

            // Process based on provided invoice ID
            if (!string.IsNullOrEmpty(paymentApplicationParam.InvoiceId))
            {
                // Retrieve invoice details from the database
                var invoice = await _context.Invoices.FindAsync(paymentApplicationParam.InvoiceId);
                if (invoice == null)
                {
                    throw new Exception("InvoiceNotFound");
                }

                // Validate currency compatibility
                if (invoice.CurrencyUomId != payment.CurrencyUomId)
                {
                    if (invoice.CurrencyUomId != payment.ActualCurrencyUomId)
                    {
                        throw new Exception("AccountingCurrenciesOfInvoiceAndPaymentNotCompatible");
                    }
                    else
                    {
                        // Handle foreign currency scenario (matching ActualCurrencyUomId)
                        notAppliedPayment =
                            await GetPaymentNotApplied(payment,
                                true); // Add 'true' for actual flag to handle foreign currency
                    }
                }

                // Calculate amount to be applied from payment to invoice
                var notAppliedInvoice = await _invoiceUtilityService.GetInvoiceNotApplied(invoice.InvoiceId);
                if (notAppliedInvoice <= notAppliedPayment)
                {
                    paymentApplicationParam.AmountApplied = notAppliedInvoice;
                }
                else
                {
                    paymentApplicationParam.AmountApplied = notAppliedPayment;
                }

                // Associate billing account if available
                if (!string.IsNullOrEmpty(invoice.BillingAccountId))
                {
                    paymentApplicationParam.BillingAccountId = invoice.BillingAccountId;
                }
            }

            // Process based on provided toPayment ID
            if (!string.IsNullOrEmpty(paymentApplicationParam.ToPaymentId))
            {
                // Retrieve toPayment details from the database
                var toPayment =
                    await _context.Payments.FindAsync(paymentApplicationParam.ToPaymentId);
                if (toPayment == null)
                {
                    throw new Exception("ToPaymentNotFound");
                }

                // Validate payment type compatibility (between payment and toPayment)
                var paymentType =
                    await _context.PaymentTypes.FirstOrDefaultAsync(pt => pt.PaymentTypeId == payment.PaymentTypeId);
                var toPaymentType =
                    await _context.PaymentTypes.FirstOrDefaultAsync(pt => pt.PaymentTypeId == toPayment.PaymentTypeId);
                if (paymentType == null || toPaymentType == null)
                {
                    throw new Exception("PaymentTypeNotFound");
                }

                // Calculate amount to be applied to toPayment if amount not provided
                if (!paymentApplicationParam.AmountApplied.HasValue)
                {
                    notAppliedPayment = await GetPaymentNotApplied(payment, true);
                    var notAppliedToPayment = await GetPaymentNotApplied(toPayment, true);
                    if (notAppliedPayment < notAppliedToPayment)
                    {
                        paymentApplicationParam.AmountApplied = notAppliedPayment;
                    }
                    else
                    {
                        paymentApplicationParam.AmountApplied = notAppliedToPayment;
                    }
                }
            }

            // Calculate amount applied if not provided and billingAccountId or taxAuthGeoId is provided
            if (!paymentApplicationParam.AmountApplied.HasValue)
            {
                if (!string.IsNullOrEmpty(paymentApplicationParam.BillingAccountId))
                {
                    paymentApplicationParam.AmountApplied = notAppliedPayment;
                }
                else if (!string.IsNullOrEmpty(paymentApplicationParam.TaxAuthGeoId))
                {
                    paymentApplicationParam.AmountApplied = notAppliedPayment;
                }
            }

            // Generate payment application next sequence from utility service
            var paymentApplicationId = await _utilityService.GetNextSequence("PaymentApplication");

            paymentApplicationParam.PaymentApplicationId = paymentApplicationId;

            // Create the payment application and populate it with the provided parameters
            var paymentApplication = new PaymentApplication
            {
                PaymentApplicationId = paymentApplicationParam.PaymentApplicationId,
                PaymentId = paymentApplicationParam.PaymentId,
                InvoiceId = paymentApplicationParam.InvoiceId,
                InvoiceItemSeqId = paymentApplicationParam.InvoiceItemSeqId,
                BillingAccountId = paymentApplicationParam.BillingAccountId,
                OverrideGlAccountId = paymentApplicationParam.OverrideGlAccountId,
                ToPaymentId = paymentApplicationParam.ToPaymentId,
                TaxAuthGeoId = paymentApplicationParam.TaxAuthGeoId,
                AmountApplied = paymentApplicationParam.AmountApplied,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            // Save payment application to the database
            _context.PaymentApplications.Add(paymentApplication);

            // Return the created payment application DTO
            return paymentApplicationParam;
        }
        catch (Exception ex)
        {
            // Log the exception (you can use your logging framework, e.g., Serilog, NLog, etc.)
            // Log.Error(ex, "Error creating payment application");

            // Optionally, you can rethrow the exception or return a more user-friendly error message
            throw new Exception("An error occurred while creating the payment application.", ex);
        }
    }

    public async Task<GeneralServiceResult<RemovePaymentApplicationResult>> RemovePaymentApplication(
        string paymentApplicationId)
    {
        try
        {
            // Query the PaymentApplication entity
            // Technical: Uses LINQ to retrieve PaymentApplication by paymentApplicationId
            // Business Purpose: Ensures the payment application exists before attempting deletion
            var paymentApplication = await _context.PaymentApplications
                .Where(pa => pa.PaymentApplicationId == paymentApplicationId)
                .SingleOrDefaultAsync();

            // Check if paymentApplication was found
            // Technical: Verifies paymentApplication is not null
            // Business Purpose: Prevents deletion of non-existent records, returning an error if not found
            if (paymentApplication == null)
            {
                return GeneralServiceResult<RemovePaymentApplicationResult>.Error(
                    $"AccountingPaymentApplicationNotFound: paymentApplicationId={paymentApplicationId}");
            }

            // Initialize message for success response
            // Technical: Prepares a string to append context-specific details
            // Business Purpose: Constructs a user-friendly message indicating what was deleted
            string toMessage = "";

            // Check payment (if paymentId exists)
            // Technical: Queries Payment if paymentId is not null
            // Business Purpose: Ensures the payment is not confirmed, as confirmed payments cannot be unapplied
            if (!string.IsNullOrEmpty(paymentApplication.PaymentId))
            {
                var payment = await _context.Payments
                    .Where(p => p.PaymentId == paymentApplication.PaymentId)
                    .SingleOrDefaultAsync();
                if (payment?.StatusId == "PMNT_CONFIRMED")
                {
                    return GeneralServiceResult<RemovePaymentApplicationResult>.Error(
                        "AccountingPaymentApplicationCannotRemovedWithConfirmedStatus");
                }
            }

            // Check invoice (if invoiceId exists)
            // Technical: Queries Invoice if invoiceId is not null
            // Business Purpose: Reverts invoice to READY if paid, and sets message for invoice application
            if (!string.IsNullOrEmpty(paymentApplication.InvoiceId))
            {
                var invoice = await _context.Invoices
                    .Where(i => i.InvoiceId == paymentApplication.InvoiceId)
                    .SingleOrDefaultAsync();
                if (invoice?.StatusId == "INVOICE_PAID")
                {
                    // Update invoice status to READY
                    // Technical: Calls setInvoiceStatus to revert invoice
                    // Business Purpose: Ensures invoice is available for new payments after unapplying
                    await _invoiceUtilityService.SetInvoiceStatus(invoice.InvoiceId, "INVOICE_READY", null);
                    
                }

                toMessage = $"AccountingPaymentApplToInvoice: InvoiceId={paymentApplication.InvoiceId}";
            }

            // Check invoice item (if invoiceItemSeqId exists)
            // Technical: Sets message if invoiceItemSeqId is not null
            // Business Purpose: Indicates the application was for a specific invoice item
            if (!string.IsNullOrEmpty(paymentApplication.InvoiceItemSeqId))
            {
                toMessage =
                    $"AccountingApplicationToInvoiceItem: InvoiceId={paymentApplication.InvoiceId}, InvoiceItemSeqId={paymentApplication.InvoiceItemSeqId}";
            }

            // Check toPayment (if toPaymentId exists)
            // Technical: Queries Payment if toPaymentId is not null
            // Business Purpose: Ensures the to-payment is not confirmed, and sets message for payment-to-payment application
            if (!string.IsNullOrEmpty(paymentApplication.ToPaymentId))
            {
                var toPayment = await _context.Payments
                    .Where(p => p.PaymentId == paymentApplication.ToPaymentId)
                    .SingleOrDefaultAsync();
                if (toPayment?.StatusId == "PMNT_CONFIRMED")
                {
                    return GeneralServiceResult<RemovePaymentApplicationResult>.Error(
                        "AccountingPaymentApplicationCannotRemovedWithConfirmedStatus");
                }

                toMessage = $"AccountingPaymentApplToPayment: ToPaymentId={paymentApplication.ToPaymentId}";
            }

            // Check billing account (if billingAccountId exists)
            // Technical: Sets message if billingAccountId is not null
            // Business Purpose: Indicates the application was for a billing account
            if (!string.IsNullOrEmpty(paymentApplication.BillingAccountId))
            {
                toMessage =
                    $"AccountingPaymentApplToBillingAccount: BillingAccountId={paymentApplication.BillingAccountId}";
            }

            // Check tax authority (if taxAuthGeoId exists)
            // Technical: Sets message if taxAuthGeoId is not null
            // Business Purpose: Indicates the application was for a tax authority
            if (!string.IsNullOrEmpty(paymentApplication.TaxAuthGeoId))
            {
                toMessage = $"AccountingPaymentApplToTaxAuth: TaxAuthGeoId={paymentApplication.TaxAuthGeoId}";
            }

            // Delete the PaymentApplication
            // Technical: Removes the PaymentApplication from the DbContext and saves changes
            // Business Purpose: Finalizes the unapplication, disconnecting the payment from its target
            _context.PaymentApplications.Remove(paymentApplication);

            // Construct result DTO
            // Technical: Creates RemovePaymentApplicationResult with message and PaymentApplication
            // Business Purpose: Provides confirmation and details of the deleted application
            var resultData = new RemovePaymentApplicationResult
            {
                Message = $"AccountingPaymentApplRemoved {toMessage}",
                PaymentApplication = paymentApplication
            };

            // Return success with result DTO
            // Technical: Returns GeneralServiceResult with type-safe ResultData
            // Business Purpose: Confirms deletion with context-specific details
            return GeneralServiceResult<RemovePaymentApplicationResult>.Success(resultData);
        }
        catch (Exception ex)
        {
            // Log and return error
            // Technical: Logs exception details
            // Business Purpose: Tracks deletion errors for auditing
            _logger.LogError(ex, "Error in RemovePaymentApplication for paymentApplicationId: {PaymentApplicationId}",
                paymentApplicationId);
            return GeneralServiceResult<RemovePaymentApplicationResult>.Error(
                "Failed to remove payment application.");
        }
    }
}
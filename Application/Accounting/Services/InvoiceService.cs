using Application.Accounting.Services;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Order.Orders;
using Application.Shipments;
using Application.Shipments.Invoices;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;

namespace Application.Accounting.Services;

public interface IInvoiceService
{
    Task CheckPaymentInvoices(string paymentId);
    Task<InvoiceContext> ListNotAppliedInvoices(string paymentId);
    Task CheckInvoicePaymentApplications(string invoiceId);
}

public class InvoiceService : IInvoiceService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly DataContext _context;
    private readonly IShipmentService _shipmentService;
    private readonly IUtilityService _utilityService;
    private readonly IPaymentApplicationService _paymentApplicationService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly ILogger<InvoiceService> _logger;


    private int _invoiceItemSeq;

    public InvoiceService(DataContext context, IUtilityService utilityService,
        IShipmentService shipmentService, IInvoiceUtilityService invoiceUtilityService,
        IAcctgMiscService acctgMiscService,
        IPaymentApplicationService paymentService,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _utilityService = utilityService;
        _shipmentService = shipmentService;
        _acctgMiscService = acctgMiscService;
        _paymentApplicationService = paymentService;
        _invoiceUtilityService = invoiceUtilityService;
        _logger = logger;
    }


    public async Task CheckPaymentInvoices(string paymentId)
    {
        // Retrieve the payment entity based on the paymentId
        var payment = await _context.Payments.SingleOrDefaultAsync(p => p.PaymentId == paymentId);

        // Retrieve payment applications associated with the payment
        var paymentApplications = await _context.PaymentApplications
            .Where(pa => pa.PaymentId == paymentId)
            .ToListAsync();

        // If no payment applications are found, return success
        if (paymentApplications.Count == 0) return;

        // Iterate over payment applications
        foreach (var paymentApplication in paymentApplications)
        {
            var invoiceId = paymentApplication.InvoiceId;

            // If an invoice ID is associated with the payment application, call the checkInvoicePaymentApplications service
            if (!string.IsNullOrEmpty(invoiceId)) await CheckInvoicePaymentApplications(invoiceId);
        }
    }


    public async Task CheckInvoicePaymentApplications(string invoiceId)
    {
        // Retrieve the invoice entity based on the invoiceId
        var invoice = await _context.Invoices.FindAsync(invoiceId);


        // Ignore invoices that aren't ready yet
        if (invoice.StatusId != "INVOICE_READY") return;

        // Get the payment applications that can be used to pay the invoice
        var paymentApplications = await _context.PaymentApplications
            .Where(pa => pa.InvoiceId == invoiceId)
            .ToListAsync();

        // Filter payment applications based on their payment types
        var filteredPaymentApplications = new List<PaymentApplication>();
        foreach (var paymentApplication in paymentApplications)
        {
            // get the payment for this payment application, try first from the change tracker
            // as a modified payment then from the database
            var payment = _context.ChangeTracker.Entries<Payment>()
                .FirstOrDefault(e =>
                    e.Entity.PaymentId == paymentApplication.PaymentId &&
                    e.State == EntityState.Modified)?.Entity;

            if (payment == null)
                payment = await _context.Payments
                    .SingleOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);

            var parentType = await _utilityService.GetPaymentParentType(payment.PaymentId);
            if (("PMNT_RECEIVED".Equals(payment.StatusId) && parentType == "RECEIPT") ||
                ("PMNT_SENT".Equals(payment.StatusId) && parentType == "DISBURSEMENT"))
                filteredPaymentApplications.Add(paymentApplication);
        }

        // Dictionary to store payments applied to the invoice
        var payments = new Dictionary<string, decimal>();
        DateTime? paidDate = null;

        // Iterate over payment applications
        foreach (var paymentApplication in paymentApplications)
        {
            var paymentId = paymentApplication.PaymentId;
            var amountApplied = (decimal)paymentApplication.AmountApplied;

            // Add payment amount to payments dictionary
            if (paymentId != null) payments.Add(paymentId, amountApplied);

            // Determine the paidDate as the last date (chronologically) of all payments applied to this invoice
            var paymentDate = paymentApplication.Payment.EffectiveDate;
            if (paymentDate != null && (paidDate == null || paymentDate > paidDate)) paidDate = paymentDate;
        }

        // Calculate total payments applied to the invoice
        var totalPayments = payments.Values.Sum();

        if (totalPayments > 0)
        {
            // Get the total invoice amount
            var invoiceTotal = await _invoiceUtilityService.GetInvoiceTotal(invoiceId, true);

            // Check if totalPayments is greater than or equal to invoiceTotal
            if (totalPayments >= invoiceTotal)
                // Set invoice status to PAID
                await _invoiceUtilityService.SetInvoiceStatus(invoice.InvoiceId, "INVOICE_PAID", DateTime.UtcNow,
                    paidDate, true);
        }
    }


    public async Task<InvoiceContext> ListNotAppliedInvoices(string paymentId)
    {
        var context = new InvoiceContext();

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
        {
            throw new Exception("Payment not found.");
        }

        var partyCond = _context.Invoices
            .Where(inv => inv.PartyId == payment.PartyIdFrom && inv.PartyIdFrom == payment.PartyIdTo);

        var statusCond = _context.Invoices
            .Where(inv => inv.StatusId == "INVOICE_APPROVED" ||
                          inv.StatusId == "INVOICE_SENT" ||
                          inv.StatusId == "INVOICE_READY" ||
                          inv.StatusId == "INVOICE_RECEIVED");

        var currCond = _context.Invoices
            .Where(inv => inv.CurrencyUomId == payment.CurrencyUomId);

        var actualCurrCond = _context.Invoices
            .Where(inv => inv.CurrencyUomId == payment.ActualCurrencyUomId);

        var topCond = partyCond
            .Where(party => statusCond.Any(status => status.InvoiceId == party.InvoiceId) &&
                            currCond.Any(curr => curr.InvoiceId == party.InvoiceId));

        var topCondActual = partyCond
            .Where(party => statusCond.Any(status => status.InvoiceId == party.InvoiceId) &&
                            actualCurrCond.Any(curr => curr.InvoiceId == party.InvoiceId));

        var invoices = await topCond
            .OrderBy(inv => inv.InvoiceDate)
            .Select(inv => new InvoiceMap
            {
                InvoiceId = inv.InvoiceId,
                InvoiceTypeId = inv.InvoiceTypeId,
                CurrencyUomId = inv.CurrencyUomId,
                Description = inv.Description,
                InvoiceDate = inv.InvoiceDate
            })
            .ToListAsync();

        context.Invoices = await GetInvoices(invoices, false, payment);

        var invoicesOtherCurrency = await topCondActual
            .OrderBy(inv => inv.InvoiceDate)
            .Select(inv => new InvoiceMap
            {
                InvoiceId = inv.InvoiceId,
                InvoiceTypeId = inv.InvoiceTypeId,
                CurrencyUomId = inv.CurrencyUomId,
                Description = inv.Description,
                InvoiceDate = inv.InvoiceDate
            })
            .ToListAsync();

        context.InvoicesOtherCurrency = await GetInvoices(invoicesOtherCurrency, true, payment);

        return context;
    }

    private async Task<List<InvoiceMap>> GetInvoices(List<InvoiceMap> invoices, bool actual, Payment payment)
    {
        var invoicesList = new List<InvoiceMap>();

        if (invoices != null && invoices.Any())
        {
            var paymentApplied = await _paymentApplicationService.GetPaymentApplied(payment, actual);
            var paymentToApply = payment.Amount - paymentApplied;

            if (actual && payment.ActualCurrencyAmount.HasValue)
            {
                paymentToApply = payment.ActualCurrencyAmount.Value - paymentApplied;
            }

            foreach (var invoice in invoices)
            {
                var invoiceAmount = await _invoiceUtilityService.GetInvoiceTotal(invoice.InvoiceId, actual);
                var invoiceApplied =
                    await _invoiceUtilityService.GetInvoiceApplied(invoice.InvoiceId, DateTime.UtcNow, actual);
                var invoiceToApply = invoiceAmount - invoiceApplied;

                if (invoiceToApply > 0)
                {
                    invoice.AmountApplied = invoiceApplied;
                    invoice.AmountToApply =
                        (decimal)(paymentToApply < invoiceToApply ? paymentToApply : invoiceToApply);

                    invoicesList.Add(invoice);
                }
            }
        }

        return invoicesList;
    }

    /* Creates InvoiceTerm entries for a list of terms, which can be BillingAccountTerms, OrderTerms, etc. */
    private void CreateInvoiceTerms(string invoiceId, IEnumerable<object> terms)
    {
        if (terms != null)
        {
            foreach (var term in terms)
            {
                // Create a new InvoiceTerm entity
                var invoiceTerm = new InvoiceTerm
                {
                    InvoiceTermId = Guid.NewGuid().ToString(),
                    InvoiceId = invoiceId,
                    InvoiceItemSeqId = "_NA_", // Default value for invoice item sequence
                };

                // Distinguish between OrderTerm and BillingAccountTerm
                if (term is OrderTerm orderTerm)
                {
                    invoiceTerm.TermTypeId = orderTerm.TermTypeId;
                    invoiceTerm.TermValue = orderTerm.TermValue;
                    invoiceTerm.TermDays = orderTerm.TermDays;
                    invoiceTerm.UomId = orderTerm.UomId;
                    // Set additional fields for OrderTerm
                    invoiceTerm.TextValue = orderTerm.TextValue;
                    invoiceTerm.Description = orderTerm.Description;
                }
                else if (term is BillingAccountTerm billingAccountTerm)
                {
                    invoiceTerm.TermTypeId = billingAccountTerm.TermTypeId;
                    invoiceTerm.TermValue = billingAccountTerm.TermValue;
                    invoiceTerm.TermDays = billingAccountTerm.TermDays;
                    invoiceTerm.UomId = billingAccountTerm.UomId;
                    // BillingAccountTerm may not have TextValue or Description
                    // If needed, handle accordingly
                }
                else
                {
                    // Handle unexpected term type
                    _logger.LogError("Unexpected term type: {Type}", term.GetType());
                    continue;
                }

                // Add the InvoiceTerm to the database
                try
                {
                    _context.InvoiceTerms.Add(invoiceTerm);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating InvoiceTerm for invoice {InvoiceId}", invoiceId);
                }
            }
        }
    }
}
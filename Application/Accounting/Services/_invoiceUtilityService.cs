using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Application.Accounting.Invoices;
using Application.Accounting.Payments;

namespace Application.Accounting.Services;

public interface IInvoiceUtilityService
{
    Task<bool> IsInvoiceInForeignCurrency(string invoiceId);
    Task<decimal> GetInvoiceTotal(string invoiceId, bool actualCurrency);
    Task<decimal> GetInvoiceNotApplied(string invoiceId);
    Task<decimal> GetInvoiceApplied(string invoiceId, DateTime asOfDateTime, bool actualCurrency);
    Task<Dictionary<string, HashSet<string>>> GetInvoiceTaxAuthPartyAndGeos(string invoiceId);

    Task<decimal> GetInvoiceTaxTotalForTaxAuthPartyAndGeo(string invoiceId, string taxAuthPartyId, string taxAuthGeoId);
    Task<decimal> GetInvoiceUnattributedTaxTotal(string invoiceId);
    string GetNextInvoiceNumber(PartyAcctgPreference partyAcctgPreference);

    Task SetInvoiceStatus(string invoiceId, string statusId, DateTime? statusDate = null,
        DateTime? paidDate = null, bool actualCurrency = false);

    Task<InvoiceStatusDto> GetInvoiceStatus(string invoiceId);
}

public class InvoiceUtilityService : IInvoiceUtilityService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly DataContext _context;
    private readonly ILogger<InvoiceUtilityService> _logger;
    private readonly IUtilityService _utilityService;
    private readonly Lazy<IPaymentApplicationService> _paymentApplicationService;
    private readonly Lazy<IPaymentHelperService> _paymentHelperService;
    private readonly Lazy<IInvoiceService> _invoiceService;
    private readonly Lazy<IGeneralLedgerService> _generalLedgerService;


    public InvoiceUtilityService(DataContext context, IAcctgMiscService acctgMiscService,
        ILogger<InvoiceUtilityService> logger, IUtilityService utilityService,
        Lazy<IGeneralLedgerService> generalLedgerService, Lazy<IPaymentHelperService> paymentHelperService,
        Lazy<IPaymentApplicationService> paymentApplicationService, Lazy<IInvoiceService> invoiceService)
    {
        _context = context;
        _acctgMiscService = acctgMiscService;
        _logger = logger;
        _utilityService = utilityService;
        _generalLedgerService = generalLedgerService;
        _paymentHelperService = paymentHelperService;
        _paymentApplicationService = paymentApplicationService;
        _invoiceService = invoiceService;
    }

    public string GetNextInvoiceNumber(PartyAcctgPreference partyAcctgPreference)
    {
        // get new invoice sequence from partyAcctgPreference
        var newInvoiceSequence = partyAcctgPreference.LastInvoiceNumber + 1;
        // update partyAcctgPreference with new invoice sequence
        partyAcctgPreference.LastInvoiceNumber = newInvoiceSequence;
        // update partyAcctgPreference in database
        _context.PartyAcctgPreferences.Update(partyAcctgPreference);
        // return new invoice sequence
        return newInvoiceSequence.ToString();
    }

    public async Task SetInvoiceStatus(string invoiceId, string statusId, DateTime? statusDate = null,
        DateTime? paidDate = null, bool actualCurrency = false)
    {
        try
        {
            // Retrieve the invoice using the FindLocalOrDatabaseAsync method
            var invoice = await _utilityService.FindLocalOrDatabaseAsync<Invoice>(invoiceId);

            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {invoiceId} not found.");
            }

            var oldStatusId = invoice.StatusId; // Get the old status ID of the invoice
            var invoiceTypeId = invoice.InvoiceTypeId; // Get the invoice type ID

            // Check if the invoice status is different from the provided status
            if (oldStatusId != statusId)
            {
                // Validate the status change using StatusValidChange
                var statusChange = await _context.StatusValidChanges
                    .FirstOrDefaultAsync(s => s.StatusId == oldStatusId && s.StatusIdTo == statusId);

                if (statusChange == null)
                {
                    // No valid status change found, mimic the Ofbiz behavior and fail
                    _logger.LogError($"Cannot change invoice {invoiceId} from {oldStatusId} to {statusId}");
                    throw new InvalidOperationException("Invalid status change.");
                }

                // If setting the invoice to paid, ensure the invoice is fully applied
                if (statusId == "INVOICE_PAID")
                {
                    var notApplied = await GetInvoiceNotApplied(invoice.InvoiceId);
                    if (notApplied != 0m)
                    {
                        // Invoice not fully applied, can't set to paid
                        _logger.LogError($"Invoice {invoiceId} cannot be marked as PAID due to unapplied amounts.");
                        throw new InvalidOperationException(
                            "Invoice cannot be marked as paid because it is not fully applied.");
                    }

                    // Set paidDate if provided, otherwise use current UTC time
                    invoice.PaidDate = paidDate ?? DateTime.UtcNow;
                }

                // If we are setting the invoice to "INVOICE_READY" and it currently has a paid date, clear it
                if (invoice.PaidDate.HasValue && statusId == "INVOICE_READY")
                {
                    invoice.PaidDate = null;
                }

                // Update the invoice status
                invoice.StatusId = statusId;


                // Create a record for the new invoice status
                _utilityService.CreateInvoiceStatus(invoice.InvoiceId, statusId);


                // If the invoice is a payroll invoice and the new status is "INVOICE_APPROVED" or "INVOICE_READY"
                if (invoiceTypeId == "PAYROL_INVOICE" &&
                    (statusId == "INVOICE_APPROVED" || statusId == "INVOICE_READY"))
                {
                    // Check if there are no existing payment applications
                    bool noPaymentApplications = !await _context.PaymentApplications
                        .AnyAsync(pa => pa.InvoiceId == invoice.InvoiceId);

                    if (noPaymentApplications)
                    {
                        // Calculate the payment amount
                        var amount = await GetInvoiceTotal(invoice.InvoiceId, actualCurrency);

                        // Create payment parameters
                        var paymentParams = new CreatePaymentParam
                        {
                            PartyIdFrom = invoice.PartyId,
                            PartyIdTo = invoice.PartyIdFrom,
                            PaymentMethodTypeId = "COMPANY_CHECK",
                            PaymentTypeId = "PAYROL_PAYMENT",
                            StatusId = "PMNT_NOT_PAID",
                            Amount = amount,
                            EffectiveDate = DateTime.UtcNow
                        };

                        // Create the payment
                        var newPayment = await _paymentHelperService.Value.CreatePayment(paymentParams);

                        // Create payment application parameters
                        var paymentApplicationParams = new PaymentApplicationParam
                        {
                            PaymentId = newPayment.PaymentId,
                            InvoiceId = invoice.InvoiceId,
                            AmountApplied = amount
                        };

                        // Create the payment application
                        await _paymentApplicationService.Value.CreatePaymentApplication(paymentApplicationParams);
                    }
                }
            }

            // ECA: Accounting transactions for INVOICE_READY
            if (!string.IsNullOrEmpty(invoiceId) &&
                statusId == "INVOICE_READY" &&
                oldStatusId != "INVOICE_READY" &&
                oldStatusId != "INVOICE_PAID")
            {
                var ledgerService = _generalLedgerService.Value;
                try
                {
                    if (invoiceTypeId != "CUST_RTN_INVOICE")
                    {
                        await ledgerService.CreateAcctgTransForPurchaseInvoice(invoiceId);
                        await ledgerService.CreateAcctgTransForSalesInvoice(invoiceId);
                    }
                    else if (invoiceTypeId == "CUST_RTN_INVOICE")
                    {
                        await ledgerService.CreateAcctgTransForCustomerReturnInvoice(invoiceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create accounting transactions for invoice {invoiceId}");
                }

                // ECA: Check payment applications and capture payments
                try
                {
                    await _invoiceService.Value.CheckInvoicePaymentApplications(invoiceId);
                    //await _paymentHelperService.Value.CapturePaymentsByInvoice(invoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to check or capture payments for invoice {invoiceId}");
                }
            }


            // ECA: Invoice cancellation
            if (!string.IsNullOrEmpty(invoiceId) && statusId == "INVOICE_CANCELLED")
            {
                try
                {
                    await CancelInvoice(invoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to cancel invoice {invoiceId}");
                }
            }

            // ECA: Create matching payment application for INVOICE_APPROVED
            if (statusId == "INVOICE_APPROVED" && oldStatusId != "INVOICE_APPROVED")
            {
                try
                {
                    await _paymentHelperService.Value.CreateMatchingPaymentApplication(null, invoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create matching payment application for invoice {invoiceId}");
                }
            }


            /// ECA: Create matching payment application for INVOICE_READY from INVOICE_IN_PROCESS
            if (statusId == "INVOICE_READY" && oldStatusId == "INVOICE_IN_PROCESS")
            {
                try
                {
                    await _paymentHelperService.Value.CreateMatchingPaymentApplication(null, invoiceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create matching payment application for invoice {invoiceId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error setting invoice {invoiceId} to status {statusId}");
            throw;
        }
    }

    public async Task<string> CancelInvoice(string invoiceId)
    {
        try
        {
            // Retrieve the invoice
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            // Corresponds to <if-empty field="invoice"> ... <add-error> ... <check-errors/>
            if (invoice == null)
            {
                _logger.LogError($"Invoice {invoiceId} not found.");
                throw new InvalidOperationException("AccountingInvoiceNotFound");
            }

            // Retrieve PaymentApplications associated with this invoice
            // OFBiz: <get-related relation-name="PaymentApplication">
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.InvoiceId == invoiceId)
                .ToListAsync();

            // OFBiz: <iterate list="paymentApplications" entry="paymentApplication">
            foreach (var paymentApplication in paymentApplications)
            {
                // <get-related-one relation-name="Payment">
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);

                // If payment is found and currently "PMNT_CONFIRMED"
                if (payment != null && payment.StatusId == "PMNT_CONFIRMED")
                {
                    // In OFBiz:
                    // Determine if payment is a receipt or disbursement
                    bool isReceipt = await _paymentHelperService.Value.IsReceipt(payment);
                    bool isDisbursement = await _paymentHelperService.Value.IsDisbursement(payment);

                    // Set payment status accordingly
                    // <call-service service-name="setPaymentStatus">
                    if (isReceipt)
                    {
                        await _paymentHelperService.Value.SetPaymentStatus(payment.PaymentId, "PMNT_RECEIVED");
                    }
                    else if (isDisbursement)
                    {
                        await _paymentHelperService.Value.SetPaymentStatus(payment.PaymentId, "PMNT_SENT");
                    }
                }

                // Remove PaymentApplication
                // OFBiz: <call-service service-name="removePaymentApplication">
                _context.PaymentApplications.Remove(paymentApplication);
            }

            // <field-to-result field="invoice.invoiceTypeId" result-name="invoiceTypeId"/>
            // Return the invoiceTypeId
            return invoice.InvoiceTypeId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error cancelling invoice {invoiceId}");
            throw; // Rethrow to mimic OFBiz error handling which stops service execution on errors
        }
    }

    public async Task<InvoiceStatusDto> GetInvoiceStatus(string invoiceId)
    {
        var invoiceStatusDto = await _context.Invoices
            .Where(i => i.InvoiceId == invoiceId)
            .Join(_context.StatusItems,
                invoice => invoice.StatusId,
                status => status.StatusId,
                (invoice, status) => new InvoiceStatusDto
                {
                    InvoiceId = invoice.InvoiceId,
                    StatusId = invoice.StatusId,
                    StatusDescription = status.Description
                })
            .FirstOrDefaultAsync();

        return invoiceStatusDto;
    }

    public async Task<bool> IsInvoiceInForeignCurrency(string invoiceId)
    {
        try
        {
            // Retrieve invoice details from the database
            var invoice = await _context.Invoices.FindAsync(invoiceId);

            // Check if invoice is found, otherwise log error and return false
            if (invoice == null)
            {
                _logger.LogError($"Invoice with ID {invoiceId} not found.");
                return false;
            }

            // Determine if the invoice is a purchase or sales invoice
            bool isPurchaseInvoice = invoice.InvoiceTypeId == "PURCHASE_INVOICE";
            bool isSalesInvoice = invoice.InvoiceTypeId == "SALES_INVOICE";

            // Get the organizationPartyId based on the invoice type
            string? organizationPartyId = isPurchaseInvoice ? invoice.PartyId :
                isSalesInvoice ? invoice.PartyIdFrom : null;

            // If organizationPartyId is null, log the error and throw an exception
            if (organizationPartyId == null)
            {
                var errorMessage = $"Organization Party ID could not be determined for invoice ID {invoiceId}.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            // Retrieve party accounting preferences for the organization
            var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);
            if (partyAcctgPreference == null)
            {
                var errorMessage =
                    $"Party Accounting Preferences not found for Organization Party ID {organizationPartyId}.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            // Determine if the invoice is in a foreign currency by comparing the currency UOM
            var baseCurrencyUomId = partyAcctgPreference.BaseCurrencyUomId;
            var isForeign = invoice.CurrencyUomId != baseCurrencyUomId;

            return isForeign;
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, $"Error checking if invoice with ID {invoiceId} is in a foreign currency.");

            // Rethrow or handle the exception (returning false in this case)
            return false;
        }
    }


    public async Task<decimal> GetInvoiceApplied(string invoiceId, DateTime asOfDateTime, bool actualCurrency)
    {
        decimal invoiceApplied = 0;

        try
        {
            // Query payment applications from the database
            var paymentApplications = await _context.PaymentApplications
                .Where(pa =>
                    pa.InvoiceId == invoiceId &&
                    (pa.Payment!.EffectiveDate == null || pa.Payment!.EffectiveDate <= asOfDateTime))
                .OrderBy(pa => pa.Payment!.EffectiveDate)
                .ToListAsync();

            // Calculate invoice applied amount
            invoiceApplied = (decimal)paymentApplications.Sum(pa => pa.AmountApplied);
        }
        catch (Exception ex)
        {
            // Log the error using the logger service
            _logger.LogError(ex, $"Error retrieving payment applications for invoice {invoiceId}.");

            // Re-throw the exception or handle it depending on your requirements
            throw new Exception("Error retrieving payment applications.", ex);
        }

        try
        {
            // Perform currency conversion if needed
            if (invoiceApplied != 0 && !actualCurrency)
            {
                // Get the invoice entity based on the invoiceId
                var invoice = await _context.Invoices.SingleOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    var errorMessage = $"Invoice with ID {invoiceId} not found for currency conversion.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                // Perform currency conversion using a utility function
                invoiceApplied *= await GetInvoiceCurrencyConversionRate(invoice);
            }
        }
        catch (Exception ex)
        {
            // Log any issues during the currency conversion process
            _logger.LogError(ex, $"Error performing currency conversion for invoice {invoiceId}.");

            // Re-throw the exception or return the current applied amount without conversion
            throw new Exception("Error performing currency conversion.", ex);
        }

        return invoiceApplied;
    }


    public async Task<decimal> GetInvoiceTotal(string invoiceId, bool actualCurrency)
    {
        decimal invoiceTotal = 0;

        try
        {
            // Retrieve invoice items, excluding taxable items (similar to Ofbiz)
            var taxableItemTypeIds =
                await GetTaxableInvoiceItemTypeIds(); // Assume this retrieves the relevant taxable item types

            var invoiceItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(
                query => query.Where(ii =>
                    ii.InvoiceId == invoiceId && !taxableItemTypeIds.Contains(ii.InvoiceItemTypeId))
            );


            // Calculate total amount for each invoice item and add to invoice total
            if (invoiceItems != null)
            {
                foreach (var invoiceItem in invoiceItems)
                {
                    invoiceTotal += GetInvoiceItemTotal(invoiceItem);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error if invoice items retrieval fails
            _logger.LogError(ex, $"Error retrieving invoice items for invoice {invoiceId}.");
            throw new Exception("Error retrieving invoice items.", ex);
        }

        try
        {
            // Calculate and add the tax total, similar to Ofbiz
            var invoiceTaxTotal = await GetInvoiceTaxTotal(invoiceId); // Assume this retrieves the tax total
            invoiceTotal += invoiceTaxTotal;

            // If invoice total is not zero and currency conversion is required
            if (invoiceTotal != 0 && !actualCurrency)
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);

                if (invoice == null)
                {
                    var errorMessage = $"Invoice with ID {invoiceId} not found for currency conversion.";
                    _logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

                // Apply currency conversion rate to the invoice total
                invoiceTotal *= await GetInvoiceCurrencyConversionRate(invoice);
            }
        }
        catch (Exception ex)
        {
            // Log error during currency conversion process
            _logger.LogError(ex, $"Error performing currency conversion for invoice {invoiceId}.");
            throw new Exception("Error performing currency conversion.", ex);
        }

        // Return the rounded invoice total
        return Math.Round(invoiceTotal, 2);
    }


    private async Task<decimal> GetInvoiceCurrencyConversionRate(Invoice invoice)
    {
        decimal? conversionRate = null;
        string otherCurrencyUomId = null;

        try
        {
            // Find the organization party currencyUomId which is different from the invoice currency
            var partyAcctgPreferences = await _context.PartyAcctgPreferences.FirstOrDefaultAsync(p =>
                (p.PartyId == invoice.PartyIdFrom && p.BaseCurrencyUomId != invoice.CurrencyUomId) ||
                (p.PartyId == invoice.PartyId && p.BaseCurrencyUomId != invoice.CurrencyUomId));

            // Set other currency UOM ID
            if (partyAcctgPreferences != null && !string.IsNullOrEmpty(partyAcctgPreferences.BaseCurrencyUomId))
            {
                otherCurrencyUomId = partyAcctgPreferences.BaseCurrencyUomId;
            }
            else
            {
                otherCurrencyUomId = "USD"; // Default to USD if no base currency is found
            }

            // If the invoice's currency matches the base currency, return 1 (no conversion needed)
            if (invoice.CurrencyUomId == otherCurrencyUomId)
            {
                return 1;
            }

            // Check if the invoice is posted and get the conversion from there
            var acctgTransEntry = await _context.AcctgTransEntries
                .Include(a => a.AcctgTrans)
                .FirstOrDefaultAsync(a => a.AcctgTrans.InvoiceId == invoice.InvoiceId);

            if (acctgTransEntry != null && acctgTransEntry.OrigAmount > 0)
            {
                conversionRate = decimal.Round((decimal)(acctgTransEntry.Amount / acctgTransEntry.OrigAmount), 2);
            }

            // Check if a payment is applied and use the currency conversion from there
            if (conversionRate == null)
            {
                var paymentAppls = await _context.PaymentApplications
                    .Include(p => p.Payment)
                    .Where(p => p.InvoiceId == invoice.InvoiceId)
                    .ToListAsync();

                foreach (var paymentAppl in paymentAppls)
                {
                    if (paymentAppl.Payment.ActualCurrencyAmount != null)
                    {
                        if (conversionRate == null)
                        {
                            conversionRate = decimal.Round(
                                (decimal)(paymentAppl.Payment.Amount / paymentAppl.Payment.ActualCurrencyAmount), 2);
                        }
                        else
                        {
                            conversionRate = decimal.Round(
                                (decimal)((conversionRate + (paymentAppl.Payment.Amount /
                                                             paymentAppl.Payment.ActualCurrencyAmount)) / 2), 2);
                        }
                    }
                }
            }

            // Use the dated conversion entity if no other conversion rate was found
            if (conversionRate == null)
            {
                var rate = await _context.UomConversionDateds
                    .FirstOrDefaultAsync(u => (u.UomIdTo == invoice.CurrencyUomId &&
                                               u.UomId == otherCurrencyUomId &&
                                               invoice.InvoiceDate >= u.FromDate &&
                                               invoice.InvoiceDate <= u.ThruDate) ||
                                              (invoice.InvoiceDate >= u.FromDate && u.ThruDate == null));

                if (rate != null)
                {
                    conversionRate = decimal.Round((decimal)(1 / rate.ConversionFactor), 2);
                }
            }

            // Return the conversion rate or 1 if no rate was found
            return conversionRate ?? 1;
        }
        catch (Exception ex)
        {
            // Log the error and rethrow the exception
            _logger.LogError(ex, $"Error calculating currency conversion rate for invoice {invoice.InvoiceId}");
            throw new Exception($"Error calculating currency conversion rate for invoice {invoice.InvoiceId}", ex);
        }
    }

    public decimal GetInvoiceItemTotal(InvoiceItem invoiceItem)
    {
        try
        {
            // Retrieve quantity and amount from the invoice item
            var quantity = invoiceItem.Quantity ?? 1; // Default to 1 if quantity is null
            var amount = invoiceItem.Amount ?? 0; // Default to 0 if amount is null

            // Calculate the total
            var total = quantity * amount;

            // Round to 2 decimal places
            return decimal.Round(total, 2);
        }
        catch (Exception ex)
        {
            // Log the error and rethrow the exception
            _logger.LogError(ex, $"Error calculating total for invoice item {invoiceItem.InvoiceItemSeqId}");
            throw new Exception($"Error calculating total for invoice item {invoiceItem.InvoiceItemSeqId}", ex);
        }
    }


    public async Task<List<string>> GetTaxableInvoiceItemTypeIds()
    {
        try
        {
            // Query the Enumeration table for taxable invoice item types
            var invoiceItemTaxTypes = await _context.Enumerations
                .Where(e => e.EnumTypeId == "TAXABLE_INV_ITM_TY")
                .ToListAsync();

            // Extract the EnumId values and return them as a list of strings
            var typeIds = invoiceItemTaxTypes.Select(e => e.EnumId).ToList();

            return typeIds;
        }
        catch (Exception ex)
        {
            // Log error if the query fails
            _logger.LogError(ex, "Error retrieving taxable invoice item type IDs.");

            // Re-throw the exception or handle it as necessary
            throw new Exception("Error retrieving taxable invoice item type IDs.", ex);
        }
    }

    public async Task<decimal> GetInvoiceTaxTotal(string invoiceId)
    {
        decimal taxTotal = 0;

        try
        {
            // Retrieve the invoice based on the invoiceId
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null)
            {
                throw new Exception($"Invoice with ID {invoiceId} not found.");
            }

            // Retrieve tax authorities and geographical areas related to the invoice
            var taxAuthPartyAndGeos = await GetInvoiceTaxAuthPartyAndGeos(invoiceId);

            // Iterate through each tax authority and geo set
            foreach (var taxAuthPartyGeos in taxAuthPartyAndGeos)
            {
                var taxAuthPartyId = taxAuthPartyGeos.Key;
                foreach (var taxAuthGeoId in taxAuthPartyGeos.Value)
                {
                    // Calculate the tax total for the tax authority and geo
                    taxTotal += await GetInvoiceTaxTotalForTaxAuthPartyAndGeo(invoice.InvoiceId, taxAuthPartyId,
                        taxAuthGeoId);
                }
            }

            // Add any unattributed tax total
            taxTotal += await GetInvoiceUnattributedTaxTotal(invoice.InvoiceId);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, $"Error calculating tax total for invoice {invoiceId}.");

            // Re-throw the exception or handle it as necessary
            throw new Exception($"Error calculating tax total for invoice {invoiceId}.", ex);
        }

        // Return the total tax amount
        return Math.Round(taxTotal, 2);
    }

    public async Task<Dictionary<string, HashSet<string>>> GetInvoiceTaxAuthPartyAndGeos(string invoiceId)
    {
        // Initialize the result dictionary where the key is taxAuthPartyId and the value is a set of taxAuthGeoIds
        var result = new Dictionary<string, HashSet<string>>();

        if (string.IsNullOrEmpty(invoiceId))
        {
            throw new ArgumentException("Invoice ID cannot be null or empty.");
        }

        try
        {
            // Retrieve taxable invoice items from the database
            var taxableItemTypeIds =
                await GetTaxableInvoiceItemTypeIds(); // Assume this retrieves the taxable item types
            /*var invoiceTaxItems = await _context.InvoiceItems
                .Where(ii => ii.InvoiceId == invoiceId && taxableItemTypeIds.Contains(ii.InvoiceItemTypeId))
                .ToListAsync();
                */

            var invoiceTaxItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(
                query => query.Where(ii =>
                    ii.InvoiceId == invoiceId && taxableItemTypeIds.Contains(ii.InvoiceItemTypeId))
            );
            // Process each invoice item to build the taxAuthPartyId -> taxAuthGeoId mapping
            foreach (var invoiceItem in invoiceTaxItems)
            {
                var taxAuthPartyId = invoiceItem.TaxAuthPartyId;
                var taxAuthGeoId = invoiceItem.TaxAuthGeoId;

                if (!string.IsNullOrEmpty(taxAuthPartyId))
                {
                    if (!result.ContainsKey(taxAuthPartyId))
                    {
                        result[taxAuthPartyId] = new HashSet<string> { taxAuthGeoId };
                    }
                    else
                    {
                        result[taxAuthPartyId].Add(taxAuthGeoId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error and re-throw the exception
            _logger.LogError(ex, $"Error retrieving tax authorization details for invoice {invoiceId}.");
            throw new Exception($"Error retrieving tax authorization details for invoice {invoiceId}.", ex);
        }

        return result;
    }

    public async Task<decimal> GetInvoiceTaxTotalForTaxAuthPartyAndGeo(string invoiceId, string taxAuthPartyId,
        string taxAuthGeoId)
    {
        decimal taxTotal = 0;

        try
        {
            // Retrieve taxable invoice items for the given tax authority and geo
            var taxableItemTypeIds =
                await GetTaxableInvoiceItemTypeIds(); // Assume this retrieves the taxable item types

            var invoiceTaxItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(
                query => query.Where(ii => ii.InvoiceId == invoiceId
                                           && taxableItemTypeIds.Contains(ii.InvoiceItemTypeId)
                                           && ii.TaxAuthPartyId == taxAuthPartyId
                                           && ii.TaxAuthGeoId == taxAuthGeoId)
            );

            // Calculate the total tax for the filtered invoice items
            taxTotal = GetTaxTotalForInvoiceItems(invoiceTaxItems);
        }
        catch (Exception ex)
        {
            // Log error and return zero or handle the error as needed
            _logger.LogError(ex,
                $"Error retrieving tax items for invoice {invoiceId}, tax authority {taxAuthPartyId}, and geo {taxAuthGeoId}.");
            throw new Exception(
                $"Error retrieving tax items for invoice {invoiceId}, tax authority {taxAuthPartyId}, and geo {taxAuthGeoId}.",
                ex);
        }

        return taxTotal;
    }

    private decimal GetTaxTotalForInvoiceItems(List<InvoiceItem> taxInvoiceItems)
    {
        int taxDecimals = 2;
        MidpointRounding taxRounding = MidpointRounding.ToEven;
        if (taxInvoiceItems == null || !taxInvoiceItems.Any())
        {
            return 0;
        }

        decimal taxTotal = 0;

        foreach (var taxInvoiceItem in taxInvoiceItems)
        {
            // Retrieve the amount and quantity for the invoice item
            var amount = taxInvoiceItem.Amount ?? 0; // Assuming Amount is a nullable decimal field
            var quantity = taxInvoiceItem.Quantity ?? 1; // Assuming Quantity is a nullable decimal field

            // Multiply amount by quantity
            var itemTotal = amount * quantity;

            // Scale the item total (taxDecimals and taxRounding need to be defined)
            itemTotal = Math.Round(itemTotal, taxDecimals, taxRounding);

            // Add the item total to the tax total
            taxTotal += itemTotal;
        }

        // Return the total tax, rounded to the desired decimals (decimals and rounding need to be defined)
        return Math.Round(taxTotal, taxDecimals, taxRounding);
    }

    /// <summary>
    /// Returns the invoice tax total for unattributed tax items, i.e., items that have no TaxAuthPartyId value.
    /// </summary>
    /// <param name="invoiceId">ID of the invoice</param>
    /// <returns>Returns the invoice tax total for unattributed tax items</returns>
    public async Task<decimal> GetInvoiceUnattributedTaxTotal(string invoiceId)
    {
        decimal taxTotal = 0;

        try
        {
            // Retrieve taxable invoice items that have no TaxAuthPartyId (unattributed tax items)
            var taxableItemTypeIds =
                await GetTaxableInvoiceItemTypeIds(); // Assume this retrieves the taxable item types

            var invoiceTaxItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(
                query => query.Where(ii => ii.InvoiceId == invoiceId
                                           && taxableItemTypeIds.Contains(ii.InvoiceItemTypeId)
                                           && ii.TaxAuthPartyId == null)
            );

            // Calculate the total tax for the unattributed invoice items
            taxTotal = GetTaxTotalForInvoiceItems(invoiceTaxItems);
        }
        catch (Exception ex)
        {
            // Log the error and re-throw the exception or return zero
            _logger.LogError(ex, $"Error retrieving unattributed tax items for invoice {invoiceId}.");
            throw new Exception($"Error retrieving unattributed tax items for invoice {invoiceId}.", ex);
        }

        return taxTotal;
    }

    public async Task<decimal> GetInvoiceNotApplied(string invoiceId)
    {
        try
        {
            // Call the methods to calculate invoice total
            var invoiceTotal = await GetInvoiceTotal(invoiceId, true);

            // Call the methods to calculate applied amount
            var invoiceApplied = await GetInvoiceApplied(invoiceId, DateTime.UtcNow, true);

            // Subtract applied amount from total to get not applied amount
            return invoiceTotal - invoiceApplied;
        }
        catch (Exception ex)
        {
            // Log error if an exception occurs
            _logger.LogError(ex, $"Error calculating not-applied amount for invoice {invoiceId}.");

            // Re-throw the exception or handle it as necessary
            throw new Exception($"Error calculating not-applied amount for invoice {invoiceId}.", ex);
        }
    }
}
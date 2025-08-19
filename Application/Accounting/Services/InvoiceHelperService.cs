using System.Globalization;
using Application.Accounting.Payments;
using Application.Accounting.Services.Models;
using Application.Catalog.ProductStores;
using Application.Core;
using Application.Interfaces;
using Application.Order.Orders;
using Application.Shipments.Invoices;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IInvoiceHelperService
{
    Task<string> CreateInvoiceFromOrder(string orderId);
    Task<InvoiceDto3> CreateInvoice(InvoiceDto3 invoice);
    Task<InvoiceDto3> UpdateInvoice(InvoiceDto3 invoice);
    Task<ServiceResult> CreateInvoiceItem(InvoiceItemParameters parameters);


    Task<CreateInvoicesFromShipmentsResponse> CreateInvoicesFromShipments(
        List<string> shipmentIds,
        bool createSalesInvoicesForDropShipments = false,
        DateTime? eventDate = null);

    Task<SetInvoicesToReadyFromShipmentResult> SetInvoicesToReadyFromShipment(string shipmentId);
}

public class InvoiceHelperService : IInvoiceHelperService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly DataContext _context;
    private readonly IUtilityService _utilityService;
    private readonly IProductStoreService _productStoreService;
    private readonly IPaymentApplicationService _paymentApplicationService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly ILogger<InvoiceHelperService> _logger;
    private readonly IPriceService _priceService;
    private readonly IUserAccessor _userAccessor;
    private readonly IRepositoryService _repositoryService;


    private int _invoiceItemSeq;

    public InvoiceHelperService(DataContext context, IUtilityService utilityService,
        IInvoiceUtilityService invoiceUtilityService,
        IAcctgMiscService acctgMiscService,
        IProductStoreService productStoreService,
        IPaymentApplicationService paymentService,
        ILogger<InvoiceHelperService> logger, IPriceService priceService, IUserAccessor userAccessor,
        IRepositoryService repositoryService)
    {
        _context = context;
        _utilityService = utilityService;
        _acctgMiscService = acctgMiscService;
        _productStoreService = productStoreService;
        _paymentApplicationService = paymentService;
        _invoiceUtilityService = invoiceUtilityService;
        _priceService = priceService;
        _userAccessor = userAccessor;
        _repositoryService = repositoryService;
        _logger = logger;
    }

    public async Task<InvoiceDto3> UpdateInvoice(InvoiceDto3 invoice)
    {
        // Find the current record
        var lookedUpValue = await _context.Invoices.SingleOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

        if (lookedUpValue == null)
        {
            throw new Exception("Invoice not found"); // Handle case where invoice is not found
        }
        
        if (lookedUpValue.StatusId != "INVOICE_IN_PROCESS")
        {
            throw new InvalidOperationException("AccountingInvoiceUpdateOnlyWithInProcessStatus");
        }
        
        var originalInvoice = new Invoice
        {
            Description = lookedUpValue.Description,
            InvoiceMessage = lookedUpValue.InvoiceMessage,
            BillingAccountId = lookedUpValue.BillingAccountId,
            CurrencyUomId = lookedUpValue.CurrencyUomId,
            DueDate = lookedUpValue.DueDate,
            PartyId = lookedUpValue.PartyId,
            PartyIdFrom = lookedUpValue.PartyIdFrom,
            RoleTypeId = lookedUpValue.RoleTypeId,
            ContactMechId = lookedUpValue.ContactMechId,
            ReferenceNumber = lookedUpValue.ReferenceNumber,
            RecurrenceInfoId = lookedUpValue.RecurrenceInfoId
        };

        lookedUpValue.Description = invoice.Description;
        lookedUpValue.InvoiceMessage = invoice.InvoiceMessage;
        lookedUpValue.BillingAccountId = invoice.BillingAccountId;
        lookedUpValue.CurrencyUomId = invoice.CurrencyUomId;
        lookedUpValue.DueDate = invoice.DueDate;
        lookedUpValue.PartyId = invoice.PartyId;
        lookedUpValue.PartyIdFrom = invoice.PartyIdFrom;
        lookedUpValue.RoleTypeId = invoice.RoleTypeId;
        lookedUpValue.ContactMechId = invoice.ContactMechId;
        lookedUpValue.ReferenceNumber = invoice.ReferenceNumber;
        lookedUpValue.LastUpdatedStamp = DateTime.UtcNow;


        // Handle status change if a new status is provided
        if (!string.IsNullOrEmpty(invoice.StatusId) && invoice.StatusId != "INVOICE_IN_PROCESS")
        {
            // REFACTOR: Use SetInvoiceStatus and pass additional parameters.
            await _invoiceUtilityService.SetInvoiceStatus(
                invoice.InvoiceId, 
                invoice.StatusId, 
                DateTime.UtcNow);
        }
        
        return new InvoiceDto3
        {
            InvoiceId = lookedUpValue.InvoiceId,
            Description = lookedUpValue.Description,
            InvoiceMessage = lookedUpValue.InvoiceMessage,
            BillingAccountId = lookedUpValue.BillingAccountId,
            CurrencyUomId = lookedUpValue.CurrencyUomId,
            DueDate = lookedUpValue.DueDate,
            StatusId = lookedUpValue.StatusId,
            PartyId = lookedUpValue.PartyId,
            PartyIdFrom = lookedUpValue.PartyIdFrom,
            RoleTypeId = lookedUpValue.RoleTypeId,
            ContactMechId = lookedUpValue.ContactMechId,
            ReferenceNumber = lookedUpValue.ReferenceNumber
        };
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

    private async Task<InvoiceItemTypeMap> GetInvoiceItemTypeForSalesOrderAdjustment(string orderAdjustmentTypeId)
    {
        // get invoice item type id from invoice item type map based on order adjustment type
        // and invoice type
        var invoiceItemTypeMap = await _context.InvoiceItemTypeMaps.SingleOrDefaultAsync(x =>
            x.InvoiceItemMapKey == orderAdjustmentTypeId
            && x.InvoiceTypeId == "SALES_INVOICE");

        return invoiceItemTypeMap;
    }

    public async Task<ServiceResult> CreateInvoiceItem(InvoiceItemParameters parameters)
    {
        // 2. Create the InvoiceItem entity and set primary key fields
        var newInvoiceItem = new InvoiceItem
        {
            InvoiceId = parameters.InvoiceId,
            InvoiceItemSeqId = parameters.InvoiceItemSeqId,
            InvoiceItemTypeId = parameters.InvoiceItemTypeId,
            Description = parameters.Description,
            Amount = parameters.Amount,
            ProductId = parameters.ProductId,
            Quantity = parameters.Quantity ?? 1.0M,
            CreatedStamp = DateTime.UtcNow,
            LastUpdatedStamp = DateTime.UtcNow
        };

        // If InvoiceItemSeqId is not provided, determine it based on existing items in the DB.
        if (string.IsNullOrWhiteSpace(parameters.InvoiceItemSeqId))
        {
            var existingItems = await _context.InvoiceItems
                .Where(x => x.InvoiceId == parameters.InvoiceId)
                .ToListAsync();

            if (existingItems.Any())
            {
                // Get the maximum existing sequence (assuming InvoiceItemSeqId is a numeric string).
                int maxSeq = existingItems.Max(x => int.Parse(x.InvoiceItemSeqId));
                int newSeq = maxSeq + 1;
                newInvoiceItem.InvoiceItemSeqId = newSeq.ToString("D2"); // Format as two-digit, e.g., "02"
            }
            else
            {
                newInvoiceItem.InvoiceItemSeqId = "01";
            }
        }

        // 3. If amount is empty, fetch the product price
        if (!newInvoiceItem.Amount.HasValue && !string.IsNullOrEmpty(newInvoiceItem.ProductId))
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == newInvoiceItem.ProductId);
            if (product != null)
            {
                newInvoiceItem.Description = product.Description;

                var userLogin = await _context.Users.FirstOrDefaultAsync(x =>
                    x.UserName == _userAccessor.GetUsername());


                // Assuming you have a service to calculate the product price
                var priceResult =
                    await _priceService.CalculateProductPrice(product, null, null, null, userLogin.ProductStoreId);
                newInvoiceItem.Amount = priceResult.Price;
            }
        }

        // 4. If amount is still empty, return an error
        if (!newInvoiceItem.Amount.HasValue)
        {
            return new ServiceResult
            {
                IsError = true,
                ErrorMessage = "Invoice amount is mandatory."
            };
        }

        // 5. Add the new InvoiceItem to the database
        try
        {
            _context.InvoiceItems.Add(newInvoiceItem);
            return new ServiceResult { IsError = false, Data = newInvoiceItem };
        }
        catch (Exception ex)
        {
            return new ServiceResult
            {
                IsError = true,
                ErrorMessage = $"Error creating invoice item: {ex.Message}"
            };
        }
    }

    public async Task<decimal> CalculateInvoicedAdjustmentTotal(OrderAdjustment orderAdjustment)
    {
        decimal invoicedTotal = 0m;

        try
        {
            // Fetch the list of related OrderAdjustmentBilling records for the given orderAdjustmentId
            var invoicedAdjustments = await _context.OrderAdjustmentBillings
                .Where(b => b.OrderAdjustmentId == orderAdjustment.OrderAdjustmentId)
                .ToListAsync();

            // Sum the amount from the invoiced adjustments
            foreach (var invoicedAdjustment in invoicedAdjustments)
            {
                invoicedTotal += invoicedAdjustment.Amount.HasValue
                    ? invoicedAdjustment.Amount.Value
                    : 0m;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating invoiced adjustment total: {ex.Message}");
            throw; // Re-throw or handle as needed
        }

        // Return the calculated total
        return invoicedTotal;
    }

    public async Task CreateOrderAdjustmentBilling(OrderAdjustmentBilling adjustmentBilling)
    {
        try
        {
            // Add the new OrderAdjustmentBilling entity to the context
            _context.OrderAdjustmentBillings.Add(adjustmentBilling);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating OrderAdjustmentBilling: {ex.Message}");
            throw; // Re-throw the exception or handle as needed
        }
    }

    private async Task<string> GetInvoiceItemType(string key1, string key2, string invoiceTypeId, string defaultValue)
    {
        InvoiceItemTypeMap itemMap = null;

        try
        {
            // First, try to find the invoice item type map using key1
            if (!string.IsNullOrEmpty(key1))
            {
                itemMap = await _context.InvoiceItemTypeMaps
                    .FirstOrDefaultAsync(map => map.InvoiceItemMapKey == key1 && map.InvoiceTypeId == invoiceTypeId);
            }

            // If no map is found, try to find the map using key2
            if (itemMap == null && !string.IsNullOrEmpty(key2))
            {
                itemMap = await _context.InvoiceItemTypeMaps
                    .FirstOrDefaultAsync(map => map.InvoiceItemMapKey == key2 && map.InvoiceTypeId == invoiceTypeId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving InvoiceItemTypeMap: {ex.Message}");
            return defaultValue; // If an error occurs, return the default value
        }

        // If a matching map is found, return its InvoiceItemTypeId, otherwise return the default value
        return itemMap != null ? itemMap.InvoiceItemTypeId : defaultValue;
    }

    private async Task<decimal> CalcHeaderAdj(
        string invoiceTypeId,
        string invoiceId,
        string invoiceItemSeqId,
        decimal divisor,
        decimal multiplier,
        decimal baseAmount,
        int decimals,
        MidpointRounding rounding,
        OrderAdjustment adj)
    {
        decimal adjAmount = 0m;

        try
        {
            if (adj.Amount != null)
            {
                // Pro-rate the amount
                decimal amount = 0m;
                if (adj.OrderAdjustmentTypeId == "DONATION_ADJUSTMENT")
                {
                    amount = baseAmount;
                }
                else if (divisor != 0) // Ensure divisor is not zero
                {
                    // Multiply first, then divide to avoid rounding errors
                    amount = baseAmount; //* multiplier / divisor;
                }

                if (amount != 0)
                {
                    amount = decimal.Round(amount, decimals, rounding);

                    var createInvoiceItemContext = new InvoiceItemParameters
                    {
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        InvoiceItemTypeId = await GetInvoiceItemType(adj.OrderAdjustmentTypeId, null, invoiceTypeId,
                            "INVOICE_ADJ"),
                        Description = adj.Description,
                        Quantity = 1, // Adjustments usually have a quantity of 1
                        Amount = amount,
                        OverrideGlAccountId = adj.OverrideGlAccountId,
                        TaxAuthPartyId = adj.TaxAuthPartyId,
                        TaxAuthGeoId = adj.TaxAuthGeoId,
                        TaxAuthorityRateSeqId = adj.TaxAuthorityRateSeqId,
                    };

                    // Call the service to create invoice item
                    var createInvoiceItemResult = await CreateInvoiceItem(createInvoiceItemContext);
                    if (createInvoiceItemResult.IsError)
                    {
                        return adjAmount;
                    }

                    // Create the OrderAdjustmentBilling record
                    var createOrderAdjustmentBillingContext = new OrderAdjustmentBilling
                    {
                        OrderAdjustmentId = adj.OrderAdjustmentId,
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        Amount = amount,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    };

                    await CreateOrderAdjustmentBilling(createOrderAdjustmentBillingContext);

                    adjAmount = amount;
                }
            }
            else //if (adj.SourcePercentage != null)
            {
                // Pro-rate the amount based on the percentage
                decimal percent = decimal.Round(adj.SourcePercentage.Value / 100m, 100, rounding);
                decimal amount = 0m;


                // Ensure the divisor is not zero
                if (divisor != 0)
                {
                    amount = percent * divisor;
                }

                if (amount != 0)
                {
                    amount = decimal.Round(amount, decimals, rounding);

                    var createInvoiceItemContext = new InvoiceItemParameters
                    {
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        InvoiceItemTypeId = await GetInvoiceItemType(adj.OrderAdjustmentTypeId, null, invoiceTypeId,
                            "INVOICE_ADJ"),
                        Description = adj.Description,
                        Quantity = 1,
                        Amount = amount,
                        OverrideGlAccountId = adj.OverrideGlAccountId,
                        TaxAuthPartyId = adj.TaxAuthPartyId,
                        TaxAuthGeoId = adj.TaxAuthGeoId,
                        TaxAuthorityRateSeqId = adj.TaxAuthorityRateSeqId,
                    };

                    // Call the service to create invoice item
                    var createInvoiceItemResult = await CreateInvoiceItem(createInvoiceItemContext);
                    if (createInvoiceItemResult.IsError)
                    {
                        return adjAmount;
                    }

                    // Create the OrderAdjustmentBilling record
                    var createOrderAdjustmentBillingContext = new OrderAdjustmentBilling
                    {
                        OrderAdjustmentId = adj.OrderAdjustmentId,
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        Amount = amount,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    };

                    await CreateOrderAdjustmentBilling(createOrderAdjustmentBillingContext);

                    adjAmount = amount;
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception (you can replace Console.WriteLine with your logging mechanism)
            Console.WriteLine($"An error occurred in CalcHeaderAdj: {ex.Message}");
            // Optionally, rethrow or handle the exception as necessary
            throw; // Rethrowing for higher-level handling, if needed
        }

        return adjAmount;
    }

    public async Task<InvoiceDto3> CreateInvoice(InvoiceDto3 invoice)
    {
        var stamp = DateTime.UtcNow;

        var partyId = "";
        var organizationPartyId = "";

        // Determine which party to use based on invoice type.
        if (new List<string>
                { "PURCHASE_INVOICE", "PAYROL_INVOICE", "COMMISSION_INVOICE", "CUST_RTN_INVOICE" }
            .Contains(invoice.InvoiceTypeId))
        {
            partyId = invoice.PartyIdFrom;
            organizationPartyId = invoice.PartyId;
        }
        else
        {
            partyId = invoice.PartyId;
            organizationPartyId = invoice.PartyIdFrom;
        }

        // Fetch the Party entity based on partyId.
        var party = await _context.Parties.FindAsync(partyId);

        // Start with the currency from the parameter if available.
        var currencyUomId = invoice.CurrencyUomId;

        // Override with party preference if one exists.
        if (party != null && !string.IsNullOrEmpty(party.PreferredCurrencyUomId))
        {
            currencyUomId = party.PreferredCurrencyUomId;
        }

        // If currencyUomId is still empty, get the default currency from the system.
        if (string.IsNullOrEmpty(currencyUomId))
        {
            var result = await _productStoreService.GetAcctgBaseCurrencyId();
            currencyUomId = result.CurrencyUomId;
        }

        // Get party accounting preferences.
        var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);

        // Get next invoice number.
        var newInvoiceSequence = _invoiceUtilityService.GetNextInvoiceNumber(partyAcctgPreference);

        // Create invoice.
        var newInvoice = new Invoice
        {
            InvoiceId = partyAcctgPreference.InvoiceIdPrefix + newInvoiceSequence,
            InvoiceTypeId = invoice.InvoiceTypeId,
            PartyIdFrom = invoice.PartyIdFrom,
            PartyId = invoice.PartyId,
            RoleTypeId = null,
            StatusId = "INVOICE_IN_PROCESS",
            BillingAccountId = invoice.BillingAccountId,
            ContactMechId = null,
            InvoiceDate = stamp,
            InvoiceMessage = null,
            ReferenceNumber = null,
            Description = null,
            CurrencyUomId = currencyUomId,
            RecurrenceInfoId = null,
            LastUpdatedStamp = stamp,
            CreatedStamp = stamp
        };
        _context.Invoices.Add(newInvoice);

        // Map new invoice to InvoiceDto3.
        var newInvoiceDto = new InvoiceDto3
        {
            InvoiceId = newInvoice.InvoiceId,
            InvoiceTypeId = newInvoice.InvoiceTypeId,
            PartyIdFrom = newInvoice.PartyIdFrom,
            PartyId = newInvoice.PartyId,
            RoleTypeId = newInvoice.RoleTypeId,
            StatusId = newInvoice.StatusId,
            BillingAccountId = newInvoice.BillingAccountId,
            ContactMechId = newInvoice.ContactMechId,
            InvoiceDate = newInvoice.InvoiceDate,
            DueDate = newInvoice.DueDate,
            PaidDate = newInvoice.PaidDate,
            InvoiceMessage = newInvoice.InvoiceMessage,
            ReferenceNumber = newInvoice.ReferenceNumber,
            Description = newInvoice.Description,
            CurrencyUomId = newInvoice.CurrencyUomId
        };

        // Create invoice status.
        _utilityService.CreateInvoiceStatus(newInvoice.InvoiceId, "INVOICE_IN_PROCESS");

        return newInvoiceDto;
    }


    private async Task<InvoiceResponse> CreateInvoiceForOrder(string orderId, IEnumerable<object> billItems,
        DateTime? eventDate)
    {
        try
        {
            string invoiceId = string.Empty;
            // Validate if billItems are not empty
            if (billItems == null || !billItems.Any())
            {
                _logger.LogInformation("No order items to invoice; not creating invoice; returning success");
                return new InvoiceResponse
                {
                    Success = true,
                    Message = "No order items to invoice."
                };
            }

            // Retrieve the order header using LINQ
            var orderHeader = await _context.OrderHeaders.FindAsync(orderId);
            if (orderHeader == null)
            {
                return new InvoiceResponse
                {
                    Success = false,
                    Message = "Order header not found."
                };
            }

            // Determine the invoice type based on order type
            string invoiceType = null;
            if (orderHeader.OrderTypeId == "SALES_ORDER")
            {
                invoiceType = "SALES_INVOICE";
            }
            else if (orderHeader.OrderTypeId == "PURCHASE_ORDER")
            {
                invoiceType = "PURCHASE_INVOICE";
            }

            // Set the precision depending on the type of invoice
            int invoiceTypeDecimals = 2;

            // Instantiate OrderReadHelper class
            var orh = new OrderReadHelper(orderId)
            {
                Context = _context
            };
            orh.InitializeOrder();

            // Get the associated product store for the order
            var productStore = await _productStoreService.GetProductStoreForLoggedInUser();

            // Determine the shipping adjustment mode (Y = Pro-Rate; N = First-Invoice)
            string prorateShipping = productStore?.ProrateShipping ?? "Y";

            // Get billing parties from the order
            string billToCustomerPartyId = (await orh.GetBillToParty())?.PartyId;
            string billFromVendorPartyId = (await orh.GetBillFromParty())?.PartyId;

            // Get pricing details from the order
            var shippableAmount = await orh.GetShippableTotal();
            var shippableQuantity = await orh.GetShippableQuantity();
            var orderSubTotal = await orh.GetOrderItemsSubTotal();
            var orderQuantity = await orh.GetTotalOrderItemsQuantity();

            // Initialize variables for prorating order amounts
            var invoiceShipProRateAmount = 0.0m;
            var invoiceShippableQuantity = 0.0m;
            var invoiceSubTotal = 0.0m;
            var invoiceQuantity = 0.0m;

            // Retrieve billing account associated with the order
            var billingAccount =
                await _context.BillingAccounts.FindAsync(orderHeader.BillingAccountId);
            string billingAccountId = billingAccount?.BillingAccountId;

            // Determine the invoice date
            var invoiceDate = eventDate ?? DateTime.UtcNow;

            // Set due date based on billing account net terms
            var orderTermNetDays = await orh.GetOrderTermNetDays();
            DateTime? dueDate = null;
            if (orderTermNetDays != null)
            {
                dueDate = invoiceDate.AddDays((double)orderTermNetDays);
            }

            // Create the invoice record if invoiceId is null or empty
            if (string.IsNullOrEmpty(invoiceId))
            {
                var invoiceDto = new InvoiceDto3
                {
                    PartyId = billToCustomerPartyId,
                    PartyIdFrom = billFromVendorPartyId,
                    BillingAccountId = billingAccountId,
                    InvoiceDate = invoiceDate,
                    DueDate = dueDate,
                    InvoiceTypeId = invoiceType,
                    StatusId = "INVOICE_IN_PROCESS",
                    CurrencyUomId = orderHeader.CurrencyUom,
                };

                // Call the CreateInvoice function
                var createdInvoice = await CreateInvoice(invoiceDto);
                invoiceId = createdInvoice.InvoiceId;
            }

            // Enhanced: Handle Billing Account Terms and Roles
            if (!string.IsNullOrEmpty(billingAccountId))
            {
                // Fetch billing account roles for the billing account (e.g., BILL_TO_CUSTOMER)
                var billingAccountRoles = await _context.BillingAccountRoles
                    .Where(bar => bar.BillingAccountId == billingAccountId && bar.RoleTypeId == "BILL_TO_CUSTOMER")
                    .ToListAsync();

                // Apply the billing account roles to the invoice if necessary
                foreach (var role in billingAccountRoles)
                {
                    if (role.PartyId != billToCustomerPartyId)
                    {
                        // Create an invoice role for the billing account party
                        var invoiceRole = new InvoiceRole
                        {
                            InvoiceId = invoiceId,
                            PartyId = role.PartyId,
                            RoleTypeId = "BILL_TO_CUSTOMER"
                        };
                        _context.InvoiceRoles.Add(invoiceRole);
                    }
                }

                // Check if billing account has a contact mechanism
                var billingAccountContactMechId = await _context.BillingAccounts
                    .Where(ba => ba.BillingAccountId == billingAccountId)
                    .Select(ba => ba.ContactMechId)
                    .FirstOrDefaultAsync();

                // If there's a billing contact mechanism, assign it to the invoice
                if (!string.IsNullOrEmpty(billingAccountContactMechId))
                {
                    var invoiceContactMech = new InvoiceContactMech
                    {
                        InvoiceId = invoiceId,
                        ContactMechId = billingAccountContactMechId,
                        ContactMechPurposeTypeId = "BILLING_LOCATION"
                    };
                    _context.InvoiceContactMeches.Add(invoiceContactMech);
                }

                // Copy billing account terms (e.g., payment terms) to the invoice
                var billingAccountTerms = await _context.BillingAccountTerms
                    .Where(bat => bat.BillingAccountId == billingAccountId)
                    .ToListAsync();

                var billingsAccountInvoiceTerms = new List<BillingAccountTerm>();
                foreach (var term in billingAccountTerms)
                {
                    var invoiceTerm = new BillingAccountTerm()
                    {
                        TermTypeId = term.TermTypeId,
                        TermValue = term.TermValue,
                        TermDays = term.TermDays,
                        UomId = term.UomId
                    };
                    billingsAccountInvoiceTerms.Add(invoiceTerm);
                }

                if (billingsAccountInvoiceTerms.Count > 0)
                {
                    CreateInvoiceTerms(invoiceId, billingsAccountInvoiceTerms);
                }
            }
            else
            {
                // No billing account associated, handle default billing logic
                var billingLocations = await orh.GetBillingLocations();
                if (billingLocations != null && billingLocations.Any())
                {
                    foreach (var location in billingLocations)
                    {
                        var invoiceContactMech = new InvoiceContactMech
                        {
                            InvoiceId = invoiceId,
                            ContactMechId = location.ContactMechId,
                            ContactMechPurposeTypeId = "BILLING_LOCATION"
                        };
                        _context.InvoiceContactMeches.Add(invoiceContactMech);
                    }
                }
            }


            // Create order roles to invoice roles
            var orderRoles = await _context.OrderRoles.Where(or => or.OrderId == orderId).ToListAsync();
            foreach (var orderRole in orderRoles)
            {
                _utilityService.CreateInvoiceRole(invoiceId, orderRole.RoleTypeId, orderRole.PartyId);
            }

            // Create order terms to invoice terms
            var orderTerms = await orh.GetOrderTerms();
            CreateInvoiceTerms(invoiceId, orderTerms);

            // Sequence for items - all OrderItems or InventoryReservations + all Adjustments
            int invoiceItemSeqNum = 1;
            string invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");

            // Process each billItem
            foreach (var billItem in billItems)
            {
                OrderItem orderItem = null;
                ItemIssuance itemIssuance = null;
                ShipmentReceipt shipmentReceipt = null;

                // Determine the type of billItem
                if (billItem is ItemIssuance ii)
                {
                    itemIssuance = ii;
                }
                else if (billItem is ShipmentReceipt sr)
                {
                    shipmentReceipt = sr;
                }
                else if (billItem is OrderItem oi)
                {
                    orderItem = oi;
                }
                else
                {
                    _logger.LogError("Unexpected bill item type: {Type}", billItem.GetType());
                    continue;
                }

                // Retrieve OrderItem if not already obtained
                if (orderItem == null)
                {
                    if (itemIssuance != null)
                    {
                        orderItem = await _context.OrderItems
                            .FirstOrDefaultAsync(oi =>
                                oi.OrderId == itemIssuance.OrderId && oi.OrderItemSeqId == itemIssuance.OrderItemSeqId);
                    }
                    else if (shipmentReceipt != null)
                    {
                        orderItem = await _context.OrderItems
                            .FirstOrDefaultAsync(oi =>
                                oi.OrderId == shipmentReceipt.OrderId &&
                                oi.OrderItemSeqId == shipmentReceipt.OrderItemSeqId);
                    }
                }

                if (orderItem == null)
                {
                    _logger.LogError("OrderItem not found for bill item");
                    continue;
                }

                // Retrieve Product if available
                Product product = null;
                if (!string.IsNullOrEmpty(orderItem.ProductId))
                {
                    product = await _context.Products.FindAsync(orderItem.ProductId);
                }

                // Calculate billing quantity
                decimal billingQuantity = 0.0m;
                if (itemIssuance != null)
                {
                    billingQuantity = (itemIssuance.Quantity ?? 0) - (itemIssuance.CancelQuantity ?? 0);
                }
                else if (shipmentReceipt != null)
                {
                    billingQuantity = shipmentReceipt.QuantityAccepted ?? 0;
                }
                else
                {
                    var orderedQuantity = orderItem.Quantity ?? 0;

                    // Calculate invoicedQuantity by summing quantities from OrderItemBilling records
                    var orderItemBillings = await _context.OrderItemBillings
                        .Where(oib =>
                            oib.OrderId == orderItem.OrderId && oib.OrderItemSeqId == orderItem.OrderItemSeqId)
                        .ToListAsync();

                    decimal invoicedQuantity = orderItemBillings.Sum(oib => oib.Quantity ?? 0);

                    billingQuantity = orderedQuantity - invoicedQuantity;
                    if (billingQuantity < 0)
                    {
                        billingQuantity = 0;
                    }
                }

                // Determine if shipping applies to this item
                bool shippingApplies = false;
                if (product != null && invoiceType == "SALES_INVOICE")
                {
                    shippingApplies = true; // Adjust based on your product's shipping applicability
                }

                // Calculate billing amount
                decimal billingAmount = orderItem.UnitPrice ?? 0;
                var orderAdj = (await _context.OrderAdjustments
                    .Where(oa =>
                        oa.OrderId == orderItem.OrderId && oa.OrderItemSeqId == orderItem.OrderItemSeqId &&
                        oa.OrderAdjustmentTypeId == "VAT_TAX")
                    .ToListAsync()).FirstOrDefault();
                if (orderAdj != null && orderAdj.Amount == 0 && orderAdj.AmountAlreadyIncluded != null &&
                    orderAdj.AmountAlreadyIncluded != 0)
                {
                    decimal sourcePercentageTotal = (orderAdj.SourcePercentage ?? 0) + 100;
                    billingAmount = (orderItem.UnitPrice ?? 0) / sourcePercentageTotal * 100;
                }

                billingAmount = decimal.Round(billingAmount, invoiceTypeDecimals, MidpointRounding.AwayFromZero);

                // Get InvoiceItemTypeId
                string invoiceItemTypeId = await GetInvoiceItemType(
                    orderItem.OrderItemTypeId, // key1: OrderItemTypeId
                    product?.ProductTypeId, // key2: ProductTypeId
                    invoiceType, // InvoiceTypeId
                    "INV_FPROD_ITEM" // Default value
                );

                // Create InvoiceItem
                var invoiceItem = new InvoiceItem
                {
                    InvoiceId = invoiceId,
                    InvoiceItemSeqId = invoiceItemSeqId,
                    InvoiceItemTypeId = invoiceItemTypeId,
                    Description = orderItem.ItemDescription,
                    Quantity = billingQuantity,
                    Amount = billingAmount,
                    ProductId = orderItem.ProductId,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };
                _context.InvoiceItems.Add(invoiceItem);

                // Update invoice totals
                var thisAmount = billingAmount * billingQuantity;
                invoiceSubTotal += thisAmount;
                invoiceQuantity += billingQuantity;

                if (shippingApplies)
                {
                    invoiceShipProRateAmount += thisAmount;
                    invoiceShippableQuantity += billingQuantity;
                }

                // Create OrderItemBilling
                var orderItemBilling = new OrderItemBilling
                {
                    InvoiceId = invoiceId,
                    InvoiceItemSeqId = invoiceItemSeqId,
                    OrderId = orderItem.OrderId,
                    OrderItemSeqId = orderItem.OrderItemSeqId,
                    Quantity = billingQuantity,
                    Amount = billingAmount,
                    // Include ItemIssuanceId or ShipmentReceiptId if applicable
                    ItemIssuanceId = itemIssuance?.ItemIssuanceId,
                    ShipmentReceiptId = shipmentReceipt?.ReceiptId,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };
                _context.OrderItemBillings.Add(orderItemBilling);

                // If ItemIssuance or ShipmentReceipt, create ShipmentItemBilling
                if (itemIssuance != null)
                {
                    var existingShipmentItemBilling = await _context.ShipmentItemBillings
                        .FirstOrDefaultAsync(sib => sib.ShipmentId == itemIssuance.ShipmentId
                                                    && sib.ShipmentItemSeqId == itemIssuance.ShipmentItemSeqId
                                                    && sib.InvoiceId == invoiceId
                                                    && sib.InvoiceItemSeqId == invoiceItemSeqId);

                    if (existingShipmentItemBilling == null)
                    {
                        var shipmentItemBilling = new ShipmentItemBilling
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = invoiceItemSeqId,
                            ShipmentId = itemIssuance.ShipmentId,
                            ShipmentItemSeqId = itemIssuance.ShipmentItemSeqId
                        };
                        _context.ShipmentItemBillings.Add(shipmentItemBilling);
                    }
                }

                // Increment the invoice item sequence number
                invoiceItemSeqNum++;
                invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");
            }

            // Process item adjustments for each order item
            foreach (var billItem in billItems)
            {
                OrderItem orderItem = null;
                if (billItem is ItemIssuance ii)
                {
                    orderItem = await _context.OrderItems
                        .FindAsync(ii.OrderId, ii.OrderItemSeqId);
                }
                else if (billItem is ShipmentReceipt sr)
                {
                    orderItem = await _context.OrderItems
                        .FindAsync(sr.OrderId, sr.OrderItemSeqId);
                }
                else if (billItem is OrderItem oi)
                {
                    orderItem = oi;
                }

                if (orderItem == null)
                {
                    continue;
                }

                // Retrieve item adjustments
                var itemAdjustments = await _context.OrderAdjustments
                    .Where(adj => adj.OrderId == orderItem.OrderId
                                  && adj.OrderItemSeqId == orderItem.OrderItemSeqId)
                    .ToListAsync();

                foreach (var adj in itemAdjustments)
                {
                    // Check if adjustment has already been fully invoiced
                    var adjAlreadyInvoicedAmount = await CalculateInvoicedAdjustmentTotal(adj);
                    var adjAmount = adj.Amount ?? 0;

                    if (Math.Abs(adjAlreadyInvoicedAmount) >= Math.Abs(adjAmount))
                    {
                        continue;
                    }

                    // Calculate the amount to invoice
                    decimal amountToInvoice = adjAmount - adjAlreadyInvoicedAmount;
                    amountToInvoice =
                        decimal.Round(amountToInvoice, invoiceTypeDecimals, MidpointRounding.AwayFromZero);

                    if (amountToInvoice != 0)
                    {
                        // Create InvoiceItem for adjustment
                        var invoiceItemTypeId = await GetInvoiceItemType(
                            adj.OrderAdjustmentTypeId, null, invoiceType, "INVOICE_ITM_ADJ");

                        var invoiceItem = new InvoiceItem
                        {
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = invoiceItemSeqId,
                            InvoiceItemTypeId = invoiceItemTypeId,
                            Description = adj.Description ?? adj.Comments,
                            TaxAuthGeoId = adj.TaxAuthGeoId,
                            TaxAuthPartyId = adj.TaxAuthPartyId,
                            OverrideGlAccountId = adj.OverrideGlAccountId,
                            Quantity = 1,
                            Amount = amountToInvoice,
                            ProductId = orderItem.ProductId,
                            ParentInvoiceId = invoiceId,
                            ParentInvoiceItemSeqId = (invoiceItemSeqNum - 1).ToString("D5"),
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };

                        // REFACTORED: Handle ProductPromo overrideOrgPartyId
                        if (!string.IsNullOrEmpty(adj.ProductPromoId))
                        {
                            var productPromo = await _context.ProductPromos.FindAsync(adj.ProductPromoId);
                            if (!string.IsNullOrEmpty(productPromo?.OverrideOrgPartyId))
                            {
                                invoiceItem.OverrideOrgPartyId = productPromo.OverrideOrgPartyId;
                            }
                        }

                        _context.InvoiceItems.Add(invoiceItem);

                        // Create OrderAdjustmentBilling
                        var orderAdjustmentBilling = new OrderAdjustmentBilling
                        {
                            OrderAdjustmentId = adj.OrderAdjustmentId,
                            InvoiceId = invoiceId,
                            InvoiceItemSeqId = invoiceItemSeqId,
                            Amount = amountToInvoice,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow,
                        };
                        _context.OrderAdjustmentBillings.Add(orderAdjustmentBilling);

                        // Update invoice totals if necessary
                        if (adj.OrderAdjustmentTypeId != "SALES_TAX" &&
                            adj.OrderAdjustmentTypeId != "SHIPPING_ADJUSTMENT")
                        {
                            invoiceSubTotal += amountToInvoice;

                            if ( /* shipping applies to this item */ true)
                            {
                                invoiceShipProRateAmount += amountToInvoice;
                            }
                        }

                        // Increment the invoice item sequence number
                        invoiceItemSeqNum++;
                        invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");
                    }
                }
            }

            // Create header adjustments as line items -- always to tax/shipping last
            var headerAdjustments = await orh.GetOrderHeaderAdjustments();

            // Prorate the adjustment based on the order subtotal or quantity
            decimal divisor = orderSubTotal;
            decimal multiplier = invoiceSubTotal;

            foreach (var adj in headerAdjustments)
            {
                // Check against OrderAdjustmentBilling to see how much of this adjustment has already been invoiced
                var adjAlreadyInvoicedAmount = await CalculateInvoicedAdjustmentTotal(adj);

                if (adjAlreadyInvoicedAmount >= Math.Abs(adj.Amount ?? 0))
                {
                    continue;
                }

                var adjustmentAmount = adj.Amount;
                if (adjustmentAmount != null && adjustmentAmount != 0)
                {
                    // Get invoice item type for the adjustment
                    var invoiceItemTypeMap = await GetInvoiceItemTypeForSalesOrderAdjustment(adj.OrderAdjustmentTypeId);

                    // Prorate the shipping adjustments if necessary
                    if (adj.OrderAdjustmentTypeId == "SHIPPING_CHARGES")
                    {
                        if (prorateShipping == "N")
                        {
                            // No prorating, apply full amount
                            divisor = 1.0m;
                            multiplier = 1.0m;
                        }
                        else
                        {
                            // Pro-rate the shipping amount based on shippable information
                            divisor = shippableAmount;
                            multiplier = invoiceShipProRateAmount;

                            if (multiplier == 0 && divisor == 0)
                            {
                                divisor = shippableQuantity;
                                multiplier = invoiceShippableQuantity;
                            }
                        }

                        decimal baseAmount = (decimal)(adjustmentAmount - adjAlreadyInvoicedAmount);

                        await CalcHeaderAdj(
                            invoiceType, // string invoiceTypeId
                            invoiceId, // string invoiceId
                            invoiceItemSeqId, // string invoiceItemSeqId
                            divisor, // decimal divisor
                            multiplier, // decimal multiplier
                            baseAmount, // decimal baseAmount
                            invoiceTypeDecimals, // int decimals
                            MidpointRounding.ToEven, // MidpointRounding rounding
                            adj // OrderAdjustment adj
                        );
                    }

                    // Handle tax adjustments
                    else if (adj.OrderAdjustmentTypeId == "SALES_TAX")
                    {
                        // Prorate the tax amount based on order subtotal
                        divisor = orderSubTotal;
                        multiplier = invoiceSubTotal;

                        if (multiplier == 0 && divisor == 0)
                        {
                            divisor = orderQuantity;
                            multiplier = invoiceQuantity;
                        }

                        decimal baseAmount = (decimal)(adjustmentAmount - adjAlreadyInvoicedAmount);
                        await CalcHeaderAdj(
                            invoiceType,
                            invoiceId,
                            invoiceItemSeqId,
                            divisor,
                            multiplier,
                            baseAmount,
                            2,
                            MidpointRounding.ToEven,
                            adj
                        );
                        /*// Increment the invoice item sequence number
                        invoiceItemSeqNum++;
                        invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");*/
                    }

                    // Handle other header adjustments
                    else
                    {
                        // Prorate the adjustment based on order subtotal or quantity
                        divisor = orderSubTotal;
                        multiplier = invoiceSubTotal;

                        if (multiplier == 0 && divisor == 0)
                        {
                            divisor = orderQuantity;
                            multiplier = invoiceQuantity;
                        }

                        decimal baseAmount = (decimal)(adjustmentAmount - adjAlreadyInvoicedAmount);
                        await CalcHeaderAdj(
                            invoiceType,
                            invoiceId,
                            invoiceItemSeqId,
                            divisor,
                            multiplier,
                            baseAmount,
                            invoiceTypeDecimals,
                            MidpointRounding.ToEven, // Pass a rounding mode here
                            adj
                        );
                        // Increment the invoice item sequence number
                        /*invoiceItemSeqNum++;
                        invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");*/
                    }

                    // Create the invoice item for this adjustment
                    /*var invoiceItem = new InvoiceItemParameters
                    {
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        InvoiceItemTypeId = invoiceItemTypeMap.InvoiceItemTypeId,
                        Description = adj.Description,
                        Quantity = 1, // Adjustments usually have a quantity of 1
                        Amount = adjustmentAmount - adjAlreadyInvoicedAmount,
                        TaxAuthPartyId = adj.TaxAuthPartyId,
                        TaxAuthGeoId = adj.TaxAuthGeoId,
                        TaxAuthorityRateSeqId = adj.TaxAuthorityRateSeqId
                    };

                    await CreateInvoiceItem(invoiceItem);

                    // Create the OrderAdjustmentBilling record
                    await CreateOrderAdjustmentBilling(new OrderAdjustmentBilling
                    {
                        OrderAdjustmentId = adj.OrderAdjustmentId,
                        InvoiceId = invoiceId,
                        InvoiceItemSeqId = invoiceItemSeqId,
                        Amount = adjustmentAmount - adjAlreadyInvoicedAmount,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow,
                    });*/

                    // Increment the invoice item sequence number
                    invoiceItemSeqNum++;
                    invoiceItemSeqId = invoiceItemSeqNum.ToString("D5");
                }
            }

            // Get a list of the payment method pref
            var orderPaymentPreferences = await _utilityService.FindLocalOrDatabaseListAsync<OrderPaymentPreference>(
                query => query.Where(opp => opp.OrderId == orderId && opp.StatusId != "PAYMENT_CANCELLED")
            );

            var currentPayments = new List<Payment>();
            foreach (var paymentPref in orderPaymentPreferences)
            {
                var payments = await _utilityService.FindLocalOrDatabaseListAsync<Payment>(
                    query => query.Where(p => p.PaymentPreferenceId == paymentPref.OrderPaymentPreferenceId)
                );

                currentPayments.AddRange(payments);
            }

            // Apply these payments to the invoice if they have any remaining amount to apply
            foreach (var payment in currentPayments)
            {
                if (payment.StatusId == "PMNT_VOID" || payment.StatusId == "PMNT_CANCELLED")
                {
                    continue;
                }

                var notApplied = await _paymentApplicationService.GetPaymentNotApplied(payment, true);
                if (notApplied > 0)
                {
                    var paymentApplicationParam = new PaymentApplicationParam
                    {
                        PaymentId = payment.PaymentId,
                        InvoiceId = invoiceId,
                        BillingAccountId = billingAccountId,
                        AmountApplied = notApplied
                    };
                    await _paymentApplicationService.CreatePaymentApplication(paymentApplicationParam);
                }
            }

            // Set invoice status based on ProductStore.autoApproveInvoice setting
            string autoApproveInvoice = productStore?.AutoApproveInvoice ?? "Y";
            if (autoApproveInvoice != "N")
            {
                string nextStatusId = invoiceType == "PURCHASE_INVOICE" ? "INVOICE_IN_PROCESS" : "INVOICE_READY";
                await _invoiceUtilityService.SetInvoiceStatus(invoiceId, nextStatusId, DateTime.UtcNow, null, false);
            }

            return new InvoiceResponse
            {
                Success = true,
                InvoiceId = invoiceId
            };
        }
        catch (Exception e)
        {
            // Log the exception in case of an error using Microsoft Logger (_logger)
            _logger.LogError(e, "Problem creating invoice from order items: {Message}", e.Message);
            return new InvoiceResponse
            {
                Success = false,
                Message = $"Problem creating invoice from order items: {e.Message}"
            };
        }
    }

    public async Task<string> CreateInvoiceFromOrder(string orderId)
    {
        InvoiceResponse invoiceResponse = null;
        string invoiceId = null;

        try
        {
            // Retrieve the order header for the given order ID
            var orderHeader = await _context.OrderHeaders
                .FindAsync(orderId);

            if (orderHeader == null)
            {
                throw new InvalidOperationException("Order not found.");
            }

            // Check if invoicePerShipment is empty; if so, set it from the resource property
            string invoicePerShipment = orderHeader.InvoicePerShipment;
            if (string.IsNullOrEmpty(invoicePerShipment))
            {
                invoicePerShipment = "N"; //await GetInvoicePerShipmentProperty();
            }

            // Proceed only if invoicePerShipment is "N"
            if (invoicePerShipment == "N")
            {
                // Check if there are billing items for the order
                var orderItemBilling = await _context.OrderItemBillings
                    .Where(oib => oib.OrderId == orderId)
                    .ToListAsync();

                if (!orderItemBilling.Any())
                {
                    invoiceId = await CreateInvoiceForOrderAllItems(orderId);
                }
                else
                {
                    // Get the order items for the order
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == orderId)
                        .OrderBy(oi => oi.OrderItemSeqId)
                        .ToListAsync();

                    var billItems = new List<object>();

                    foreach (var orderItem in orderItems)
                    {
                        // Check if the order item is in the billing list
                        var checkOrderItem = await _context.OrderItemBillings
                            .AnyAsync(oib => oib.OrderId == orderId && oib.OrderItemSeqId == orderItem.OrderItemSeqId);

                        if (!checkOrderItem)
                        {
                            billItems.Add(orderItem);
                        }
                    }


                    // Create invoice for the specified billing items
                    invoiceResponse = await CreateInvoiceForOrder(orderId, billItems, null);
                    invoiceId = invoiceResponse.InvoiceId;
                }
            }
        }
        catch (Exception ex)
        {
            // Handle exception (log it, rethrow, etc.)
            throw new Exception("An error occurred while creating the invoice from order.", ex);
        }

        return invoiceId;
    }

    public async Task<string> CreateInvoiceForOrderAllItems(string orderId)
    {
        string invoiceId = null;

        try
        {
            // Retrieve the order items associated with the given order ID
            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .OrderBy(oi => oi.OrderItemSeqId)
                .ToListAsync();

            // If there are order items, store them in the context
            if (orderItems.Any())
            {
                // Call the service to create an invoice for the order
                var result = await CreateInvoiceForOrder(orderId, orderItems, null);

                if (string.IsNullOrEmpty(result.InvoiceId))
                {
                    throw new InvalidOperationException("Failed to create invoice for order.");
                }

                invoiceId = result.InvoiceId;
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions and log the error
            _logger.LogError($"Entity/data problem creating invoice from order items: {ex}");
            throw new Exception("An error occurred while creating the invoice from order items.", ex);
        }

        return invoiceId;
    }

    /*Create invoice(s) from a shipment list.
        All the order items associated with the shipments will be selected and
        one invoice for each order will be created (each invoice could contain
        items shipped in different shipments).
        If the shipments are drop shipments, the type of invoices (purchase or sales) created
        will be controlled by the createSalesInvoicesForDropShipments parameter (purchase by default).
        invoicesCreated = List of invoiceIds which were created by this service*/
    public async Task<CreateInvoicesFromShipmentsResponse> CreateInvoicesFromShipments(
        List<string> shipmentIds,
        bool createSalesInvoicesForDropShipments = false,
        DateTime? eventDate = null)
    {
        var invoicesCreated = new List<string>();
        bool salesShipmentFound = false;
        bool purchaseShipmentFound = false;
        bool dropShipmentFound = false;

        // Part 2: Determine Shipment Types
        foreach (var tmpShipmentId in shipmentIds)
        {
            try
            {
                // Use FindAsync for single Shipment lookup (optimized for primary key)
                var shipment = await _context.Shipments.FindAsync(tmpShipmentId);

                if (shipment == null)
                {
                    return new CreateInvoicesFromShipmentsResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Error: Shipment entity with ID {tmpShipmentId} could not be retrieved."
                    };
                }

                string shipmentTypeId = shipment.ShipmentTypeId;
                if (shipmentTypeId == "PURCHASE_SHIPMENT")
                {
                    purchaseShipmentFound = true;
                }
                else if (shipmentTypeId == "DROP_SHIPMENT")
                {
                    dropShipmentFound = true;
                }
                else
                {
                    salesShipmentFound = true;
                }

                if (purchaseShipmentFound && salesShipmentFound && dropShipmentFound)
                {
                    return new CreateInvoicesFromShipmentsResponse
                    {
                        IsSuccess = false,
                        ErrorMessage =
                            $"Error: Mixed shipment types detected in shipment ID {tmpShipmentId} with type {shipmentTypeId}."
                    };
                }
            }
            catch (Exception ex)
            {
                return new CreateInvoicesFromShipmentsResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception occurred while retrieving shipment ID {tmpShipmentId}: {ex.Message}"
                };
            }
        }

        // Part 3: Retrieve Items Based on Shipment Type
        List<ShipmentReceipt> shipmentReceipts = new List<ShipmentReceipt>();
        List<OrderItem> orderItems = new List<OrderItem>();
        List<ItemIssuance> itemIssuances = new List<ItemIssuance>();
        List<OrderItemAssoc> orderItemAssocs = new List<OrderItemAssoc>();

        try
        {
            if (purchaseShipmentFound)
            {
                try
                {
                    // Validate shipmentIds
                    if (!shipmentIds.Any())
                    {
                        _logger.LogWarning("No shipmentIds provided.");
                        shipmentReceipts = new List<ShipmentReceipt>();
                        return new CreateInvoicesFromShipmentsResponse
                        {
                            IsSuccess = false,
                            ErrorMessage = $"Error: Shipment entity with ID  could not be retrieved."
                        };
                    }

                    // Get shipment receipts for all shipmentIds
                    var allShipmentReceipts = new List<ShipmentReceipt>();
                    foreach (var shipmentId in shipmentIds)
                    {
                        var receipts = await _repositoryService.GetShipmentReceiptsById(shipmentId);
                        allShipmentReceipts.AddRange(receipts);
                    }

                    // Get inventory items for all shipmentIds
                    var allInventoryItems = new List<InventoryItem>();
                    foreach (var shipmentId in shipmentIds)
                    {
                        var items = await _repositoryService.GetInventoryItemsByShipmentId(shipmentId);
                        allInventoryItems.AddRange(items);
                    }

                    // Create a lookup for inventory items by InventoryItemId
                    var inventoryItemLookup = allInventoryItems.ToDictionary(ii => ii.InventoryItemId, ii => ii);

                    // Filter shipment receipts based on inventory item owner
                    shipmentReceipts = new List<ShipmentReceipt>();
                    foreach (var receipt in allShipmentReceipts)
                    {
                        if (inventoryItemLookup.TryGetValue(receipt.InventoryItemId, out var inventoryItem))
                        {
                            // Check if the OwnerPartyId has a PartyRole with RoleTypeId = "INTERNAL_ORGANIZATION"
                            var hasInternalOrgRole = await _context.Set<PartyRole>()
                                .AsNoTracking()
                                .AnyAsync(pr =>
                                    pr.PartyId == inventoryItem.OwnerPartyId &&
                                    pr.RoleTypeId == "INTERNAL_ORGANIZATIO");

                            if (hasInternalOrgRole)
                            {
                                shipmentReceipts.Add(receipt);
                            }
                        }
                        else
                        {
                            _logger.LogWarning(
                                "No InventoryItem found for ShipmentReceipt with ReceiptId {ReceiptId} and InventoryItemId {InventoryItemId}.",
                                receipt.ReceiptId, receipt.InventoryItemId);
                        }
                    }

                    // Log if no receipts remain after filtering
                    if (!shipmentReceipts.Any())
                    {
                        _logger.LogInformation(
                            "No ShipmentReceipts found for shipmentIds after filtering: {ShipmentIds}",
                            string.Join(", ", shipmentIds));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error filtering shipment receipts for shipmentIds: {ShipmentIds}",
                        string.Join(", ", shipmentIds));
                    throw;
                }
            }
            else if (dropShipmentFound)
            {
                // Use utility function for Shipment list
                var dropShipments = await _utilityService.RetrieveLocalOrDatabaseListAsync<Shipment>(
                    query => query.Where(s => shipmentIds.Contains(s.ShipmentId)));

                var purchaseOrderIds = dropShipments
                    .Select(s => s.PrimaryOrderId)
                    .Distinct()
                    .ToList();

                if (createSalesInvoicesForDropShipments)
                {
                    // Use utility function for OrderItemAssoc list
                    orderItemAssocs = await _utilityService.RetrieveLocalOrDatabaseListAsync<OrderItemAssoc>(
                        query => query.Where(oia => purchaseOrderIds.Contains(oia.ToOrderId)));

                    // Materialize the list of FromOrderId values in memory
                    var fromOrderIds = orderItemAssocs.Select(oia => oia.OrderId).ToList();

                    // Use utility function for OrderItem list
                    orderItems = await _utilityService.RetrieveLocalOrDatabaseListAsync<OrderItem>(
                        query => query.Where(oi => fromOrderIds.Contains(oi.OrderId)));
                }
                else
                {
                    // Use utility function for OrderItem list
                    orderItems = await _utilityService.RetrieveLocalOrDatabaseListAsync<OrderItem>(
                        query => query.Where(oi => purchaseOrderIds.Contains(oi.OrderId)));
                }
            }
            else
            {
                // Use utility function for ItemIssuance list
                itemIssuances = await _utilityService.RetrieveLocalOrDatabaseListAsync<ItemIssuance>(
                    query => query.Where(ii => shipmentIds.Contains(ii.ShipmentId)));
            }
        }
        catch (Exception ex)
        {
            return new CreateInvoicesFromShipmentsResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Error retrieving items from shipments: {ex.Message}"
            };
        }

        var shippedOrderItems = new Dictionary<string, List<object>>();

        try
        {
            if (purchaseShipmentFound)
            {
                foreach (var receipt in shipmentReceipts)
                {
                    var orderId = receipt.OrderId;
                    if (dropShipmentFound) // Skip billing check for drop shipments
                    {
                        if (!shippedOrderItems.ContainsKey(orderId))
                            shippedOrderItems[orderId] = new List<object>();
                        shippedOrderItems[orderId].Add(receipt);
                        continue;
                    }

                    // Retain explicit join query for OrderItemBillings and Invoices
                    var isBilled = await _context.OrderItemBillings
                        .Join(_context.Invoices,
                            oib => oib.InvoiceId,
                            inv => inv.InvoiceId,
                            (oib, inv) => new { oib, inv })
                        .AnyAsync(x =>
                            x.oib.OrderId == orderId &&
                            x.oib.OrderItemSeqId == receipt.OrderItemSeqId &&
                            x.oib.ShipmentReceiptId == receipt.ReceiptId &&
                            x.inv.StatusId != "INVOICE_CANCELLED");

                    if (!isBilled)
                    {
                        if (!shippedOrderItems.ContainsKey(orderId))
                            shippedOrderItems[orderId] = new List<object>();
                        shippedOrderItems[orderId].Add(receipt);
                    }
                }
            }
            else if (dropShipmentFound)
            {
                foreach (var orderItem in orderItems)
                {
                    var orderId = orderItem.OrderId;
                    if (!shippedOrderItems.ContainsKey(orderId))
                        shippedOrderItems[orderId] = new List<object>();
                    shippedOrderItems[orderId].Add(orderItem);
                }
            }
            else
            {
                foreach (var issuance in itemIssuances)
                {
                    var orderId = issuance.OrderId;

                    // Retain explicit join query for OrderItemBillings and Invoices
                    var isBilled = await _context.OrderItemBillings
                        .Join(_context.Invoices,
                            oib => oib.InvoiceId,
                            inv => inv.InvoiceId,
                            (oib, inv) => new { oib, inv })
                        .AnyAsync(x =>
                            x.oib.OrderId == orderId &&
                            x.oib.OrderItemSeqId == issuance.OrderItemSeqId &&
                            x.oib.ItemIssuanceId == issuance.ItemIssuanceId &&
                            x.inv.StatusId != "INVOICE_CANCELLED");

                    if (!isBilled)
                    {
                        if (!shippedOrderItems.ContainsKey(orderId))
                            shippedOrderItems[orderId] = new List<object>();
                        shippedOrderItems[orderId].Add(issuance);
                    }
                }
            }
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            return new CreateInvoicesFromShipmentsResponse
            {
                IsSuccess = false,
                ErrorMessage = $"Error grouping items by order: {ex.Message}"
            };
        }

        // Part 5: Filter Items for Invoicing
        var toBillItems = new List<object>();
        var itemQtyAvail = new Dictionary<string, decimal>();

        foreach (var orderEntry in shippedOrderItems)
        {
            string orderId = orderEntry.Key;
            List<object> billItems = orderEntry.Value;

            foreach (var issue in billItems)
            {
                decimal issueQty = 0m;
                string orderItemSeqId = null;

                if (issue is ShipmentReceipt shipmentReceipt)
                {
                    issueQty = shipmentReceipt.QuantityAccepted ?? 0m;
                    orderItemSeqId = shipmentReceipt.OrderItemSeqId;
                }
                else if (issue is ItemIssuance itemIssuance)
                {
                    issueQty = (decimal)itemIssuance.Quantity;
                    orderItemSeqId = itemIssuance.OrderItemSeqId;
                }
                else if (issue is OrderItem orderItem)
                {
                    issueQty = (decimal)orderItem.Quantity;
                    orderItemSeqId = orderItem.OrderItemSeqId;

                    if (dropShipmentFound && createSalesInvoicesForDropShipments)
                    {
                        var orderItemAssoc = orderItemAssocs
                            .FirstOrDefault(oia =>
                                oia.OrderId == orderItem.OrderId && oia.OrderItemSeqId == orderItem.OrderItemSeqId);
                        if (orderItemAssoc != null)
                        {
                            // Use FirstOrDefaultAsync for single OrderItem lookup
                            var purchaseOrderItem = await _context.OrderItems
                                .FirstOrDefaultAsync(oi =>
                                    oi.OrderId == orderItemAssoc.ToOrderId &&
                                    oi.OrderItemSeqId == orderItemAssoc.ToOrderItemSeqId);

                            if (purchaseOrderItem != null)
                            {
                                issueQty = (decimal)purchaseOrderItem.Quantity;
                            }
                        }
                    }
                }

                if (orderItemSeqId == null) continue;

                if (!itemQtyAvail.TryGetValue(orderItemSeqId, out decimal billAvail))
                {
                    // Use FirstOrDefaultAsync for single OrderItem lookup
                    var orderItem = await _context.OrderItems
                        .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.OrderItemSeqId == orderItemSeqId);

                    if (orderItem == null)
                    {
                        return new CreateInvoicesFromShipmentsResponse
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"Order item not found for OrderId: {orderId}, OrderItemSeqId: {orderItemSeqId}"
                        };
                    }

                    decimal orderedQty = (decimal)orderItem.Quantity;

                    // Retain explicit join query for OrderItemBillings, Invoices, and InvoiceItems
                    var billedQuantities = await (
                        from oib in _context.OrderItemBillings
                        join inv in _context.Invoices on oib.InvoiceId equals inv.InvoiceId
                        join ii in _context.InvoiceItems on new { oib.InvoiceId, oib.InvoiceItemSeqId } equals new
                            { ii.InvoiceId, ii.InvoiceItemSeqId }
                        where oib.OrderId == orderId &&
                              oib.OrderItemSeqId == orderItemSeqId &&
                              inv.StatusId != "INVOICE_CANCELLED"
                        select oib.Quantity
                    ).ToListAsync();

                    decimal billedQuantity = (decimal)billedQuantities.Sum();
                    billAvail = orderedQty - billedQuantity;
                    itemQtyAvail[orderItemSeqId] = billAvail;
                }

                if (billAvail > 0)
                {
                    if (issueQty > billAvail)
                    {
                        if (issue is ShipmentReceipt sr)
                        {
                            sr.QuantityAccepted = billAvail;
                        }
                        else if (issue is ItemIssuance ii)
                        {
                            ii.Quantity = billAvail;
                        }
                        else if (issue is OrderItem oi)
                        {
                            oi.Quantity = billAvail;
                        }

                        billAvail = 0;
                    }
                    else
                    {
                        billAvail -= issueQty;
                    }

                    toBillItems.Add(issue);
                    itemQtyAvail[orderItemSeqId] = billAvail;
                }
            }
        }

        // Part 6: Handle Additional Shipping Charges
        var additionalShippingCharges = new Dictionary<string, decimal>();
        decimal totalAdditionalShippingCharges = 0m;

        // Use utility function for Shipment list
        var invoiceableShipments = await _utilityService.RetrieveLocalOrDatabaseListAsync<Shipment>(
            query => query.Where(s => shipmentIds.Contains(s.ShipmentId)));

        foreach (var shipment in invoiceableShipments)
        {
            if (shipment.AdditionalShippingCharge.HasValue)
            {
                additionalShippingCharges[shipment.ShipmentId] = shipment.AdditionalShippingCharge.Value;
                totalAdditionalShippingCharges += shipment.AdditionalShippingCharge.Value;
            }
        }

        // If additional shipping charges exist, process them
        if (totalAdditionalShippingCharges > 0)
        {
            foreach (var entry in additionalShippingCharges)
            {
                string shipmentId = entry.Key;
                decimal additionalCharge = entry.Value;

                // Create an order adjustment for the additional shipping charge
                var orderAdjustment = new OrderAdjustment
                {
                    OrderId = shippedOrderItems.Keys.First(),
                    OrderAdjustmentTypeId = "SHIPPING_CHARGES",
                    Description = $"Additional shipping charge for shipment {shipmentId}",
                    SourceReferenceId = shipmentId,
                    Amount = additionalCharge,
                    CreatedDate = DateTime.UtcNow
                };

                _context.OrderAdjustments.Add(orderAdjustment);
            }
        }

        // Part 7: Create Invoices for Orders
        foreach (var orderEntry in shippedOrderItems)
        {
            string orderId = orderEntry.Key;
            toBillItems = orderEntry.Value;

            string invoiceId = null;

            // Check if there's already a ShipmentItemBilling associated with one of the shipments
            var shipmentId = shipmentIds.FirstOrDefault();

            // Use FirstOrDefaultAsync for single ShipmentItemBilling lookup
            var shipmentItemBilling = await _context.ShipmentItemBillings
                .FirstOrDefaultAsync(sib => sib.ShipmentId == shipmentId);

            if (shipmentItemBilling != null)
            {
                invoiceId = shipmentItemBilling.InvoiceId;
            }

            try
            {
                var invoiceResult = await CreateInvoiceForOrder(orderId, toBillItems, eventDate);

                if (!invoiceResult.Success)
                {
                    return new CreateInvoicesFromShipmentsResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Error creating invoice for order {orderId}: {invoiceResult.Message}"
                    };
                }

                invoicesCreated.Add(invoiceResult.InvoiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating invoice for order {orderId}");
                return new CreateInvoicesFromShipmentsResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Exception occurred while creating invoice for order {orderId}: {ex.Message}"
                };
            }
        }

        // Return a successful response with the list of created invoices
        return new CreateInvoicesFromShipmentsResponse
        {
            IsSuccess = true,
            InvoicesCreated = invoicesCreated
        };
    }

    public async Task<SetInvoicesToReadyFromShipmentResult> SetInvoicesToReadyFromShipment(string shipmentId)
    {
        // -------------------------------
        // 1. Retrieve the Shipment entity based on shipmentId, including tracked but unsaved entities.
        // Technical: Check ChangeTracker for unsaved Shipment, then query database if not found.
        // Business: Ensure that the shipment exists before processing any invoices.
        // -------------------------------
        Shipment shipment = null;
        try
        {
            // Check ChangeTracker for unsaved Shipment entities
            shipment = _context.ChangeTracker.Entries<Shipment>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .FirstOrDefault(s => s.ShipmentId == shipmentId);

            // If not found in tracker, query the database
            if (shipment == null)
            {
                shipment = await _context.Shipments.FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trouble getting Shipment entity for shipment {ShipmentId}", shipmentId);
            return SetInvoicesToReadyFromShipmentResult.ReturnError(
                $"Trouble getting Shipment entity for shipment {shipmentId}");
        }

        // Check if the shipment was not found
        if (shipment == null)
        {
            _logger.LogError("Shipment not found for shipment id: {ShipmentId}", shipmentId);
            return SetInvoicesToReadyFromShipmentResult.ReturnError(
                $"Shipment not found for shipment id: {shipmentId}");
        }

        // -------------------------------
        // 2. Retrieve all ItemIssuance records for this shipment, including tracked but unsaved.
        // Technical: Check ChangeTracker and database, group by orderId to ensure uniqueness.
        // Business: Each ItemIssuance represents an order where items have been issued from this shipment.
        // -------------------------------
        List<ItemIssuance> itemIssuances = null;
        try
        {
            // Get tracked ItemIssuance entities
            var trackedItemIssuances = _context.ChangeTracker.Entries<ItemIssuance>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .Where(ii => ii.ShipmentId == shipmentId)
                .ToList();

            // Query database for persisted ItemIssuance entities
            var dbItemIssuances = await _context.ItemIssuances
                .Where(ii => ii.ShipmentId == shipmentId)
                .ToListAsync();

            // Combine tracked and database results, ensuring uniqueness by OrderId
            itemIssuances = trackedItemIssuances
                .Concat(dbItemIssuances)
                .GroupBy(ii => ii.OrderId)
                .Select(g => g.First())
                .OrderBy(ii => ii.OrderId)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Problem getting issued items from shipments for shipment id: {ShipmentId}",
                shipmentId);
            return SetInvoicesToReadyFromShipmentResult.ReturnError(
                $"Problem getting issued items from shipments for shipment id: {shipmentId}");
        }

        // If no item issuance records were found, log and return success
        if (itemIssuances.Count == 0)
        {
            _logger.LogInformation("No items issued for shipment id: {ShipmentId}", shipmentId);
            return SetInvoicesToReadyFromShipmentResult.ReturnSuccess();
        }

        // -------------------------------
        // 3. For each order, check for an invoice in "INVOICE_IN_PROCESS" status, including tracked entities.
        // Technical: Check ChangeTracker and database for OrderItemBilling and related Invoice.
        // Business: Only process orders that have an invoice currently in process.
        // -------------------------------
        List<OrderInvoiceDto> ordersWithInProcessInvoice = new List<OrderInvoiceDto>();

        foreach (var itemIssuance in itemIssuances)
        {
            string orderId = itemIssuance.OrderId;

            // -------------------------------
            // 3.a. Look up the OrderItemBilling record for the orderId, including tracked entities.
            // Technical: Check ChangeTracker and database for OrderItemBilling.
            // Business: Indicates if an invoice exists for the order.
            // -------------------------------
            OrderItemBilling orderItemBilling = null;
            try
            {
                // Check ChangeTracker for unsaved OrderItemBilling
                orderItemBilling = _context.ChangeTracker.Entries<OrderItemBilling>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .Select(e => e.Entity)
                    .FirstOrDefault(oib => oib.OrderId == orderId);

                // If not found in tracker, query the database
                if (orderItemBilling == null)
                {
                    orderItemBilling = await _context.OrderItemBillings
                        .FirstOrDefaultAsync(oib => oib.OrderId == orderId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Problem looking up OrderItemBilling records for orderId: {OrderId}", orderId);
                return SetInvoicesToReadyFromShipmentResult.ReturnError(
                    $"Problem looking up OrderItemBilling records for orderId: {orderId}");
            }

            // -------------------------------
            // 3.b. If OrderItemBilling exists, retrieve its related Invoice, including tracked entities.
            // Technical: Check navigation property or ChangeTracker for Invoice.
            // Business: Process only orders that have an invoice.
            // -------------------------------
            if (orderItemBilling != null)
            {
                Invoice invoice = null;
                try
                {
                    // Check if Invoice is already loaded via navigation property
                    if (orderItemBilling.InvoiceI?.Invoice != null)
                    {
                        invoice = orderItemBilling.InvoiceI.Invoice;
                    }
                    else
                    {
                        // Check ChangeTracker for unsaved Invoice
                        invoice = _context.ChangeTracker.Entries<Invoice>()
                            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                            .Select(e => e.Entity)
                            .FirstOrDefault(i => i.InvoiceId == orderItemBilling.InvoiceId);

                        // If not found in tracker, query the database
                        if (invoice == null)
                        {
                            invoice = await _context.Invoices
                                .FirstOrDefaultAsync(i => i.InvoiceId == orderItemBilling.InvoiceId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error retrieving related Invoice for OrderItemBilling with orderId: {OrderId}", orderId);
                    return SetInvoicesToReadyFromShipmentResult.ReturnError(
                        $"Error retrieving related Invoice for OrderItemBilling with orderId: {orderId}");
                }

                // -------------------------------
                // 3.c. Check if the invoice status is "INVOICE_IN_PROCESS".
                // Business: Only invoices in process should be updated to "READY".
                // -------------------------------
                if (invoice != null && invoice.StatusId == "INVOICE_IN_PROCESS")
                {
                    ordersWithInProcessInvoice.Add(new OrderInvoiceDto { OrderId = orderId, Invoice = invoice });
                }
            }
        }

        // -------------------------------
        // 4. For each invoice in "INVOICE_IN_PROCESS" status, update the status to "INVOICE_READY".
        // Technical: Call setInvoiceStatus to perform the status update.
        // Business: Moves the invoice lifecycle forward and captures the payment.
        // -------------------------------
        foreach (var orderInvoice in ordersWithInProcessInvoice)
        {
            string invoiceId = orderInvoice.Invoice.InvoiceId;
            try
            {
                await _invoiceUtilityService.SetInvoiceStatus(invoiceId, "INVOICE_READY");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while setting invoice status for invoice: {InvoiceId}", invoiceId);
                return SetInvoicesToReadyFromShipmentResult.ReturnError(
                    $"Error while setting invoice status for invoice: {invoiceId}");
            }
        }

        // -------------------------------
        // 5. Return a successful result if all invoices were updated.
        // Business: Indicates that the shipment's invoices have been successfully updated to "READY".
        // -------------------------------
        return SetInvoicesToReadyFromShipmentResult.ReturnSuccess();
    }
}
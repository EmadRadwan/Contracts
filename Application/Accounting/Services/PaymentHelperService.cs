using Application.Accounting.FinAccounts;
using Application.Accounting.Payments;
using Application.Accounting.Services.Models;
using Application.Core;
using Application.Shipments.Invoices;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IPaymentHelperService
{
    Task<Payment> CreatePayment(CreatePaymentParam parameters);
    Task<Payment> UpdatePayment(CreatePaymentParam parameters);
    Task<PaymentStatusChangeResult> SetPaymentStatus(string paymentId, string statusId);
    Task CreateMatchingPaymentApplication(string? paymentId, string? invoiceId);
    Task<bool> IsDisbursement(Payment payment);
    Task<bool> IsReceipt(Payment payment);
    Task<string> CheckAndCreateBatchForValidPayments(List<string> paymentIds);
    Task<Result<string>> CreatePaymentGroup(CreatePaymentGroupDto dto);
    Task<UpdatePaymentGroupMemberResult> UpdatePaymentGroupMember(UpdatePaymentGroupMemberInput input);
    Task<CreatePaymentGroupResult> UpdatePaymentGroup(CreatePaymentGroupInput input);

    Task<Result<string>> CreatePaymentGroupMember(CreatePaymentGroupMemberDto dto);

    Task<Result<CreatePaymentAndApplicationResponse>> CreatePaymentAndApplication(
        CreatePaymentAndApplicationRequest request);

    Task<Result<CreatePaymentAndFinAccountTransResponse>> CreatePaymentAndFinAccountTrans(
        CreatePaymentAndFinAccountTransRequest request);

    Task<GeneralServiceResult<object>> CancelCheckRunPayments(string paymentGroupId);

    Task<ExpirePaymentGroupMemberResult> ExpirePaymentGroupMember(ExpirePaymentGroupMemberInput input);
}

public class PaymentHelperService : IPaymentHelperService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly DataContext _context;
    private readonly IGeneralLedgerService _generalLedgerService;
    private readonly IInvoiceService _invoiceService;
    private readonly IUtilityService _utilityService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly IPaymentApplicationService _paymentApplicationService;
    private readonly Lazy<IFinAccountService> _finAccountService;
    private readonly ILogger _logger;


    public PaymentHelperService(DataContext context, IUtilityService utilityService,
        IInvoiceService invoiceService, IInvoiceUtilityService invoiceUtilityService,
        IAcctgMiscService acctgMiscService, IGeneralLedgerService generalLedgerService,
        IPaymentApplicationService paymentApplicationService, Lazy<IFinAccountService> finAccountService,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _utilityService = utilityService;
        _invoiceService = invoiceService;
        _acctgMiscService = acctgMiscService;
        _generalLedgerService = generalLedgerService;
        _logger = logger;
        _invoiceUtilityService = invoiceUtilityService;
        _paymentApplicationService = paymentApplicationService;
        _finAccountService = finAccountService;
    }


    public async Task<Payment> CreatePayment(CreatePaymentParam parameters)
    {
        // get the next sequence for the PaymentId
        var stamp = DateTime.UtcNow;
        var newPaymentSequence = await _utilityService.GetNextSequence("Payment");

        // Determine the organization partyId by checking PartyRole for INTERNAL_ORGANIZATION
        var partyIdsToCheck = new[] { parameters.PartyIdFrom, parameters.PartyIdTo };
        string organizationPartyId = null;

        foreach (var partyId in partyIdsToCheck)
        {
            var hasInternalOrgRole = await _context.PartyRoles
                .AnyAsync(pr => pr.PartyId == partyId && pr.RoleTypeId == "INTERNAL_ORGANIZATIO");
            if (hasInternalOrgRole)
            {
                organizationPartyId = partyId;
                break;
            }
        }

        if (string.IsNullOrEmpty(organizationPartyId))
        {
            _logger.LogWarning(
                $"No party with INTERNAL_ORGANIZATION role found for PartyIdFrom: {parameters.PartyIdFrom}, PartyIdTo: {parameters.PartyIdTo}.");
            throw new Exception("No organization party found for accounting preferences.");
        }

        // Get party accounting preferences for the organization
        var partyAccountingPreferences = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);
        if (partyAccountingPreferences == null)
        {
            throw new Exception($"No accounting preferences found for party: {organizationPartyId}");
        }


        var payment = new Payment
        {
            PaymentId = newPaymentSequence,
            PaymentMethodTypeId = parameters.PaymentMethodTypeId,
            PaymentMethodId = parameters.PaymentMethodId,
            PaymentPreferenceId = parameters.PaymentPreferenceId,
            StatusId = parameters.StatusId,
            EffectiveDate = parameters.EffectiveDate ?? stamp,
            Amount = (decimal)parameters.Amount,
            PartyIdFrom = parameters.PartyIdFrom,
            PartyIdTo = parameters.PartyIdTo,
            PaymentTypeId = parameters.PaymentTypeId,
            CurrencyUomId = partyAccountingPreferences!.BaseCurrencyUomId,
            //todo: needs to be added in the form
            ActualCurrencyUomId = partyAccountingPreferences!.BaseCurrencyUomId,
            ActualCurrencyAmount = parameters.Amount,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };


        // Save to database
        _context.Payments.Add(payment);

        // --- Simulating the XML event ---
        // <eca service="createPayment" event="commit">
        //     <condition field-name="statusId" operator="equals" value="PMNT_RECEIVED"/>
        //     <action service="createAcctgTransAndEntriesForIncomingPayment" mode="sync"/>
        // </eca>
        // If the payment status equals "PMNT_RECEIVED", then immediately trigger the accounting transaction creation.
        if (payment.StatusId == "PMNT_RECEIVED")
        {
            // We call the method synchronously (awaiting its completion) as specified by the event configuration.
            var acctgTransId =
                await _generalLedgerService.CreateAcctgTransAndEntriesForIncomingPayment(payment.PaymentId);
            _logger.LogInformation(
                $"Event triggered: Created AcctgTrans {acctgTransId} for incoming payment {payment.PaymentId}.");
        }

        // 2. For outgoing payments (PMNT_SENT), the event is:
        // <eca service="createPayment" event="commit">
        //     <condition field-name="statusId" operator="equals" value="PMNT_SENT"/>
        //     <action service="createAcctgTransAndEntriesForOutgoingPayment" mode="sync"/>
        // </eca>
        if (payment.StatusId == "PMNT_SENT")
        {
            var acctgTransId =
                await _generalLedgerService.CreateAcctgTransAndEntriesForOutgoingPayment(payment.PaymentId);
            _logger.LogInformation(
                $"Event triggered: Created AcctgTrans {acctgTransId} for outgoing payment {payment.PaymentId}.");
        }

        return payment;
    }

    public async Task<Payment> UpdatePayment(CreatePaymentParam param)
    {
        // Fetch the payment
        var payment = await _context.Payments
            .FindAsync(param.PaymentId);


        // Status check (OFBiz: PMNT_NOT_PAID check)
        if (payment.StatusId != "PMNT_NOT_PAID")
        {
            // Check if only allowed fields are updated
            var oldPayment = new Payment
            {
                StatusId = payment.StatusId,
                Comments = payment.Comments,
                PaymentRefNum = payment.PaymentRefNum,
                FinAccountTransId = payment.FinAccountTransId
            };
            var newPayment = new Payment
            {
                StatusId = param.StatusId ?? payment.StatusId,
                Comments = param.Comments ?? payment.Comments,
                PaymentRefNum = param.PaymentRefNum,
                FinAccountTransId = param.FinAccountTransId
            };
        }

        // Prevent status change directly (OFBiz: statusIdSave)
        string statusIdSave = payment.StatusId;

        // Update non-PK fields (OFBiz: setNonPKFields)
        payment.Comments = param.Comments ?? payment.Comments;
        payment.PaymentRefNum = param.PaymentRefNum;
        payment.FinAccountTransId = param.FinAccountTransId;
        payment.EffectiveDate = payment.EffectiveDate ?? DateTime.UtcNow;
        payment.PaymentPreferenceId = param.PaymentPreferenceId;
        payment.Amount = param.Amount ?? payment.Amount;
        payment.ActualCurrencyAmount = param.ActualCurrencyAmount ?? payment.ActualCurrencyAmount;
        payment.ActualCurrencyUomId = param.ActualCurrencyUomId ?? payment.ActualCurrencyUomId;

        // Validate payment method (OFBiz: paymentMethod check)
        if (!string.IsNullOrEmpty(param.PaymentMethodId))
        {
            var paymentMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(pm => pm.PaymentMethodId == param.PaymentMethodId);


            if (param.PaymentMethodTypeId != paymentMethod.PaymentMethodTypeId)
            {
                // Log replacement (OFBiz: logInfo)
                Console.WriteLine(
                    $"Replacing payment method type [{param.PaymentMethodTypeId}] with [{paymentMethod.PaymentMethodTypeId}] for payment method [{param.PaymentMethodId}]");
                payment.PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId;
            }
            else
            {
                payment.PaymentMethodTypeId = param.PaymentMethodTypeId;
            }

            payment.PaymentMethodId = param.PaymentMethodId;
        }


        // Handle status change via separate service (OFBiz: setPaymentStatus)
        if (!string.IsNullOrEmpty(param.StatusId) && param.StatusId != statusIdSave)
        {
            var statusResult = await SetPaymentStatus(payment.PaymentId, param.StatusId);
        }

        return payment;
    }


    public async Task<PaymentStatusChangeResult> SetPaymentStatus(string paymentId, string statusId)
    {
        // Retrieve the payment entity
        var payment = await _context.Payments.SingleOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment == null)
        {
            return new PaymentStatusChangeResult
            {
                Success = false,
                ErrorCode = "PAYMENT_NOT_FOUND",
                ErrorMessage = $"Payment with ID {paymentId} not found."
            };
        }

        var oldStatusId = payment.StatusId;

        // Retrieve the new status item
        var statusItem = await _context.StatusItems.SingleOrDefaultAsync(s => s.StatusId == statusId);
        if (statusItem == null)
        {
            return new PaymentStatusChangeResult
            {
                Success = false,
                ErrorCode = "INVALID_STATUS",
                ErrorMessage = $"Invalid status ID: {statusId}."
            };
        }

        // Check if status is changing
        if (oldStatusId != statusId)
        {
            // Validate the status change
            var statusChange = await _context.StatusValidChanges
                .SingleOrDefaultAsync(sc => sc.StatusId == oldStatusId && sc.StatusIdTo == statusId);

            if (statusChange == null)
            {
                return new PaymentStatusChangeResult
                {
                    Success = false,
                    ErrorCode = "INVALID_STATUS_TRANSITION",
                    ErrorMessage = $"Cannot change status from {oldStatusId} to {statusId}."
                };
            }

            // Payment method mandatory check for PMNT_RECEIVED or PMNT_SENT
            if ((statusId == "PMNT_RECEIVED" || statusId == "PMNT_SENT") && payment.PaymentMethodId == null)
            {
                return new PaymentStatusChangeResult
                {
                    Success = false,
                    ErrorCode = "MISSING_PAYMENT_METHOD",
                    ErrorMessage = "Payment method is mandatory when setting status to 'Received' or 'Sent'."
                };
            }

            // Check if payment is fully applied when confirming
            if (statusId == "PMNT_CONFIRMED" &&
                await _paymentApplicationService.GetPaymentNotApplied(payment, true) != 0)
            {
                return new PaymentStatusChangeResult
                {
                    Success = false,
                    ErrorCode = "PAYMENT_NOT_APPLIED",
                    ErrorMessage = "Payment is not fully applied."
                };
            }

            // If changing to PMNT_SENT from another status, check invoices
            if (statusId == "PMNT_SENT" && oldStatusId != "PMNT_SENT")
            {
                try
                {
                    await _invoiceService.CheckPaymentInvoices(paymentId);
                }
                catch (Exception ex)
                {
                    // REFACTOR: Catch and return specific invoice check error
                    return new PaymentStatusChangeResult
                    {
                        Success = false,
                        ErrorCode = "INVOICE_CHECK_FAILED",
                        ErrorMessage = $"Invoice validation failed: {ex.Message}"
                    };
                }
            }
        }

        // If the new status is cancelled, remove payment applications and update order preference
        if (statusId == "PMNT_CANCELLED")
        {
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == payment.PaymentId)
                .ToListAsync();

            _context.PaymentApplications.RemoveRange(paymentApplications);

            var orderPayPref = await _context.OrderPaymentPreferences
                .SingleOrDefaultAsync(op => op.OrderPaymentPreferenceId == payment.PaymentPreferenceId);

            if (orderPayPref != null)
            {
                orderPayPref.StatusId = "PAYMENT_CANCELLED";
                orderPayPref.LastUpdatedStamp = DateTime.UtcNow;
            }
        }

        // Update the payment status
        payment.StatusId = statusId;
        payment.LastUpdatedStamp = DateTime.UtcNow;

        // --------------------------------------------------------------
        // 6. ECA-Driven Logic (Ofbiz "event=commit" logic)
        // --------------------------------------------------------------

        // 6.1 If status changed to PMNT_RECEIVED:
        //     - oldStatusId != PMNT_RECEIVED, oldStatusId != PMNT_CONFIRMED => createAcctgTransAndEntriesForIncomingPayment
        //     - oldStatusId != PMNT_RECEIVED => checkPaymentInvoices, createMatchingPaymentApplication
        if (statusId == "PMNT_RECEIVED" && oldStatusId != "PMNT_RECEIVED")
        {
            try
            {
                if (oldStatusId != "PMNT_CONFIRMED")
                {
                    await _generalLedgerService.CreateAcctgTransAndEntriesForIncomingPayment(paymentId);
                }

                await _invoiceService.CheckPaymentInvoices(paymentId);
                await CreateMatchingPaymentApplication(paymentId, null);
            }
            catch (Exception ex)
            {
                // REFACTOR: Catch and return specific error for ECA logic failure
                return new PaymentStatusChangeResult
                {
                    Success = false,
                    ErrorCode = "ECA_LOGIC_FAILED",
                    ErrorMessage = $"Failed to process accounting transactions: {ex.Message}"
                };
            }
        }


        // 6.2 If status changed to PMNT_SENT:
        //     - oldStatusId != PMNT_SENT, oldStatusId != PMNT_CONFIRMED => createAcctgTransAndEntriesForOutgoingPayment
        //     - oldStatusId != PMNT_SENT => checkPaymentInvoices, createMatchingPaymentApplication
        if (statusId == "PMNT_SENT" && oldStatusId != "PMNT_SENT")
        {
            try
            {
                if (oldStatusId != "PMNT_CONFIRMED")
                {
                    await _generalLedgerService.CreateAcctgTransAndEntriesForOutgoingPayment(paymentId);
                }

                await _invoiceService.CheckPaymentInvoices(paymentId);
                await CreateMatchingPaymentApplication(paymentId, null);
            }
            catch (Exception ex)
            {
                // REFACTOR: Catch and return specific error for ECA logic failure
                return new PaymentStatusChangeResult
                {
                    Success = false,
                    ErrorCode = "ECA_LOGIC_FAILED",
                    ErrorMessage = $"Failed to process accounting transactions: {ex.Message}"
                };
            }
        }


        return new PaymentStatusChangeResult
        {
            Success = true,
            UpdatedPayment = payment
        };
    }

    public async Task CreateMatchingPaymentApplication(string? paymentId, string? invoiceId)
    {
        try
        {
            // Start with null to properly check if we set it later
            PaymentApplicationParam? createPaymentApplicationParam = null;

            // If invoiceId is provided, try to find a matching payment first
            if (!string.IsNullOrEmpty(invoiceId))
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice != null)
                {
                    // Get the total amount of the invoice
                    var invoiceTotal = await _invoiceUtilityService.GetInvoiceTotal(invoice.InvoiceId, true);

                    // Check if the invoice is in a foreign currency
                    var isInvoiceInForeignCurrency =
                        await _invoiceUtilityService.IsInvoiceInForeignCurrency(invoice.InvoiceId);

                    // Build a query for Payment based on foreign currency condition
                    IQueryable<Payment> paymentQuery = _context.Payments
                        .Where(p => p.StatusId != "PMNT_CONFIRMED"
                                    && p.PartyIdFrom == invoice.PartyId
                                    && p.PartyIdTo == invoice.PartyIdFrom);

                    if (isInvoiceInForeignCurrency)
                    {
                        paymentQuery = paymentQuery.Where(p => p.ActualCurrencyAmount == invoiceTotal &&
                                                               p.ActualCurrencyUomId == invoice.CurrencyUomId);
                    }
                    else
                    {
                        paymentQuery = paymentQuery.Where(p => p.Amount == invoiceTotal &&
                                                               p.CurrencyUomId == invoice.CurrencyUomId);
                    }

                    // Optionally order by effectiveDate if needed
                    // paymentQuery = paymentQuery.OrderBy(p => p.EffectiveDate);
                    var matchingPayment = await paymentQuery.FirstOrDefaultAsync();

                    if (matchingPayment != null)
                    {
                        // Check if a PaymentApplication already exists for this payment
                        bool paymentApplicationExists = await _context.PaymentApplications
                            .AnyAsync(pa => pa.PaymentId == matchingPayment.PaymentId);

                        if (!paymentApplicationExists)
                        {
                            createPaymentApplicationParam = new PaymentApplicationParam
                            {
                                PaymentId = matchingPayment.PaymentId,
                                InvoiceId = invoiceId,
                                AmountApplied = isInvoiceInForeignCurrency
                                    ? matchingPayment.ActualCurrencyAmount
                                    : matchingPayment.Amount
                            };
                        }
                    }
                }
            }

            // If no application params created yet and paymentId is provided, try to find a matching invoice
            if (createPaymentApplicationParam == null && !string.IsNullOrEmpty(paymentId))
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
                if (payment != null)
                {
                    // Find invoices that can still be matched (not in ended statuses)
                    var blockedStatuses = new List<string>
                        { "INVOICE_READY", "INVOICE_PAID", "INVOICE_CANCELLED", "INVOICE_WRITEOFF" };

                    var invoicesQuery = _context.Invoices
                        .Where(inv => !blockedStatuses.Contains(inv.StatusId)
                                      && inv.PartyIdFrom == payment.PartyIdTo
                                      && inv.PartyId == payment.PartyIdFrom);

                    // Potentially order by invoiceDate if needed
                    // invoicesQuery = invoicesQuery.OrderBy(inv => inv.InvoiceDate);

                    var invoices = await invoicesQuery.ToListAsync();
                    foreach (var invoice in invoices)
                    {
                        // Simplified invoice type checks
                        bool isPurchaseInvoice =
                            invoice.InvoiceTypeId?.Equals("PURCHASE_INVOICE", StringComparison.OrdinalIgnoreCase) ==
                            true;
                        bool isSalesInvoice =
                            invoice.InvoiceTypeId?.Equals("SALES_INVOICE", StringComparison.OrdinalIgnoreCase) == true;

                        if (isPurchaseInvoice || isSalesInvoice)
                        {
                            var invoiceTotal = await _invoiceUtilityService.GetInvoiceTotal(invoice.InvoiceId, true);
                            var isInvoiceInForeignCurrency =
                                await _invoiceUtilityService.IsInvoiceInForeignCurrency(invoice.InvoiceId);

                            bool matchesInvoice = false;
                            if (isInvoiceInForeignCurrency)
                            {
                                if (invoiceTotal == payment.ActualCurrencyAmount &&
                                    invoice.CurrencyUomId == payment.ActualCurrencyUomId)
                                {
                                    matchesInvoice = true;
                                }
                            }
                            else
                            {
                                if (invoiceTotal == payment.Amount &&
                                    invoice.CurrencyUomId == payment.CurrencyUomId)
                                {
                                    matchesInvoice = true;
                                }
                            }

                            if (matchesInvoice)
                            {
                                // Check if there's already a PaymentApplication for this invoice
                                bool paymentApplicationExists = await _context.PaymentApplications
                                    .AnyAsync(pa => pa.InvoiceId == invoice.InvoiceId);

                                if (!paymentApplicationExists)
                                {
                                    createPaymentApplicationParam = new PaymentApplicationParam
                                    {
                                        PaymentId = payment.PaymentId,
                                        InvoiceId = invoice.InvoiceId,
                                        AmountApplied = isInvoiceInForeignCurrency
                                            ? payment.ActualCurrencyAmount
                                            : payment.Amount
                                    };
                                    break; // Found a match, break out of the loop
                                }
                            }
                        }
                    }
                }
            }

            // If we have all required fields for a payment application, create it
            if (createPaymentApplicationParam != null &&
                !string.IsNullOrEmpty(createPaymentApplicationParam.PaymentId) &&
                !string.IsNullOrEmpty(createPaymentApplicationParam.InvoiceId))
            {
                await _paymentApplicationService.CreatePaymentApplication(createPaymentApplicationParam);
                _logger.LogInformation(
                    "Payment application automatically created between invoiceId: {0} and paymentId: {1} for the amount: {2}",
                    createPaymentApplicationParam.InvoiceId, createPaymentApplicationParam.PaymentId,
                    createPaymentApplicationParam.AmountApplied);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating matching payment application");
            throw;
        }
    }

    public async Task<bool> IsDisbursement(Payment payment)
    {
        return await IsPaymentType(payment, "DISBURSEMENT");
    }

    public async Task<bool> IsReceipt(Payment payment)
    {
        return await IsPaymentType(payment, "RECEIPT");
    }

    private async Task<bool> IsPaymentType(Payment payment, string inputTypeId)
    {
        // Get the PaymentType associated with this payment
        var paymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(pt => pt.PaymentTypeId == payment.PaymentTypeId);

        if (paymentType == null)
        {
            return false;
        }

        // If paymentTypeId equals inputTypeId, we found it directly
        if (paymentType.PaymentTypeId == inputTypeId)
        {
            return true;
        }

        // Otherwise, recurse/iterate through parent types
        return await IsPaymentTypeRecurse(paymentType, inputTypeId);
    }

    private async Task<bool> IsPaymentTypeRecurse(PaymentType paymentType, string inputTypeId)
    {
        // Check the parentTypeId directly
        var parentTypeId = paymentType.ParentTypeId;
        if (parentTypeId == null)
        {
            return false;
        }

        if (parentTypeId.Equals(inputTypeId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Fetch the parent PaymentType entity
        var parentPaymentType = await _context.PaymentTypes
            .FirstOrDefaultAsync(pt => pt.PaymentTypeId == parentTypeId);

        if (parentPaymentType == null)
        {
            // No parent found, cannot recurse further
            return false;
        }

        // Recurse with the parent payment type
        return await IsPaymentTypeRecurse(parentPaymentType, inputTypeId);
    }

    public async Task<string> CheckAndCreateBatchForValidPayments(List<string> paymentIds)
    {
        // Validate input
        if (paymentIds == null || !paymentIds.Any())
            throw new ArgumentException("Payment IDs cannot be null or empty", nameof(paymentIds));

        // 1. Retrieve and validate payments
        var payments = await _context.Payments
            .Where(p => paymentIds.Contains(p.PaymentId))
            .ToListAsync();

        // Check for missing payments
        var missingIds = paymentIds.Except(payments.Select(p => p.PaymentId)).ToList();
        if (missingIds.Any())
        {
            throw new KeyNotFoundException($"Missing payments: {string.Join(", ", missingIds)}");
        }

        // 2. Verify all payments are receipts (no disbursements)
        var disbursementPayments = new List<Payment>();
        foreach (var payment in payments)
        {
            var result = await IsReceipt(payment);
            if (!result)
            {
                disbursementPayments.Add(payment);
            }
        }

        if (disbursementPayments.Any())
        {
            var invalidIds = disbursementPayments.Select(p => p.PaymentId);
            throw new InvalidOperationException(
                $"Cannot include AP payments: {string.Join(", ", invalidIds)}");
        }

        // 3. Check for existing batch memberships
        var existingBatchMembers = await _context.PaymentGroupMembers
            .Where(pgm => paymentIds.Contains(pgm.PaymentId))
            .Select(pgm => pgm.PaymentId)
            .Distinct()
            .ToListAsync();

        if (existingBatchMembers.Any())
        {
            throw new InvalidOperationException(
                $"Payments already batched: {string.Join(", ", existingBatchMembers)}");
        }

        // 4. Create payment group with members
        var paymentGroupId = await CreatePaymentGroupAndMember(
            paymentIds,
            "PAYMENT_BATCH",
            DateTime.UtcNow,
            $"PaymentBatch_{DateTime.UtcNow:yyyyMMdd}"
        );
        return paymentGroupId;
    }

    public async Task<string> CreatePaymentGroupAndMember(
        List<string> paymentIds,
        string paymentGroupTypeId,
        DateTime? fromDate = null,
        string paymentGroupName = null)
    {
        // Validate inputs
        if (paymentIds == null || !paymentIds.Any())
            throw new ArgumentException("Payment IDs cannot be empty", nameof(paymentIds));

        if (string.IsNullOrEmpty(paymentGroupTypeId))
            throw new ArgumentNullException(nameof(paymentGroupTypeId));

        try
        {
            // Set default fromDate if not provided
            var effectiveFromDate = fromDate ?? DateTime.UtcNow;

            // Create payment group parameters
            var groupParams = new CreatePaymentGroupDto()
            {
                PaymentGroupTypeId = paymentGroupTypeId,
                PaymentGroupName = string.IsNullOrEmpty(paymentGroupName)
                    ? $"PaymentGroup_{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : paymentGroupName
            };

            // Create parent payment group
            var paymentGroupId = await CreatePaymentGroup(groupParams);

            // Create group members
            foreach (var paymentId in paymentIds)
            {
                await CreatePaymentGroupMember(new CreatePaymentGroupMemberDto()
                {
                    PaymentGroupId = paymentGroupId.Value,
                    PaymentId = paymentId,
                    FromDate = effectiveFromDate
                });
            }

            return paymentGroupId.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create payment group and members");
            throw new ApplicationException("Payment group creation failed", ex);
        }
    }

    public async Task<Result<string>> CreatePaymentGroup(CreatePaymentGroupDto dto)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(dto.PaymentGroupTypeId))
            {
                return Result<string>.Failure("PaymentGroupTypeId is required.");
            }

            if (string.IsNullOrEmpty(dto.PaymentGroupName))
            {
                return Result<string>.Failure("PaymentGroupName is required.");
            }

            // Create a new PaymentGroup entity
            var paymentGroup = new Domain.PaymentGroup
            {
                PaymentGroupId = await _utilityService.GetNextSequence("PaymentGroup"),
                PaymentGroupTypeId = dto.PaymentGroupTypeId,
                PaymentGroupName = dto.PaymentGroupName,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow,
                // Map other optional fields from dto
            };

            // Add the PaymentGroup to the database
            _context.PaymentGroups.Add(paymentGroup);

            // Return the primary key of the created PaymentGroup
            return Result<string>.Success(paymentGroup.PaymentGroupId);
        }
        catch (Exception ex)
        {
            // Log exception and return failure
            _logger.LogError(ex, "Error creating PaymentGroup.");
            return Result<string>.Failure("An error occurred while creating the PaymentGroup.");
        }
    }

    public async Task<Result<string>> CreatePaymentGroupMember(CreatePaymentGroupMemberDto dto)
    {
        try
        {
            // Validate input: Ensure required fields (PK fields) are provided
            if (string.IsNullOrEmpty(dto.PaymentGroupId) || string.IsNullOrEmpty(dto.PaymentId))
            {
                return Result<string>.Failure("PaymentGroupId and PaymentId are required.");
            }

            // Fetch the PaymentGroup
            var paymentGroup = await _context.PaymentGroups
                .FirstOrDefaultAsync(pg => pg.PaymentGroupId == dto.PaymentGroupId);
            if (paymentGroup == null)
            {
                return Result<string>.Failure("PaymentGroup not found.");
            }

            // Fetch the Payment
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == dto.PaymentId);
            if (payment == null)
            {
                return Result<string>.Failure("Payment not found.");
            }

            // Set default value for fromDate if not provided
            var fromDate = dto.FromDate ?? DateTime.UtcNow;

            // Check the PaymentGroupType and validate the payment type (Outgoing/Incoming)
            if (paymentGroup.PaymentGroupTypeId == "CHECK_RUN")
            {
                bool isDisbursement = await IsDisbursement(payment); // Custom method for disbursement check
                if (!isDisbursement)
                {
                    return Result<string>.Failure("Cannot create an incoming payment for CHECK_RUN.");
                }
            }
            else if (paymentGroup.PaymentGroupTypeId == "BATCH_PAYMENT")
            {
                bool isReceipt = await IsReceipt(payment); // Custom method for receipt check
                if (!isReceipt)
                {
                    return Result<string>.Failure("Cannot create an outgoing payment for BATCH_PAYMENT.");
                }
            }

            // Create and save the PaymentGroupMember
            var paymentGroupMember = new PaymentGroupMember
            {
                PaymentGroupId = dto.PaymentGroupId,
                PaymentId = dto.PaymentId,
                FromDate = fromDate,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.PaymentGroupMembers.Add(paymentGroupMember);

            // Return the ID of the newly created PaymentGroupMember
            return Result<string>.Success(paymentGroupMember.PaymentGroupId);
        }
        catch (Exception ex)
        {
            // Log the exception and return a failure result
            _logger.LogError(ex, "Error creating PaymentGroupMember.");
            return Result<string>.Failure("An error occurred while creating the PaymentGroupMember.");
        }
    }

    public async Task<Result<CreatePaymentAndApplicationResponse>> CreatePaymentAndApplication(
        CreatePaymentAndApplicationRequest request)
    {
        try
        {
            // Step 1: Create Payment
            var payment = new Payment
            {
                PaymentTypeId = request.PaymentTypeId,
                PaymentMethodTypeId = "EXT_BILLACT",
                PartyIdFrom = request.PartyIdFrom,
                PartyIdTo = request.PartyIdTo,
                StatusId = request.StatusId,
                Amount = request.Amount,
                OverrideGlAccountId = request.OverrideGlAccountId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Step 2: Create Payment Application
            var paymentApplication = new PaymentApplication
            {
                PaymentId = payment.PaymentId,
                AmountApplied = request.Amount,
                InvoiceId = request.InvoiceId,
                InvoiceItemSeqId = request.InvoiceItemSeqId,
                BillingAccountId = request.BillingAccountId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            _context.PaymentApplications.Add(paymentApplication);

            // Step 3: Return Result
            return Result<CreatePaymentAndApplicationResponse>.Success(new CreatePaymentAndApplicationResponse
            {
                PaymentId = payment.PaymentId,
                PaymentApplicationId = paymentApplication.PaymentApplicationId
            });
        }
        catch (Exception ex)
        {
            return Result<CreatePaymentAndApplicationResponse>.Failure(
                $"Error creating payment and application: {ex.Message}");
        }
    }

    public async Task<CreatePaymentGroupResult> CreatePaymentGroup(CreatePaymentGroupInput input)
    {
        var result = new CreatePaymentGroupResult();

        try
        {
            // Validate required fields
            if (string.IsNullOrEmpty(input.PaymentGroupTypeId) || string.IsNullOrEmpty(input.PaymentGroupName))
            {
                result.Message = "Missing required fields: paymentGroupTypeId and paymentGroupName";
                return result;
            }

            // Create a new PaymentGroup entity
            var paymentGroup = new Domain.PaymentGroup
            {
                PaymentGroupId = await _utilityService.GetNextSequence("PaymentGroup"),
                PaymentGroupTypeId = input.PaymentGroupTypeId,
                PaymentGroupName = input.PaymentGroupName,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };

            // Add to the database
            _context.PaymentGroups.Add(paymentGroup);

            // Set result
            result.PaymentGroupId = paymentGroup.PaymentGroupId;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in CreatePaymentGroup: {0}", ex.Message);
            result.Message = "ErrorProcessingPaymentGroup";
            return result;
        }
    }

    public async Task<CreatePaymentGroupResult> UpdatePaymentGroup(CreatePaymentGroupInput input)
    {
        var result = new CreatePaymentGroupResult();

        try
        {
            // Step 2: Fetch the existing PaymentGroup record
            var paymentGroup =
                await _context.PaymentGroups.FirstOrDefaultAsync(pg => pg.PaymentGroupId == input.PaymentGroupId);
            if (paymentGroup == null)
            {
                result.Message = "PaymentGroup not found";
                return result;
            }

            // Step 3: Update fields only if they are provided
            if (!string.IsNullOrEmpty(input.PaymentGroupTypeId))
            {
                paymentGroup.PaymentGroupTypeId = input.PaymentGroupTypeId;
            }

            if (!string.IsNullOrEmpty(input.PaymentGroupName))
            {
                paymentGroup.PaymentGroupName = input.PaymentGroupName;
            }

            // Step 4: Update timestamps
            paymentGroup.LastUpdatedStamp = DateTime.UtcNow;

            // Step 5: Save changes to the database

            // Set success result
            result.PaymentGroupId = paymentGroup.PaymentGroupId;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in UpdatePaymentGroup: {0}", ex.Message);
            result.Message = "ErrorUpdatingPaymentGroup";
            return result;
        }
    }

    public async Task<CreatePaymentGroupMemberResult> CreatePaymentGroupMember(CreatePaymentGroupMemberInput input)
    {
        var result = new CreatePaymentGroupMemberResult();

        try
        {
            // Step 1: Validate required fields (Primary Key)
            if (string.IsNullOrEmpty(input.PaymentGroupId) || string.IsNullOrEmpty(input.PaymentId))
            {
                result.Message = "Missing required fields: PaymentGroupId and PaymentId";
                return result;
            }

            // Step 2: Create a new PaymentGroupMember entity
            var newPaymentGroupMember = new PaymentGroupMember
            {
                PaymentGroupId = input.PaymentGroupId,
                PaymentId = input.PaymentId,
                FromDate = input.FromDate ?? DateTime.UtcNow
            };

            // Step 3: Fetch related PaymentGroup & Payment entities
            var paymentGroup =
                await _context.PaymentGroups.FirstOrDefaultAsync(pg => pg.PaymentGroupId == input.PaymentGroupId);
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == input.PaymentId);

            if (paymentGroup == null || payment == null)
            {
                result.Message = "Payment Group or Payment not found";
                return result;
            }

            // Step 4: Validate Payment Type based on PaymentGroupTypeId
            if (paymentGroup.PaymentGroupTypeId == "CHECK_RUN")
            {
                // Check if the payment is a disbursement
                bool isDisbursement = await IsDisbursement(payment);
                if (!isDisbursement)
                {
                    result.Message = "Cannot create incoming payment for CHECK_RUN Payment Group";
                    return result;
                }
            }
            else if (paymentGroup.PaymentGroupTypeId == "BATCH_PAYMENT")
            {
                // Check if the payment is a receipt
                bool isReceipt = await IsReceipt(payment);
                if (!isReceipt)
                {
                    result.Message = "Cannot create outgoing payment for BATCH_PAYMENT Payment Group";
                    return result;
                }
            }

            // Step 5: Add and Save the new PaymentGroupMember record
            _context.PaymentGroupMembers.Add(newPaymentGroupMember);

            // Set success response
            result.PaymentGroupId = newPaymentGroupMember.PaymentGroupId;
            result.PaymentId = newPaymentGroupMember.PaymentId;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in CreatePaymentGroupMember: {0}", ex.Message);
            result.Message = "ErrorProcessingPaymentGroupMember";
            return result;
        }
    }

    public async Task<List<PaymentGroupMemberDTO>> GetPaymentGroupMembers(string paymentGroupId)
    {
        var query = from pgm in _context.PaymentGroupMembers
            join payment in _context.Payments on pgm.PaymentId equals payment.PaymentId
            join creditCard in _context.CreditCards on payment.PaymentMethodId equals creditCard.PaymentMethodId into cc
            from creditCard in cc.DefaultIfEmpty() // Left join for credit card info
            where pgm.PaymentGroupId == paymentGroupId
            select new PaymentGroupMemberDTO
            {
                PaymentGroupId = pgm.PaymentGroupId,
                PaymentId = pgm.PaymentId,
                PaymentRefNum = payment.PaymentRefNum,
                PartyIdFrom = payment.PartyIdFrom,
                PartyIdTo = payment.PartyIdTo,
                PaymentTypeId = payment.PaymentTypeId,
                PaymentMethodTypeId = payment.PaymentMethodTypeId,
                CardType = creditCard != null ? creditCard.CardType : null,
                Amount = payment.Amount,
                CurrencyUomId = payment.CurrencyUomId,
                FromDate = pgm.FromDate,
                ThruDate = pgm.ThruDate
            };

        return await query.ToListAsync();
    }

    public async Task<ExpirePaymentGroupMemberResult> ExpirePaymentGroupMember(ExpirePaymentGroupMemberInput input)
    {
        var result = new ExpirePaymentGroupMemberResult();

        try
        {
            // Step 1: Validate required fields (Primary Key)
            if (string.IsNullOrEmpty(input.PaymentGroupId) || string.IsNullOrEmpty(input.PaymentId) ||
                input.FromDate == null)
            {
                result.Message = "Missing required fields: PaymentGroupId, PaymentId, and FromDate";
                return result;
            }

            // Step 2: Fetch the Payment Group Member
            var paymentGroupMember = await _context.PaymentGroupMembers
                .FirstOrDefaultAsync(pgm =>
                    pgm.PaymentGroupId == input.PaymentGroupId &&
                    pgm.PaymentId == input.PaymentId &&
                    pgm.FromDate == input.FromDate);

            if (paymentGroupMember == null)
            {
                result.Message = "PaymentGroupMember not found";
                return result;
            }

            // Step 3: Set the `thruDate` to the current timestamp
            paymentGroupMember.ThruDate = DateTime.UtcNow;

            // Step 4: Call the `updatePaymentGroupMember` function
            var updateInput = new UpdatePaymentGroupMemberInput
            {
                PaymentGroupId = paymentGroupMember.PaymentGroupId,
                PaymentId = paymentGroupMember.PaymentId,
                FromDate = paymentGroupMember.FromDate,
                ThruDate = paymentGroupMember.ThruDate
            };

            var updateResult = await UpdatePaymentGroupMember(updateInput);

            if (!updateResult.IsSuccess)
            {
                result.Message = "Failed to update PaymentGroupMember";
                return result;
            }

            // Step 5: Return success
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ExpirePaymentGroupMember: {0}", ex.Message);
            result.Message = "ErrorProcessingPaymentGroupMember";
            return result;
        }
    }

    public async Task<UpdatePaymentGroupMemberResult> UpdatePaymentGroupMember(UpdatePaymentGroupMemberInput input)
    {
        var result = new UpdatePaymentGroupMemberResult();

        try
        {
            // Step 1: Validate required fields (Primary Key)
            if (string.IsNullOrEmpty(input.PaymentGroupId) || string.IsNullOrEmpty(input.PaymentId) ||
                input.FromDate == null)
            {
                result.Message = "Missing required fields: PaymentGroupId, PaymentId, and FromDate";
                return result;
            }

            // Step 2: Fetch the existing Payment Group Member record
            var paymentGroupMember = await _context.PaymentGroupMembers
                .FirstOrDefaultAsync(pgm =>
                    pgm.PaymentGroupId == input.PaymentGroupId &&
                    pgm.PaymentId == input.PaymentId &&
                    pgm.FromDate == input.FromDate);

            if (paymentGroupMember == null)
            {
                result.Message = "PaymentGroupMember not found";
                return result;
            }

            // Step 3: Update fields dynamically (if provided)
            if (input.ThruDate.HasValue)
            {
                paymentGroupMember.ThruDate = input.ThruDate.Value;
            }


            // Step 5: Return success
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in UpdatePaymentGroupMember: {0}", ex.Message);
            result.Message = "ErrorUpdatingPaymentGroupMember";
            return result;
        }
    }

    public async Task<Result<CreatePaymentAndFinAccountTransResponse>> CreatePaymentAndFinAccountTrans(
        CreatePaymentAndFinAccountTransRequest request)
    {
        try
        {
            // 1) Map parameters to createPayment
            var createPaymentMap = new CreatePaymentParam
            {
                PartyIdFrom = request.PartyIdFrom,
                PartyIdTo = request.PartyIdTo,
                Amount = request.Amount,
                StatusId = request.StatusId,
                EffectiveDate = request.PaymentDate ?? DateTime.UtcNow,
                PaymentTypeId = request.PaymentTypeId
            };

            // If PaymentMethodId is provided, fetch PaymentMethod and set PaymentMethodTypeId
            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.PaymentMethodId == request.PaymentMethodId);

                if (paymentMethod == null)
                {
                    return Result<CreatePaymentAndFinAccountTransResponse>.Failure("PaymentMethod not found.");
                }

                createPaymentMap.PaymentMethodId = paymentMethod.PaymentMethodId;
                createPaymentMap.PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId;
            }

            // 2) Create Payment
            var payment = await CreatePayment(createPaymentMap);
            var paymentId = payment.PaymentId;
            string finAccountTransId = null;

            // 3) Handle Financial Account Transaction if FinAccountId exists
            if (!string.IsNullOrEmpty(request.PaymentMethodId))
            {
                var paymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(pm => pm.PaymentMethodId == request.PaymentMethodId);

                if (paymentMethod != null && !string.IsNullOrEmpty(paymentMethod.FinAccountId))
                {
                    // Fetch FinAccount
                    var finAccount = await _context.FinAccounts
                        .FirstOrDefaultAsync(f => f.FinAccountId == paymentMethod.FinAccountId);

                    if (finAccount == null)
                    {
                        return Result<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "FinAccount not found for the PaymentMethod.");
                    }

                    // Validate FinAccount status
                    if (finAccount.StatusId == "FNACT_MANFROZEN")
                    {
                        return Result<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "Cannot process a manually frozen FinAccount.");
                    }

                    if (finAccount.StatusId == "FNACT_CANCELLED")
                    {
                        return Result<CreatePaymentAndFinAccountTransResponse>.Failure(
                            "Cannot process a cancelled FinAccount.");
                    }

                    // If IsDepositWithDrawPayment is "Y", create FinAccountTrans
                    if ("Y".Equals(request.IsDepositWithDrawPayment, StringComparison.OrdinalIgnoreCase))
                    {
                        // Map parameters to createFinAccountTrans
                        var createFinAccountTransMap = new CreateFinAccountTransParam
                        {
                            FinAccountId = paymentMethod.FinAccountId,
                            PaymentId = paymentId,
                            StatusId = "FINACT_TRNS_CREATED",
                            PartyId = request.PartyIdFrom,
                            Amount = request.Amount,
                            FinAccountTransTypeId = request.FinAccountTransTypeId,
                            EffectiveDate = request.PaymentDate ?? DateTime.UtcNow
                        };

                        // Create FinAccountTrans
                        finAccountTransId =
                            await _finAccountService.Value.CreateFinAccountTrans(createFinAccountTransMap);

                        await _context.SaveChangesAsync();


                        // Update Payment with FinAccountTransId
                        var updatePaymentCtx = new CreatePaymentParam
                        {
                            PaymentId = paymentId,
                            PaymentMethodId = request.PaymentMethodId,
                            FinAccountTransId = finAccountTransId,
                            StatusId = payment.StatusId,
                            Comments = payment.Comments,
                            PaymentRefNum = payment.PaymentRefNum,
                        };

                        await UpdatePayment(updatePaymentCtx);
                    }
                }
            }

            // 4) Return result
            var response = new CreatePaymentAndFinAccountTransResponse
            {
                PaymentId = paymentId,
                CurrencyUomId = payment.CurrencyUomId,
                ActualCurrencyUomId = payment.ActualCurrencyUomId,
                FinAccountTransId = finAccountTransId
            };

            return Result<CreatePaymentAndFinAccountTransResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<CreatePaymentAndFinAccountTransResponse>.Failure(
                $"Error creating Payment and FinAccountTrans: {ex.Message}");
        }
    }

    public async Task<GeneralServiceResult<object>> CancelCheckRunPayments(string paymentGroupId)
    {
        try
        {
            // Query the underlying tables to replicate the PmtGrpMembrPaymentAndFinAcctTrans view-entity
            // Technical: Uses LINQ to join PaymentGroupMember, Payment, and FinAccountTrans
            // Business Purpose: Retrieves payments in the payment group to check if they can be canceled
            List<PaymentGroupMemberPaymentAndFinAccountTrans> paymentGroupMemberAndTransList =
                (from pgm in _context.PaymentGroupMembers
                    join py in _context.Payments on pgm.PaymentId equals py.PaymentId
                    join fat in _context.FinAccountTrans on py.FinAccountTransId equals fat.FinAccountTransId
                    where pgm.PaymentGroupId == paymentGroupId
                    select new PaymentGroupMemberPaymentAndFinAccountTrans
                    {
                        PaymentGroupId = pgm.PaymentGroupId,
                        PaymentId = pgm.PaymentId,
                        FinAccountTransId = py.FinAccountTransId,
                        FinAccountId = fat.FinAccountId,
                        PartyId = fat.PartyId,
                        FinAccountTransStatusId = fat.StatusId,
                        FinAccountTransAmount = fat.Amount,
                        GlReconciliationId = fat.GlReconciliationId
                    }).ToList();

            // Check if any records were found
            // Technical: Ensures the list is not empty
            // Business Purpose: If no payments exist, return success as there's nothing to cancel
            if (paymentGroupMemberAndTransList.Any())
            {
                // Check if the first transaction is not approved
                // Technical: Checks FinAccountTransStatusId
                // Business Purpose: Prevents cancellation of issued checks
                if (paymentGroupMemberAndTransList[0].FinAccountTransStatusId != "FINACT_TRNS_APPROVED")
                {
                    foreach (var paymentGroupMemberAndTrans in paymentGroupMemberAndTransList)
                    {
                        // Query the Payment entity
                        // Technical: Retrieves Payment for voiding
                        // Business Purpose: Ensures payment details are available
                        var payment = _context.Payments
                            .Where(p => p.PaymentId == paymentGroupMemberAndTrans.PaymentId)
                            .SingleOrDefault();

                        // Void the payment
                        // Technical: Calls voidPayment with paymentId
                        // Business Purpose: Cancels the payment
                        await VoidPayment(payment.PaymentId);


                        // Expire the payment group member
                        // Technical: Calls expirePaymentGroupMember with DTO
                        // Business Purpose: Removes payment from the group
                        var expireInput = new ExpirePaymentGroupMemberInput
                        {
                            PaymentGroupId = paymentGroupMemberAndTrans.PaymentGroupId,
                            PaymentId = paymentGroupMemberAndTrans.PaymentId,
                        };

                        // Expire the payment group member
                        // Technical: Calls ExpirePaymentGroupMember with input object
                        // Business Purpose: Removes payment from the group
                        await ExpirePaymentGroupMember(expireInput);
                    }
                }
                else
                {
                    // Return error if check is issued
                    // Technical: Returns error message
                    // Business Purpose: Prevents cancellation of finalized payments
                    return GeneralServiceResult<object>.Error("AccountingCheckIsAlreadyIssued");
                }
            }

            // Return success
            // Technical: Indicates successful operation
            // Business Purpose: Confirms cancellation completed
            return GeneralServiceResult<object>.Success();
        }
        catch (Exception ex)
        {
            // Log and return error
            // Technical: Logs exception details
            // Business Purpose: Tracks errors for auditing
            _logger.LogError(ex, "Error in cancelCheckRunPayments for paymentGroupId: {PaymentGroupId}",
                paymentGroupId);
            return GeneralServiceResult<object>.Error(
                "An unexpected error occurred while canceling the payment group.");
        }
    }

    public async Task<GeneralServiceResult<VoidPaymentResult>> VoidPayment(string paymentId)
    {
        try
        {
            // Query the Payment entity
            // Technical: Uses LINQ to retrieve Payment by paymentId
            // Business Purpose: Ensures the payment exists before voiding
            var payment = await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .SingleOrDefaultAsync();

            // Check if payment was found
            // Technical: Verifies payment is not null
            // Business Purpose: Prevents voiding non-existent payments
            if (payment == null)
            {
                return GeneralServiceResult<VoidPaymentResult>.Error("AccountingNoPaymentsfound");
            }

            // Store paymentId for use
            // Technical: Assigns paymentId for clarity
            // Business Purpose: Links related records
            string localPaymentId = payment.PaymentId;

            // Set payment status to PMNT_VOID
            // Technical: Calls setPaymentStatus with paymentId and status
            // Business Purpose: Voids the payment to invalidate it
            await SetPaymentStatus(localPaymentId, "PMNT_VOID");


            // Query PaymentApplications for the payment
            // Technical: Uses LINQ to retrieve PaymentApplications
            // Business Purpose: Identifies applications to remove and invoices to update
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == localPaymentId)
                .ToListAsync();

            // Process each PaymentApplication
            // Technical: Iterates over applications
            // Business Purpose: Updates invoices and removes applications
            foreach (var paymentApplication in paymentApplications)
            {
                // Query the Invoice
                // Technical: Retrieves Invoice by InvoiceId
                // Business Purpose: Checks if invoice needs status update
                var invoice = await _context.Invoices
                    .Where(i => i.InvoiceId == paymentApplication.InvoiceId)
                    .SingleOrDefaultAsync();

                // Check if invoice is paid and update status
                // Technical: Calls setInvoiceStatus if invoice is paid
                // Business Purpose: Reverts invoice to READY if payment is voided
                if (invoice?.StatusId == "INVOICE_PAID")
                {
                    await _invoiceUtilityService.SetInvoiceStatus(invoice.InvoiceId, "INVOICE_READY", null);
                }

                // Remove the PaymentApplication
                // Technical: Calls removePaymentApplication
                // Business Purpose: Disconnects payment from invoice
                await _paymentApplicationService.RemovePaymentApplication(paymentApplication.PaymentApplicationId);
            }

            // Query AcctgTrans for the payment
            // Technical: Retrieves AcctgTrans with null invoiceId
            // Business Purpose: Identifies transactions to reverse
            var acctgTransList = await _context.AcctgTrans
                .Where(at => at.InvoiceId == null && at.PaymentId == localPaymentId)
                .ToListAsync();

            // Process each AcctgTrans
            // Technical: Iterates over transactions
            // Business Purpose: Reverses accounting entries
            foreach (var acctgTrans in acctgTransList)
            {
                // Copy and revert AcctgTrans
                // Technical: Calls copyAcctgTransAndEntries
                // Business Purpose: Creates reversing entries
                var result = await _generalLedgerService.CopyAcctgTransAndEntries(acctgTrans.AcctgTransId, true);
                if (result.IsSuccess)
                {
                    // Type-safe access to ResultData as string
                    string newAcctgTransId = result.ResultData; // No casting needed
                    Console.WriteLine($"New transaction ID: {newAcctgTransId}");
                }
                else
                {
                    Console.WriteLine($"Error: {result.ErrorMessage}");
                }

                // Post the new transaction if original was posted
                // Technical: Calls postAcctgTrans if isPosted is Y
                // Business Purpose: Ensures reversing entries are posted
                if (acctgTrans.IsPosted == "Y")
                {
                    string newAcctgTransId = result.ResultData;
                    _generalLedgerService.PostAcctgTrans(newAcctgTransId);
                }
            }

            // Return success with finAccountTransId and status
            // Technical: Returns result with data
            // Business Purpose: Confirms payment voided and transaction canceled
            return GeneralServiceResult<VoidPaymentResult>.Success(new VoidPaymentResult
            {
                FinAccountTransId = payment.FinAccountTransId,
                StatusId = "FINACT_TRNS_CANCELED"
            });
        }
        catch (Exception ex)
        {
            // Log and return error
            // Technical: Logs exception
            // Business Purpose: Tracks voiding errors
            _logger.LogError(ex, "Error in voidPayment for paymentId: {PaymentId}", paymentId);
            return GeneralServiceResult<VoidPaymentResult>.Error("Failed to void payment.");
        }
    }
}
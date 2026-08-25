using Application.Accounting.Accounting;
using Application.Accounting.FinAccounts;
using Application.Accounting.Invoices;
using Application.Accounting.Services.Models;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.Products.Services.Inventory;
using Application.Catalog.ProductStores;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Services;

public interface IGeneralLedgerService
{
    Task<string> CreateAcctgTransForSalesShipmentIssuance(string itemIssuanceId);
    Task<string> CreateAcctgTransForSalesInvoice(string invoiceId);
    Task<string> CreateAcctgTransForSalesInvoice_2(string invoiceId);
    Task<string> CreateAcctgTransForPurchaseInvoice(string invoiceId);
    Task<string> CreateAcctgTransForConstructionCertificateInvoice(string invoiceId);
    Task<string> CreateAcctgTransForShipmentReceipt(string receiptId);
    Task<string> CreateAcctgTransForShipmentReceiptForProject(string receiptId);
    Task<string> CreateAcctgTransAndEntries(CreateAcctgTransAndEntriesParams parameters);
    Task<string> CreateAcctgTransAndEntriesForIncomingPayment(string paymentId);
    Task<string> CreateAcctgTransAndEntriesForPaymentApplication(string paymentApplicationId);
    Task<string> CreateAcctgTransForWorkEffortIssuance(string workEffortId, string inventoryItemId);
    Task<string> CreateAcctgTransForCertificateIssuance(string workEffortId, string inventoryItemId,
        string certificateItemId, decimal issuedQuantity);
    Task<string> QuickCreateAcctgTransAndEntries(CreateQuickAcctgTransAndEntriesParams parameters);
    Task<string> CreateAcctgTransForCustomerReturnInvoice(string invoiceId);
    Task<string> CreateAcctgTransAndEntriesForOutgoingPayment(string paymentId);
    Task<string> CreateAcctgTransAndEntriesForPayrollPayment(string paymentId);

    Task<string> CreateAcctgTransForWorkEffortInventoryProduced(string workEffortId,
        string inventoryItemId);

    Task<string> CreateAcctgTransForPhysicalInventoryVariance(
        CreateAcctgTransForPhysicalInventoryVarianceDto dto);

    Task<string> CreateAcctgTransForWorkEffortCost(string workEffortId, string costComponentId);

    Task<Result<string>> PostFinAccountTransToGl(PostFinAccountTransToGlRequest request);

    Task<List<CustomTimePeriod>> FindCustomTimePeriods(
        DateTime findDate,
        string organizationPartyId = null,
        string excludeNoOrganizationPeriods = null,
        List<string> onlyIncludePeriodTypeIdList = null);

    Task CloseFinancialTimePeriod(string customTimePeriodId);

    Task CompleteAcctgTransEntries(string acctgTransId);
    Task<GeneralServiceResult<string>> CopyAcctgTransAndEntries(string fromAcctgTransId, bool revert);
    Task<List<string>> PostAcctgTrans(string acctgTransId, bool verifyOnly = false);
    Task<string> CreateAccountingTransactionForApartmentIncomingPayment(string paymentId);
    Task<string> CreateAccountingTransactionForMaintenanceDepositPayment(string paymentId);
    Task<string> CreatePostdatedChequeAccountingTransaction(string paymentId);
    Task<string> CreatePostdatedChequeIssuedAccountingTransaction(string paymentId);
    Task<string> CreateAccountingTransactionForEmployeeAdvance(string paymentId);
    Task<string> CreateAcctgTransForPayrollInvoice(string invoiceId);
    Task<bool> DeletePostdatedChequeAccountingTransaction(string paymentId);
}

public class GeneralLedgerService : IGeneralLedgerService
{
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly IAcctgTransService _acctgTransService;
    private readonly DataContext _context;
    private readonly ICostService _costService;
    private readonly IInventoryService _inventoryService;
    private readonly IInvoiceUtilityService _invoiceUtilityService;
    private readonly ILogger<GeneralLedgerService> _logger;
    private readonly IUtilityService _utilityService;
    private readonly ICommonService _commonService;
    private readonly IProductStoreService _productStoreService;
    private readonly Lazy<IPaymentHelperService> _paymentHelperService;
    private readonly Lazy<IAcctgReportsService> _acctgReportsService;


    public GeneralLedgerService(DataContext context,
        IUtilityService utilityService,
        IInvoiceUtilityService invoiceUtilityService,
        IAcctgTransService acctgTransService,
        IAcctgMiscService acctgMiscService, ILogger<GeneralLedgerService> logger, ICostService costService,
        IInventoryService inventoryService, ICommonService commonService,
        Lazy<IPaymentHelperService> paymentHelperService, Lazy<IAcctgReportsService> acctgReportsService,
        IProductStoreService productStoreService)
    {
        _context = context;
        _utilityService = utilityService;
        _acctgTransService = acctgTransService;
        _acctgMiscService = acctgMiscService;
        _costService = costService;
        _logger = logger;
        _inventoryService = inventoryService;
        _invoiceUtilityService = invoiceUtilityService;
        _commonService = commonService;
        _paymentHelperService = paymentHelperService;
        _acctgReportsService = acctgReportsService;
        _productStoreService = productStoreService;
    }

    public async Task<string> CreateAcctgTransForShipmentReceipt(string receiptId)
    {
        // Resolve from the change tracker first (the receipt is normally still unsaved at this
        // point), then fall back to the database. Matching ONLY tracked entries in the Added state
        // meant any flow that had already saved produced a null here and threw on the next line,
        // which ShipmentService's catch-all then swallowed — persisting the receipt with no ledger
        // entry at all. See CreateAcctgTransForShipmentReceiptForProject for the incident this cost.
        var shipmentReceipt = _context.ChangeTracker
                                  .Entries<ShipmentReceipt>()
                                  .FirstOrDefault(e => e.Entity.ReceiptId == receiptId)?.Entity
                              ?? await _context.ShipmentReceipts
                                  .FirstOrDefaultAsync(sr => sr.ReceiptId == receiptId);

        if (shipmentReceipt == null)
            throw new InvalidOperationException(
                $"ShipmentReceipt {receiptId} not found — cannot post its receipt accounting.");

        var inventoryItem = _context.ChangeTracker
                                .Entries<InventoryItem>()
                                .FirstOrDefault(e => e.Entity.InventoryItemId == shipmentReceipt.InventoryItemId)
                                ?.Entity
                            ?? await _context.InventoryItems
                                .FirstOrDefaultAsync(ii => ii.InventoryItemId == shipmentReceipt.InventoryItemId);

        if (inventoryItem == null)
            throw new InvalidOperationException(
                $"InventoryItem {shipmentReceipt.InventoryItemId} not found for ShipmentReceipt {receiptId}.");

        var acctgTransEntries = new List<AcctgTransEntry>();

        // Logic to determine credit account type
        var creditAccountTypeId = string.IsNullOrEmpty(shipmentReceipt?.ReturnId)
            ? "UNINVOICED_SHIP_RCPT"
            : "COGS_ACCOUNT";

        //get partyAcctPreference by organizationPartyId
        var partyAcctgPreference =
            await _acctgMiscService.GetPartyAccountingPreferences(inventoryItem.OwnerPartyId!);


        // Logic to calculate unit cost based on preferences
        decimal unitCost;

        if (!string.IsNullOrEmpty(shipmentReceipt?.ReturnId) &&
            (partyAcctgPreference.CogsMethodId == "COGS_INV_COST" ||
             partyAcctgPreference.CogsMethodId == "COGS_AVG_COST"))
        {
            if (partyAcctgPreference.CogsMethodId == "COGS_AVG_COST")
                // Logic to calculate average cost
                unitCost = await _costService.GetProductAverageCost(inventoryItem);
            else
                unitCost = (decimal)inventoryItem?.UnitCost;
        }
        else
        {
            unitCost = (decimal)inventoryItem.UnitCost;
        }

        // get shipment using receiptId
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentReceipt!.ShipmentId);


        // Logic to calculate origAmount, create inventory item detail, and prepare entries
        var origAmount = shipmentReceipt.QuantityAccepted * unitCost;

        var createInventoryItemDetailParam = new CreateInventoryItemDetailParam
        {
            InventoryItemId = inventoryItem.InventoryItemId,
            AccountingQuantityDiff = shipmentReceipt.QuantityAccepted
        };
        await _inventoryService.CreateInventoryItemDetail(createInventoryItemDetailParam);

        //prepare the double posting (D/C) entries (AcctgTransEntry
        var stamp = DateTime.UtcNow;
        var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");


        //Credit
        var creditEntry = new AcctgTransEntry
        {
            AcctgTransId = newAcctgTransSequence,
            AcctgTransEntrySeqId = "1",
            AcctgTransEntryTypeId = "_NA_",
            DebitCreditFlag = "C",
            OrganizationPartyId = inventoryItem.OwnerPartyId,
            GlAccountTypeId = creditAccountTypeId,
            OrigAmount = origAmount,
            ProductId = inventoryItem.ProductId,
            CurrencyUomId = partyAcctgPreference.BaseCurrencyUomId,
            OrigCurrencyUomId = inventoryItem.CurrencyUomId,
            PartyId = shipment.PartyIdFrom,
            RoleTypeId = "BILL_FROM_VENDOR",
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        acctgTransEntries.Add(creditEntry);

        //Debit
        var debitEntry = new AcctgTransEntry
        {
            AcctgTransId = newAcctgTransSequence,
            AcctgTransEntrySeqId = "2",
            AcctgTransEntryTypeId = "_NA_",
            DebitCreditFlag = "D",
            OrganizationPartyId = inventoryItem.OwnerPartyId,
            GlAccountTypeId = "INVENTORY_ACCOUNT",
            OrigAmount = origAmount,
            ProductId = inventoryItem.ProductId,
            CurrencyUomId = partyAcctgPreference.BaseCurrencyUomId,
            OrigCurrencyUomId = inventoryItem.CurrencyUomId,
            PartyId = shipment.PartyIdFrom,
            RoleTypeId = "BILL_FROM_VENDOR",
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        acctgTransEntries.Add(debitEntry);

        var createAcctgTransAndEntriesParams = new CreateAcctgTransAndEntriesParams
        {
            AcctgTransEntries = acctgTransEntries,
            GlFiscalTypeId = "ACTUAL",
            AcctgTransTypeId = "SHIPMENT_RECEIPT",
            ShipmentId = shipmentReceipt.ShipmentId,
            PaymentId = null,
            PartyId = shipment.PartyIdFrom,
            TransactionDate = shipmentReceipt.DatetimeReceived
        };

        var acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesParams);

        return acctgTransId;
    }

    public async Task<string> CreateAcctgTransForShipmentReceiptForProject(string receiptId)
    {
        // Resolve from the change tracker first (the receipt is normally still unsaved at this
        // point), then fall back to the database. Matching ONLY tracked entries in the Added state
        // meant any flow that had already saved produced a null here and threw on the next line —
        // and ShipmentService's catch-all swallowed it, persisting the receipt with no ledger entry.
        // That is how shipments 10566-10568 (PO10887-10889, 2026-05-12) left GL 140000 short by
        // 324,623.31 for eight weeks, with nothing but a log line to show for it.
        var shipmentReceipt = _context.ChangeTracker
                                  .Entries<ShipmentReceipt>()
                                  .FirstOrDefault(e => e.Entity.ReceiptId == receiptId)?.Entity
                              ?? await _context.ShipmentReceipts
                                  .FirstOrDefaultAsync(sr => sr.ReceiptId == receiptId);

        if (shipmentReceipt == null)
            throw new InvalidOperationException(
                $"ShipmentReceipt {receiptId} not found — cannot post its receipt accounting.");

        var inventoryItem = _context.ChangeTracker
                                .Entries<InventoryItem>()
                                .FirstOrDefault(e => e.Entity.InventoryItemId == shipmentReceipt.InventoryItemId)
                                ?.Entity
                            ?? await _context.InventoryItems
                                .FirstOrDefaultAsync(ii => ii.InventoryItemId == shipmentReceipt.InventoryItemId);

        if (inventoryItem == null)
            throw new InvalidOperationException(
                $"InventoryItem {shipmentReceipt.InventoryItemId} not found for ShipmentReceipt {receiptId}.");

        var acctgTransEntries = new List<AcctgTransEntry>();

        //get partyAcctPreference by organizationPartyId
        var partyAcctgPreference =
            await _acctgMiscService.GetPartyAccountingPreferences(inventoryItem.OwnerPartyId!);


        // Logic to calculate unit cost based on preferences
        decimal unitCost;
        decimal totalLandedCostForFullQty = 0;


        if (string.IsNullOrEmpty(shipmentReceipt.OrderId) || string.IsNullOrEmpty(shipmentReceipt.OrderItemSeqId))
        {
            // Fallback if no order link — fallback to inventory item cost
            unitCost = (decimal)(inventoryItem.UnitCost ?? 0m);
        }
        else
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi =>
                    oi.OrderId == shipmentReceipt.OrderId &&
                    oi.OrderItemSeqId == shipmentReceipt.OrderItemSeqId);

            if (orderItem == null || orderItem.UnitPrice == null)
            {
                unitCost = (decimal)(inventoryItem.UnitCost ?? 0m);
            }
            else
            {
                decimal baseUnitPrice = (decimal)orderItem.UnitPrice;
                decimal orderedQty = (decimal)(orderItem.Quantity ?? 1m);

                // Get ALL adjustments linked to this order item (gratuities, shipping, discount, etc.)
                var adjustments = await _context.OrderAdjustments
                    .Where(adj =>
                        adj.OrderId == shipmentReceipt.OrderId &&
                        adj.OrderItemSeqId == shipmentReceipt.OrderItemSeqId &&
                        adj.Amount != null)
                    .Select(adj => adj.Amount ?? 0m)
                    .ToListAsync();

                decimal totalAdjustmentAmount = adjustments.Sum();

                // Total cost for the entire ordered quantity
                totalLandedCostForFullQty = (baseUnitPrice * orderedQty) + totalAdjustmentAmount;

                // Unit cost = total landed cost ÷ ordered quantity
                unitCost = orderedQty > 0
                    ? totalLandedCostForFullQty / orderedQty
                    : baseUnitPrice;

                inventoryItem.UnitCost = unitCost;
            }
        }

        // get shipment using receiptId
        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentReceipt!.ShipmentId);


        // Logic to calculate origAmount, create inventory item detail, and prepare entries.
        // totalLandedCostForFullQty is only set on the order-item path above; when there is no order
        // link, or the order item is missing/has no price, it is still 0 — and posting that booked a
        // silent zero-value receipt. Fall back to the unit cost actually resolved for the accepted
        // quantity, and refuse to post a receipt that cannot be valued at all.
        var origAmount = totalLandedCostForFullQty > 0
            ? totalLandedCostForFullQty
            : unitCost * (shipmentReceipt.QuantityAccepted ?? 0m);

        if (origAmount <= 0)
            throw new InvalidOperationException(
                $"Cannot value ShipmentReceipt {receiptId}: no order item price and no inventory unit cost.");

        var createInventoryItemDetailParam = new CreateInventoryItemDetailParam
        {
            InventoryItemId = inventoryItem.InventoryItemId,
            AccountingQuantityDiff = shipmentReceipt.QuantityAccepted
        };
        await _inventoryService.CreateInventoryItemDetail(createInventoryItemDetailParam);

        //prepare the double posting (D/C) entries (AcctgTransEntry
        var stamp = DateTime.UtcNow;
        var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");

        string? workEffortId = null;
        string? projectId = null;
        string? certificateCategory = null;

        if (!string.IsNullOrEmpty(shipmentReceipt.OrderId))
        {
            var workEffort = await _context.WorkEfforts
                .Where(we => we.RelatedOrderId == shipmentReceipt.OrderId)
                .Select(we => new
                {
                    we.WorkEffortId, we.ProjectId, we.CertificateCategory
                }) // REFACTOR: Project both needed IDs in a single query for efficiency
                .FirstOrDefaultAsync();

            if (workEffort != null)
            {
                workEffortId = workEffort.WorkEffortId;
                projectId = workEffort
                    .ProjectId; // REFACTOR: Extract ProjectId alongside WorkEffortId to avoid multiple DB queries
                certificateCategory = workEffort.CertificateCategory;
            }
        }

        // Logic to determine credit account type. SUPPLY_PROCUREMENT_CERTIFICATE receipts book to the
        // UNINVOICED_SHIP_RCPT clearing account (GL 214000) instead of ACCOUNTS_PAYABLE directly: the
        // purchase invoice is auto-generated right after this receipt (see
        // ReceiveInventoryFromPurchaseOrder.cs) and is what now credits the vendor's real
        // ACCOUNTS_PAYABLE — crediting AP here too would double the liability for the same delivery.
        // Other certificate categories (e.g. workmanship) keep crediting ACCOUNTS_PAYABLE directly, as
        // before, since they don't have a matching invoice posted right after receipt.
        var creditAccountTypeId = !string.IsNullOrEmpty(shipmentReceipt?.ReturnId)
            ? "COGS_ACCOUNT"
            : certificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE"
                ? "UNINVOICED_SHIP_RCPT"
                : "ACCOUNTS_PAYABLE";

        //Credit
        var creditEntry = new AcctgTransEntry
        {
            AcctgTransId = newAcctgTransSequence,
            AcctgTransEntrySeqId = "1",
            AcctgTransEntryTypeId = "_NA_",
            DebitCreditFlag = "C",
            OrganizationPartyId = inventoryItem.OwnerPartyId,
            GlAccountTypeId = creditAccountTypeId,
            OrigAmount = origAmount,
            ProductId = inventoryItem.ProductId,
            CurrencyUomId = partyAcctgPreference.BaseCurrencyUomId,
            OrigCurrencyUomId = inventoryItem.CurrencyUomId,
            PartyId = shipment.PartyIdFrom,
            RoleTypeId = "BILL_FROM_VENDOR",
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        acctgTransEntries.Add(creditEntry);

        // get project record from workefforts using workEffortId
        var project = await _context.WorkEfforts
            .FirstOrDefaultAsync(we => we.WorkEffortId == projectId);

        //Debit
        var debitEntry = new AcctgTransEntry
        {
            AcctgTransId = newAcctgTransSequence,
            AcctgTransEntrySeqId = "2",
            AcctgTransEntryTypeId = "_NA_",
            DebitCreditFlag = "D",
            OrganizationPartyId = inventoryItem.OwnerPartyId,
            GlAccountTypeId = "INVENTORY_ACCOUNT",
            //GlAccountId = project?.GlAccountId,
            OrigAmount = origAmount,
            ProductId = inventoryItem.ProductId,
            CurrencyUomId = partyAcctgPreference.BaseCurrencyUomId,
            OrigCurrencyUomId = inventoryItem.CurrencyUomId,
            PartyId = shipment.PartyIdFrom,
            RoleTypeId = "BILL_FROM_VENDOR",
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        acctgTransEntries.Add(debitEntry);

        var createAcctgTransAndEntriesParams = new CreateAcctgTransAndEntriesParams
        {
            AcctgTransEntries = acctgTransEntries,
            GlFiscalTypeId = "ACTUAL",
            AcctgTransTypeId = "SHIPMENT_RECEIPT",
            ShipmentId = shipmentReceipt.ShipmentId,
            PaymentId = null,
            PartyId = shipment.PartyIdFrom,
            TransactionDate = shipmentReceipt.DatetimeReceived,
            WorkEffortId = workEffortId,
            RoleTypeId = "BILL_FROM_VENDOR"
        };

        var acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesParams);

        return acctgTransId;
    }

    public async Task<string> CreateAcctgTransAndEntriesForIncomingPayment(string paymentId)
    {
        try
        {
            // 1. Retrieve the payment by ID
            var payment = await _context.Payments.FindAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with ID {paymentId} was not found.");
                return null;
            }

            // 2. Determine if this payment is a receipt (incoming)
            var paymentParentTypeId = await _utilityService.GetPaymentParentType(payment.PaymentTypeId);
            if (paymentParentTypeId != "RECEIPT")
            {
                // Not an incoming payment, so skip creating AcctgTrans for it.
                _logger.LogInformation($"Payment {paymentId} is not a receipt. Skipping acctg trans creation.");
                return null;
            }

            // 2b. Idempotency guard: a payment must never accumulate more than one posted
            // INCOMING_PAYMENT AcctgTrans. Without this, any path that flips a payment back
            // to PMNT_NOT_PAID without deleting its prior posting (the generic SetPaymentStatus
            // transition does this — only the dedicated Reset action cleans up AcctgTrans) and
            // then re-approves it will silently double-post the same amount to GL.
            var existingAcctgTransId = await _context.AcctgTrans
                .Where(t => t.PaymentId == paymentId &&
                            t.AcctgTransTypeId == "INCOMING_PAYMENT" &&
                            t.IsPosted == "Y")
                .Select(t => t.AcctgTransId)
                .FirstOrDefaultAsync();

            if (existingAcctgTransId != null)
            {
                _logger.LogWarning(
                    $"Payment {paymentId} already has posted AcctgTrans {existingAcctgTransId}. Skipping duplicate GL posting.");
                return existingAcctgTransId;
            }

            // 3. Prepare some reusable values
            var stamp = DateTime.UtcNow;
            var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");

            // Ofbiz logic: origAmount = payment.actualCurrencyAmount (fallback to payment.amount if null)
            var origAmount = payment.ActualCurrencyAmount ?? payment.Amount;
            var origCurrencyUomId = payment.ActualCurrencyUomId ?? payment.CurrencyUomId;
            var amount = payment.Amount;
            var currencyUomId = payment.CurrencyUomId;

            // 4. Create the Debit Entry
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "1",
                AcctgTransEntryTypeId = "_NA_", // Matches Ofbiz logic
                DebitCreditFlag = "D", // Debit
                Amount = amount,
                CurrencyUomId = currencyUomId,
                OrigAmount = origAmount,
                OrigCurrencyUomId = origCurrencyUomId,
                OrganizationPartyId = payment.PartyIdTo, // Payment is incoming -> 'partyIdTo' is the org
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            // 5. Lookup the GL Account Type for the credit entry
            //    This matches the PaymentGlAccountTypeMap logic in Ofbiz
            var creditGlAccountTypeId = await _context.PaymentGlAccountTypeMaps
                .Where(map =>
                    map.PaymentTypeId == payment.PaymentTypeId &&
                    map.OrganizationPartyId == payment.PartyIdTo)
                .Select(map => map.GlAccountTypeId)
                .FirstOrDefaultAsync();

            // 6. Create the Credit Entry (with “diff amount” in Ofbiz)
            var creditEntryWithDiffAmount = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "2",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "C", // Credit
                Amount = amount,
                CurrencyUomId = currencyUomId,
                OrigAmount = origAmount,
                OrigCurrencyUomId = origCurrencyUomId,
                GlAccountId = payment.OverrideGlAccountId, // If present, override
                GlAccountTypeId = creditGlAccountTypeId, // Derived above
                OrganizationPartyId = payment.PartyIdTo,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            // 7. Prepare the header info for AcctgTrans
            //    (Ofbiz sets fields like: glFiscalTypeId, acctgTransTypeId, etc.)
            var createParams = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                PartyId = payment.PartyIdFrom, // The “from” party is the customer for a receipt
                RoleTypeId = "BILL_TO_CUSTOMER",
                Description = payment.Comments,
                PaymentId = payment.PaymentId,
                SalesRequestId = payment.SalesRequestId,
                AcctgTransTypeId = "INCOMING_PAYMENT",
                TransactionDate = payment.EffectiveDate,

                AcctgTransEntries = new List<AcctgTransEntry>
                {
                    debitEntry,
                    creditEntryWithDiffAmount
                }
            };

            // 8. Call your service or method to create the AcctgTrans and entries
            var acctgTransId = await CreateAcctgTransAndEntries(createParams);
            _logger.LogInformation($"Created AcctgTrans {acctgTransId} for payment {paymentId}.");

            // 9. For each PaymentApplication, create separate accounting transactions
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == paymentId)
                .ToListAsync();

            foreach (var paymentApplication in paymentApplications)
            {
                var acctgTransIdForPaymentApplication =
                    await CreateAcctgTransAndEntriesForPaymentApplication(paymentApplication.PaymentApplicationId);

                _logger.LogInformation(
                    $"Accounting transaction {acctgTransIdForPaymentApplication} created for payment application {paymentApplication.PaymentApplicationId}."
                );
            }

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating AcctgTrans for payment {paymentId}");
            return null;
        }
    }

    public async Task<string> CreateAcctgTransAndEntriesForPaymentApplication(string paymentApplicationId)
    {
        try
        {
            // 1. Retrieve the PaymentApplication
            var paymentApplication = await _context.PaymentApplications
                .FirstOrDefaultAsync(pa => pa.PaymentApplicationId == paymentApplicationId);

            if (paymentApplication == null)
            {
                _logger.LogWarning($"PaymentApplication with ID {paymentApplicationId} was not found.");
                return null;
            }

            // 2. Retrieve the Payment associated with this PaymentApplication
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with ID {paymentApplication.PaymentId} was not found.");
                return null;
            }

            // 3. If the payment transaction has not been posted to GL yet, do nothing:
            //    In Ofbiz, if payment.statusId == "PMNT_NOT_PAID", we skip creating app-level transactions.
            if (payment.StatusId == "PMNT_NOT_PAID")
            {
                _logger.LogInformation(
                    $"Payment {payment.PaymentId} not yet posted to GL. Skipping PaymentApplication {paymentApplicationId}.");
                return null;
            }

            // 4. Check if this is a 'RECEIPT' or a 'DISBURSEMENT'
            //    Ofbiz calls `isReceipt`; here we compare to "RECEIPT" based on the PaymentType's parent.
            var parentPaymentType = await _utilityService.GetPaymentParentType(payment.PaymentTypeId);

            // We'll collect AcctgTransEntry objects in a list
            var acctgTransEntries = new List<AcctgTransEntry>();
            int seqNum = 1; // Initialize sequence number for AcctgTransEntrySeqId

            AcctgTransEntry debitEntry = null;
            AcctgTransEntry creditEntry = null;

            // 5. If it is an incoming payment ("RECEIPT"):
            if (parentPaymentType == "RECEIPT")
            {
                // 5.1. Check if the PaymentGlAccountTypeMap for the organization party is already ACCOUNTS_RECEIVABLE
                var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                    .FirstOrDefaultAsync(map =>
                        map.PaymentTypeId == payment.PaymentTypeId &&
                        map.OrganizationPartyId == payment.PartyIdTo);

                if (paymentGlAccountTypeMap?.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
                {
                    // If the credited account is already accounts receivable, do nothing
                    _logger.LogInformation(
                        $"Payment {payment.PaymentId} credited account is 'ACCOUNTS_RECEIVABLE'. Skipping PaymentApplication {paymentApplicationId}.");
                    return null;
                }

                // 5.2. Build the DEBIT entry
                debitEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = payment.PartyIdTo, // The org party for incoming payment
                    PartyId = payment.PartyIdFrom, // The customer
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountId = payment.OverrideGlAccountId, // If present
                    GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId // Mapped from PaymentType & PartyIdTo
                };
                seqNum++; // Increment sequence number


                // 5.3. Build the CREDIT entry
                creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    OrganizationPartyId = payment.PartyIdTo,
                    PartyId = payment.PartyIdFrom,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE" // Hard-coded to AR for receipts
                };
                seqNum++; // Increment sequence number


                acctgTransEntries.Add(debitEntry);
                acctgTransEntries.Add(creditEntry);
            }
            // 6. Otherwise, it's an outgoing payment ("DISBURSEMENT" or another type)
            else
            {
                // 6.1. If the PaymentGlAccountTypeMap for the paying party is ACCOUNTS_PAYABLE, skip
                var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                    .FirstOrDefaultAsync(map =>
                        map.PaymentTypeId == payment.PaymentTypeId &&
                        map.OrganizationPartyId == payment.PartyIdFrom);

                if (paymentGlAccountTypeMap?.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                {
                    _logger.LogInformation(
                        $"Payment {payment.PaymentId} debited account is 'ACCOUNTS_PAYABLE'. Skipping PaymentApplication {paymentApplicationId}.");
                    return null;
                }

                // 6.2. Get exchange rates for the invoice and the outgoing payment
                decimal? invoiceExchangeRate =
                    await _acctgMiscService.GetGlExchangeRateOfPurchaseInvoice(paymentApplication);
                decimal? paymentExchangeRate =
                    await _acctgMiscService.GetGlExchangeRateOfOutgoingPayment(paymentApplication);

                // Fallback to 1.0 if null
                var ieRate = invoiceExchangeRate ?? 1.0m;
                var payRate = paymentExchangeRate ?? 1.0m;

                // 6.3. Build the CREDIT entry (the org paying out)
                creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    OrganizationPartyId = payment.PartyIdFrom, // The org
                    PartyId = payment.PartyIdTo, // The vendor
                    RoleTypeId = "BILL_FROM_VENDOR",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    Amount = paymentApplication.AmountApplied * payRate,
                    GlAccountId = payment.OverrideGlAccountId,
                    GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId
                };

                acctgTransEntries.Add(creditEntry);
                seqNum++; // Increment sequence number


                // 6.4. If invoiceRate != paymentRate, add an FX gain/loss entry
                if (ieRate != payRate)
                {
                    var fxGainLossEntry = new AcctgTransEntry
                    {
                        AcctgTransEntrySeqId = seqNum.ToString("D2"),
                        AcctgTransEntryTypeId = "_NA_",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        DebitCreditFlag = "D",
                        OrganizationPartyId = payment.PartyIdFrom,
                        PartyId = payment.PartyIdTo,
                        RoleTypeId = "BILL_FROM_VENDOR",
                        Amount = paymentApplication.AmountApplied * (payRate - ieRate),
                        GlAccountTypeId = "FX_GAIN_LOSS_ACCT"
                    };
                    acctgTransEntries.Add(fxGainLossEntry);
                    seqNum++; // Increment sequence number
                }

                // 6.5. Now build the DEBIT entry for Accounts Payable
                debitEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    OrigAmount = paymentApplication.AmountApplied,
                    Amount = paymentApplication.AmountApplied * ieRate,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountTypeId = "ACCOUNTS_PAYABLE"
                };

                // 6.6. Override GL account or handle tax authority
                if (!string.IsNullOrEmpty(paymentApplication.OverrideGlAccountId))
                {
                    debitEntry.GlAccountId = paymentApplication.OverrideGlAccountId;
                }
                else if (!string.IsNullOrEmpty(paymentApplication.TaxAuthGeoId))
                {
                    // Attempt to retrieve a TaxAuthorityGlAccount
                    var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
                        .FirstOrDefaultAsync(gl =>
                            gl.OrganizationPartyId == payment.PartyIdFrom &&
                            gl.TaxAuthGeoId == paymentApplication.TaxAuthGeoId &&
                            gl.TaxAuthPartyId == payment.PartyIdTo);

                    if (taxAuthorityGlAccount != null)
                        debitEntry.GlAccountId = taxAuthorityGlAccount.GlAccountId;
                }

                acctgTransEntries.Add(debitEntry);
                seqNum++; // Increment sequence number
            }

            // 7. Prepare the parameters for creating the AcctgTrans (header)
            //    (Mirrors the last block of logic in Ofbiz that sets "acctgTransEntries", "acctgTransTypeId", etc.)
            var createParams = new CreateAcctgTransAndEntriesParams
            {
                AcctgTransEntries = acctgTransEntries,
                AcctgTransTypeId = "PAYMENT_APPL",
                GlFiscalTypeId = "ACTUAL",
                PaymentId = paymentApplication.PaymentId,
                InvoiceId = paymentApplication.InvoiceId,
                PartyId = parentPaymentType == "RECEIPT" ? payment.PartyIdFrom : payment.PartyIdTo,
                RoleTypeId = parentPaymentType == "RECEIPT" ? "BILL_TO_CUSTOMER" : "BILL_FROM_VENDOR",
                TransactionDate = payment.EffectiveDate
            };

            // 8. Call your existing method to create the accounting transaction and entries
            var acctgTransId = await CreateAcctgTransAndEntries(createParams);

            // 9. Return the newly created accounting transaction ID
            //_logger.LogInformation($"AcctgTrans {acctgTransId} created for PaymentApplication {paymentApplicationId}.");
            return acctgTransId;
        }
        catch (Exception ex)
        {
            // 10. Error handling
            _logger.LogError(ex,
                $"Error creating accounting transaction for PaymentApplication {paymentApplicationId}");
            // Re-throw or return null based on your preferred error-handling strategy.
            // Typically, we might re-throw to bubble up the exception:
            throw new Exception($"An error occurred while processing payment application {paymentApplicationId}", ex);
        }
    }

    /// <summary>
    /// Creates an AcctgTrans plus two offsetting AcctgTransEntry records (Debit and Credit).
    /// Mirrors the Ofbiz quickCreateAcctgTransAndEntries design.
    /// </summary>
    public async Task<string> QuickCreateAcctgTransAndEntries(CreateQuickAcctgTransAndEntriesParams parameters)
    {
        try
        {
            // 0) Basic validation
            if (string.IsNullOrEmpty(parameters.DebitGlAccountId))
                throw new ApplicationException("Missing debitGlAccountId (required).");

            if (string.IsNullOrEmpty(parameters.CreditGlAccountId))
                throw new ApplicationException("Missing creditGlAccountId (required).");

            if (parameters.Amount <= 0)
                throw new ApplicationException("Amount must be > 0.");

            // 1) Create the AcctgTrans record
            var stamp = DateTime.UtcNow;

            var acctgTransParams = new CreateAcctgTransParams
            {
                GlFiscalTypeId = parameters.GlFiscalTypeId,
                AcctgTransTypeId = parameters.AcctgTransTypeId,
                InvoiceId = parameters.InvoiceId,
                PaymentId = parameters.PaymentId,
                ShipmentId = parameters.ShipmentId,
                PartyId = parameters.PartyId,
                RoleTypeId = parameters.RoleTypeId,
                TransactionDate = parameters.TransactionDate != default
                    ? parameters.TransactionDate
                    : DateOnly.FromDateTime(DateTime.UtcNow), // fallback if not set
                Description = parameters.Description,
                IsPosted = parameters.IsPosted ?? "N" // ensure a default of "N"
            };

            // This is your existing service or method to create an AcctgTrans row
            var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

            // 2) Create the Debit AcctgTransEntry record
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = "1",
                GlAccountId = parameters.DebitGlAccountId,
                DebitCreditFlag = "D", // 'D' = Debit
                AcctgTransEntryTypeId = "_NA_", // placeholder
                Amount = parameters.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED", // typical default
                PartyId = parameters.PartyId, // optional
                ProductId = null, // set if relevant
                Description = parameters.Description,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            await _acctgTransService.CreateAcctgTransEntry(debitEntry);

            // 3) Create the Credit AcctgTransEntry record
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = "2",
                GlAccountId = parameters.CreditGlAccountId,
                DebitCreditFlag = "C", // 'C' = Credit
                AcctgTransEntryTypeId = "_NA_", // placeholder
                Amount = parameters.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                PartyId = parameters.PartyId,
                ProductId = null,
                Description = parameters.Description,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };

            await _acctgTransService.CreateAcctgTransEntry(creditEntry);

            // Return the newly created AcctgTransId
            return acctgTransId;
        }
        catch (Exception ex)
        {
            // The try/catch ensures we return a meaningful message if something fails.
            throw new ApplicationException($"Error in QuickCreateAcctgTransAndEntries: {ex.Message}", ex);
        }
    }

    public async Task<string> CreateAcctgTransForWorkEffortIssuance(string workEffortId, string inventoryItemId)
    {
        try
        {
            // Fetch WorkEffort entity
            var workEffort = await _context.WorkEfforts
                .FindAsync(workEffortId);
            if (workEffort == null) throw new Exception($"WorkEffort with ID {workEffortId} not found.");

            WorkEffortGoodStandard workEffortGoodStandard = null;

            // If WorkEffort type is 'PROD_ORDER_TASK' and has a parent
            if (workEffort.WorkEffortTypeId == "PROD_ORDER_TASK" &&
                !string.IsNullOrEmpty(workEffort.WorkEffortParentId))
                // Fetch WorkEffortGoodStandard associated with the parent WorkEffort
                workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .Where(wgs => wgs.WorkEffortId == workEffort.WorkEffortParentId &&
                                  wgs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                    .OrderByDescending(wgs => wgs.FromDate)
                    .FirstOrDefaultAsync();

            // Fetch WorkEffortInventoryAssign entity

            var workEffortInventoryAssign =
                await _utilityService.FindLocalOrDatabaseAsync<WorkEffortInventoryAssign>(workEffortId,
                    inventoryItemId);

            if (workEffortInventoryAssign == null)
                throw new Exception(
                    $"WorkEffortInventoryAssign with WorkEffortId {workEffortId} and InventoryItemId {inventoryItemId} not found.");

            // Fetch related InventoryItem entity
            var inventoryItem = await _context.InventoryItems
                .FindAsync(inventoryItemId);
            if (inventoryItem == null) throw new Exception($"InventoryItem with ID {inventoryItemId} not found.");

            // Calculate the original amount
            var origAmount = workEffortInventoryAssign.Quantity * (double?)inventoryItem.UnitCost;

            var stamp = DateTime.UtcNow;
            var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");

            // Create Debit entry for WIP_INVENTORY
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "1",
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "D",
                GlAccountTypeId = "WIP_INVENTORY",
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = workEffortGoodStandard?.ProductId,
                OrigAmount = (decimal?)origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                CurrencyUomId = inventoryItem.CurrencyUomId // Assuming same currency
            };

            // Create Credit entry for RAWMAT_INVENTORY
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "2",
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "C",
                GlAccountTypeId = "RAWMAT_INVENTORY",
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = inventoryItem.ProductId,
                OrigAmount = (decimal?)origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                CurrencyUomId = inventoryItem.CurrencyUomId // Assuming same currency
            };

            // Prepare the accounting transaction creation input map
            var acctgTransEntries = new List<AcctgTransEntry> { debitEntry, creditEntry };
            var acctgTransInMap = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",

                AcctgTransTypeId = "INVENTORY",
                WorkEffortId = workEffortId,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AcctgTransEntries = acctgTransEntries
            };

            // Call the createAcctgTransAndEntries service
            var acctgTransId = await CreateAcctgTransAndEntries(acctgTransInMap);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error creating accounting transaction for WorkEffortId {workEffortId} and InventoryItemId {inventoryItemId}: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CreateAcctgTransForSalesShipmentIssuance(string itemIssuanceId)
    {
        // Initialize variables
        string acctgTransId = null;
        var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams();
        var acctgTransEntries = new List<AcctgTransEntry>();
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        var ledgerDecimals = glSettings.DecimalScale;
        var roundingMode = glSettings.RoundingMode;

        // Retrieve ItemIssuance entity
        var itemIssuance = await _utilityService.FindLocalOrDatabaseAsync<ItemIssuance>(itemIssuanceId);

        if (itemIssuance == null) throw new ArgumentException($"ItemIssuance with ID {itemIssuanceId} not found.");

        // Retrieve related InventoryItem
        var inventoryItem = await _context.InventoryItems.FindAsync(itemIssuance.InventoryItemId);
        if (inventoryItem == null)
            throw new ArgumentException($"InventoryItem with ID {itemIssuance.InventoryItemId} not found.");

        // Retrieve Bill-To Customers
        var billToCustomer = await _utilityService.GetOrderRole(itemIssuance.OrderId, "BILL_TO_CUSTOMER");

        // Prepare the double posting (D/C) entries (AcctgTransEntry)
        // Credit Entry
        // TODO: handle serialized inventory

        // Retrieve Party Accounting Preferences
        var partyAcctgPreference =
            await _acctgMiscService.GetPartyAccountingPreferences(inventoryItem.OwnerPartyId);


        var totalAmount = 0.0m;
        var cogsMethodId = partyAcctgPreference.CogsMethodId;

        if (cogsMethodId == "COGS_INV_COST" || cogsMethodId == "COGS_AVG_COST")
        {
            decimal unitCost;
            if (cogsMethodId == "COGS_AVG_COST")
                unitCost = await _costService.GetProductAverageCost(inventoryItem);
            else
                unitCost = (decimal)inventoryItem.UnitCost;


            // Calculate totalAmount with proper rounding
            totalAmount = _acctgMiscService.CustomRound(
                (decimal)(itemIssuance.Quantity * unitCost),
                (int)ledgerDecimals,
                roundingMode
            );

            // Create Credit Entry
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "01",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "C",
                GlAccountTypeId = "INVENTORY_ACCOUNT",
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = inventoryItem.ProductId,
                InventoryItemId = inventoryItem.InventoryItemId,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                OrigAmount = totalAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                PartyId = billToCustomer?.PartyId,
                RoleTypeId = billToCustomer?.RoleTypeId,
                LastUpdatedStamp = DateTime.UtcNow,
                CreatedStamp = DateTime.UtcNow
            };
            acctgTransEntries.Add(creditEntry);
        }
        else if (cogsMethodId == "COGS_FIFO" || cogsMethodId == "COGS_LIFO")
        {
            var isFifo = cogsMethodId == "COGS_FIFO";

            // Directly query InventoryItems based on conditions
            var query = _context.InventoryItems
                .Where(item =>
                    item.OwnerPartyId == inventoryItem.OwnerPartyId &&
                    item.FacilityId == inventoryItem.FacilityId &&
                    item.ProductId == inventoryItem.ProductId &&
                    item.AccountingQuantityTotal > 0.0m);

            // Apply ordering based on COGS method
            if (isFifo)
                query = query.OrderBy(item => item.DatetimeReceived); // FIFO: oldest first
            else
                query = query.OrderByDescending(item => item.DatetimeReceived); // LIFO: newest first

            var costInventoryItems = await query.ToListAsync();

            var remainingQuantity = (decimal)itemIssuance.Quantity;

            foreach (var costInventoryItem in costInventoryItems)
            {
                if (remainingQuantity <= 0.0m)
                    break;

                decimal costInventoryItemQuantity;
                if (remainingQuantity <= costInventoryItem.AccountingQuantityTotal)
                {
                    costInventoryItemQuantity = remainingQuantity;
                    remainingQuantity = 0.0m;
                }
                else
                {
                    costInventoryItemQuantity = (decimal)costInventoryItem.AccountingQuantityTotal;
                    remainingQuantity -= (decimal)costInventoryItem.AccountingQuantityTotal;
                }

                // Create Inventory Item Detail
                var createDetailMap = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = costInventoryItem.InventoryItemId,
                    AccountingQuantityDiff = -1 * costInventoryItemQuantity
                };
                await _inventoryService.CreateInventoryItemDetail(createDetailMap);

                var costInventoryItemAmount = _acctgMiscService.CustomRound(
                    (decimal)(costInventoryItemQuantity * costInventoryItem.UnitCost),
                    (int)ledgerDecimals, // Ensure this is an int representing decimal places
                    roundingMode // Pass the rounding mode string
                );
                totalAmount += costInventoryItemAmount;

                // Create Credit Entry
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = "01",
                    AcctgTransEntryTypeId = "_NA_",
                    DebitCreditFlag = "C",
                    GlAccountTypeId = "INVENTORY_ACCOUNT",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    OrganizationPartyId = inventoryItem.OwnerPartyId,
                    ProductId = inventoryItem.ProductId,
                    InventoryItemId = costInventoryItem.InventoryItemId,
                    OrigAmount = costInventoryItemAmount,
                    OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                    PartyId = billToCustomer?.PartyId,
                    RoleTypeId = billToCustomer?.RoleTypeId,
                    LastUpdatedStamp = DateTime.UtcNow,
                    CreatedStamp = DateTime.UtcNow
                };
                acctgTransEntries.Add(creditEntry);
            }

            if (remainingQuantity > 0.0m)
                throw new InvalidOperationException("Accounting not find accounting inventory.");
        }
        else
        {
            throw new InvalidOperationException("Accounting COGS costing method is not supported.");
        }

        // Debit Entry
        var debitEntry = new AcctgTransEntry
        {
            AcctgTransEntrySeqId = "02",
            AcctgTransEntryTypeId = "_NA_",
            DebitCreditFlag = "D",
            GlAccountTypeId = "COGS_ACCOUNT",
            OrganizationPartyId = inventoryItem.OwnerPartyId,
            ProductId = inventoryItem.ProductId,
            ReconcileStatusId = "AES_NOT_RECONCILED",
            OrigAmount = totalAmount,

            OrigCurrencyUomId = inventoryItem.CurrencyUomId,
            PartyId = billToCustomer?.PartyId,
            RoleTypeId = billToCustomer?.RoleTypeId,
            LastUpdatedStamp = DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow
        };
        acctgTransEntries.Add(debitEntry);

        // Set header fields
        createAcctgTransAndEntriesInMap.GlFiscalTypeId = "ACTUAL";
        createAcctgTransAndEntriesInMap.AcctgTransTypeId = "SALES_SHIPMENT";
        createAcctgTransAndEntriesInMap.ShipmentId = itemIssuance.ShipmentId;
        createAcctgTransAndEntriesInMap.InventoryItemId = inventoryItem.InventoryItemId;
        createAcctgTransAndEntriesInMap.TransactionDate = itemIssuance.IssuedDateTime;
        createAcctgTransAndEntriesInMap.AcctgTransEntries = acctgTransEntries;

        // Create Accounting Transaction and Entries
        acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

        return acctgTransId;
    }

    public async Task<string> CreateAcctgTransForSalesInvoice(string invoiceId)
    {
        // Retrieve ledger rounding properties
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        var ledgerDecimals = glSettings.DecimalScale;
        var roundingMode = glSettings.RoundingMode;

        decimal totalOrigAmount = 0;

        try
        {
            // Retrieve invoice details
            var invoice = await _utilityService.FindLocalOrDatabaseAsync<Invoice>(invoiceId);

            if (invoice == null) throw new ArgumentException("Invoice not found.");

            if (invoice.InvoiceTypeId != "SALES_INVOICE")
            {
                _logger.LogWarning(
                    $"Invoice {invoiceId} is not of type 'SALES_INVOICE'. No accounting transaction created.");
                return null;
            }

            // Retrieve invoice items excluding taxable items
            var taxableItemTypeIds = new List<string> { "INV_SALES_TAX", "ITM_SALES_TAX" };
            var invoiceItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(query =>
                query.Where(ii =>
                    ii.InvoiceId == invoiceId && !taxableItemTypeIds.Contains(ii.InvoiceItemTypeId)));

            var acctgTransEntries = new List<AcctgTransEntry>();
            int seqNum = 1; // Initialize sequence number for AcctgTransEntrySeqId

            foreach (var invoiceItem in invoiceItems)
            {
                // TODO: handle serialized inventory

                decimal quantity = 1;
                if (invoiceItem.Quantity != null) quantity = invoiceItem.Quantity.Value;

                var origAmount = _acctgMiscService.CustomRound(quantity * (invoiceItem.Amount ?? 0),
                    (int)ledgerDecimals,
                    roundingMode);

                totalOrigAmount =
                    _acctgMiscService.CustomRound(totalOrigAmount + origAmount, (int)ledgerDecimals, roundingMode);

                // Create credit entry
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    GlAccountTypeId = invoiceItem.InvoiceItemTypeId,
                    OrganizationPartyId = invoice.PartyIdFrom,
                    ProductId = invoiceItem.ProductId,
                    OrigAmount = origAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    GlAccountId = invoiceItem.OverrideGlAccountId
                };

                if (!string.IsNullOrEmpty(invoiceItem.TaxAuthPartyId))
                {
                    creditEntry.PartyId = invoiceItem.TaxAuthPartyId;
                    creditEntry.RoleTypeId = "TAX_AUTHORITY";
                }

                acctgTransEntries.Add(creditEntry);
                seqNum++; // Increment sequence number
            }

            // Credit entries for SALES_TAX
            decimal invoiceTaxTotal = 0;

            var taxAuthPartyAndGeos = await _invoiceUtilityService.GetInvoiceTaxAuthPartyAndGeos(invoice.InvoiceId);

            foreach (var taxAuthPartyId in taxAuthPartyAndGeos.Keys)
            {
                var taxAuthGeoIds = taxAuthPartyAndGeos[taxAuthPartyId];
                foreach (var taxAuthGeoId in taxAuthGeoIds)
                {
                    var taxAmount = await _invoiceUtilityService.GetInvoiceTaxTotalForTaxAuthPartyAndGeo(
                        invoice.InvoiceId, taxAuthPartyId, taxAuthGeoId);

                    var creditEntry = new AcctgTransEntry
                    {
                        AcctgTransEntrySeqId = seqNum.ToString("D2"),
                        AcctgTransEntryTypeId = "_NA_",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        DebitCreditFlag = "C",
                        OrganizationPartyId = invoice.PartyIdFrom,
                        OrigAmount = taxAmount,
                        OrigCurrencyUomId = invoice.CurrencyUomId,
                        PartyId = taxAuthPartyId,
                        RoleTypeId = "TAX_AUTHORITY"
                    };

                    // Retrieve taxAuthorityGlAccount
                    var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
                        .FirstOrDefaultAsync(t => t.OrganizationPartyId == creditEntry.OrganizationPartyId &&
                                                  t.TaxAuthGeoId == taxAuthGeoId &&
                                                  t.TaxAuthPartyId == taxAuthPartyId);

                    if (taxAuthorityGlAccount != null)
                        creditEntry.GlAccountId = taxAuthorityGlAccount.GlAccountId;
                    else
                        creditEntry.GlAccountTypeId = "TAX_ACCOUNT";

                    acctgTransEntries.Add(creditEntry);
                    seqNum++; // Increment sequence number

                    invoiceTaxTotal =
                        _acctgMiscService.CustomRound(invoiceTaxTotal + taxAmount, (int)ledgerDecimals, roundingMode);
                }
            }

            // Another entry for tax not attributed to a taxAuthPartyId
            var unattributedTaxAmount = await _invoiceUtilityService.GetInvoiceUnattributedTaxTotal(invoice.InvoiceId);

            if (unattributedTaxAmount != 0)
            {
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    OrganizationPartyId = invoice.PartyIdFrom,
                    OrigAmount = unattributedTaxAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    GlAccountTypeId = "TAX_ACCOUNT"
                };

                acctgTransEntries.Add(creditEntry);
                seqNum++; // Increment sequence number

                invoiceTaxTotal = _acctgMiscService.CustomRound(invoiceTaxTotal + unattributedTaxAmount,
                    (int)ledgerDecimals,
                    roundingMode);
            }

            // Debit entry
            totalOrigAmount =
                _acctgMiscService.CustomRound(totalOrigAmount + invoiceTaxTotal, (int)ledgerDecimals, roundingMode);

            var debitEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "D",
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                OrganizationPartyId = invoice.PartyIdFrom,
                OrigAmount = totalOrigAmount,
                OrigCurrencyUomId = invoice.CurrencyUomId,
                PartyId = invoice.PartyId,
                RoleTypeId = "BILL_TO_CUSTOMER",
            };

            acctgTransEntries.Add(debitEntry);

            // Prepare parameters for createAcctgTransAndEntries
            var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "SALES_INVOICE",
                Description = invoice.Description,
                InvoiceId = invoiceId,
                PartyId = invoice.PartyId,
                RoleTypeId = "BILL_TO_CUSTOMER",
                AcctgTransEntries = acctgTransEntries,
                TransactionDate = invoice.InvoiceDate
            };

            // Call service createAcctgTransAndEntries
            var acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

            return acctgTransId;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"An error occurred while creating accounting transaction for sales invoice {invoiceId}.");
            throw;
        }
    }

    public async Task<string> CreateAcctgTransForSalesInvoice_2(string invoiceId)
    {
        try
        {
            // Retrieve ledger rounding properties
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            // Retrieve invoice
            var invoice = await _utilityService.FindLocalOrDatabaseAsync<Invoice>(invoiceId);
            if (invoice == null)
                throw new ArgumentException($"Invoice {invoiceId} not found.");

            if (invoice.InvoiceTypeId != "SALES_INVOICE")
            {
                _logger.LogWarning(
                    $"Invoice {invoiceId} is not of type 'SALES_INVOICE'. No accounting transaction created.");
                return null;
            }

            // Retrieve non-tax invoice items
            var taxableItemTypeIds = new List<string> { "INV_SALES_TAX", "ITM_SALES_TAX" };
            var invoiceItems = await _utilityService.FindLocalOrDatabaseListAsync<InvoiceItem>(query =>
                query.Where(ii => ii.InvoiceId == invoiceId
                                  && !taxableItemTypeIds.Contains(ii.InvoiceItemTypeId)));

            if (!invoiceItems.Any())
            {
                _logger.LogWarning($"No valid items found for invoice {invoiceId}.");
                return null;
            }

            var acctgTransEntries = new List<AcctgTransEntry>();
            int seqNum = 1;

            decimal totalDebitAmount = 0;

            foreach (var item in invoiceItems)
            {
                decimal quantity = item.Quantity ?? 1;
                decimal itemAmount = item.Amount ?? 0;

                decimal origAmount = _acctgMiscService.CustomRound(
                    quantity * itemAmount,
                    (int)ledgerDecimals,
                    roundingMode);

                totalDebitAmount = _acctgMiscService.CustomRound(
                    totalDebitAmount + origAmount,
                    (int)ledgerDecimals,
                    roundingMode);
                
                //get invoice item type from InvoiceItemTypes
                var invoiceItemType = await _context.InvoiceItemTypes.FindAsync(item.InvoiceItemTypeId);

                // === CREDIT ENTRY (Revenue) - Use DEFAULT_GL_ACCOUNT_ID from InvoiceItemType ===
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    OrganizationPartyId = invoice.PartyIdFrom,
                    ProductId = item.ProductId,
                    OrigAmount = origAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    // Credit Side: Use configured GL Account from InvoiceItemType
                    GlAccountId = invoiceItemType?.DefaultGlAccountId, // We'll set this below
                    GlAccountTypeId = null // We don't want to pass type when we have direct GlAccountId
                };

                acctgTransEntries.Add(creditEntry);
                seqNum++;

                // === DEBIT ENTRY (Asset / Override) - Use OverrideGlAccountId from InvoiceItem ===
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = invoice.PartyIdFrom,
                    ProductId = item.ProductId,
                    OrigAmount = origAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    // Debit Side: Use the Override GL Account set by user on this invoice item
                    GlAccountId = item.OverrideGlAccountId,
                    GlAccountTypeId = null,
                    // Party info for receivable (if needed)
                    PartyId = invoice.PartyId,
                    RoleTypeId = "BILL_TO_CUSTOMER"
                };

                acctgTransEntries.Add(debitEntry);
                seqNum++;
            }

            // Prepare parameters for CreateAcctgTransAndEntries
            var createParams = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "SALES_INVOICE",
                Description = invoice.Description,
                InvoiceId = invoiceId,
                PartyId = invoice.PartyId,
                RoleTypeId = "BILL_TO_CUSTOMER",
                AcctgTransEntries = acctgTransEntries,
                TransactionDate = invoice.InvoiceDate
            };

            // Create the transaction
            var acctgTransId = await CreateAcctgTransAndEntries(createParams);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"An error occurred while creating accounting transaction for sales invoice {invoiceId}.");
            throw;
        }
    }

    private async Task<PartyGlAccount> GetPartyGlAccount(string organizationPartyId, string partyId,
        string roleTypeId, string glAccountTypeId)
    {
        // Example usage:
        // ROLE_TYPE_ID= BILL_TO_CUSTOMER, GL_ACCOUNT_TYPE_ID = ACCOUNTS_RECEIVABLE,
        // ORGANIZATION_PARTY_ID = Company, PARTY_ID = DemoCustomer

        var partyGlAccountInventoryAccount =
            await _context.PartyGlAccounts.SingleOrDefaultAsync(x =>
                x.OrganizationPartyId == organizationPartyId
                && x.PartyId == partyId
                && x.GlAccountTypeId == glAccountTypeId
                && x.RoleTypeId == roleTypeId);
        return partyGlAccountInventoryAccount!;
    }


    private async Task<ProductGlAccount> GetProductGlAccount(string organizationPartyId, string productId,
        string glAccountTypeId)
    {
        // Example usage:
        // productId = 'GZ-1001' AND glAccountTypeId = 'INV_FPROD_ITEM' 
        // AND organizationPartyId = 'Company')

        var productGlAccountInventoryAccount =
            await _context.ProductGlAccounts.SingleOrDefaultAsync(x =>
                x.OrganizationPartyId == organizationPartyId
                && x.ProductId == productId
                && x.GlAccountTypeId == glAccountTypeId);
        return productGlAccountInventoryAccount!;
    }


    private async Task<GlAccountTypeDefault> GetDefaultGlAccount(string organizationPartyId, string glAccountTypeId)
    {
        var glAccountTypeDefault =
            await _context.GlAccountTypeDefaults.SingleOrDefaultAsync(x =>
                x.OrganizationPartyId == organizationPartyId
                && x.GlAccountTypeId == glAccountTypeId);
        return glAccountTypeDefault!;
    }

    private async Task<TaxAuthorityGlAccount> GetTaxAuthorityGlAccount(string organizationPartyId, string taxAuthGeoId,
        string taxAuthPartyId)
    {
        var taxAuthorityGlAccount =
            await _context.TaxAuthorityGlAccounts.SingleOrDefaultAsync(x =>
                x.OrganizationPartyId == organizationPartyId
                && x.TaxAuthGeoId == taxAuthGeoId
                && x.TaxAuthPartyId == taxAuthPartyId);
        return taxAuthorityGlAccount!;
    }

    private async Task<VarianceReasonGlAccount> GetVarianceReasonGlAccount(string varianceReasonId,
        string organizationPartyId)
    {
        var varianceReasonGlAccount = await _context.VarianceReasonGlAccounts
            .Where(x => x.VarianceReasonId == varianceReasonId && x.OrganizationPartyId == organizationPartyId)
            .FirstOrDefaultAsync();

        return varianceReasonGlAccount!;
    }

    private async Task<string> GetGlAccountFromAccountType(GetGlAccountFromAccountTypeParams parameters)
    {
        string? glAccountId = null;

        try
        {
            // Check if it's an inventory variance
            if (parameters.AcctgTransTypeId == "ITEM_VARIANCE")
            {
                // Retrieve GlAccountId from VarianceReasonGlAccount
                var varianceReasonGlAccount =
                    await GetVarianceReasonGlAccount(parameters.GlAccountTypeId!, parameters.OrganizationPartyId!);

                if (!string.IsNullOrEmpty(varianceReasonGlAccount?.GlAccountId))
                    return varianceReasonGlAccount.GlAccountId;
            }


            // Check for fixed asset depreciation
            if (parameters.AcctgTransTypeId == "DEPRECIATION" && parameters.FixedAssetId != null)
            {
                List<FixedAssetTypeGlAccount>? fixedAssetTypeGlAccounts = null;

                // Fetch FixedAssetTypeGlAccount entities
                fixedAssetTypeGlAccounts = await _context.FixedAssetTypeGlAccounts
                    .Where(f => f.FixedAssetId == parameters.FixedAssetId)
                    .ToListAsync();


                // If no records found, try fetching with default FixedAssetTypeId
                if (fixedAssetTypeGlAccounts == null || fixedAssetTypeGlAccounts.Count == 0)
                {
                    var fixedAsset = _context.FixedAssets
                        .FirstOrDefault(fa => fa.FixedAssetId == parameters.FixedAssetId);

                    if (fixedAsset != null)
                    {
                        var fixedAssetTypeId = fixedAsset.FixedAssetTypeId;

                        _context.FixedAssetTypeGlAccounts
                            .Where(f => f.FixedAssetId == null || f.FixedAssetId == "_NA_" ||
                                        f.FixedAssetTypeId == fixedAssetTypeId)
                            .ToList();
                    }
                }


                // Retrieve the appropriate GlAccountId based on conditions
                if (fixedAssetTypeGlAccounts != null && fixedAssetTypeGlAccounts.Count > 0)
                {
                    var fixedAssetTypeGlAccount = fixedAssetTypeGlAccounts.First();

                    if (!string.IsNullOrEmpty(fixedAssetTypeGlAccount.AccDepGlAccountId) &&
                        parameters.DebitCreditFlag == "C")
                    {
                        glAccountId = fixedAssetTypeGlAccount.AccDepGlAccountId;
                        return glAccountId;
                    }

                    if (!string.IsNullOrEmpty(fixedAssetTypeGlAccount.DepGlAccountId) &&
                        parameters.DebitCreditFlag == "D")
                    {
                        glAccountId = fixedAssetTypeGlAccount.DepGlAccountId;
                        return glAccountId;
                    }
                }
            }

            // Check for party-specific account mapping
            if (!string.IsNullOrEmpty(parameters.GlAccountTypeId) && !string.IsNullOrEmpty(parameters.PartyId) &&
                !string.IsNullOrEmpty(parameters.RoleTypeId))
            {
                var partyGlAccount = await GetPartyGlAccount(parameters.OrganizationPartyId!, parameters.PartyId,
                    parameters.RoleTypeId, parameters.GlAccountTypeId!);

                if (!string.IsNullOrEmpty(partyGlAccount?.GlAccountId))
                {
                    glAccountId = partyGlAccount.GlAccountId;
                    return glAccountId;
                }
            }

            // Check for specific conditions related to payments
            if ((parameters.AcctgTransTypeId == "OUTGOING_PAYMENT" && parameters.DebitCreditFlag == "C") ||
                (parameters.AcctgTransTypeId == "INCOMING_PAYMENT" && parameters.DebitCreditFlag == "D"))
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == parameters.PaymentId);

                if (payment != null)
                {
                    var paymentMethod = await _context.PaymentMethods
                        .FirstOrDefaultAsync(pm => pm.PaymentMethodId == payment.PaymentMethodId);

                    if (paymentMethod != null && !string.IsNullOrEmpty(paymentMethod.GlAccountId))
                    {
                        glAccountId = paymentMethod.GlAccountId;
                        return glAccountId;
                    }

                    if (payment.PaymentMethodTypeId == "CREDIT_CARD")
                    {
                        var creditCard = _context.CreditCards
                            .FirstOrDefault(cc => cc.PaymentMethodId == payment.PaymentMethodId);

                        if (creditCard != null)
                        {
                            var creditCardTypeGlAccount =
                                await GetCreditCardTypeGlAccount(creditCard.CardType!, parameters.OrganizationPartyId!);

                            if (!string.IsNullOrEmpty(creditCardTypeGlAccount!.GlAccountId))
                            {
                                glAccountId = creditCardTypeGlAccount.GlAccountId;
                                return glAccountId;
                            }
                        }
                    }

                    var paymentMethodTypeGlAccount =
                        await GetPaymentMethodTypeGlAccount(payment.PaymentMethodTypeId!,
                            parameters.OrganizationPartyId!);

                    if (!string.IsNullOrEmpty(paymentMethodTypeGlAccount!.GlAccountId))
                    {
                        glAccountId = paymentMethodTypeGlAccount.GlAccountId;
                        return glAccountId;
                    }
                }
            }

            // Check for product-specific conditions
            if (parameters.ProductId != null)
            {
                var productGlAccount = await GetProductGlAccount(parameters.OrganizationPartyId!, parameters.ProductId,
                    parameters.GlAccountTypeId!);

                // If nothing found, check ProductCategoryGlAccount
                if (string.IsNullOrEmpty(productGlAccount?.GlAccountId))
                {
                    var productCategoryMembers = _context.ProductCategoryMembers
                        .Where(pcm => pcm.ProductId == parameters.ProductId)
                        .OrderByDescending(pcm => pcm.FromDate)
                        .ToList();

                    foreach (var productCategoryMember in productCategoryMembers)
                    {
                        var productCategoryGlAccount =
                            await GetProductCategoryGlAccount(productCategoryMember.ProductCategoryId,
                                parameters.GlAccountTypeId!, parameters.OrganizationPartyId!);

                        if (!string.IsNullOrEmpty(productCategoryGlAccount?.GlAccountId))
                        {
                            glAccountId = productCategoryGlAccount.GlAccountId;
                            return glAccountId;
                        }
                    }
                }
                else
                {
                    glAccountId = productGlAccount.GlAccountId;
                    return glAccountId;
                }
            }

            // Check for specific conditions related to invoices
            if ((parameters.AcctgTransTypeId == "PURCHASE_INVOICE" && parameters.DebitCreditFlag == "D") ||
                (parameters.AcctgTransTypeId == "CUST_RTN_INVOICE" && parameters.DebitCreditFlag == "D") ||
                (parameters.AcctgTransTypeId == "SALES_INVOICE" && parameters.DebitCreditFlag == "C"))
            {
                // Check if invoiceId and glAccountTypeId are not empty
                if (!string.IsNullOrEmpty(parameters.InvoiceId) && !string.IsNullOrEmpty(parameters.GlAccountTypeId))
                {
                    var invoiceItemTypeGlAccount =
                        await GetInvoiceItemTypeGlAccount(parameters.InvoiceItemTypeId!,
                            parameters.OrganizationPartyId!);

                    if (!string.IsNullOrEmpty(invoiceItemTypeGlAccount?.GlAccountId))
                    {
                        glAccountId = invoiceItemTypeGlAccount.GlAccountId;
                        return glAccountId;
                    }
                }

                var invoiceItemType = _context.InvoiceItemTypes
                    .FirstOrDefault(iit => iit.InvoiceItemTypeId == parameters.GlAccountTypeId);

                if (invoiceItemType != null && !string.IsNullOrEmpty(invoiceItemType.DefaultGlAccountId))
                {
                    glAccountId = invoiceItemType.DefaultGlAccountId;
                    return glAccountId;
                }

                if (!string.IsNullOrEmpty(parameters.ProductId))
                {
                    if (parameters.AcctgTransTypeId == "PURCHASE_INVOICE")
                        parameters.GlAccountTypeId = "UNINVOICED_SHIP_RCPT";
                    else if (parameters.AcctgTransTypeId == "CUST_RTN_INVOICE")
                        parameters.GlAccountTypeId = "SALES_RETURNS";
                    else if (parameters.AcctgTransTypeId == "SALES_INVOICE")
                        parameters.GlAccountTypeId = "SALES_ACCOUNT";

                    var glAccountTypeDefault =
                        await GetDefaultGlAccount(parameters.OrganizationPartyId!, parameters.GlAccountTypeId!);

                    if (string.IsNullOrEmpty(glAccountTypeDefault.GlAccountId))
                    {
                        glAccountId = glAccountTypeDefault.GlAccountId;
                        return glAccountId!;
                    }
                }
            }

            //if nothing found or if no such parameters were passed (lookedUpValue empty in both cases), try GlAccountTypeDefault -->
            if (string.IsNullOrEmpty(glAccountId))
            {
                var glAccountTypeDefault =
                    await GetDefaultGlAccount(parameters.OrganizationPartyId!, parameters.GlAccountTypeId!);

                if (!string.IsNullOrEmpty(glAccountTypeDefault.GlAccountId))
                {
                    glAccountId = glAccountTypeDefault.GlAccountId;
                    return glAccountId!;
                }
            }

            return glAccountId!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<InvoiceItemTypeGlAccount?> GetInvoiceItemTypeGlAccount(string invoiceItemTypeId,
        string organizationPartyId)
    {
        var invoiceItemTypeGlAccount = await _context.InvoiceItemTypeGlAccounts
            .Where(x => x.InvoiceItemTypeId == invoiceItemTypeId && x.OrganizationPartyId == organizationPartyId)
            .SingleOrDefaultAsync();

        return invoiceItemTypeGlAccount;
    }

    private async Task<ProductCategoryGlAccount?> GetProductCategoryGlAccount(string productCategoryId,
        string glAccountTypeId, string organizationPartyId)
    {
        var productCategoryGlAccount = await _context.ProductCategoryGlAccounts
            .Where(x => x.ProductCategoryId == productCategoryId && x.GlAccountTypeId == glAccountTypeId
                                                                 && x.OrganizationPartyId == organizationPartyId)
            .SingleOrDefaultAsync();

        return productCategoryGlAccount;
    }

    // GetGlAccountTypeId method based on GlAccountTypeId
    private async Task<GlAccountType?> GetGlAccountType(string glAccountTypeId)
    {
        var glAccountType = await _context.GlAccountTypes
            .Where(x => x.GlAccountTypeId == glAccountTypeId)
            .SingleOrDefaultAsync();

        return glAccountType;
    }

    private async Task<PaymentMethodTypeGlAccount?> GetPaymentMethodTypeGlAccount(string paymentMethodTypeId,
        string organizationPartyId)
    {
        var paymentMethodTypeGlAccount = await _context.PaymentMethodTypeGlAccounts
            .Where(x => x.OrganizationPartyId == organizationPartyId && x.PaymentMethodTypeId == paymentMethodTypeId)
            .SingleOrDefaultAsync();

        return paymentMethodTypeGlAccount;
    }

    private async Task<CreditCardTypeGlAccount?> GetCreditCardTypeGlAccount(string creditCardTypeId,
        string organizationPartyId)
    {
        var creditCardTypeGlAccount = await _context.CreditCardTypeGlAccounts
            .Where(x => x.CardType == creditCardTypeId && x.OrganizationPartyId == organizationPartyId)
            .FirstOrDefaultAsync();

        return creditCardTypeGlAccount;
    }

    private async Task<bool> IsAccountingOrganization(string organizationPartyId)
    {
        var partyRole = await GetPartyRole(organizationPartyId, "INTERNAL_ORGANIZATIO");

        var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(organizationPartyId);

        if (partyRole != null && partyAcctgPreference != null &&
            partyAcctgPreference.EnableAccounting == "Y") return true;

        return false;
    }


    private async Task<PartyRole?> GetPartyRole(string partyId, string roleTypeId)
    {
        var partyRole =
            await _context.PartyRoles.FirstOrDefaultAsync(pr => pr.PartyId == partyId && pr.RoleTypeId == roleTypeId);

        return partyRole;
    }


    /*private decimal ConvertUom(decimal? OriginalValue, string? UomId, string? UomIdTo, DateTime? AsOfDate)
    {
        // Implement your UOM conversion logic here and return the converted value
        // This function should call the service or method responsible for UOM conversion
        // and return the converted value as a decimal.
        // For simplicity, I'm returning 0.0 as a placeholder.
        return 0.0M;
    }*/


    /*public async Task<decimal?> HandleAmountAndCurrency(AcctgTransEntry acctgTransEntry, DateTime transactionDate)
    {
        if (acctgTransEntry.Amount == null)
            if (acctgTransEntry.OrigAmount != null)
            {
                var partyAcctgPreference =
                    await _acctgMiscService.GetPartyAccountingPreferences(acctgTransEntry.OrganizationPartyId!);
                if (string.IsNullOrEmpty(acctgTransEntry.OrigCurrencyUomId))
                    acctgTransEntry.OrigCurrencyUomId = partyAcctgPreference!.BaseCurrencyUomId;

                acctgTransEntry.CurrencyUomId = partyAcctgPreference!.BaseCurrencyUomId;


                if (acctgTransEntry.OrigCurrencyUomId != acctgTransEntry.CurrencyUomId)
                {
                    var convertedValue = ConvertUom(acctgTransEntry.OrigAmount, acctgTransEntry.OrigCurrencyUomId!,
                        acctgTransEntry.CurrencyUomId!, transactionDate!);

                    acctgTransEntry.Amount = convertedValue;
                }
                else
                {
                    acctgTransEntry.Amount = acctgTransEntry.OrigAmount;
                }
            }

        return acctgTransEntry.Amount!;
    }
    */


    public async Task<string> CreateAcctgTransAndEntries(CreateAcctgTransAndEntriesParams parameters)
    {
        // ------------------------------------------------------------------------------------
        // 1) Retrieve ledger rounding properties (Ofbiz: <call-simple-method method-name="getGlArithmeticSettingsInline"/>)
        // ------------------------------------------------------------------------------------
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        // e.g. glSettings might have { ledgerDecimals, roundingMode } if you need them below.
        // In the Ofbiz snippet, these might be used in numeric calculations, but
        // we’ll include them for completeness:
        int ledgerDecimals = (int)glSettings.DecimalScale;
        string roundingMode = glSettings.RoundingMode;

        // ------------------------------------------------------------------------------------
        // 2) Copy relevant fields from 'parameters' into a new 'createAcctgTransParams'
        //    (Ofbiz uses <set-service-fields service-name="createAcctgTrans" map="parameters" to-map="createAcctgTransParams"/>)
        // ------------------------------------------------------------------------------------
        var createAcctgTransParams = new CreateAcctgTransParams
        {
            GlFiscalTypeId = parameters.GlFiscalTypeId,
            AcctgTransTypeId = parameters.AcctgTransTypeId,
            InvoiceId = parameters.InvoiceId,
            PaymentId = parameters.PaymentId,
            Description = parameters.Description,
            PartyId = parameters.PartyId,
            RoleTypeId = parameters.RoleTypeId,
            TransactionDate = parameters.TransactionDate, // might set later if null
            ShipmentId = parameters.ShipmentId,
            WorkEffortId = parameters.WorkEffortId,
            SalesRequestId = parameters.SalesRequestId,
            // The Ofbiz snippet sets 'IsPosted' to "Y" and 'PostedDate' to now at creation-time
            IsPosted = "Y",
            PostedDate = DateTime.UtcNow
        };

        // ------------------------------------------------------------------------------------
        // 3) Prepare to iterate through 'acctgTransEntries' 
        //    (Ofbiz: <iterate list="parameters.acctgTransEntries" entry="acctgTransEntry">)
        // ------------------------------------------------------------------------------------
        var normalizedAcctgTransEntries = new List<AcctgTransEntry>();
        if (parameters.AcctgTransEntries == null || parameters.AcctgTransEntries.Count == 0)
        {
            _logger.LogWarning("Cannot process an accounting transaction with an empty list of entries.");
            return string.Empty; // or return a null/empty ID
        }

        foreach (var acctgTransEntry in parameters.AcctgTransEntries)
        {
            // -------------------------------------------------------------------
            // a) Must be an internal organization => Ofbiz checks PartyRole
            //    <entity-one entity-name="PartyRole" ... roleTypeId="INTERNAL_ORGANIZATIO"/>
            // -------------------------------------------------------------------
            bool isInternalOrg = await IsAccountingOrganization(acctgTransEntry.OrganizationPartyId);
            if (!isInternalOrg)
            {
                _logger.LogWarning(
                    $"The party with id [{acctgTransEntry.OrganizationPartyId}] is not an internal organization; " +
                    $"the following accounting transaction will be ignored: {acctgTransEntry}");
                // In Ofbiz, this logs a warning and continues to the next entry.
                // The snippet doesn't "continue" or "return" the entire method. We'll do the same:
                continue;
            }

            // -------------------------------------------------------------------
            // b) getPartyAccountingPreferences => check enableAccounting != 'N'
            // -------------------------------------------------------------------
            var partyAcctgPreference =
                await _acctgMiscService.GetPartyAccountingPreferences(acctgTransEntry.OrganizationPartyId);
            if (partyAcctgPreference == null || partyAcctgPreference.EnableAccounting == "N")
            {
                // Ofbiz snippet: <then><log level="warning" message=" ... "/><return/></then>
                // Means: once we find an org with disabled accounting, we immediately stop the entire method.
                _logger.LogWarning(
                    $"The internal organization with id [{acctgTransEntry.OrganizationPartyId}] " +
                    "has no PartyAcctgPreference or enableAccounting == 'N'; " +
                    $"the following accounting transaction will be ignored: {acctgTransEntry}");
                return string.Empty; // Return and skip the entire create process
            }

            // -------------------------------------------------------------------
            // c) If <if-empty field="acctgTransEntry.amount"> 
            //    => possibly derive from 'origAmount' & currency conversion
            // -------------------------------------------------------------------
            if (acctgTransEntry.Amount == null)
            {
                if (acctgTransEntry.OrigAmount != null)
                {
                    // If 'origCurrencyUomId' is empty => set to partyAcctgPreference.baseCurrencyUomId
                    if (string.IsNullOrEmpty(acctgTransEntry.OrigCurrencyUomId))
                    {
                        acctgTransEntry.OrigCurrencyUomId = partyAcctgPreference.BaseCurrencyUomId;
                    }

                    // Also ensure 'currencyUomId' is set to that same base currency
                    if (string.IsNullOrEmpty(acctgTransEntry.CurrencyUomId))
                    {
                        acctgTransEntry.CurrencyUomId = partyAcctgPreference.BaseCurrencyUomId;
                    }

                    // If they differ => call "convertUom"
                    if (!string.Equals(acctgTransEntry.OrigCurrencyUomId, acctgTransEntry.CurrencyUomId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        // Build a map => in Ofbiz: <clear-field field="convertUomInMap"/> ...
                        var asOfDate = createAcctgTransParams.TransactionDate;
                        acctgTransEntry.Amount = await _commonService.ConvertUom(
                            acctgTransEntry.OrigCurrencyUomId,
                            acctgTransEntry.CurrencyUomId,
                            asOfDate,
                            acctgTransEntry.OrigAmount.Value,
                            null
                        );
                    }
                    else
                    {
                        acctgTransEntry.Amount = acctgTransEntry.OrigAmount;
                    }
                }
            }

            // -------------------------------------------------------------------
            // d) If glAccountId is empty => call getGlAccountFromAccountType
            // -------------------------------------------------------------------
            if (string.IsNullOrEmpty(acctgTransEntry.GlAccountId))
            {
                var accountParams = new GetGlAccountFromAccountTypeParams
                {
                    OrganizationPartyId = acctgTransEntry.OrganizationPartyId,
                    AcctgTransTypeId = createAcctgTransParams.AcctgTransTypeId,
                    GlAccountTypeId = acctgTransEntry.GlAccountTypeId,
                    DebitCreditFlag = acctgTransEntry.DebitCreditFlag,
                    ProductId = acctgTransEntry.ProductId,
                    PartyId = createAcctgTransParams.PartyId,
                    RoleTypeId = createAcctgTransParams.RoleTypeId,
                    InvoiceId = createAcctgTransParams.InvoiceId,
                    PaymentId = createAcctgTransParams.PaymentId
                };
                acctgTransEntry.GlAccountId = await GetGlAccountFromAccountType(accountParams);
            }

            // -------------------------------------------------------------------
            // e) If <if-empty field="acctgTransEntry.origAmount"> => set to amount
            // -------------------------------------------------------------------
            if (acctgTransEntry.OrigAmount == null)
            {
                acctgTransEntry.OrigAmount = acctgTransEntry.Amount;
            }

            // -------------------------------------------------------------------
            // f) <entity-one entity-name="GlAccountType" ... if-empty => clear field
            // -------------------------------------------------------------------
            var glAccountType = await GetGlAccountType(acctgTransEntry.GlAccountTypeId);
            if (glAccountType == null)
            {
                acctgTransEntry.GlAccountTypeId = null;
            }
            else
            {
                // Use the actual ID returned
                acctgTransEntry.GlAccountTypeId = glAccountType.GlAccountTypeId;
            }

            // -------------------------------------------------------------------
            // g) Add the validated entry to 'normalizedAcctgTransEntries'
            // -------------------------------------------------------------------
            normalizedAcctgTransEntries.Add(acctgTransEntry);
        }

        // ------------------------------------------------------------------------------------
        // 4) If we have normalized entries => create AcctgTrans, then create each entry
        // ------------------------------------------------------------------------------------
        if (!normalizedAcctgTransEntries.Any())
        {
            _logger.LogWarning("Cannot process an accounting transaction with an empty list of normalized entries.");
            return string.Empty;
        }

        // <if-empty field="createAcctgTransParams.transactionDate"><now-timestamp field="createAcctgTransParams.transactionDate"/></if-empty>
        if (!createAcctgTransParams.TransactionDate.HasValue)
        {
            createAcctgTransParams.TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        // call-service service-name="createAcctgTrans" ...
        var acctgTransId = await _acctgTransService.CreateAcctgTrans(createAcctgTransParams);

        // ------------------------------------------------------------------------------------
        // 5) Iterate normalized entries => invert negative amounts => call createAcctgTransEntry
        // ------------------------------------------------------------------------------------
        foreach (var entry in normalizedAcctgTransEntries)
        {
            // <if-compare field="acctgTransEntry.origAmount" operator="less" value="0">
            if (entry.OrigAmount < 0)
            {
                _logger.LogDebug($"AcctgTransEntry {entry} is going to get inverted due to negative amount.");

                // Flip sign of 'origAmount' and 'amount'
                entry.OrigAmount = -entry.OrigAmount.Value;
                if (entry.Amount.HasValue)
                {
                    entry.Amount = -entry.Amount.Value;
                }

                // Flip the DebitCreditFlag: D->C, C->D
                if (entry.DebitCreditFlag == "D") entry.DebitCreditFlag = "C";
                else if (entry.DebitCreditFlag == "C") entry.DebitCreditFlag = "D";
            }

            // <call-service service-name="createAcctgTransEntry" in-map-name="createAcctgTransEntryParams"/>
            // We replicate that by calling an EF or service method:
            entry.AcctgTransId = acctgTransId;
            await _acctgTransService.CreateAcctgTransEntry(entry);
        }

        // --------------------------------------------------------------------
        // 8) Ofbiz ECA event: If acctgTransId is not empty => postAcctgTrans
        //    <eca service="createAcctgTransAndEntries" event="commit">
        //      <condition field-name="acctgTransId" operator="is-not-empty"/>
        //      <action service="postAcctgTrans" mode="sync"/>
        //    </eca>
        // --------------------------------------------------------------------
        if (!string.IsNullOrEmpty(acctgTransId))
        {
            // In Ofbiz, this is done automatically after commit. We replicate by calling:
            var postMessages = await PostAcctgTrans(acctgTransId, verifyOnly: false);
            if (postMessages.Any())
            {
                // If errors/warnings, log them
                var combined = string.Join("; ", postMessages);
                _logger.LogWarning($"PostAcctgTrans returned warnings or errors: {combined}");
            }
            else
            {
                _logger.LogInformation($"AcctgTrans {acctgTransId} posted successfully (ECA event).");
            }
        }


        // <field-to-result field="acctgTransId"/>
        return acctgTransId;
    }


    private static AcctgTransEntry InvertIfNegative(AcctgTransEntry entry)
    {
        if (entry.OrigAmount < 0)
        {
            Console.WriteLine($"{entry} is going to get inverted");

            entry.OrigAmount = -entry.OrigAmount;
            entry.Amount = -entry.Amount;

            if (entry.DebitCreditFlag == "D")
                entry.DebitCreditFlag = "C";
            else if (entry.DebitCreditFlag == "C") entry.DebitCreditFlag = "D";
        }

        return entry;
    }

    public async Task<string> CreateAcctgTransForInventoryItemCostChange(string inventoryItemId,
        string inventoryItemDetailSeqId)
    {
        try
        {
            // Retrieve ledger rounding properties (decimal scale and rounding mode)
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            // Retrieve InventoryItemDetail entity
            var newInventoryItemDetail = await _context.InventoryItemDetails
                .FirstOrDefaultAsync(d =>
                    d.InventoryItemDetailSeqId == inventoryItemDetailSeqId && d.InventoryItemId == inventoryItemId);

            if (newInventoryItemDetail == null) throw new ArgumentException("InventoryItemDetail not found.");

            // Retrieve related InventoryItem
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryItemId == newInventoryItemDetail.InventoryItemId);

            if (inventoryItem == null) throw new ArgumentException("InventoryItem not found.");

            // Retrieve old inventory item details
            var inventoryItemDetails = await _context.InventoryItemDetails
                .Where(d => d.InventoryItemId == newInventoryItemDetail.InventoryItemId
                            && d.UnitCost != null
                            && d.InventoryItemDetailSeqId != inventoryItemDetailSeqId)
                .OrderByDescending(d => d.EffectiveDate)
                .ToListAsync();

            var oldInventoryItemDetail = inventoryItemDetails.FirstOrDefault();

            if (oldInventoryItemDetail != null)
            {
                // Calculate the original amount
                var origAmount = _acctgMiscService.CustomRound(
                    (decimal)(inventoryItem.QuantityOnHandTotal *
                              (oldInventoryItemDetail.UnitCost - newInventoryItemDetail.UnitCost)),
                    (int)ledgerDecimals,
                    roundingMode);

                // If origAmount is 0, skip creating transaction
                if (origAmount != 0)
                {
                    // Prepare the list of accounting transaction entries
                    var acctgTransEntries = new List<AcctgTransEntry>();

                    // Create Credit entry
                    var creditEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "C",
                        GlAccountTypeId = "INVENTORY_ACCOUNT",
                        OrganizationPartyId = inventoryItem.OwnerPartyId,
                        ProductId = inventoryItem.ProductId,
                        OrigAmount = origAmount,
                        OrigCurrencyUomId = inventoryItem.CurrencyUomId
                    };
                    acctgTransEntries.Add(creditEntry);

                    // Create Debit entry
                    var debitEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "D",
                        GlAccountTypeId = "INV_ADJ_VAL",
                        OrganizationPartyId = inventoryItem.OwnerPartyId,
                        ProductId = inventoryItem.ProductId,
                        OrigAmount = origAmount,
                        OrigCurrencyUomId = inventoryItem.CurrencyUomId
                    };
                    acctgTransEntries.Add(debitEntry);

                    // Prepare parameters for creating the accounting transaction
                    var createAcctgTransAndEntriesInMap =
                        new CreateAcctgTransAndEntriesParams
                        {
                            GlFiscalTypeId = "ACTUAL",
                            AcctgTransTypeId = "INVENTORY",
                            InventoryItemId = inventoryItemId,
                            AcctgTransEntries = acctgTransEntries
                        };

                    // Call service to create accounting transaction and entries
                    var acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

                    return acctgTransId;
                }
            }

            // Return null if no transaction was created
            return null;
        }
        catch (Exception ex)
        {
            // Log and handle exceptions as needed
            throw new Exception($"Error creating accounting transaction: {ex.Message}", ex);
        }
    }

    private async Task<List<string>> GetOrderIdsForInvoiceAsync(string invoiceId)
    {
        // 1. Look in change tracker first (includes Added, Modified, sometimes Unchanged)
        var trackerOrderIds = _context.ChangeTracker
            .Entries<OrderItemBilling>()
            .Where(e =>
                (e.State == EntityState.Added ||
                 e.State == EntityState.Modified ||
                 e.State == EntityState.Unchanged) &&
                e.Entity.InvoiceId == invoiceId
            )
            .Select(e => e.Entity.OrderId)
            .Distinct()
            .ToList(); // synchronous – change tracker is in-memory

        // 2. If nothing found in tracker → ask database (or always ask and union)
        var dbOrderIds = await _context.OrderItemBillings
            .Where(ob => ob.InvoiceId == invoiceId)
            .Select(ob => ob.OrderId)
            .Distinct()
            .ToListAsync();

        // 3. Combine & remove duplicates
        var allOrderIds = trackerOrderIds
            .Concat(dbOrderIds)
            .Distinct(StringComparer.Ordinal) // or OrdinalIgnoreCase if needed
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();

        return allOrderIds;
    }

    public async Task<string> CreateAcctgTransForPurchaseInvoice(string invoiceId)
    {
        try
        {
            // Retrieve ledger rounding properties
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            decimal totalAmountFromInvoice = 0;

            // Fetch the invoice
            var invoice = await _context.Invoices.FindAsync(invoiceId);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID {InvoiceId} not found.", invoiceId);
                return null;
            }

            // Check if the invoice is a purchase invoice
            bool isPurchaseInvoice = invoice.InvoiceTypeId == "PURCHASE_INVOICE";

            if (!isPurchaseInvoice)
            {
                _logger.LogDebug("Invoice {InvoiceId} is not a purchase invoice. No transaction created.", invoiceId);
                return null;
            }

            // Retrieve InvoiceItems excluding tax items
            var invoiceItems = await _context.InvoiceItems
                .Where(i => i.InvoiceId == invoiceId && i.InvoiceItemTypeId != "PINV_SALES_TAX" &&
                            i.InvoiceItemTypeId != "PITM_SALES_TAX")
                .ToListAsync();

            // Resolve the certificate (if any) this invoice was generated from — used below both as a
            // fallback description for entries whose invoiceItem.Description is empty, and to stamp
            // WorkEffortId onto the AcctgTrans header so GetGlAccountTransactionDetails' report can
            // resolve the certificate number/project for these entries the same way it already does
            // for OUTGOING_PAYMENT transactions.
            var certificate = await _context.OrderItemBillings
                .Where(ob => ob.InvoiceId == invoiceId)
                .Join(_context.WorkEfforts.Where(w => w.WorkEffortTypeId == "PROJECT_CERTIFICATE"),
                    ob => ob.OrderId, w => w.RelatedOrderId,
                    (ob, w) => new { w.WorkEffortId, w.Description, w.CertificateCategory })
                .FirstOrDefaultAsync();
            string? certificateWorkEffortId = certificate?.WorkEffortId;
            string? certificateDescription = certificate?.Description;

            // SUPPLY_PROCUREMENT_CERTIFICATE invoices debit UNINVOICED_SHIP_RCPT (GL 214000) instead of
            // the invoice item's own default account (PINV_CERTIFICATE_SUPPLY_ITEM -> 124420, Projects
            // Under Construction) — but ONLY when this certificate's own SHIPMENT_RECEIPT actually
            // credited 214000 in the first place (post-fix receipts). Certificates received before this
            // fix existed still have their SHIPMENT_RECEIPT crediting ACCOUNTS_PAYABLE directly; for
            // those, debiting 214000 here would leave it permanently uncleared (nothing ever credited
            // it) while ALSO leaving AP double-credited (receipt + this invoice) — a new, worse bug than
            // the one being fixed. Branching on the certificate's CertificateCategory alone doesn't
            // distinguish these two cases; checking what the receipt actually posted does. For legacy
            // receipts, this falls back to the original behavior (debit 124420), which reproduces the
            // familiar receipt+invoice AP duplication — already handled by
            // RemediateSupplyCertificateDuplicateAp, so nothing new is left unbalanced.
            bool isSupplyCertificateInvoice = false;
            if (certificate?.CertificateCategory == "SUPPLY_PROCUREMENT_CERTIFICATE" &&
                !string.IsNullOrEmpty(certificateWorkEffortId))
            {
                isSupplyCertificateInvoice = await _context.AcctgTrans
                    .Where(t => t.WorkEffortId == certificateWorkEffortId &&
                                t.AcctgTransTypeId == "SHIPMENT_RECEIPT" &&
                                t.IsPosted == "Y")
                    .SelectMany(t => t.AcctgTransEntries)
                    .AnyAsync(e => e.DebitCreditFlag == "C" && e.GlAccountTypeId == "UNINVOICED_SHIP_RCPT");
            }

            var acctgTransEntries = new List<AcctgTransEntry>();
            int seqNum = 1; // Initialize sequence number for AcctgTransEntrySeqId


            foreach (var invoiceItem in invoiceItems)
            {
                decimal amountFromOrder = 0;
                decimal amountFromInvoice = 0;
                decimal quantity = invoiceItem.Quantity ?? 1m;

                // Calculate the amount from the invoice
                if (invoiceItem.Amount == null)
                {
                    _logger.LogWarning("InvoiceItem {InvoiceItemSeqId} has no amount. Using 0.",
                        invoiceItem.InvoiceItemSeqId);
                }


                // Calculate amount from invoice
                decimal itemAmount = invoiceItem.Amount ?? 0m;
                amountFromInvoice =
                    _acctgMiscService.CustomRound(quantity * itemAmount, (int)ledgerDecimals, roundingMode);

                // Add to the total amount from invoice
                totalAmountFromInvoice = _acctgMiscService.CustomRound(totalAmountFromInvoice + amountFromInvoice,
                    (int)ledgerDecimals, roundingMode);

                // Get order item billing related to invoice item
                var orderItemBillings = await _context.OrderItemBillings
                    .Where(ob => ob.InvoiceItemSeqId == invoiceItem.InvoiceItemSeqId &&
                                 ob.InvoiceId == invoiceItem.InvoiceId)
                    .ToListAsync();

                foreach (var orderItemBilling in orderItemBillings)
                {
                    var orderItem = await _context.OrderItems.FirstOrDefaultAsync(x =>
                        x.OrderId == orderItemBilling.OrderId &&
                        x.OrderItemSeqId == orderItemBilling.OrderItemSeqId);

                    if (orderItem != null)
                    {
                        // amountFromOrder += orderItemBilling.Quantity * orderItem.UnitPrice
                        decimal orderLineAmount = (decimal)(orderItemBilling.Quantity * orderItem.UnitPrice);
                        amountFromOrder = _acctgMiscService.CustomRound(amountFromOrder + orderLineAmount,
                            (int)ledgerDecimals, roundingMode);
                    }
                    else
                    {
                        _logger.LogWarning("OrderItem not found for OrderId={OrderId}, OrderItemSeqId={SeqId}",
                            orderItemBilling.OrderId, orderItemBilling.OrderItemSeqId);
                    }
                }

                /// If there is a variance between amountFromInvoice and amountFromOrder, and amountFromOrder > 0, create a PURCHASE_PRICE_VAR debit entry
                if (amountFromInvoice != amountFromOrder && amountFromOrder > 0)
                {
                    var varianceAmount = _acctgMiscService.CustomRound(amountFromInvoice - amountFromOrder,
                        (int)ledgerDecimals, roundingMode);

                    var debitEntryVariance = new AcctgTransEntry
                    {
                        AcctgTransEntrySeqId = seqNum.ToString("D2"),
                        AcctgTransEntryTypeId = "_NA_",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        DebitCreditFlag = "D",
                        OrganizationPartyId = invoice.PartyId,
                        PartyId = invoice.PartyIdFrom,
                        RoleTypeId = "BILL_FROM_VENDOR",
                        ProductId = invoiceItem.ProductId,
                        GlAccountTypeId = "PURCHASE_PRICE_VAR",
                        OrigAmount = varianceAmount,
                        OrigCurrencyUomId = invoice.CurrencyUomId,
                        Description = invoiceItem.Description ?? certificateDescription
                    };

                    acctgTransEntries.Add(debitEntryVariance);
                    seqNum++; // Increment sequence number
                }

                // Create a debit entry for the order item itself
                var finalAmount = amountFromOrder > 0 ? amountFromOrder : amountFromInvoice;

                var debitEntryOrder = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = invoice.PartyId,
                    PartyId = invoice.PartyIdFrom,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    ProductId = invoiceItem.ProductId,
                    // InvoiceItemTypeId is used as glAccountTypeId which will be resolved later
                    GlAccountTypeId = isSupplyCertificateInvoice ? "UNINVOICED_SHIP_RCPT" : invoiceItem.InvoiceItemTypeId,
                    GlAccountId = isSupplyCertificateInvoice ? null : invoiceItem.OverrideGlAccountId,
                    OrigAmount = finalAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    Description = invoiceItem.Description ?? certificateDescription
                };

                acctgTransEntries.Add(debitEntryOrder);
                seqNum++; // Increment sequence number
            }

            // Tax handling
            decimal invoiceTaxTotal = 0;
            // Get the taxAuthPartyId -> taxAuthGeoId mapping for the invoice
            var taxAuthPartyAndGeos = await _invoiceUtilityService.GetInvoiceTaxAuthPartyAndGeos(invoice.InvoiceId);

            // For each taxAuthPartyId and associated geo IDs, calculate tax and add debit entries
            foreach (var (taxAuthPartyId, taxAuthGeoIds) in taxAuthPartyAndGeos)
            {
                foreach (var taxAuthGeoId in taxAuthGeoIds)
                {
                    var taxAmount =
                        await _invoiceUtilityService.GetInvoiceTaxTotalForTaxAuthPartyAndGeo(invoice.InvoiceId,
                            taxAuthPartyId, taxAuthGeoId);

                    var taxEntry = new AcctgTransEntry
                    {
                        AcctgTransEntrySeqId = seqNum.ToString("D2"),
                        AcctgTransEntryTypeId = "_NA_",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        DebitCreditFlag = "D",
                        OrganizationPartyId = invoice.PartyId,
                        OrigAmount = taxAmount,
                        OrigCurrencyUomId = invoice.CurrencyUomId,
                        PartyId = taxAuthPartyId,
                        RoleTypeId = "TAX_AUTHORITY",
                        GlAccountTypeId = "TAX_ACCOUNT"
                    };

                    acctgTransEntries.Add(taxEntry);
                    seqNum++; // Increment sequence number

                    invoiceTaxTotal = _acctgMiscService.CustomRound(invoiceTaxTotal + taxAmount, (int)ledgerDecimals,
                        roundingMode);
                }
            }

            // Handle unattributed tax
            var unattributedTaxAmount = await _invoiceUtilityService.GetInvoiceUnattributedTaxTotal(invoice.InvoiceId);

            if (unattributedTaxAmount > 0)
            {
                var unattributedTaxEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum.ToString("D2"),
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    OrganizationPartyId = invoice.PartyId,
                    OrigAmount = unattributedTaxAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId,
                    RoleTypeId = "TAX_AUTHORITY",
                    GlAccountTypeId = "TAX_ACCOUNT"
                };

                acctgTransEntries.Add(unattributedTaxEntry);
                seqNum++; // Increment sequence number

                invoiceTaxTotal = _acctgMiscService.CustomRound(invoiceTaxTotal + unattributedTaxAmount,
                    (int)ledgerDecimals, roundingMode);
            }

            // Create a credit entry for ACCOUNTS_PAYABLE
            decimal totalWithTax = _acctgMiscService.CustomRound(totalAmountFromInvoice + invoiceTaxTotal,
                (int)ledgerDecimals, roundingMode);

            var creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum.ToString("D2"),
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "C",
                OrganizationPartyId = invoice.PartyId,
                GlAccountTypeId = "ACCOUNTS_PAYABLE",
                OrigAmount = totalWithTax,
                OrigCurrencyUomId = invoice.CurrencyUomId,
                PartyId = invoice.PartyIdFrom,
                RoleTypeId = "BILL_FROM_VENDOR",
                Description = certificateDescription ?? invoice.Description
            };

            acctgTransEntries.Add(creditEntry);
            seqNum++; // Increment sequence number

            var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams()
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "PURCHASE_INVOICE",
                Description = invoice.Description,
                InvoiceId = invoice.InvoiceId,
                PartyId = invoice.PartyIdFrom,
                RoleTypeId = "BILL_FROM_VENDOR",
                TransactionDate = invoice.InvoiceDate,
                AcctgTransEntries = acctgTransEntries,
                WorkEffortId = certificateWorkEffortId
            };

            // Call createAcctgTransAndEntries service and return the acctgTransId
            var AcctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

            // Now handle payment applications
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.InvoiceId == invoiceId)
                .ToListAsync();

            foreach (var paymentApplication in paymentApplications)
            {
                // Fetch the related payment for each payment application
                var payment = await _context.Payments.FindAsync(paymentApplication.PaymentId);

                // Check if payment status is either "PMNT_SENT" or "PMNT_CONFIRMED"
                if (payment != null && (payment.StatusId == "PMNT_SENT" || payment.StatusId == "PMNT_CONFIRMED"))
                {
                    // Check if the payment is a "CUSTOMER_REFUND"
                    if (payment.PaymentTypeId == "CUSTOMER_REFUND")
                    {
                        // Call the service to create accounting transactions for a customer refund
                        // Call service similar to createAcctgTransAndEntriesForCustomerRefundPaymentApplication
                        var refundResult =
                            await CreateAcctgTransAndEntriesForCustomerRefundPaymentApplication(paymentApplication
                                .PaymentId);
                        _logger.LogInformation(
                            "Accounting transaction {refundResult} created for customer refund payment application {paymentApplicationId}",
                            refundResult, paymentApplication.PaymentApplicationId);
                    }
                    else
                    {
                        // Call the service to create accounting transactions for a regular payment application
                        var paymentResult =
                            await CreateAcctgTransAndEntriesForPaymentApplication(paymentApplication
                                .PaymentApplicationId);
                        _logger.LogInformation(
                            "Accounting transaction {paymentResult} created for payment application {paymentApplicationId}",
                            paymentResult, paymentApplication.PaymentApplicationId);
                    }
                }
            }

            // Return the generated acctgTransId from the service call
            return AcctgTransId;
        }
        catch (Exception ex)
        {
            // Log the error and rethrow or return null depending on your preference
            _logger.LogError(ex, "Error creating accounting transaction for purchase invoice {InvoiceId}",
                invoiceId);
            throw;
        }
    }

    // REFACTOR: Added full support for Additional Insurance as a separate withholding line.
// In many construction contracts, there are two types of insurance:
//   • Regular Insurance (e.g. 5% performance guarantee)
//   • Additional Insurance (e.g. advance payment guarantee, bid bond, etc.)
// Both must be withheld and posted to separate liability accounts if configured.
// This change ensures audit clarity and allows independent release of each type later.
    public async Task<string> CreateAcctgTransForConstructionCertificateInvoice(string invoiceId)
    {
        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice with ID {InvoiceId} not found.", invoiceId);
                return null;
            }

            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            var acctgTransEntries = new List<AcctgTransEntry>();
            int seqNum = 1;

            // REFACTOR: Allow MULTIPLE main certificate items — sum all PINV_CERTIFICATE_ITEM lines
            // This supports multi-phase, multi-contract, or split certificate invoices
            var certificateItems = invoice.InvoiceItems
                .Where(ii => ii.InvoiceItemTypeId == "PINV_CERTIFICATE_ITEM")
                .ToList();

            // REFACTOR: Handle multiple insurance/withholding/deduction items — real-world requirement
            var insuranceItems = invoice.InvoiceItems
                .Where(ii => ii.InvoiceItemTypeId == "PINV_INSURANCE_ITEM")
                .ToList();

            var additionalInsuranceItems = invoice.InvoiceItems
                .Where(ii => ii.InvoiceItemTypeId == "PINV_ADDITIONAL_INSURANCE_ITEM")
                .ToList();

            var deductionItems = invoice.InvoiceItems
                .Where(ii => ii.InvoiceItemTypeId == "PINV_DEDUCTION_ITEM")
                .ToList();

            // REFACTOR: Fail if no certificate items at all — invoice is invalid for this flow
            if (!certificateItems.Any() || certificateItems.All(ii => ii.Amount == null))
            {
                _logger.LogError(
                    "Construction certificate invoice {InvoiceId} has no valid PINV_CERTIFICATE_ITEM lines.",
                    invoiceId);
                return null;
            }

            // Sum all main certificate amounts
            decimal deservedAmountGross = certificateItems.Sum(ii => ii.Amount ?? 0m);
            decimal deservedAmount =
                _acctgMiscService.CustomRound(deservedAmountGross, (int)ledgerDecimals, roundingMode);

            // Sum all withholding categories
            decimal insuranceAmount = _acctgMiscService.CustomRound(
                insuranceItems.Sum(ii => ii.Amount ?? 0m), (int)ledgerDecimals, roundingMode);

            decimal additionalInsuranceAmount = _acctgMiscService.CustomRound(
                additionalInsuranceItems.Sum(ii => ii.Amount ?? 0m), (int)ledgerDecimals, roundingMode);

            decimal deductionAmount = _acctgMiscService.CustomRound(
                deductionItems.Sum(ii => ii.Amount ?? 0m), (int)ledgerDecimals, roundingMode);

            decimal totalWithheld = insuranceAmount + additionalInsuranceAmount + deductionAmount;
            decimal netPayable =
                _acctgMiscService.CustomRound(deservedAmount - totalWithheld, (int)ledgerDecimals, roundingMode);

            if (netPayable < 0)
            {
                _logger.LogWarning("Net payable negative for invoice {InvoiceId}. Setting to zero.", invoiceId);
                netPayable = 0;
            }

            // Use ProductId from first certificate item (or handle differently if needed)
            // If ProductId varies per line, you may want to group by ProductId later
            string productId = certificateItems.First().ProductId;
            string contractorPartyId = invoice.PartyIdFrom;

            //var (workEffortId, projectGlAccountId) = await GetProjectInfoViaOrderItemBillingAsync(invoiceId);

            var orderIds = await _context.OrderItemBillings
                .Where(ob => ob.InvoiceId == invoiceId)
                .Select(ob => ob.OrderId)
                .Distinct()
                .ToListAsync();
            string? workEffortId = null;
            WorkEffort? workEffort = null;
            string? projectGlAccountId = null;

            if (orderIds.Any())
            {
                // Prefer the most specific match: SUPPLY_PROCUREMENT_CERTIFICATE + RelatedOrderId
                workEffort = await _context.WorkEfforts
                    .Where(w => orderIds.Contains(w.RelatedOrderId!)) // RelatedOrderId is usually string
                    .OrderByDescending(w => w.LastStatusUpdate) // most recent if somehow multiple
                    .FirstOrDefaultAsync();

                if (workEffort != null)
                {
                    workEffortId = workEffort.WorkEffortId;
                    var project = await _context.WorkEfforts.FindAsync(workEffort.ProjectId);
                    projectGlAccountId = project.GlAccountId;
                }
                else
                {
                    _logger.LogInformation(
                        "No matching WorkEffort found for invoice {InvoiceId} with RelatedOrderId in {OrderIds}",
                        invoiceId, string.Join(", ", orderIds));
                }
            }
            else
            {
                _logger.LogDebug("No OrderItemBillings found for invoice {InvoiceId}", invoiceId);
            }

            if (string.IsNullOrEmpty(projectGlAccountId))
            {
                _logger.LogError("Failed to resolve Project GlAccountId for invoice {InvoiceId} via OrderItemBilling.",
                    invoiceId);
                return null;
            }

            // Optional: Log if WorkEffortId is missing (but GlAccountId exists)
            if (string.IsNullOrEmpty(workEffortId))
            {
                _logger.LogWarning(
                    "Project WorkEffortId not found for invoice {InvoiceId}, but GlAccountId was resolved.", invoiceId);
            }

            // 1. Debit: Projects Under Construction – TOTAL deserved value (sum of all cert items)
            acctgTransEntries.Add(new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum++.ToString("D2"),
                DebitCreditFlag = "D",
                OrganizationPartyId = invoice.PartyId,
                PartyId = contractorPartyId,
                RoleTypeId = "BILL_FROM_VENDOR",
                ProductId = productId,
                GlAccountId = projectGlAccountId,
                GlAccountTypeId = "PINV_CERTIFICATE_ITEM", // → 124420
                OrigAmount = deservedAmount,
                OrigCurrencyUomId = invoice.CurrencyUomId
            });

            // 2. Credit: Regular Insurance Withheld (aggregated)
            if (insuranceAmount > 0)
            {
                acctgTransEntries.Add(new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum++.ToString("D2"),
                    DebitCreditFlag = "C",
                    OrganizationPartyId = invoice.PartyId,
                    PartyId = contractorPartyId,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    ProductId = productId,
                    GlAccountTypeId = "INSURANCE_WITHHELD", // → 250400
                    OrigAmount = insuranceAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId
                });
            }

            // 3. Credit: Additional Insurance Withheld
            if (additionalInsuranceAmount > 0)
            {
                acctgTransEntries.Add(new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum++.ToString("D2"),
                    DebitCreditFlag = "C",
                    OrganizationPartyId = invoice.PartyId,
                    PartyId = contractorPartyId,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    ProductId = productId,
                    GlAccountTypeId = "ADDITIONAL_INSURANCE_WITHHELD",
                    OrigAmount = additionalInsuranceAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId
                });
            }

            // 4. Credit: Other Deductions (penalties, LD, advance recovery, etc.)
            if (deductionAmount > 0)
            {
                acctgTransEntries.Add(new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = seqNum++.ToString("D2"),
                    DebitCreditFlag = "C",
                    OrganizationPartyId = invoice.PartyId,
                    PartyId = contractorPartyId,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    ProductId = productId,
                    GlAccountId = projectGlAccountId,
                    GlAccountTypeId = "CONSTRUCTION_DEDUCTIONS",
                    OrigAmount = deductionAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId
                });
            }

            // 5. Credit: Accounts Payable – net amount actually payable
            acctgTransEntries.Add(new AcctgTransEntry
            {
                AcctgTransEntrySeqId = seqNum++.ToString("D2"),
                DebitCreditFlag = "C",
                OrganizationPartyId = invoice.PartyId,
                PartyId = contractorPartyId,
                RoleTypeId = "BILL_FROM_VENDOR",
                GlAccountTypeId = "ACCOUNTS_PAYABLE", // → 210000
                OrigAmount = netPayable,
                OrigCurrencyUomId = invoice.CurrencyUomId
            });

            // Persist via existing OFBiz service
            var createParams = new CreateAcctgTransAndEntriesParams()
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "PURCHASE_INVOICE",
                InvoiceId = invoice.InvoiceId,
                PartyId = invoice.PartyIdFrom,
                RoleTypeId = "BILL_FROM_VENDOR",
                TransactionDate = invoice.InvoiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                AcctgTransEntries = acctgTransEntries,
                Description = workEffort?.Description,
                WorkEffortId = workEffortId
            };

            var acctgTransId = await CreateAcctgTransAndEntries(createParams);

            _logger.LogInformation(
                "Construction certificate accounting posted: {AcctgTransId} | Invoice: {InvoiceId} | " +
                "Deserved: {Deserved} | Insurance: {Ins} | Add.Ins: {AddIns} | Deductions: {Ded} | Net: {Net}",
                acctgTransId, invoiceId, deservedAmount, insuranceAmount, additionalInsuranceAmount, deductionAmount,
                netPayable);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating accounting transaction for construction certificate invoice {InvoiceId}", invoiceId);
            throw;
        }
    }

    // REFACTOR: Changed return type to a tuple containing both WorkEffortId (project) and GlAccountId
    private async Task<(string? WorkEffortId, string? GlAccountId)> GetProjectInfoViaOrderItemBillingAsync(
        string invoiceId)
    {
        var query = from oib in _context.OrderItemBillings
            where oib.InvoiceId == invoiceId
            join cert in _context.WorkEfforts on oib.OrderId equals cert.RelatedOrderId
            where cert.WorkEffortTypeId == "PROJECT_CERTIFICATE"
                  && cert.ProjectId != null
            join proj in _context.WorkEfforts on cert.ProjectId equals proj.WorkEffortId
            where proj.WorkEffortTypeId == "PROJECT"
                  && proj.GlAccountId != null
            select new { proj.WorkEffortId, proj.GlAccountId };

        var results = await query.Distinct().ToListAsync();

        if (!results.Any())
            return (null, null);

        if (results.Count > 1)
        {
            var first = results.First();
            _logger.LogWarning(
                "Invoice {InvoiceId} references multiple projects. Using first project: {WorkEffortId}, GlAccountId: {GlAccountId}",
                invoiceId, first.WorkEffortId, first.GlAccountId);
            return (first.WorkEffortId, first.GlAccountId);
        }

        var result = results.Single();
        return (result.WorkEffortId, result.GlAccountId);
    }

    public async Task<string> CreateAcctgTransAndEntriesForCustomerRefundPaymentApplication(
        string paymentApplicationId)
    {
        try
        {
            // 1. Retrieve Payment Application Information
            var paymentApplication = await _context.PaymentApplications
                .FirstOrDefaultAsync(pa => pa.PaymentApplicationId == paymentApplicationId);

            if (paymentApplication == null)
            {
                throw new Exception("Payment application not found.");
            }

            AcctgTransEntry debitEntry = null;
            AcctgTransEntry creditEntry = null;

            // 2. Retrieve Related Payment Information
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentApplication.PaymentId);

            if (payment == null)
            {
                throw new Exception("Payment not found.");
            }

            // 3. Check If Payment Transaction Has Already Been Posted to GL
            if (payment.StatusId == "PMNT_NOT_PAID")
            {
                return null;
            }

            // 4. Check if Payment is a Receipt (mirroring the "call-class-method" for isReceipt in Ofbiz)
            var parentPaymentType = await _utilityService.GetPaymentParentType(payment.PaymentTypeId);

            if (parentPaymentType == "RECEIPT")
            {
                // 5. Check if Payment Credited Account is Already "Accounts Receivable"
                var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                    .FirstOrDefaultAsync(map =>
                        map.PaymentTypeId == payment.PaymentTypeId && map.OrganizationPartyId == payment.PartyIdTo);

                if (paymentGlAccountTypeMap != null &&
                    paymentGlAccountTypeMap.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
                {
                    return null;
                }

                // 6. Create Credit Entry for Receipt
                creditEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "C",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountId = payment.OverrideGlAccountId,
                    GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId
                };

                // 7. Create Debit Entry for Receipt
                debitEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "D",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_TO_CUSTOMER",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountTypeId = "ACCOUNTS_RECEIVABLE"
                };
            }
            else
            {
                // 8. Check if Payment Debited Account is Already "Accounts Payable"
                var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                    .FirstOrDefaultAsync(map =>
                        map.PaymentTypeId == payment.PaymentTypeId &&
                        map.OrganizationPartyId == payment.PartyIdFrom);

                if (paymentGlAccountTypeMap != null &&
                    paymentGlAccountTypeMap.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                {
                    return null;
                }

                // 9. Get Exchange Rates (mirroring the calls to getGlExchangeRateOfPurchaseInvoice and getGlExchangeRateOfOutgoingPayment)
                decimal? invoiceExchangeRate =
                    await _acctgMiscService.GetGlExchangeRateOfPurchaseInvoice(paymentApplication);
                var paymentExchangeRate =
                    await _acctgMiscService.GetGlExchangeRateOfOutgoingPayment(paymentApplication);

                // 10. Create Credit Entry for Non-Receipt
                creditEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "C",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    Amount = paymentApplication.AmountApplied * paymentExchangeRate,
                    GlAccountId = payment.OverrideGlAccountId,
                    GlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId
                };

                // 11. Create Debit Entry for Non-Receipt and FX Gain/Loss Adjustments
                if (invoiceExchangeRate != paymentExchangeRate)
                {
                    debitEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "D",
                        OrganizationPartyId = payment.PartyIdFrom,
                        PartyId = payment.PartyIdTo,
                        RoleTypeId = "BILL_FROM_VENDOR",
                        Amount = paymentApplication.AmountApplied * (paymentExchangeRate - invoiceExchangeRate),
                        GlAccountTypeId = "FX_GAIN_LOSS_ACCT"
                    };
                    // Add FX Gain/Loss Debit Entry
                    _context.AcctgTransEntries.Add(debitEntry);
                    debitEntry = null;
                }

                // 12. Create Debit Entry for Accounts Payable
                debitEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "D",
                    OrganizationPartyId = payment.PartyIdFrom,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "BILL_FROM_VENDOR",
                    OrigAmount = paymentApplication.AmountApplied,
                    Amount = paymentApplication.AmountApplied * invoiceExchangeRate,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountTypeId = "ACCOUNTS_PAYABLE"
                };

                // 13. Override GL Account Handling (mirroring the logic in Ofbiz)
                if (!string.IsNullOrEmpty(paymentApplication.OverrideGlAccountId))
                {
                    debitEntry.GlAccountId = paymentApplication.OverrideGlAccountId;
                }
                else if (!string.IsNullOrEmpty(paymentApplication.TaxAuthGeoId))
                {
                    var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
                        .FirstOrDefaultAsync(gl =>
                            gl.OrganizationPartyId == payment.PartyIdFrom &&
                            gl.TaxAuthGeoId == paymentApplication.TaxAuthGeoId &&
                            gl.TaxAuthPartyId == payment.PartyIdTo);

                    if (taxAuthorityGlAccount != null)
                        debitEntry.GlAccountId = taxAuthorityGlAccount.GlAccountId;
                }

                _context.AcctgTransEntries.Add(debitEntry);
            }

            // 14. Prepare Parameters for Creating Accounting Transaction
            var createParams = new CreateAcctgTransAndEntriesParams
            {
                AcctgTransEntries = new List<AcctgTransEntry> { debitEntry, creditEntry },
                AcctgTransTypeId = "PAYMENT_APPL",
                GlFiscalTypeId = "ACTUAL",
                PaymentId = paymentApplication.PaymentId,
                InvoiceId = paymentApplication.InvoiceId,
                PartyId = parentPaymentType == "RECEIPT" ? payment.PartyIdFrom : payment.PartyIdTo,
                RoleTypeId = parentPaymentType == "RECEIPT" ? "BILL_TO_CUSTOMER" : "BILL_FROM_VENDOR",
                TransactionDate = payment.EffectiveDate
            };

            // 15. Call Service to Create Accounting Transaction
            var acctgTransId = await CreateAcctgTransAndEntries(createParams);

            // 16. Return Accounting Transaction ID
            return acctgTransId;
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(
                $"Error creating accounting transaction for payment application {paymentApplicationId}: {ex.Message}",
                ex);
            throw new Exception($"An error occurred while processing payment application {paymentApplicationId}.",
                ex);
        }
    }


    public async Task<string> CreateAcctgTransForWorkEffortInventoryProduced(string workEffortId,
        string inventoryItemId)
    {
        try
        {
            // 1. Retrieve Ledger Rounding Properties
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            // 2. Retrieve Work Effort Inventory Produced Data
            var workEffortInventoryProduced = await _context.WorkEffortInventoryProduceds
                .FindAsync(workEffortId, inventoryItemId);

            if (workEffortInventoryProduced == null)
            {
                throw new Exception(
                    $"Work effort inventory produced not found for workEffortId {workEffortId} and inventoryItemId {inventoryItemId}");
            }

            // 3. Get Related Inventory Item Data
            var inventoryItem = await _context.InventoryItems
                .FindAsync(inventoryItemId);

            if (inventoryItem == null)
            {
                throw new Exception($"Inventory item not found for inventoryItemId {inventoryItemId}");
            }

            // 4. Handle Inventory Calculations (Amount Calculation)
            decimal origAmount = _acctgMiscService.CustomRound(
                (decimal)(inventoryItem.QuantityOnHandTotal * inventoryItem.UnitCost), (int)ledgerDecimals,
                roundingMode);

            // 5. Create Inventory Item Detail Record
            var createDetailMap = new CreateInventoryItemDetailParam
            {
                InventoryItemId = inventoryItem.InventoryItemId,
                AccountingQuantityDiff = inventoryItem.QuantityOnHandTotal
            };
            await _inventoryService.CreateInventoryItemDetail(createDetailMap);


            // 6. Prepare Double Posting (Debit and Credit Entries)
            List<AcctgTransEntry> acctgTransEntries = new List<AcctgTransEntry>();

            // Credit Entry
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "01",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "C",
                GlAccountTypeId = "WIP_INVENTORY",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = inventoryItem.ProductId,
                OrigAmount = origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId
            };
            acctgTransEntries.Add(creditEntry);

            // Debit Entry
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "02",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "D",
                GlAccountTypeId = "INVENTORY_ACCOUNT",
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = inventoryItem.ProductId,
                OrigAmount = origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId
            };

            acctgTransEntries.Add(debitEntry);

            // 7. Set Header Fields for Accounting Transaction
            var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams
            {
                AcctgTransEntries = acctgTransEntries,
                AcctgTransTypeId = "INVENTORY",
                GlFiscalTypeId = "ACTUAL",
                WorkEffortId = workEffortId
            };

            // 8. Call Service to Create Accounting Transaction and Entries
            var acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

            // 9. Return Accounting Transaction ID
            return acctgTransId;
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(
                $"Error creating accounting transaction for work effort inventory produced. Exception: {ex.Message}",
                ex);
            throw new Exception("An error occurred while creating the accounting transaction.", ex);
        }
    }

    /// <summary>
    /// Mirrors the OFBiz simple‑method <c>createAcctgTransForWorkEffortCost</c>.
    /// Creates an accounting transaction that moves manufacturing costs 
    /// from the WIP (debit) to the proper expense / inventory (credit) GL accounts.
    /// </summary>
    /// <param name="workEffortId">ID of the production run header or task incurring the cost.</param>
    /// <param name="costComponentId">ID of the <c>CostComponent</c> record that holds the calculated cost.</param>
    /// <returns>The ID of the newly‑created <c>AcctgTrans</c> header.</returns>
    public async Task<string> CreateAcctgTransForWorkEffortCost(string workEffortId, string costComponentId)
    {
        // Retrieve GL arithmetic settings (ledgerDecimals and roundingMode)
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        var ledgerDecimals = glSettings.DecimalScale;
        var roundingMode = glSettings.RoundingMode;
        try
        {
            // ────────────────────────────────────────────────────────────────
            // 1. 𝐅𝐞𝐭𝐜𝐡 𝐜𝐨𝐬𝐭 𝐬𝐨𝐮𝐫𝐜𝐞
            //    Technical: single‑row primary‑key lookup.
            //    Business: this record stores the *monetary value* we must post.
            // ────────────────────────────────────────────────────────────────
            var costComponent = await _context.CostComponents.FindAsync(costComponentId);
            if (costComponent == null)
                throw new Exception($"Cost component {costComponentId} not found (cannot post cost).");

            // ────────────────────────────────────────────────────────────────
            // 2. 𝐆𝐞𝐭 𝐜𝐨𝐬𝐭 𝐜𝐚𝐥𝐜𝐮𝐥𝐚𝐭𝐢𝐨𝐧 𝐫𝐮𝐥𝐞𝐬
            //    Links each cost to its GL mapping (what account we credit / debit).
            // ────────────────────────────────────────────────────────────────
            var costComponentCalc = await _context.CostComponentCalcs
                .FirstOrDefaultAsync(cc => cc.CostComponentCalcId == costComponent.CostComponentCalcId);
            if (costComponentCalc == null)
                throw new Exception(
                    $"CostComponentCalc {costComponent.CostComponentCalcId} missing for cost {costComponentId}.");

            // ────────────────────────────────────────────────────────────────
            // 3. 𝐑𝐞𝐭𝐫𝐢𝐞𝐯𝐞 𝐰𝐨𝐫𝐤 𝐜𝐨𝐧𝐭𝐞𝐱𝐭
            // ────────────────────────────────────────────────────────────────
            var workEffort = await _context.WorkEfforts
                .FindAsync(workEffortId);
            if (workEffort == null)
                throw new Exception($"WorkEffort {workEffortId} not found (cannot assign cost).");

            // ────────────────────────────────────────────────────────────────
            // 4. 𝐅𝐚𝐜𝐢𝐥𝐢𝐭𝐲: 𝐰𝐡𝐨 𝐨𝐰𝐧𝐬 𝐭𝐡𝐞 𝐠𝐨𝐨𝐝𝐬 𝐚𝐧𝐝 𝐚𝐜𝐜𝐨𝐮𝐧𝐭𝐢𝐧𝐠
            // ────────────────────────────────────────────────────────────────
            var facility = await _context.Facilities
                .FindAsync(workEffort.FacilityId);
            if (facility == null)
                throw new Exception($"Facility {workEffort.FacilityId} missing for WorkEffort {workEffortId}.");

            // ────────────────────────────────────────────────────────────────
            // 5. 𝐃𝐞𝐫𝐢𝐯𝐞 𝐭𝐡𝐞 𝐩𝐫𝐨𝐝𝐮𝐜𝐭 (𝐒𝐊𝐔) 𝐛𝐞𝐢𝐧𝐠 𝐩𝐫𝐨𝐝𝐮𝐜𝐞𝐝
            //    The GL posting must reference the item manufactured.
            // ────────────────────────────────────────────────────────────────
            WorkEffortGoodStandard workEffortGoodStandard = null;

            if (workEffort.WorkEffortTypeId == "PROD_ORDER_TASK" &&
                !string.IsNullOrEmpty(workEffort.WorkEffortParentId))
            {
                // Task ⇒ look at parent header for the delivery standard line.
                workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .Where(wgs => wgs.WorkEffortId == workEffort.WorkEffortParentId &&
                                  wgs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                    .OrderByDescending(wgs => wgs.FromDate)
                    .FirstOrDefaultAsync();
            }
            else if (workEffort.WorkEffortTypeId == "PROD_ORDER_HEADER")
            {
                // Header ⇒ its own delivery standard.
                workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .Where(wgs => wgs.WorkEffortId == workEffortId &&
                                  wgs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                    .OrderByDescending(wgs => wgs.FromDate)
                    .FirstOrDefaultAsync();
            }

            // ⚠ Business‑critical guard: posting *must* reference a product.
            if (workEffortGoodStandard == null || string.IsNullOrEmpty(workEffortGoodStandard.ProductId))
                throw new Exception($"No PRUN_PROD_DELIV record (product) found for work effort {workEffortId}.");

            // ────────────────────────────────────────────────────────────────
            // 6. 𝐑𝐨𝐮𝐧𝐝 𝐭𝐡𝐞 𝐜𝐨𝐬𝐭 𝐯𝐚𝐥𝐮𝐞
            //    Uses the client‑defined helper per coding standards.
            // ────────────────────────────────────────────────────────────────
            decimal roundedCost =
                _acctgMiscService.CustomRound(costComponent.Cost ?? 0m, (int)ledgerDecimals, roundingMode);


            // ────────────────────────────────────────────────────────────────
            // 7. 𝐂𝐫𝐞𝐝𝐢𝐭 𝐞𝐧𝐭𝐫𝐲 (𝐂 = 𝐢𝐧𝐜𝐫𝐞𝐚𝐬𝐞 𝐞𝐱𝐩𝐞𝐧𝐬𝐞 / 𝐜𝐨𝐬𝐭 𝐬𝐢𝐝𝐞)
            //    Business: removes cost from WIP and books it to expense/inventory.
            // ────────────────────────────────────────────────────────────────
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "01",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "C",
                OrganizationPartyId = facility.OwnerPartyId,
                ProductId = workEffortGoodStandard.ProductId,
                OrigAmount = roundedCost,
                OrigCurrencyUomId = costComponent.CostUomId,
                GlAccountTypeId = !string.IsNullOrEmpty(costComponentCalc.CostGlAccountTypeId)
                    ? costComponentCalc.CostGlAccountTypeId
                    : !string.IsNullOrEmpty(costComponent.FixedAssetId)
                        ? "OPERATING_EXPENSE"
                        : null // Let posting rules supply default if none.
            };

            // ────────────────────────────────────────────────────────────────
            // 8. 𝐃𝐞𝐛𝐢𝐭 𝐞𝐧𝐭𝐫𝐲 (𝐃 = 𝐝𝐞𝐜𝐫𝐞𝐚𝐬𝐞 𝐚𝐬𝐬𝐞𝐭 / 𝐢𝐧𝐜𝐫𝐞𝐚𝐬𝐞 𝐖𝐈𝐏)
            // ────────────────────────────────────────────────────────────────
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "02",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "D",
                OrganizationPartyId = facility.OwnerPartyId,
                ProductId = workEffortGoodStandard.ProductId,
                OrigAmount = roundedCost,
                OrigCurrencyUomId = costComponent.CostUomId,
                GlAccountTypeId = !string.IsNullOrEmpty(costComponentCalc.OffsettingGlAccountTypeId)
                    ? costComponentCalc.OffsettingGlAccountTypeId
                    : "WIP_INVENTORY"
            };

            var acctgTransEntries = new List<AcctgTransEntry> { creditEntry, debitEntry };

            // ────────────────────────────────────────────────────────────────
            // 9. 𝐀𝐜𝐭𝐠𝐓𝐫𝐚𝐧𝐬 𝐡𝐞𝐚𝐝𝐞𝐫: 𝐬𝐞𝐭 𝐜𝐨𝐧𝐭𝐞𝐱𝐭
            // ────────────────────────────────────────────────────────────────
            var acctgTransParams = new CreateAcctgTransAndEntriesParams
            {
                AcctgTransEntries = acctgTransEntries,
                AcctgTransTypeId = "MANUFACTURING", // Same as OFBiz constant
                GlFiscalTypeId = "ACTUAL",
                WorkEffortId = workEffortId
            };

            // ────────────────────────────────────────────────────────────────
            // 10. 𝐏𝐞𝐫𝐬𝐢𝐬𝐭 𝐭𝐡𝐞 𝐓𝐫𝐚𝐧𝐬𝐚𝐜𝐭𝐢𝐨𝐧
            //     Mirrors the simple‑method’s service call.
            // ────────────────────────────────────────────────────────────────
            string acctgTransId = await CreateAcctgTransAndEntries(acctgTransParams);

            // ────────────────────────────────────────────────────────────────
            // 11. 𝐑𝐞𝐭𝐮𝐫𝐧 𝐩𝐫𝐢𝐦𝐚𝐫𝐲 𝐤𝐞𝐲 𝐟𝐨𝐫 𝐟𝐮𝐫𝐭𝐡𝐞𝐫 𝐥𝐢𝐧𝐤𝐢𝐧𝐠
            // ────────────────────────────────────────────────────────────────
            return acctgTransId;
        }
        catch (Exception ex)
        {
            // Technical: persist full stack for ops.  
            // Business: surfaces clean message to caller.
            _logger.LogError(ex, "Error creating accounting transaction for WorkEffort {WorkEffortId}.", workEffortId);
            throw new Exception("Failed to post manufacturing cost to General Ledger.", ex);
        }
    }

    public async Task<string> CreateAcctgTransForCustomerReturnInvoice(string invoiceId)
    {
        try
        {
            // Retrieve GL arithmetic settings (ledgerDecimals and roundingMode)
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;
            // Retrieve the invoice
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with id {invoiceId} not found.");
            }

            // Retrieve InvoiceType
            var invoiceType = await _context.InvoiceTypes
                .FirstOrDefaultAsync(it => it.InvoiceTypeId == invoice.InvoiceTypeId);

            // Check invoiceTypeId for CUST_RTN_INVOICE
            if (invoiceType == null || invoiceType.InvoiceTypeId != "CUST_RTN_INVOICE")
            {
                // Not a Customer Return Invoice, return error
                throw new InvalidOperationException("This invoice is not a Customer Return invoice.");
            }

            // Initialize variables
            decimal totalAmountFromInvoice = 0.0m;
            string transPartyRoleTypeId = "BILL_TO_CUSTOMER";
            string acctgTransTypeId = "CUST_RTN_INVOICE";

            // Retrieve invoice items excluding tax items
            var invoiceItems = await _context.InvoiceItems
                .Where(ii => ii.InvoiceId == invoiceId
                             && ii.InvoiceItemTypeId != "INV_SALES_TAX"
                             && ii.InvoiceItemTypeId != "ITM_SALES_TAX")
                .ToListAsync();

            var acctgTransEntries = new List<AcctgTransEntry>();

            // Iterate over invoice items
            foreach (var invoiceItem in invoiceItems)
            {
                decimal quantity = invoiceItem.Quantity ?? 1.0m;
                decimal amount = invoiceItem.Amount ?? 0.0m;
                decimal amountFromInvoice =
                    _acctgMiscService.CustomRound(quantity * amount, (int)ledgerDecimals, roundingMode);

                // Keep building invoice total for use in credit entry
                totalAmountFromInvoice = _acctgMiscService.CustomRound(totalAmountFromInvoice + amountFromInvoice,
                    (int)ledgerDecimals, roundingMode);

                // Create debit entry
                var debitEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "D",
                    OrganizationPartyId = invoice.PartyId,
                    PartyId = invoice.PartyIdFrom,
                    RoleTypeId = transPartyRoleTypeId,
                    ProductId = invoiceItem.ProductId,
                    GlAccountTypeId = invoiceItem.InvoiceItemTypeId,
                    GlAccountId = invoiceItem.OverrideGlAccountId,
                    OrigAmount = amountFromInvoice,
                    OrigCurrencyUomId = invoice.CurrencyUomId
                };
                acctgTransEntries.Add(debitEntry);
            }

            // Handle tax entries
            decimal invoiceTaxTotal = 0.0m;

            // Get taxAuthPartyAndGeos from the utility service
            var taxAuthPartyAndGeos = await _invoiceUtilityService.GetInvoiceTaxAuthPartyAndGeos(invoiceId);

            // For each taxAuthPartyId and taxAuthGeoIds
            foreach (var kvp in taxAuthPartyAndGeos)
            {
                var taxAuthPartyId = kvp.Key;
                var taxAuthGeoIds = kvp.Value;

                foreach (var taxAuthGeoId in taxAuthGeoIds)
                {
                    decimal taxAmount = await _invoiceUtilityService.GetInvoiceTaxTotalForTaxAuthPartyAndGeo(
                        invoiceId, taxAuthPartyId, taxAuthGeoId);

                    // Create debit entry for tax
                    var taxDebitEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "D",
                        OrganizationPartyId = invoice.PartyId,
                        PartyId = taxAuthPartyId,
                        RoleTypeId = "TAX_AUTHORITY",
                        GlAccountTypeId = "TAX_ACCOUNT",
                        OrigAmount = taxAmount,
                        OrigCurrencyUomId = invoice.CurrencyUomId
                    };
                    acctgTransEntries.Add(taxDebitEntry);

                    invoiceTaxTotal = _acctgMiscService.CustomRound(invoiceTaxTotal + taxAmount,
                        (int)ledgerDecimals, roundingMode);
                }
            }

            // Another entry for unattributed tax
            decimal unattributedTaxAmount = await _invoiceUtilityService.GetInvoiceUnattributedTaxTotal(invoiceId);
            if (unattributedTaxAmount != 0.0m)
            {
                var unattributedTaxDebitEntry = new AcctgTransEntry
                {
                    DebitCreditFlag = "D",
                    OrganizationPartyId = invoice.PartyId,
                    GlAccountTypeId = "TAX_ACCOUNT",
                    OrigAmount = unattributedTaxAmount,
                    OrigCurrencyUomId = invoice.CurrencyUomId
                };
                acctgTransEntries.Add(unattributedTaxDebitEntry);

                invoiceTaxTotal = _acctgMiscService.CustomRound(invoiceTaxTotal + unattributedTaxAmount,
                    (int)ledgerDecimals, roundingMode);
            }

            // Create the credit entry
            totalAmountFromInvoice = _acctgMiscService.CustomRound(totalAmountFromInvoice + invoiceTaxTotal,
                (int)ledgerDecimals, roundingMode);

            var creditEntry = new AcctgTransEntry
            {
                DebitCreditFlag = "C",
                OrganizationPartyId = invoice.PartyId,
                GlAccountTypeId = "ACCOUNTS_RECEIVABLE",
                OrigAmount = totalAmountFromInvoice,
                OrigCurrencyUomId = invoice.CurrencyUomId,
                PartyId = invoice.PartyIdFrom,
                RoleTypeId = transPartyRoleTypeId
            };
            acctgTransEntries.Add(creditEntry);

            // Prepare param for createAcctgTransAndEntries
            var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = acctgTransTypeId,
                InvoiceId = invoice.InvoiceId,
                PartyId = invoice.PartyIdFrom,
                RoleTypeId = "BILL_TO_CUSTOMER",
                AcctgTransEntries = acctgTransEntries
            };

            // Call service to create AcctgTrans and Entries
            string acctgTransId = await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating accounting transaction for Customer Return Invoice.");
            throw;
        }
    }

    public async Task<string> CreateAcctgTransAndEntriesForOutgoingPayment(string paymentId)
    {
        try
        {
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;
            // Retrieve Payment record

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with ID {paymentId} was not found.");
                return null;
            }

            // Check if Payment is a disbursement
            var isDisbursement = await _paymentHelperService.Value.IsDisbursement(payment);
            if (!isDisbursement)
            {
                _logger.LogInformation(
                    $"Payment {paymentId} is not a disbursement. Skipping outgoing payment transaction.");
                return null;
            }

            // Idempotency guard — see CreateAcctgTransAndEntriesForIncomingPayment for why this
            // is required: the generic SetPaymentStatus transition back to PMNT_NOT_PAID doesn't
            // clean up AcctgTrans, so a payment re-approved after that drift would otherwise be
            // posted to GL twice.
            var existingAcctgTransId = await _context.AcctgTrans
                .Where(t => t.PaymentId == paymentId &&
                            t.AcctgTransTypeId == "OUTGOING_PAYMENT" &&
                            t.IsPosted == "Y")
                .Select(t => t.AcctgTransId)
                .FirstOrDefaultAsync();

            if (existingAcctgTransId != null)
            {
                _logger.LogWarning(
                    $"Payment {paymentId} already has posted AcctgTrans {existingAcctgTransId}. Skipping duplicate GL posting.");
                return existingAcctgTransId;
            }

            // Set organizationPartyId, partyId, and roleTypeId
            var organizationPartyId = payment.PartyIdFrom;
            var partyId = payment.PartyIdTo;
            var roleTypeId = "BILL_FROM_VENDOR";

            // ── Get WorkEffortId ────────────────────────────────────────────────
            string? workEffortId = null;

            workEffortId = await (
                from pyt in _context.Payments
                where pyt.PaymentId == paymentId
                join opp in _context.OrderPaymentPreferences
                    on pyt.PaymentPreferenceId equals opp.OrderPaymentPreferenceId
                join ord in _context.OrderHeaders
                    on opp.OrderId equals ord.OrderId
                join we in _context.WorkEfforts
                    on ord.OrderId equals we.RelatedOrderId into weGroup
                from we in weGroup.DefaultIfEmpty()
                select we.WorkEffortId
            ).FirstOrDefaultAsync();

            // Retrieve PaymentGlAccountTypeMap
            var paymentGlAccountTypeMap = await _context.PaymentGlAccountTypeMaps
                .FirstOrDefaultAsync(map =>
                    map.PaymentTypeId == payment.PaymentTypeId &&
                    map.OrganizationPartyId == organizationPartyId);

            var debitGlAccountTypeId = paymentGlAccountTypeMap?.GlAccountTypeId;

            // Initialize accounting entries list
            var acctgTransEntries = new List<AcctgTransEntry>();

            // Generate unique sequence ID for AcctgTrans
            var stamp = DateTime.UtcNow;
            var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");

            var paymentAmount = _acctgMiscService.CustomRound(payment.Amount,
                (int)ledgerDecimals, roundingMode);

            // Create CREDIT entry
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "1",
                AcctgTransEntryTypeId = "_NA_",
                DebitCreditFlag = "C",
                OrigAmount = paymentAmount,
                OrigCurrencyUomId = payment.CurrencyUomId,
                OrganizationPartyId = organizationPartyId,
                PartyId = partyId,
                RoleTypeId = roleTypeId,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            acctgTransEntries.Add(creditEntry);

            // Initialize amountAppliedTotal
            decimal amountAppliedTotal = 0m;

            // Retrieve PaymentApplications
            var paymentApplications = await _context.PaymentApplications
                .Where(pa => pa.PaymentId == payment.PaymentId)
                .ToListAsync();

            // Create DEBIT entries for PaymentApplications (mirroring commented-out OFBiz logic)
            int entrySeqId = 2; // Start after credit entry
            foreach (var paymentApplication in paymentApplications)
            {
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransId = newAcctgTransSequence,
                    AcctgTransEntrySeqId = entrySeqId.ToString(),
                    AcctgTransEntryTypeId = "_NA_",
                    DebitCreditFlag = "D",
                    OrigAmount = paymentApplication.AmountApplied,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountTypeId = "ACCOUNTS_PAYABLE", // Default GL account type
                    OrganizationPartyId = organizationPartyId,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = payment.Comments,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                // Handle overrideGlAccountId or TaxAuthorityGlAccount
                if (!string.IsNullOrEmpty(paymentApplication.OverrideGlAccountId))
                {
                    debitEntry.GlAccountId = paymentApplication.OverrideGlAccountId;
                }
                else if (!string.IsNullOrEmpty(paymentApplication.TaxAuthGeoId))
                {
                    var taxAuthorityGlAccount = await _context.TaxAuthorityGlAccounts
                        .FirstOrDefaultAsync(t =>
                            t.OrganizationPartyId == organizationPartyId &&
                            t.TaxAuthGeoId == paymentApplication.TaxAuthGeoId &&
                            t.TaxAuthPartyId == partyId);
                    if (taxAuthorityGlAccount != null)
                    {
                        debitEntry.GlAccountId = taxAuthorityGlAccount.GlAccountId;
                    }
                }

                acctgTransEntries.Add(debitEntry);
                amountAppliedTotal += (decimal)paymentApplication.AmountApplied;
                entrySeqId++;
            }

            // Calculate remaining amount
            var amount = payment.Amount - amountAppliedTotal;

            // Create debit entry for any remaining amount
            if (amount > 0)
            {
                var debitEntryWithDiffAmount = new AcctgTransEntry
                {
                    AcctgTransId = newAcctgTransSequence,
                    AcctgTransEntrySeqId = entrySeqId.ToString(),
                    AcctgTransEntryTypeId = "_NA_",
                    DebitCreditFlag = "D",
                    OrigAmount = amount,
                    OrigCurrencyUomId = payment.CurrencyUomId,
                    GlAccountId = payment.OverrideGlAccountId,
                    GlAccountTypeId = debitGlAccountTypeId,
                    OrganizationPartyId = organizationPartyId,
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = payment.Comments,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };
                acctgTransEntries.Add(debitEntryWithDiffAmount);
                entrySeqId++;
            }

            // Prepare AcctgTrans parameters
            var createParams = new CreateAcctgTransAndEntriesParams
            {
                RoleTypeId = roleTypeId,
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "OUTGOING_PAYMENT",
                PartyId = partyId,
                PaymentId = payment.PaymentId,
                WorkEffortId = workEffortId,
                Description = payment.Comments,
                TransactionDate = payment.EffectiveDate,
                AcctgTransEntries = acctgTransEntries
            };

            // Create AcctgTrans
            var acctgTransId = await CreateAcctgTransAndEntries(createParams);

            // Process PaymentApplications for additional transactions
            foreach (var paymentApplication in paymentApplications)
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.InvoiceId == paymentApplication.InvoiceId);
                if (invoice == null) continue;
                if (invoice.StatusId == "INVOICE_READY" || invoice.StatusId == "INVOICE_PAID")
                {
                    if (payment.PaymentTypeId == "CUSTOMER_REFUND")
                    {
                        var cRefundAcctgTransId = await CreateAcctgTransAndEntriesForCustomerRefundPaymentApplication(
                            paymentApplication.PaymentApplicationId);
                        _logger.LogInformation(
                            $"Accounting transaction {cRefundAcctgTransId} created for customer refund payment application {paymentApplication.PaymentApplicationId}");
                    }
                    else
                    {
                        var paAcctgTransId = await CreateAcctgTransAndEntriesForPaymentApplication(
                            paymentApplication.PaymentApplicationId);
                        _logger.LogInformation(
                            $"Accounting transaction {paAcctgTransId} created for payment application {paymentApplication.PaymentApplicationId}");
                    }
                }
            }

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating AcctgTrans for outgoing payment {paymentId}");
            throw new Exception($"An error occurred while creating outgoing payment AcctgTrans for {paymentId}.", ex);
        }
    }

    public async Task<string> CreateAcctgTransAndEntriesForPayrollPayment(string paymentId)
    {
        try
        {
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                _logger.LogWarning($"Payment with ID {paymentId} was not found.");
                return null;
            }

            var organizationPartyId = payment.PartyIdFrom;
            var stamp = DateTime.UtcNow;
            var paymentAmount = _acctgMiscService.CustomRound(payment.Amount, (int)ledgerDecimals, roundingMode);

            // 1. Resolve Credit Account (Bank/Cash)
            var creditAccountParams = new GetGlAccountFromAccountTypeParams
            {
                OrganizationPartyId = organizationPartyId,
                AcctgTransTypeId = "OUTGOING_PAYMENT",
                DebitCreditFlag = "C",
                PaymentId = paymentId
            };
            var creditGlAccountId = await GetGlAccountFromAccountType(creditAccountParams);

            // 2. Identify relevant invoices
            if (!payment.EffectiveDate.HasValue)
            {
                _logger.LogWarning($"Payment {paymentId} has no effective date.");
                return null;
            }

            int month = payment.EffectiveDate.Value.Month;
            int year = payment.EffectiveDate.Value.Year;
            DateOnly monthStart = new DateOnly(year, month, 1);
            DateOnly monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // PreferredPaymentMethodId mapping (CASH -> CASH, everything else -> BANK_TRANSFER)
            string preferredMethodId = payment.PaymentMethodId == PayrollConstants.CashMethod
                ? PayrollConstants.CashMethod
                : PayrollConstants.BankTransferMethod;

            // Which invoices belong to THIS payment is decided by the method snapshot the batch
            // wrote onto each invoice at run time (PayrollConstants.PaymentMethodAttrName), not by
            // the employee's CURRENT Party.PreferredPayrollPaymentMethodId. Editing an employee's
            // preference after a run used to silently move their invoice between the cash and bank
            // settlements, so a payment created months earlier no longer matched any run total and
            // could never be posted.
            //
            // Invoices predating the snapshot have no attribute; for those we fall back to the live
            // preference, which is exactly the old behaviour — no worse for historical data, and
            // every run executed from now on is immutable. The fallback keys on the ROW's absence,
            // not on a null value: an employee run with no preference set gets a snapshot row with a
            // null AttrValue, and that must stay matched by neither payment (the two run totals in
            // BatchCreatePayrollInvoices exclude them too) rather than falling through to the party.
            var invoiceData = await (from inv in _context.Invoices
                                     join prty in _context.Parties on inv.PartyIdFrom equals prty.PartyId
                                     join attr in _context.InvoiceAttributes
                                             .Where(a => a.AttrName == PayrollConstants.PaymentMethodAttrName)
                                         on inv.InvoiceId equals attr.InvoiceId into attrJoin
                                     from attr in attrJoin.DefaultIfEmpty()
                                     where inv.InvoiceTypeId == "PAYROL_INVOICE"
                                           && inv.PartyId == organizationPartyId
                                           && inv.InvoiceDate >= monthStart
                                           && inv.InvoiceDate <= monthEnd
                                           && (attr != null
                                               ? attr.AttrValue
                                               : prty.PreferredPayrollPaymentMethodId) == preferredMethodId
                                     select new { Invoice = inv, Party = prty }).ToListAsync();

            // 3. Total the candidate run so we can check the aggregate settlement against it.
            //    NOTE: an early version debited EVERY month+method invoice for any payment, so each
            //    extra cash/bank payroll payment re-booked the whole run (triple-booking).
            var invoiceTotals = new Dictionary<string, decimal>();
            decimal runTotal = 0m;
            foreach (var d in invoiceData)
            {
                var t = await _invoiceUtilityService.GetInvoiceTotal(d.Invoice.InvoiceId, true);
                invoiceTotals[d.Invoice.InvoiceId] = t;
                runTotal += t;
            }
            runTotal = _acctgMiscService.CustomRound(runTotal, (int)ledgerDecimals, roundingMode);

            // A payment is the batch's aggregate settlement (it may fan out to every employee
            // invoice) when it is addressed to the shared staff party with no directed account.
            // Anything else — a raise, a bonus, an individual settlement — posts only what it pays.
            //
            // This is deliberately NOT inferred from `runTotal == paymentAmount`. The run total is
            // recomputed here from live data, so any later change to it reclassified a months-old
            // aggregate payment as ad-hoc, which then went looking for an accrued account on the
            // staff party — a party that has none by design, and never will. The identity of a
            // payment is a fact about the payment, not about whether the arithmetic still lines up.
            // Same marker GetProjectPayroll / GetCompanyReport.GetPayroll / Fact_Project_Payroll use.
            bool isAggregateRunSettlement =
                payment.PartyIdTo == PayrollConstants.StaffPartyId
                && string.IsNullOrEmpty(payment.OverrideGlAccountId);

            // The arithmetic is now a validation rather than a classification, so a mismatch fails
            // where it can be explained. It means the run moved after this payment was created — an
            // invoice edited, or an employee's payment method changed — and either the payment
            // amount or the invoices need correcting before it can settle the run.
            if (isAggregateRunSettlement && runTotal != paymentAmount)
            {
                throw new InvalidOperationException(
                    $"Payroll payment {paymentId} settles the whole {preferredMethodId} run for " +
                    $"{month:D2}/{year}, but its amount no longer matches that run: payment {paymentAmount} " +
                    $"vs run total {runTotal} across {invoiceData.Count} invoice(s), a difference of " +
                    $"{paymentAmount - runTotal}. The run has changed since the payment was created " +
                    $"(an invoice was edited, or an employee's payroll payment method was changed). " +
                    $"Correct the payment amount or the invoices so the two agree, then post again.");
            }

            var entries = new List<AcctgTransEntry>();

            // Line 001 — credit cash/bank for THIS payment's own amount (always correct).
            entries.Add(new AcctgTransEntry
            {
                AcctgTransEntrySeqId = "001",
                GlAccountId = creditGlAccountId,
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = paymentAmount,
                OrganizationPartyId = organizationPartyId,
                PartyId = payment.PartyIdTo,
                RoleTypeId = "BILL_FROM_VENDOR",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            });

            if (isAggregateRunSettlement)
            {
                // ===== Mode 1: aggregate — debit each employee's payroll invoice (unchanged behavior) =====
                int seq = 2;
                foreach (var data in invoiceData)
                {
                    var invoiceTotal = invoiceTotals[data.Invoice.InvoiceId];
                    if (invoiceTotal == 0) continue;

                    // The payment ALWAYS clears the employee's accrued Net Salary Payable — the
                    // ACCOUNTS_PAYABLE account the payroll invoice credited. It must NOT use
                    // GlAccountIdAdvancedPayment: that is the EXPENSE account (e.g. project 124426 or
                    // 601000) which the invoice already debited, so re-debiting it here would double-book
                    // the salary cost and leave the payable uncleared. (Bank employees only posted
                    // correctly before because they have no GlAccountIdAdvancedPayment and fell back to
                    // this same accrued lookup.)
                    var accruedGlAccountId = await _context.PartyGlAccounts
                        .Where(pga => pga.PartyId == data.Invoice.PartyIdFrom &&
                                      pga.RoleTypeId == "EMPLOYEE" &&
                                      pga.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                        .Select(pga => pga.GlAccountId)
                        .FirstOrDefaultAsync();

                    // Skipping the debit here would leave the transaction short by this invoice and
                    // fail the balance guard below with an arithmetic message that names no cause,
                    // so fail here instead, where the missing account can be pointed at.
                    if (string.IsNullOrEmpty(accruedGlAccountId))
                    {
                        throw new InvalidOperationException(
                            $"Cannot post payroll payment {paymentId}: employee {data.Invoice.PartyIdFrom} " +
                            $"(invoice {data.Invoice.InvoiceId}, {invoiceTotal}) has no EMPLOYEE " +
                            $"ACCOUNTS_PAYABLE account to clear. Link the employee's accrued salary " +
                            $"account, then post again.");
                    }

                    entries.Add(new AcctgTransEntry
                    {
                        AcctgTransEntrySeqId = (seq++).ToString("D3"),
                        GlAccountId = accruedGlAccountId,
                        GlAccountTypeId = "ACCOUNTS_PAYABLE",
                        DebitCreditFlag = "D",
                        AcctgTransEntryTypeId = "_NA_",
                        Amount = invoiceTotal,
                        OrganizationPartyId = organizationPartyId,
                        PartyId = data.Invoice.PartyIdFrom,
                        RoleTypeId = "EMPLOYEE",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        Description = data.Invoice.Description ?? payment.Comments,
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    });
                }
            }
            else
            {
                // ===== Mode 2: directed / ad-hoc payment — post ONLY what this payment pays =====
                // A payment to an individual employee: a raise, a bonus, or settling one salary
                // outside the batch run. Debit account precedence: explicit OverrideGlAccountId,
                // else the recipient's own accrued payable. Never fans out to the whole run.
                var debitGlAccountId = payment.OverrideGlAccountId;
                if (string.IsNullOrEmpty(debitGlAccountId))
                {
                    debitGlAccountId = await _context.PartyGlAccounts
                        .Where(pga => pga.PartyId == payment.PartyIdTo &&
                                      pga.OrganizationPartyId == organizationPartyId &&
                                      pga.RoleTypeId == "EMPLOYEE" &&
                                      pga.GlAccountTypeId == "ACCOUNTS_PAYABLE")
                        .Select(pga => pga.GlAccountId)
                        .FirstOrDefaultAsync();
                }

                if (string.IsNullOrEmpty(debitGlAccountId))
                {
                    throw new InvalidOperationException(
                        $"Cannot post payroll payment {paymentId} to party {payment.PartyIdTo}: it has no " +
                        $"OverrideGlAccountId and no EMPLOYEE ACCOUNTS_PAYABLE account. A payroll payment " +
                        $"that is not the batch's aggregate settlement (those go to party " +
                        $"{PayrollConstants.StaffPartyId}) must be addressed to an employee with an accrued " +
                        $"salary account, or carry an explicit GL account.");
                }

                entries.Add(new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = "002",
                    GlAccountId = debitGlAccountId,
                    GlAccountTypeId = "ACCOUNTS_PAYABLE",
                    DebitCreditFlag = "D",
                    AcctgTransEntryTypeId = "_NA_",
                    Amount = paymentAmount,
                    OrganizationPartyId = organizationPartyId,
                    PartyId = payment.PartyIdTo,
                    RoleTypeId = "EMPLOYEE",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    Description = payment.Comments,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                });
            }

            // 4. Hard balance guard — never persist an unbalanced payroll transaction.
            decimal debitTotal = entries.Where(e => e.DebitCreditFlag == "D").Sum(e => e.Amount ?? 0m);
            decimal creditTotal = entries.Where(e => e.DebitCreditFlag == "C").Sum(e => e.Amount ?? 0m);
            if (Math.Abs(debitTotal - creditTotal) >= 0.01m)
            {
                throw new InvalidOperationException(
                    $"Refusing to post unbalanced payroll AcctgTrans for payment {paymentId}: " +
                    $"debit {debitTotal} vs credit {creditTotal} " +
                    $"(mode={(isAggregateRunSettlement ? "aggregate" : "adhoc")}, month {month}/{year}, method {preferredMethodId}).");
            }

            // 5. Persist atomically via the shared helper (assigns AcctgTransId, sets IsPosted='Y').
            var acctgTransId = await CreateAcctgTransAndEntries(new CreateAcctgTransAndEntriesParams
            {
                AcctgTransTypeId = "OUTGOING_PAYMENT",
                GlFiscalTypeId = "ACTUAL",
                PaymentId = payment.PaymentId,
                TransactionDate = payment.EffectiveDate,
                Description = payment.Comments,
                PartyId = payment.PartyIdTo,
                RoleTypeId = "BILL_FROM_VENDOR",
                AcctgTransEntries = entries
            });

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating hard-coded payroll payment AcctgTrans for {paymentId}");
            throw;
        }
    }

    public async Task<List<string>> PostAcctgTrans(string acctgTransId, bool verifyOnly = false)
    {
        var messages = new List<string>();

        // -------------------------------------------------------------------
        // 1) <entity-one entity-name="AcctgTrans" value-field="acctgTrans">
        // -------------------------------------------------------------------
        var acctgTrans = await _context.AcctgTrans.FindAsync(acctgTransId);

        if (acctgTrans == null)
        {
            messages.Add($"AcctgTrans with ID {acctgTransId} not found.");
            return messages; // <add-error><check-errors/>
        }

        // <if-compare field="acctgTrans.isPosted" operator="equals" value="Y">
        if (acctgTrans.IsPosted == "Y")
        {
            messages.Add("AccountingNotPostingGlAccountTransactionAlreadyPosted");
            return messages; // <add-error><check-errors/>
        }

        // -------------------------------------------------------------------
        // 2) Check trial balance via <call-service service-name="calculateAcctgTransTrialBalance">
        //    Storing result in "trialBalanceResultMap"
        // -------------------------------------------------------------------
        var trialBalanceResultMap = await CalculateAcctgTransTrialBalance(acctgTransId);

        // Evaluate trial balance differences
        if (trialBalanceResultMap.DebitCreditDifference >= 0.01m)
        {
            messages.Add("AccountingNotPostingGlAccountTransactionTrialBalanceFailed");
        }

        if (trialBalanceResultMap.DebitCreditDifference <= -0.01m)
        {
            messages.Add("AccountingNotPostingGlAccountTransactionTrialBalanceFailed");
        }

        if (trialBalanceResultMap.DebitTotal == 0 && trialBalanceResultMap.CreditTotal != 0)
        {
            messages.Add("AccountingNotPostingGlAccountTransactionDebitZero");
        }

        if (trialBalanceResultMap.CreditTotal == 0 && trialBalanceResultMap.DebitTotal != 0)
        {
            messages.Add("AccountingNotPostingGlAccountTransactionCreditZero");
        }

        // -------------------------------------------------------------------
        // 3) <entity-condition entity-name="AcctgTransEntry" list="acctgTransEntryList">
        // -------------------------------------------------------------------

        var acctgTransEntryList = await _utilityService.FindLocalOrDatabaseListAsync<AcctgTransEntry>(query =>
            query.Where(cc =>
                cc.AcctgTransId == acctgTransId));

        // -------------------------------------------------------------------
        // 4) If scheduledPostingDate is set => check if in the future
        //    <if-not-empty field="acctgTrans.scheduledPostingDate">
        // Business Explanation:
        // Some transactions are scheduled to be posted at a future date (e.g., for recurring payments or
        // deferred accounting events). This step checks if the transaction has a ScheduledPostingDate. If it
        // does, and the current date is earlier than the scheduled date, the transaction cannot be posted yet.
        // An error message is added to indicate that the transaction is not yet due for posting. This ensures
        // that transactions are processed in accordance with their intended timing, maintaining proper
        // financial period reporting and compliance with organizational policies.
        // -------------------------------------------------------------------
        if (acctgTrans.ScheduledPostingDate.HasValue)
        {
            var scheduledPostingDate = acctgTrans.ScheduledPostingDate.Value;
            bool beforeScheduled = DateTime.UtcNow < scheduledPostingDate;
            if (beforeScheduled)
            {
                messages.Add("AccountingNotPostingGlAccountTransactionNotScheduledToBePosted");
            }
        }

        // -------------------------------------------------------------------
        // 5) Create list: onlyIncludePeriodTypeIdList[] => FISCAL_YEAR, ...
        // -------------------------------------------------------------------
        var onlyIncludePeriodTypeIdList = new List<string>
        {
            "FISCAL_YEAR",
            "FISCAL_QUARTER",
            "FISCAL_MONTH",
            "FISCAL_WEEK",
            "FISCAL_BIWEEK"
        };

        // -------------------------------------------------------------------
        // 6) For each acctgTransEntry => if empty in customTimePeriod cache => call findCustomTimePeriods
        //    Also check glAccountId & amount
        // This step validates each transaction entry to ensure it is associated with a valid fiscal period,
        // a proper general ledger account, and a valid amount. The process is broken down as follows:
        // - Custom Time Period Check: For each entry, the system retrieves the fiscal periods
        //   (CustomTimePeriod) applicable to the transaction’s date and the organization
        //   (OrganizationPartyId). The FindCustomTimePeriods service ensures that only periods matching the
        //   defined onlyIncludePeriodTypeIdList are considered, and it excludes periods not tied to a
        //   specific organization. If no valid periods are found, an error is added, indicating the
        //   transaction cannot be posted due to a lack of an appropriate fiscal period. Additionally, if
        //   any period is closed (e.g., the fiscal month is finalized), an error is added to prevent posting
        //   to a closed period, which could disrupt finalized financial statements.
        // - General Ledger Account Check: Each entry must specify a valid GlAccountId to indicate which
        //   account in the chart of accounts is affected. If this field is missing, an error is added, as
        //   this would prevent proper allocation of the transaction in the general ledger.
        // - Amount Check: Each entry must have a non-null amount, as this represents the financial value of
        //   the debit or credit. Missing amounts would render the transaction incomplete, so an error is
        //   added if this is the case.
        // These checks ensure that the transaction entries are complete, correctly assigned to open fiscal
        // periods, and properly linked to general ledger accounts, maintaining compliance with accounting
        // standards.
        // -------------------------------------------------------------------
        var customTimePeriodListByOrganizationPartyIdMap = new Dictionary<string, List<CustomTimePeriod>>();
        foreach (var entry in acctgTransEntryList)
        {
            if (!customTimePeriodListByOrganizationPartyIdMap.ContainsKey(entry.OrganizationPartyId))
            {
                // <call-service service-name="findCustomTimePeriods">
                var customTimePeriodList = await FindCustomTimePeriods(
                    (DateTime)acctgTrans.TransactionDate,
                    entry.OrganizationPartyId,
                    "Y", // excludeNoOrganizationPeriods
                    onlyIncludePeriodTypeIdList
                );

                if (customTimePeriodList == null || customTimePeriodList.Count == 0)
                {
                    messages.Add("AccountingNoCustomTimePeriodFoundForTransactionDate");
                }
                else
                {
                    // Check if any isClosed == Y
                    foreach (var customTimePeriod in customTimePeriodList)
                    {
                        if (customTimePeriod.IsClosed == "Y")
                        {
                            messages.Add("AccountingNoCustomTimePeriodClosed");
                        }
                    }
                }

                customTimePeriodListByOrganizationPartyIdMap[entry.OrganizationPartyId] =
                    customTimePeriodList ?? new List<CustomTimePeriod>();
            }

            if (string.IsNullOrEmpty(entry.GlAccountId))
            {
                messages.Add("AccountingGlAccountNotSetForAccountType");
            }

            if (!entry.Amount.HasValue)
            {
                messages.Add("AccountingGlAccountAmountNotSet");
            }
        }

        // -------------------------------------------------------------------
        // 7) If verifyOnly => return
        // -------------------------------------------------------------------
        if (verifyOnly)
        {
            return messages;
        }

        // -------------------------------------------------------------------
        // 8) If errors => handle error journal logic
        // -------------------------------------------------------------------
        if (messages.Any())
        {
            // For each entry => <call-service service-name="getPartyAccountingPreferences">
            foreach (var entry in acctgTransEntryList)
            {
                var partyAcctgPreference =
                    await _acctgMiscService.GetPartyAccountingPreferences(entry.OrganizationPartyId);
                if (partyAcctgPreference == null || partyAcctgPreference.EnableAccounting == "N")
                {
                    // <log ... ><return/>
                    return messages;
                }

                if (string.IsNullOrEmpty(partyAcctgPreference.ErrorGlJournalId))
                {
                    // <check-errors/>
                    return messages;
                }
                else
                {
                    // <set field="acctgTrans.glJournalId" from-field="partyAcctgPreference.errorGlJournalId"/>
                    acctgTrans.GlJournalId = partyAcctgPreference.ErrorGlJournalId;
                    _context.AcctgTrans.Update(acctgTrans);

                    // Simplified to use string concatenation to avoid potential issues with string interpolation
                    var warningMessage = "The accounting transaction " + (acctgTrans?.AcctgTransId ?? "Unknown") +
                                         " has been posted to the Error Journal " +
                                         (partyAcctgPreference?.ErrorGlJournalId ?? "Unknown") + ".";
                    messages.Add(warningMessage);

                    return messages;
                }
            }

            return messages;
        }

        // <check-errors/>
        if (messages.Any())
        {
            return messages;
        }

        // -------------------------------------------------------------------
        // 9) Mark AcctgTrans as posted => <call-service service-name="updateAcctgTrans" ...>
        // -------------------------------------------------------------------
        acctgTrans.IsPosted = "Y";
        acctgTrans.PostedDate = DateTime.UtcNow;
        await _acctgTransService.UpdateAcctgTrans(acctgTrans);

        return messages;
    }


    public async Task<TrialBalanceResult> CalculateAcctgTransTrialBalance(string acctgTransId)
    {
        // 1. Retrieve rounding settings from GlArithmeticSettings
        var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
        var ledgerDecimals = glSettings.DecimalScale;
        var roundingMode = glSettings.RoundingMode;

        // 2. Load the AcctgTransEntry records for this AcctgTrans
        var acctgTransEntries = await _context.AcctgTransEntries
            .Where(e => e.AcctgTransId == acctgTransId)
            .OrderBy(e => e.AcctgTransEntrySeqId)
            .ToListAsync();

        decimal debitTotal = 0m;
        decimal creditTotal = 0m;

        // 3. Sum the amounts by DebitCreditFlag
        foreach (var entry in acctgTransEntries)
        {
            if (entry.DebitCreditFlag == "D")
            {
                debitTotal += entry.Amount ?? 0m;
            }
            else if (entry.DebitCreditFlag == "C")
            {
                creditTotal += entry.Amount ?? 0m;
            }
            else
            {
                // This matches the Ofbiz logic that raises an error for invalid flags
                var msg = $"Invalid debitCreditFlag '{entry.DebitCreditFlag}' " +
                          $"for AcctgTransEntry (Id={entry.AcctgTransId}, Seq={entry.AcctgTransEntrySeqId}).";
                _logger.LogError(msg);
                throw new Exception(msg);
            }
        }

        // 4. Round debitTotal and creditTotal using your CustomRound method
        debitTotal = _acctgMiscService.CustomRound(debitTotal, (int)ledgerDecimals, roundingMode);
        creditTotal = _acctgMiscService.CustomRound(creditTotal, (int)ledgerDecimals, roundingMode);

        // 5. Compute the difference (debits - credits) and round again
        var difference = debitTotal - creditTotal;
        difference = _acctgMiscService.CustomRound(difference, (int)ledgerDecimals, roundingMode);

        // 6. Return the results
        return new TrialBalanceResult
        {
            DebitTotal = debitTotal,
            CreditTotal = creditTotal,
            DebitCreditDifference = difference
        };
    }

    /// <summary>
    /// Replicates the Ofbiz "findCustomTimePeriods" logic.
    /// </summary>
    /// <param name="findDate">A date to filter periods by.</param>
    /// <param name="organizationPartyId">Optional. If present, find periods for this org and its parents.</param>
    /// <param name="excludeNoOrganizationPeriods">If "Y", skip periods with no organizationPartyId.</param>
    /// <param name="onlyIncludePeriodTypeIdList">Optional list of period types to include.</param>
    /// <returns>A combined list of matching CustomTimePeriod records.</returns>
    public async Task<List<CustomTimePeriod>> FindCustomTimePeriods(
        DateTime findDate,
        string organizationPartyId = null,
        string excludeNoOrganizationPeriods = null,
        List<string> onlyIncludePeriodTypeIdList = null)
    {
        // This will hold all the matching records
        var listSoFar = new List<CustomTimePeriod>();

        // ----------------------------------------------------------------------
        // 1) If organizationPartyId is provided:
        //    - Find all parent organizations (including the org itself)
        //    - For each one, get CustomTimePeriods that match the constraints
        // ----------------------------------------------------------------------
        if (!string.IsNullOrEmpty(organizationPartyId))
        {
            // In Ofbiz, it calls getParentOrganizations to walk up the tree.
            // We'll assume you have a method that returns a list of parent IDs
            var parentOrganizationPartyIdList = await GetParentOrganizations(organizationPartyId);

            // Include the current orgPartyId if not already in the list
            if (!parentOrganizationPartyIdList.Contains(organizationPartyId))
            {
                parentOrganizationPartyIdList.Add(organizationPartyId);
            }

            foreach (var curOrganizationPartyId in parentOrganizationPartyIdList)
            {
                // Build a query for CustomTimePeriod
                var query = _context.CustomTimePeriods.AsQueryable();

                // Match the organizationPartyId exactly
                query = query.Where(ct => ct.OrganizationPartyId == curOrganizationPartyId);

                // fromDate <= findDate
                query = query.Where(ct => ct.FromDate <= findDate);

                // (thruDate >= findDate OR thruDate == null)
                query = query.Where(ct => ct.ThruDate == null || ct.ThruDate >= findDate);

                // If onlyIncludePeriodTypeIdList is provided, filter by PeriodTypeId in that list
                if (onlyIncludePeriodTypeIdList != null && onlyIncludePeriodTypeIdList.Any())
                {
                    query = query.Where(ct => onlyIncludePeriodTypeIdList.Contains(ct.PeriodTypeId));
                }

                // Fetch and add to listSoFar
                var orgTimePeriodList = await query.ToListAsync();
                listSoFar.AddRange(orgTimePeriodList);
            }
        }

        // ----------------------------------------------------------------------
        // 2) If excludeNoOrganizationPeriods != "Y":
        //    - Also find CustomTimePeriods where organizationPartyId is NULL or "_NA_"
        // ----------------------------------------------------------------------
        if (!string.Equals(excludeNoOrganizationPeriods, "Y", StringComparison.OrdinalIgnoreCase))
        {
            var query = _context.CustomTimePeriods.AsQueryable();

            // organizationPartyId == null or == "_NA_"
            query = query.Where(ct =>
                ct.OrganizationPartyId == null ||
                ct.OrganizationPartyId == "_NA_"
            );

            // fromDate <= findDate
            query = query.Where(ct => ct.FromDate <= findDate);

            // (thruDate >= findDate OR thruDate == null)
            query = query.Where(ct => ct.ThruDate == null || ct.ThruDate >= findDate);

            // Filter by onlyIncludePeriodTypeIdList if provided
            if (onlyIncludePeriodTypeIdList != null && onlyIncludePeriodTypeIdList.Any())
            {
                query = query.Where(ct => onlyIncludePeriodTypeIdList.Contains(ct.PeriodTypeId));
            }

            var generalCustomTimePeriodList = await query.ToListAsync();
            listSoFar.AddRange(generalCustomTimePeriodList);
        }

        // ----------------------------------------------------------------------
        // 3) Return the combined results
        //    (Ofbiz puts them in "listSoFar" and returns "customTimePeriodList")
        // ----------------------------------------------------------------------
        return listSoFar;
    }

    /// <summary>
    /// Replicates the Ofbiz getParentOrganizations logic:
    /// - Finds PARENT_ORGANIZATION from relationships of type GROUP_ROLLUP
    /// - The child side has a role of ORGANIZATION_UNIT (or children of that type)
    /// - If getParentsOfParents == "Y", recurse up for each parent
    /// - The returned list includes the original orgPartyId
    /// </summary>
    /// <param name="organizationPartyId">The child org's PartyId.</param>
    /// <param name="getParentsOfParents">Optional. Defaults to "Y". If "Y", recurses further up.</param>
    /// <returns>A list of parent organization PartyIds, including the original orgPartyId.</returns>
    public async Task<List<string>> GetParentOrganizations(
        string organizationPartyId,
        string getParentsOfParents = "Y"
    )
    {
        if (string.IsNullOrEmpty(organizationPartyId))
            return new List<string>();

        // We'll keep track of all related party IDs in this list
        // Start by adding the current org to match the Ofbiz behavior.
        var relatedPartyIdList = new List<string> { organizationPartyId };

        // We only proceed if the orgPartyId is actually an organization
        // (In Ofbiz, this is implied by roleTypeIdFrom=ORGANIZATION_UNIT, etc.)

        // Recurse logic:
        bool recurse = (getParentsOfParents ?? "Y").Equals("Y", StringComparison.OrdinalIgnoreCase);

        // We'll use a queue/stack approach to get all parents 
        // or keep track of visited orgs to prevent loops
        var queue = new Queue<string>();
        queue.Enqueue(organizationPartyId);

        var visited = new HashSet<string> { organizationPartyId };

        while (queue.Count > 0)
        {
            var currentOrgId = queue.Dequeue();

            // 1) Find direct parent relationships for currentOrgId
            //    where relationshipType == GROUP_ROLLUP
            //    and roles are {child=ORGANIZATION_UNIT, parent=PARENT_ORGANIZATION}
            //    *We can also handle the reversed relationship if "includeFromToSwitched" is "Y" in Ofbiz*
            var parentRelationships = await _context.PartyRelationships
                .Where(pr =>
                    pr.PartyRelationshipTypeId == "GROUP_ROLLUP" &&
                    (
                        // child is currentOrgId with role=ORGANIZATION_UNIT, parent is role=PARENT_ORGANIZATION
                        (pr.PartyIdFrom == currentOrgId && pr.RoleTypeIdFrom == "ORGANIZATION_UNIT"
                                                        && pr.RoleTypeIdTo == "PARENT_ORGANIZATION")
                        // or reversed roles if needed
                        ||
                        (pr.PartyIdTo == currentOrgId && pr.RoleTypeIdTo == "ORGANIZATION_UNIT"
                                                      && pr.RoleTypeIdFrom == "PARENT_ORGANIZATION")
                    )
                )
                .ToListAsync();

            // 2) For each found parent, add to the list if not already
            foreach (var rel in parentRelationships)
            {
                // Determine which side is the parent org
                // e.g., if PartyIdFrom is the parent, that means PartyIdTo is the child
                var parentOrgId = rel.PartyIdFrom;
                if (rel.RoleTypeIdFrom == "ORGANIZATION_UNIT")
                {
                    // That means pr.PartyIdTo is the parent
                    parentOrgId = rel.PartyIdTo;
                }

                // Add to results if not visited
                if (!visited.Contains(parentOrgId))
                {
                    visited.Add(parentOrgId);
                    relatedPartyIdList.Add(parentOrgId);
                    // If recursion is desired, queue the parent
                    if (recurse) queue.Enqueue(parentOrgId);
                }
            }
        }

        return relatedPartyIdList;
    }

    /// <summary>
    /// Translates the OFBiz "closeFinancialTimePeriod" service into C#.
    /// 1) Load the customTimePeriod by primary key.
    /// 2) Ensure there are no open child time periods.
    /// 3) Find the last closed date/time period (call "findLastClosedDate").
    /// 4) Gather the GL account classes for expense, revenue, income, etc.
    /// 5) Sum transactions from [lastClosedDate, customTimePeriod.thruDate) for expense/revenue/income.
    /// 6) Retrieve party accounting prefs, find "profitLossAccount," see if already posted or create period-closing entries.
    /// 7) For all relevant GL accounts, compute and store glAccountHistory.
    /// 8) Update the customTimePeriod -> isClosed = "Y".
    /// 
    /// We add a try/catch to handle errors and rethrow or handle as needed.
    /// </summary>
    public async Task CloseFinancialTimePeriod(string customTimePeriodId)
    {
        try
        {
            // 1) Load the CustomTimePeriod by pk
            var customTimePeriod = await _context.CustomTimePeriods
                .Where(ct => ct.CustomTimePeriodId == customTimePeriodId)
                .FirstOrDefaultAsync();
            if (customTimePeriod == null)
            {
                // Equivalent to entity-one with no result
                throw new Exception($"CustomTimePeriod not found [customTimePeriodId={customTimePeriodId}].");
            }

            // 2) We gather open child time periods
            // The script does get-related "ChildCustomTimePeriod" with isClosed = "N"
            var openChildTimePeriods = await _context.CustomTimePeriods
                .Where(ct => ct.ParentPeriodId == customTimePeriodId && ct.IsClosed == "N")
                .ToListAsync();
            if (openChildTimePeriods.Any())
            {
                // The script adds an error:
                // <fail-property resource="AccountingUiLabels" property="AccountingNoCustomTimePeriodClosedChild"/>
                throw new Exception("Cannot close this period because child time periods are still open.");
            }

            // 3) FindLastClosedDate now requires 3 params: (organizationPartyId, findDate, periodTypeId)
            // The script sets: 
            //   findLastClosedDateInMap.organizationPartyId = customTimePeriod.organizationPartyId
            //   findLastClosedDateInMap.periodTypeId = customTimePeriod.periodTypeId
            //   We pass customTimePeriod.thruDate as the 'findDate'
            var lastClosedResult = await _acctgReportsService.Value.FindLastClosedDate(
                customTimePeriod.OrganizationPartyId,
                customTimePeriod.ThruDate, // pass the time period's ThruDate as findDate
                customTimePeriod.PeriodTypeId // pass the same periodTypeId 
            );

            var lastClosedDate = lastClosedResult.LastClosedDate;
            var lastClosedTimePeriod = lastClosedResult.LastClosedTimePeriod;
            if (!lastClosedDate.HasValue)
            {
                throw new Exception(
                    "No closed time period found for this type. Unable to proceed with closeFinancialTimePeriod.");
            }

            // 4) Gather the GL account classes for expense, revenue, income, asset, contra-asset, liability, equity
            var expenseAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("EXPENSE");
            var revenueAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("REVENUE");
            var incomeAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("INCOME");
            var assetAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("ASSET");
            var contraAssetAccountClassIds =
                await _acctgReportsService.Value.GetDescendantGlAccountClassIds("CONTRA_ASSET");
            var liabilityAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("LIABILITY");
            var equityAccountClassIds = await _acctgReportsService.Value.GetDescendantGlAccountClassIds("EQUITY");

            // 5) Query AcctgTransAndEntries for expense/revenue/income in [lastClosedDate, customTimePeriod.thruDate), posted=Y, glFiscalTypeId=ACTUAL, not PERIOD_CLOSING
            // We'll sum totalCreditAmount, totalDebitAmount => totalAmount = totalCreditAmount - totalDebitAmount
            decimal totalCreditAmount = 0m;
            decimal totalDebitAmount = 0m;
            decimal totalAmount = 0m;

            var queryTransAndEntries = from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == customTimePeriod.OrganizationPartyId
                      && act.IsPosted == "Y" // posted
                      && act.GlFiscalTypeId == "ACTUAL"
                      && act.TransactionDate >= lastClosedDate.Value
                      && act.TransactionDate < customTimePeriod.ThruDate // exclusive
                      && act.AcctgTransTypeId != "PERIOD_CLOSING"
                select new
                {
                    ate,
                    act
                };

            // Now filter by (glAccountClassId in expense/revenue/income)
            // That means we need to join glAccounts
            // Then join glAccounts to filter by account classes (expense, revenue, income)
            queryTransAndEntries = queryTransAndEntries
                .Join(_context.GlAccounts,
                    x => x.ate.GlAccountId,
                    ga => ga.GlAccountId,
                    (x, ga) => new { x.ate, x.act, glAccount = ga })
                .Where(z => expenseAccountClassIds.Contains(z.glAccount.GlAccountClassId)
                            || revenueAccountClassIds.Contains(z.glAccount.GlAccountClassId)
                            || incomeAccountClassIds.Contains(z.glAccount.GlAccountClassId))
                .Select(z => new { z.ate, z.act }); // Project back to the original shape

            var transEntriesList = await queryTransAndEntries.ToListAsync();
            foreach (var row in transEntriesList)
            {
                // If debitCreditFlag == "D", add to totalDebitAmount, else add to totalCreditAmount
                if (row.ate.DebitCreditFlag == "D")
                {
                    totalDebitAmount += (decimal)row.ate.Amount;
                }
                else
                {
                    totalCreditAmount += (decimal)row.ate.Amount;
                }
            }

            totalAmount = totalCreditAmount - totalDebitAmount;
            // that's how the script does <calculate field="totalAmount"> <calcop operator="subtract" ...

            // 6) getPartyAccountingPreferences => we call a method, then load profitLossAccount, etc.
            var partyAcctgPreference =
                await _acctgMiscService.GetPartyAccountingPreferences(customTimePeriod.OrganizationPartyId);

            // get the profitLossAccount from GlAccountTypeDefault => "PROFIT_LOSS_ACCOUNT"
            var profitLossAccount = await _context.GlAccountTypeDefaults
                .Include(x => x.GlAccount) // so we can load the related glAccount
                .Where(td => td.OrganizationPartyId == customTimePeriod.OrganizationPartyId
                             && td.GlAccountTypeId == "PROFIT_LOSS_ACCOUNT")
                .FirstOrDefaultAsync();
            // Now we have profitLossAccount.GlAccountId => the actual profit/loss gl account

            // 6.1) Check if there's already a GlAccountHistory record for that customTimePeriod => 
            // if endingBalance != total => error
            if (profitLossAccount?.GlAccount != null)
            {
                var profitLossAccountHistory = await _context.GlAccountHistories
                    .Where(h =>
                        h.OrganizationPartyId == customTimePeriod.OrganizationPartyId
                        && h.CustomTimePeriodId == customTimePeriod.CustomTimePeriodId
                        && h.GlAccountId == profitLossAccount.GlAccountId)
                    .FirstOrDefaultAsync();

                if (profitLossAccountHistory != null)
                {
                    // check if posted equals total
                    if (profitLossAccountHistory.EndingBalance.HasValue
                        && profitLossAccountHistory.EndingBalance.Value != totalAmount)
                    {
                        throw new Exception("Period already posted with a different balance. Can't close.");
                    }
                    // else do nothing => it's consistent
                }
                else
                {
                    // Means we have not posted the period closing yet. We create the periodClosing transaction
                    // Credit to RETAINED_EARNINGS, debit to PROFIT_LOSS_ACCOUNT for totalAmount
                    // transactionDate = customTimePeriod.thruDate - 1 second
                    var transactionDate = customTimePeriod.ThruDate.GetValueOrDefault().AddSeconds(-1);

                    // We'll build 2 entries
                    var creditEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "C",
                        GlAccountTypeId = "RETAINED_EARNINGS",
                        OrganizationPartyId = customTimePeriod.OrganizationPartyId,
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = partyAcctgPreference.BaseCurrencyUomId
                        // ...
                    };
                    var debitEntry = new AcctgTransEntry
                    {
                        DebitCreditFlag = "D",
                        GlAccountTypeId = "PROFIT_LOSS_ACCOUNT",
                        OrganizationPartyId = customTimePeriod.OrganizationPartyId,
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = partyAcctgPreference.BaseCurrencyUomId
                        // ...
                    };

                    // We'll call a method that replicates "createAcctgTransAndEntries"
                    var createAcctgTransAndEntriesInMap = new CreateAcctgTransAndEntriesParams
                    {
                        GlFiscalTypeId = "ACTUAL",
                        AcctgTransTypeId = "PERIOD_CLOSING",
                        TransactionDate = DateOnly.FromDateTime(transactionDate),
                        AcctgTransEntries = new List<AcctgTransEntry> { creditEntry, debitEntry }
                    };
                    await CreateAcctgTransAndEntries(createAcctgTransAndEntriesInMap);
                }
            }

            // 7) For each relevant GlAccount in GlAccountOrganization that partially overlaps
            //    we do the computeAndStoreGlAccountHistoryBalance
            //    The script does <entity-condition entity-name="GlAccountOrganization" ...>
            //    and calls computeAndStoreGlAccountHistoryBalance for each
            var orgGlAccounts = await _context.GlAccountOrganizations
                .Where(go =>
                    go.OrganizationPartyId == customTimePeriod.OrganizationPartyId
                    && go.FromDate < customTimePeriod.ThruDate
                    && (go.ThruDate == null || go.ThruDate >= customTimePeriod.FromDate)
                )
                .ToListAsync();

            foreach (var orgGlAccount in orgGlAccounts)
            {
                // load or create glAccountHistory
                var glAccountHistory = await _context.GlAccountHistories
                    .Where(h =>
                        h.CustomTimePeriodId == customTimePeriod.CustomTimePeriodId &&
                        h.OrganizationPartyId == customTimePeriod.OrganizationPartyId &&
                        h.GlAccountId == orgGlAccount.GlAccountId)
                    .FirstOrDefaultAsync();
                if (glAccountHistory == null)
                {
                    glAccountHistory = new GlAccountHistory
                    {
                        CustomTimePeriodId = customTimePeriod.CustomTimePeriodId,
                        OrganizationPartyId = customTimePeriod.OrganizationPartyId,
                        GlAccountId = orgGlAccount.GlAccountId
                        // ...
                    };
                    // insert if needed
                    _context.GlAccountHistories.Add(glAccountHistory);
                }

                // call the "computeAndStoreGlAccountHistoryBalance" service
                await ComputeAndStoreGlAccountHistoryBalance(
                    glAccountHistory.CustomTimePeriodId,
                    glAccountHistory.OrganizationPartyId,
                    glAccountHistory.GlAccountId);
            }

            // 8) Update the customTimePeriod => isClosed = "Y"
            // The script calls "updateCustomTimePeriod" with isClosed=Y
            customTimePeriod.IsClosed = "Y";
        }
        catch (Exception ex)
        {
            // Handle or rethrow
            throw new Exception("Error closing financial time period", ex);
        }
    }

    /// <summary>
    /// Updates a CustomTimePeriod record based on the provided primary and non-primary key fields.
    /// </summary>
    /// <param name="customTimePeriodId">The primary key of the CustomTimePeriod to update.</param>
    /// <param name="organizationPartyId">Optional: The organization party ID to update.</param>
    /// <param name="periodTypeId">Optional: The period type ID to update.</param>
    /// <param name="isClosed">Optional: The closed status to update.</param>
    /// <param name="fromDate">Optional: The start date to update.</param>
    /// <param name="thruDate">Optional: The end date to update.</param>
    /// <param name="userPermissions">The permissions of the user performing the operation.</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if the user lacks the required permissions.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified CustomTimePeriod does not exist.</exception>
    /// <exception cref="Exception">Thrown for other unexpected errors.</exception>
    public async Task UpdateCustomTimePeriod(
        string customTimePeriodId,
        string? organizationPartyId = null,
        string? periodTypeId = null,
        string? isClosed = null,
        DateTime? fromDate = null,
        DateTime? thruDate = null
    )
    {
        try
        {
            // Step 2: Validate input
            if (string.IsNullOrEmpty(customTimePeriodId))
            {
                throw new ArgumentException("CustomTimePeriodId is required.");
            }

            // Step 3: Retrieve the existing record by primary key
            var existingRecord = await _context.CustomTimePeriods
                .FirstOrDefaultAsync(ctp => ctp.CustomTimePeriodId == customTimePeriodId);

            if (existingRecord == null)
            {
                throw new KeyNotFoundException($"CustomTimePeriod with ID '{customTimePeriodId}' not found.");
            }

            // Step 4: Update non-primary key fields
            if (!string.IsNullOrEmpty(organizationPartyId))
            {
                existingRecord.OrganizationPartyId = organizationPartyId;
            }

            if (!string.IsNullOrEmpty(periodTypeId))
            {
                existingRecord.PeriodTypeId = periodTypeId;
            }

            if (!string.IsNullOrEmpty(isClosed))
            {
                existingRecord.IsClosed = isClosed;
            }

            if (fromDate.HasValue)
            {
                existingRecord.FromDate = fromDate.Value;
            }

            if (thruDate.HasValue)
            {
                existingRecord.ThruDate = thruDate.Value;
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Rethrow specific permission exceptions for higher-level handling
            throw;
        }
        catch (KeyNotFoundException)
        {
            // Rethrow specific not-found exceptions for higher-level handling
            throw;
        }
        catch (Exception ex)
        {
            // Log and rethrow general exceptions
            throw new Exception("An error occurred while updating the CustomTimePeriod.", ex);
        }
    }

    /// <summary>
    /// Translates the "computeAndStoreGlAccountHistoryBalance" service.
    /// 1) Load GlAccountHistory by PK: (organizationPartyId, customTimePeriodId, glAccountId).
    /// 2) Call ComputeGlAccountBalanceForTimePeriod to get openingBalance, endingBalance, postedDebits, postedCredits.
    /// 3) Update the GlAccountHistory record with these values.
    /// 4) Store (SaveChanges).
    /// </summary>
    /// <param name="organizationPartyId">Organization Party ID (PK field in GlAccountHistory)</param>
    /// <param name="customTimePeriodId">The custom time period ID (PK field)</param>
    /// <param name="glAccountId">The GL Account ID (PK field)</param>
    /// <exception cref="KeyNotFoundException">If the GlAccountHistory record is not found.</exception>
    /// <exception cref="Exception">For other unexpected errors</exception>
    public async Task ComputeAndStoreGlAccountHistoryBalance(
        string organizationPartyId,
        string customTimePeriodId,
        string glAccountId)
    {
        try
        {
            // 1) Load the existing GlAccountHistory record by primary key
            var glAccountHistory = await _context.GlAccountHistories
                .FirstOrDefaultAsync(h =>
                    h.OrganizationPartyId == organizationPartyId &&
                    h.CustomTimePeriodId == customTimePeriodId &&
                    h.GlAccountId == glAccountId);

            if (glAccountHistory == null)
            {
                // The script finds an existing record, or you might decide 
                // to create a new one. In OFBiz, 
                // "entity-one entity-name='GlAccountHistory' auto-field-map='true' value-field='glAccountHistory'"
                // typically implies an existing record. 
                throw new KeyNotFoundException("GlAccountHistory record not found for the specified PK fields.");
            }

            // 2) Call computeGlAccountBalanceForTimePeriod, which returns opening/ending balances, postedDebits/postedCredits
            var balanceResult = await ComputeGlAccountBalanceForTimePeriod(
                organizationPartyId,
                customTimePeriodId,
                glAccountId);

            // 3) Update the glAccountHistory with the computed values
            glAccountHistory.OpeningBalance = balanceResult.OpeningBalance;
            glAccountHistory.EndingBalance = balanceResult.EndingBalance;
            glAccountHistory.PostedDebits = balanceResult.PostedDebits;
            glAccountHistory.PostedCredits = balanceResult.PostedCredits;
        }
        catch (KeyNotFoundException)
        {
            // rethrow for higher-level handling
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Error computing/storing GL Account History balance.", ex);
        }
    }

    /// <summary>
    /// Translates the OFBiz "computeGlAccountBalanceForTimePeriod" service using the
    /// underlying tables (AcctgTransEntry, AcctgTrans, GlAccount) instead of the AcctgTransEntrySums view,
    /// and without extracting the summations into a separate helper function.
    ///
    /// 1) Load the CustomTimePeriod and GlAccount.
    /// 2) Query for 4 sums (Debits & Credits at "opening" and "ending"),
    ///    by joining AcctgTransEntry -> AcctgTrans -> GlAccount inline.
    /// 3) Compute postedDebits, postedCredits, and opening/ending balances
    ///    based on whether the account is debit-based or credit-based.
    /// 4) Return them in ComputeGlAccountBalanceResult.
    /// </summary>
    public async Task<ComputeGlAccountBalanceResult> ComputeGlAccountBalanceForTimePeriod(
        string organizationPartyId,
        string customTimePeriodId,
        string glAccountId)
    {
        try
        {
            // 1) Load the CustomTimePeriod
            var customTimePeriod = await _context.CustomTimePeriods
                .FirstOrDefaultAsync(ctp =>
                    ctp.CustomTimePeriodId == customTimePeriodId &&
                    ctp.OrganizationPartyId == organizationPartyId);

            if (customTimePeriod == null)
            {
                throw new Exception(
                    $"CustomTimePeriod not found [ID={customTimePeriodId}, Org={organizationPartyId}].");
            }

            if (!customTimePeriod.FromDate.HasValue || !customTimePeriod.ThruDate.HasValue)
            {
                throw new Exception("CustomTimePeriod missing FromDate or ThruDate.");
            }

            // 1.2) Load the GlAccount
            var glAccount = await _context.GlAccounts
                .FirstOrDefaultAsync(ga => ga.GlAccountId == glAccountId);
            if (glAccount == null)
            {
                throw new Exception($"GlAccount not found [ID={glAccountId}].");
            }

            var fromDate = customTimePeriod.FromDate.Value;
            var thruDate = customTimePeriod.ThruDate.Value;

            // REFACTOR (2026-08-14): all four sums below now prefer an explicit OPENING_BALANCE
            // snapshot (created via CreateInitialBalanceTrans.cs — a deliberate "record the balance
            // as of go-live" entry) over summing raw history, for exactly the same reason as
            // AcctgReportsService.ComputeGlAccountBalanceForTimePeriod and GetGlAccountTransactionDetails.cs
            // (see either for the full writeup): a snapshot already reflects the net effect of
            // everything dated before it, so also summing that older detail double-counts it. Proven
            // against a real bank statement on GL 110100 — its OPENING_BALANCE snapshot is dated
            // 2025-12-30 and every one of the 41 older rows on that account predates it, never follows.
            //
            // This method computes cumulative-to-date totals ("as of fromDate" and "as of thruDate")
            // rather than in-period deltas, and PostedDebits/PostedCredits are the difference between
            // them — so the SAME preference has to apply to both the opening-date and ending-date
            // queries, or the subtraction stops cancelling out the pre-fromDate history correctly and
            // reintroduces the double count into PostedDebits/PostedCredits instead. Anything dated on
            // or after fromDate always counts normally either way; only the pre-fromDate portion is
            // preference-gated.
            bool hasOpeningBalanceEntry = await (
                from ate in _context.AcctgTransEntries
                join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                where ate.OrganizationPartyId == organizationPartyId
                      && ate.GlAccountId == glAccountId
                      && act.IsPosted == "Y"
                      && act.GlFiscalTypeId == "ACTUAL"
                      && act.AcctgTransTypeId == "OPENING_BALANCE"
                      && act.TransactionDate < fromDate
                select ate.AcctgTransId
            ).AnyAsync();

            // 2) "totalDebitsToOpeningDate": sum of all debit posted amounts with transactionDate < fromDate
            decimal totalDebitsToOpeningDate = 0m;
            {
                // inline join across AcctgTransEntry -> AcctgTrans -> GlAccount
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                    where
                        ate.OrganizationPartyId == organizationPartyId &&
                        ate.GlAccountId == glAccountId &&
                        act.IsPosted == "Y" && // posted
                        act.GlFiscalTypeId == "ACTUAL" &&
                        ate.DebitCreditFlag == "D" && // debit
                        act.TransactionDate < fromDate && // < fromDate
                        act.AcctgTransTypeId != "PERIOD_CLOSING" &&
                        (!hasOpeningBalanceEntry || act.AcctgTransTypeId == "OPENING_BALANCE")
                    select ate.Amount;

                if (await query.AnyAsync())
                {
                    totalDebitsToOpeningDate = (decimal)await query.SumAsync();
                }
            }

            // 2.1) "totalDebitsToEndingDate": sum of all debit posted amounts with transactionDate < thruDate
            decimal totalDebitsToEndingDate = 0m;
            {
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                    where
                        ate.OrganizationPartyId == organizationPartyId &&
                        ate.GlAccountId == glAccountId &&
                        act.IsPosted == "Y" &&
                        act.GlFiscalTypeId == "ACTUAL" &&
                        ate.DebitCreditFlag == "D" && // debit
                        act.TransactionDate < thruDate &&
                        act.AcctgTransTypeId != "PERIOD_CLOSING" &&
                        (act.TransactionDate >= fromDate || !hasOpeningBalanceEntry || act.AcctgTransTypeId == "OPENING_BALANCE")
                    select ate.Amount;

                if (await query.AnyAsync())
                {
                    totalDebitsToEndingDate = (decimal)await query.SumAsync();
                }
            }

            // 2.2) "totalCreditsToOpeningDate": sum of all credit posted amounts with transactionDate < fromDate
            decimal totalCreditsToOpeningDate = 0m;
            {
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                    where
                        ate.OrganizationPartyId == organizationPartyId &&
                        ate.GlAccountId == glAccountId &&
                        act.IsPosted == "Y" &&
                        act.GlFiscalTypeId == "ACTUAL" &&
                        ate.DebitCreditFlag == "C" && // credit
                        act.TransactionDate < fromDate &&
                        act.AcctgTransTypeId != "PERIOD_CLOSING" &&
                        (!hasOpeningBalanceEntry || act.AcctgTransTypeId == "OPENING_BALANCE")
                    select ate.Amount;

                if (await query.AnyAsync())
                {
                    totalCreditsToOpeningDate = (decimal)await query.SumAsync();
                }
            }

            // 2.3) "totalCreditsToEndingDate": sum of all credit posted amounts with transactionDate < thruDate
            decimal totalCreditsToEndingDate = 0m;
            {
                var query = from ate in _context.AcctgTransEntries
                    join act in _context.AcctgTrans on ate.AcctgTransId equals act.AcctgTransId
                    join gla in _context.GlAccounts on ate.GlAccountId equals gla.GlAccountId
                    where
                        ate.OrganizationPartyId == organizationPartyId &&
                        ate.GlAccountId == glAccountId &&
                        act.IsPosted == "Y" &&
                        act.GlFiscalTypeId == "ACTUAL" &&
                        ate.DebitCreditFlag == "C" &&
                        act.TransactionDate < thruDate &&
                        act.AcctgTransTypeId != "PERIOD_CLOSING" &&
                        (act.TransactionDate >= fromDate || !hasOpeningBalanceEntry || act.AcctgTransTypeId == "OPENING_BALANCE")
                    select ate.Amount;

                if (await query.AnyAsync())
                {
                    totalCreditsToEndingDate = (decimal)await query.SumAsync();
                }
            }

            // 3) postedDebits = totalDebitsToEndingDate - totalDebitsToOpeningDate
            decimal postedDebits = totalDebitsToEndingDate - totalDebitsToOpeningDate;

            // postedCredits = totalCreditsToEndingDate - totalCreditsToOpeningDate
            decimal postedCredits = totalCreditsToEndingDate - totalCreditsToOpeningDate;

            // 4) is the account debit-based or credit-based?
            bool isDebit = await _acctgMiscService.IsDebitAccount(glAccount.GlAccountTypeId);

            decimal openingBalance;
            decimal endingBalance;
            if (isDebit)
            {
                // Debit-based
                openingBalance = totalDebitsToOpeningDate - totalCreditsToOpeningDate;
                endingBalance = totalDebitsToEndingDate - totalCreditsToEndingDate;
            }
            else
            {
                // Credit-based
                openingBalance = totalCreditsToOpeningDate - totalDebitsToOpeningDate;
                endingBalance = totalCreditsToEndingDate - totalDebitsToEndingDate;
            }

            // 5) Return the four values
            return new ComputeGlAccountBalanceResult
            {
                OpeningBalance = openingBalance,
                EndingBalance = endingBalance,
                PostedDebits = postedDebits,
                PostedCredits = postedCredits
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error computing GL account balance for time period.", ex);
        }
    }

    // <summary>
    /// Posts a Financial Account Transaction to the General Ledger.
    /// Mirrors the logic of the Ofbiz service postFinAccountTransToGl.
    /// NOTE: This is not yet complete. The quickCreateAcctgTransAndEntries method is simulated.
    /// </summary>
    /// <param name="request">Contains finAccountTransId and glAccountId</param>
    /// <returns>A result indicating success or failure, possibly returning an AcctgTransId.</returns>
    public async Task<Result<string>> PostFinAccountTransToGl(PostFinAccountTransToGlRequest request)
    {
        try
        {
            var nowTimestamp = DateTime.UtcNow;

            // 1) Fetch FinAccountTrans
            var finAccountTrans = await _context.FinAccountTrans
                .FindAsync(request.FinAccountTransId);
            if (finAccountTrans == null)
                return Result<string>.Failure($"FinAccountTrans not found for ID {request.FinAccountTransId}.");

            // 2) Fetch FinAccount
            var finAccount = await _context.FinAccounts.FindAsync(finAccountTrans.FinAccountId);
            if (finAccount == null)
                return Result<string>.Failure("FinAccount not found.");

            // We'll track a local copy of glAccountId
            var finalGlAccountId = request.GlAccountId;

            // If finAccount.postToGlAccountId is not empty, use that
            if (!string.IsNullOrEmpty(finAccount.PostToGlAccountId))
            {
                finalGlAccountId = finAccount.PostToGlAccountId;
            }
            else
            {
                // Otherwise, attempt to fetch FinAccountTypeGlAccount for fallback
                var finAccountTypeGlAccount = await _context.FinAccountTypeGlAccounts.FirstOrDefaultAsync(g =>
                    g.OrganizationPartyId == finAccount.OrganizationPartyId &&
                    g.FinAccountTypeId == finAccount.FinAccountTypeId);

                if (finAccountTypeGlAccount != null)
                {
                    finalGlAccountId = finAccountTypeGlAccount.GlAccountId;
                }
            }

            // Build an object to pass to the 'quickCreateAcctgTransAndEntries' logic
            var acctgTransDto = new CreateQuickAcctgTransAndEntriesParams
            {
                FinAccountTransId = request.FinAccountTransId,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                GlFiscalTypeId = "ACTUAL",
                PartyId = finAccountTrans.PartyId, // if PartyId is a column in FinAccountTrans
                IsPosted = "N",
                OrganizationPartyId = finAccount.OrganizationPartyId,
                Amount = finAccountTrans.Amount ?? 0m,
                AcctgTransEntryTypeId = "_NA_"
            };

            // Distinguish based on FinAccountTransType
            if (finAccountTrans.FinAccountTransTypeId == "DEPOSIT")
            {
                // credit = request.GlAccountId, debit = finalGlAccountId
                acctgTransDto.CreditGlAccountId = request.GlAccountId;
                acctgTransDto.DebitGlAccountId = finalGlAccountId;
                acctgTransDto.AcctgTransTypeId = "RECEIPT";
            }
            else if (finAccountTrans.FinAccountTransTypeId == "WITHDRAWAL")
            {
                // debit = request.GlAccountId, credit = finalGlAccountId
                acctgTransDto.DebitGlAccountId = request.GlAccountId;
                acctgTransDto.CreditGlAccountId = finalGlAccountId;
                acctgTransDto.AcctgTransTypeId = "PAYMENT_ACCTG_TRANS";
            }
            else if (finAccountTrans.FinAccountTransTypeId == "ADJUSTMENT")
            {
                // If amount < 0 => (like outgoing), else => incoming
                var amt = finAccountTrans.Amount ?? 0m;
                if (amt < 0)
                {
                    // multiply by -1
                    acctgTransDto.Amount = Math.Abs(amt);
                    acctgTransDto.DebitGlAccountId = request.GlAccountId;
                    acctgTransDto.CreditGlAccountId = finalGlAccountId;
                    acctgTransDto.AcctgTransTypeId = "OUTGOING_PAYMENT";
                }
                else
                {
                    acctgTransDto.CreditGlAccountId = request.GlAccountId;
                    acctgTransDto.DebitGlAccountId = finalGlAccountId;
                    acctgTransDto.AcctgTransTypeId = "INCOMING_PAYMENT";
                }
            }

            // Validate required fields: debitGlAccountId, creditGlAccountId, organizationPartyId
            if (string.IsNullOrEmpty(acctgTransDto.DebitGlAccountId))
            {
                return Result<string>.Failure("Cannot post transaction: missing Debit GL Account.");
            }

            if (string.IsNullOrEmpty(acctgTransDto.CreditGlAccountId))
            {
                return Result<string>.Failure("Cannot post transaction: missing Credit GL Account.");
            }

            if (string.IsNullOrEmpty(acctgTransDto.OrganizationPartyId))
            {
                return Result<string>.Failure("Cannot post transaction: missing OrganizationPartyId.");
            }

            // 3) Call the (simulated) method that creates the AcctgTrans + entries
            var acctgTransId = await QuickCreateAcctgTransAndEntries(acctgTransDto);

            return Result<string>.Success(acctgTransId);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error posting FinAccountTrans to GL: {ex.Message}");
        }
    }

    public async Task<string> CreateAcctgTransForCertificateIssuance(string workEffortId, string inventoryItemId,
        string certificateItemId, decimal issuedQuantity)
    {
        try
        {
            // Fetch certificate header
            var workEffort = await _context.WorkEfforts
                .FirstOrDefaultAsync(w =>
                    w.WorkEffortId == workEffortId && w.WorkEffortTypeId == "PROJECT_CERTIFICATE");
            if (workEffort == null)
                throw new Exception($"Certificate WorkEffort with ID {workEffortId} not found.");

            // get projectId from workEffort
            var projectId = workEffort.ProjectId;

            // get project work effort
            var projectWorkEffort = await _context.WorkEfforts
                .FirstOrDefaultAsync(w =>
                    w.WorkEffortId == projectId && w.WorkEffortTypeId == "PROJECT");

            // Fetch InventoryItem
            var inventoryItem = await _context.InventoryItems
                .FindAsync(inventoryItemId);
            if (inventoryItem == null)
                throw new Exception($"InventoryItem with ID {inventoryItemId} not found.");

            // Fetch the exact certificate item the caller is issuing against.
            // Identified by its own WorkEffortId — matching on ProductId + QuantityUomId cannot
            // distinguish two lines that share both (common when several lines use the generic
            // "misc" product), which made FirstOrDefault return the same line for every issuance.
            var certificateItem = await _context.WorkEfforts
                .FirstOrDefaultAsync(w => w.WorkEffortId == certificateItemId
                                          && w.WorkEffortParentId == workEffortId
                                          && w.WorkEffortTypeId == "CERTIFICATE_ITEM");
            if (certificateItem == null)
                throw new Exception(
                    $"Certificate item {certificateItemId} not found under WorkEffortId {workEffortId}.");

            if (certificateItem.Rate == null)
                throw new Exception($"Rate missing for certificate item {certificateItemId}.");

            // Calculate amount (issued quantity * certificate line rate)
            // The caller issues a line's quantity across as many InventoryItems as it takes, calling
            // this method once per deduction. Cost the quantity actually deducted — using the whole
            // line total here posted the full line once per deduction.
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var origAmount = _acctgMiscService.CustomRound(
                issuedQuantity * (decimal)certificateItem.Rate,
                (int)glSettings.DecimalScale,
                glSettings.RoundingMode);

            var stamp = DateTime.UtcNow;
            var newAcctgTransSequence = await _utilityService.GetNextSequence("AcctgTrans");

            // Create Debit entry for PROJECTS_UNDER_DEVELOPMENT
            // REFACTOR: Debits PROJECTS_UNDER_DEVELOPMENT (143000) instead of WIP_INVENTORY;
            // reflects project-specific asset account for certificate issuance.
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "1",
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "D",
                GlAccountId = projectWorkEffort.GlAccountId,
                Description = certificateItem.Description,
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = certificateItem.ProductId, // From certificate item
                PartyId = workEffort.PartyIdContractor,
                OrigAmount = (decimal?)origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                CurrencyUomId = inventoryItem.CurrencyUomId
            };

            // Create Credit entry for INVENTORY_ACCOUNT
            // REFACTOR: Credits INVENTORY_ACCOUNT (140000) as requested;
            // aligns with inventory reduction for certificate issuance.
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = newAcctgTransSequence,
                AcctgTransEntrySeqId = "2",
                AcctgTransEntryTypeId = "_NA_",
                ReconcileStatusId = "AES_NOT_RECONCILED",
                DebitCreditFlag = "C",
                Description = certificateItem.Description,
                GlAccountTypeId = "INVENTORY_ACCOUNT", // GL account 140000
                PartyId = workEffort.PartyIdContractor,
                OrganizationPartyId = inventoryItem.OwnerPartyId,
                ProductId = inventoryItem.ProductId,
                OrigAmount = (decimal?)origAmount,
                OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                CurrencyUomId = inventoryItem.CurrencyUomId
            };

            // Prepare accounting transaction
            var acctgTransEntries = new List<AcctgTransEntry> { debitEntry, creditEntry };
            var acctgTransInMap = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "INVENTORY",
                Description = workEffort.Description,
                WorkEffortId = workEffortId,
                TransactionDate = DateHelper.ToDateOnly(certificateItem.ProcurementDate),
                AcctgTransEntries = acctgTransEntries,
            };

            // Create transaction and entries
            var inventoryTransId = await CreateAcctgTransAndEntries(acctgTransInMap);

            // ---------------------------------------------------------------------
            // 2. Second transaction: Transportation Expenses + Gratuities → Cash payment
            // ---------------------------------------------------------------------
            // REFACTOR: Extract and sum TransportationExpenses + Gratuities if they exist
            decimal additionalExpenses = 0m;
            if (certificateItem.TransportationExpenses.HasValue)
                additionalExpenses += certificateItem.TransportationExpenses.Value;
            if (certificateItem.Gratuities.HasValue)
                additionalExpenses += certificateItem.Gratuities.Value;

            // These are line-level totals, but this method runs once per inventory deduction, so
            // prorate them over the quantity being issued — otherwise a line split across several
            // InventoryItems charges its full transport/gratuities once per split.
            var lineQuantity = certificateItem.Quantity ?? 0m;
            if (additionalExpenses > 0 && lineQuantity > 0 && issuedQuantity != lineQuantity)
            {
                additionalExpenses = _acctgMiscService.CustomRound(
                    additionalExpenses * (issuedQuantity / lineQuantity),
                    (int)glSettings.DecimalScale,
                    glSettings.RoundingMode);
            }

            string cashTransId = null;

            if (additionalExpenses > 0)
            {
                var cashTransSeq = await _utilityService.GetNextSequence("AcctgTrans");

                var debitCashEntry = new AcctgTransEntry
                {
                    AcctgTransId = cashTransSeq,
                    AcctgTransEntrySeqId = "1",
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "D",
                    // Same project asset account as the inventory debit above — was hardcoded to
                    // "124420", which charged every project's transport/gratuities to one project.
                    GlAccountId = projectWorkEffort.GlAccountId,
                    OrganizationPartyId = inventoryItem.OwnerPartyId,
                    ProductId = certificateItem.ProductId,
                    OrigAmount = additionalExpenses,
                    OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                    CurrencyUomId = inventoryItem.CurrencyUomId
                };

                var creditCashEntry = new AcctgTransEntry
                {
                    AcctgTransId = cashTransSeq,
                    AcctgTransEntrySeqId = "2",
                    AcctgTransEntryTypeId = "_NA_",
                    ReconcileStatusId = "AES_NOT_RECONCILED",
                    DebitCreditFlag = "C",
                    GlAccountId = "111010", // 111010
                    OrganizationPartyId = inventoryItem.OwnerPartyId,
                    ProductId = certificateItem.ProductId,
                    OrigAmount = additionalExpenses,
                    OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                    CurrencyUomId = inventoryItem.CurrencyUomId
                };

                // REFACTOR: Use a more descriptive transaction type for expense-related cash outflow
                var cashTransParams = new CreateAcctgTransAndEntriesParams
                {
                    GlFiscalTypeId = "ACTUAL",
                    AcctgTransTypeId = "PROJECT_EXPENSE", // or "PROJECT_EXPENSE" if you have that type
                    WorkEffortId = workEffortId,
                    TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    AcctgTransEntries = new List<AcctgTransEntry> { debitCashEntry, creditCashEntry }
                };

                cashTransId = await CreateAcctgTransAndEntries(cashTransParams);
            }

            _logger.LogInformation(
                "Created accounting transactions for certificate issuance WorkEffortId {WorkEffortId}: " +
                "Inventory transaction {InventoryTransId}, " +
                "Cash/Expenses transaction {CashTransId} (amount {AdditionalExpenses})",
                workEffortId, inventoryTransId, cashTransId ?? "N/A", additionalExpenses);

            return inventoryTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Error creating accounting transaction for certificate WorkEffortId {workEffortId}, InventoryItemId {inventoryItemId}.");
            throw;
        }
    }


    public async Task<string> CreateAcctgTransForPhysicalInventoryVariance(
        CreateAcctgTransForPhysicalInventoryVarianceDto dto)
    {
        try
        {
            // 1) Retrieve ledger rounding properties 
            var glSettings = _acctgMiscService.GetGlArithmeticSettingsInline();
            var ledgerDecimals = glSettings.DecimalScale;
            var roundingMode = glSettings.RoundingMode;

            // 2) Find InventoryItemDetails for physicalInventoryId 
            //    => In Ofbiz: <entity-and entity-name="InventoryItemDetail" list="inventoryItemDetails">
            var inventoryItemDetails = await _context.InventoryItemDetails
                .Where(detail => detail.PhysicalInventoryId == dto.PhysicalInventoryId)
                .OrderBy(detail => detail.InventoryItemId)
                .ToListAsync();

            // We'll store credit/debit entries in a list for final call
            var acctgTransEntries = new List<AcctgTransEntry>();

            // 3) Iterate over inventoryItemDetails 
            foreach (var itemDetail in inventoryItemDetails)
            {
                // <get-related-one value-field="inventoryItemDetail" relation-name="InventoryItem" to-value-field="inventoryItem"/>
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.InventoryItemId == itemDetail.InventoryItemId);

                if (inventoryItem == null)
                {
                    // If missing item, skip or handle error
                    continue;
                }

                // 3a) calculate origAmount = quantityOnHandDiff * inventoryItem.unitCost 
                //     with decimal-scale ledgerDecimals, roundingMode
                var rawAmount = (itemDetail.QuantityOnHandDiff ?? 0m) * (inventoryItem.UnitCost ?? 0m);
                var origAmount = _acctgMiscService.CustomRound(rawAmount, (int)ledgerDecimals, roundingMode);

                // 3b) Create Credit entry
                var creditEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = "01",
                    AcctgTransEntryTypeId = "_NA_",
                    DebitCreditFlag = "C",
                    GlAccountTypeId = itemDetail.ReasonEnumId, // from itemDetail
                    ProductId = inventoryItem.ProductId,
                    OrigAmount = origAmount,
                    OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                    OrganizationPartyId = inventoryItem.OwnerPartyId
                };

                // 3c) Create Debit entry
                var debitEntry = new AcctgTransEntry
                {
                    AcctgTransEntrySeqId = "02",
                    AcctgTransEntryTypeId = "_NA_",
                    DebitCreditFlag = "D",
                    GlAccountTypeId = "INVENTORY_ACCOUNT",
                    ProductId = inventoryItem.ProductId,
                    OrigAmount = origAmount,
                    OrigCurrencyUomId = inventoryItem.CurrencyUomId,
                    OrganizationPartyId = inventoryItem.OwnerPartyId
                };

                acctgTransEntries.Add(creditEntry);
                acctgTransEntries.Add(debitEntry);
            }

            // 4) Prepare map for createAcctgTransAndEntries
            //    In Ofbiz: "createAcctgTransAndEntriesInMap" 
            //    We'll define a C# object for the sub-service
            var subServiceDto = new CreateAcctgTransAndEntriesParams
            {
                GlFiscalTypeId = "ACTUAL",
                AcctgTransTypeId = "ITEM_VARIANCE",
                PhysicalInventoryId = dto.PhysicalInventoryId,
                AcctgTransEntries = acctgTransEntries
            };

            // 5) Call the sub-service: createAcctgTransAndEntries
            //    We'll define a method for that:
            var acctgTransId = await CreateAcctgTransAndEntries(subServiceDto);


            // Return acctgTransId as result
            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in createAcctgTransForPhysicalInventoryVariance");
            throw new ApplicationException($"Failed to create AcctgTrans for Physical Inventory Variance. {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Completes the AcctgTransEntries for the given AcctgTrans by ensuring each entry has its GL Account ID,
    /// setting original amounts, and clearing invalid GlAccountTypeId if needed.
    /// </summary>
    /// <param name="acctgTransId">The ID of the accounting transaction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task CompleteAcctgTransEntries(string acctgTransId)
    {
        // 1. Load the AcctgTrans entity (mirroring entity-one)
        var acctgTrans = await _context.AcctgTrans.FindAsync(acctgTransId);
        if (acctgTrans == null)
        {
            throw new Exception("AcctgTrans not found");
        }

        // 2. Check if transaction is already posted (mirrors if-compare on isPosted equals "Y")
        if (acctgTrans.IsPosted == "Y")
        {
            // In OFBiz this would add an error and halt execution.
            throw new ApplicationException("Accounting Transaction has already been posted");
        }

        // 3. Get related AcctgTransEntry records (mirrors get-related relation "AcctgTransEntry")
        var entries = await _context.AcctgTransEntries
            .Where(e => e.AcctgTransId == acctgTransId)
            .ToListAsync();

        // 4. Iterate over each AcctgTransEntry
        foreach (var entry in entries)
        {
            // If glAccountId is empty but glAccountTypeId exists, determine glAccountId from gl setup settings.
            if (string.IsNullOrEmpty(entry.GlAccountId) && !string.IsNullOrEmpty(entry.GlAccountTypeId))
            {
                // Build the parameter object (mirrors clear-field and set operations)
                var getGlAccountParams = new GetGlAccountFromAccountTypeParams
                {
                    OrganizationPartyId = entry.OrganizationPartyId,
                    AcctgTransTypeId = acctgTrans.AcctgTransTypeId,
                    GlAccountTypeId = entry.GlAccountTypeId,
                    DebitCreditFlag = entry.DebitCreditFlag,
                    ProductId = entry.ProductId,
                    PartyId = acctgTrans.PartyId,
                    RoleTypeId = acctgTrans.RoleTypeId,
                    InvoiceId = acctgTrans.InvoiceId,
                    PaymentId = acctgTrans.PaymentId
                };

                // Call the service to determine the correct glAccountId (mirrors call-service getGlAccountFromAccountType)
                entry.GlAccountId = await GetGlAccountFromAccountType(getGlAccountParams);
            }

            // If origAmount is empty, set it from the current amount (mirrors if-empty check and set operation)
            if (entry.OrigAmount == null || entry.OrigAmount == 0)
            {
                entry.OrigAmount = entry.Amount;
            }

            // Retrieve the GlAccountType entity (mirrors entity-one call with use-cache="true")
            if (!string.IsNullOrEmpty(entry.GlAccountTypeId))
            {
                var glAccountType = await _context.GlAccountTypes.FindAsync(entry.GlAccountTypeId);
                // If no GlAccountType exists, clear the glAccountTypeId (mirrors if-empty and clear-field)
                if (glAccountType == null)
                {
                    entry.GlAccountTypeId = null;
                }
            }

            // Mark the entry as modified (mirrors store-value)
            _context.AcctgTransEntries.Update(entry);
        }
    }

    // Method to copy an AcctgTrans and its entries, optionally reversing debit/credit flags
    public async Task<GeneralServiceResult<string>> CopyAcctgTransAndEntries(string fromAcctgTransId, bool revert)
    {
        try
        {
            // Query the AcctgTrans entity by fromAcctgTransId
            // Technical: Uses LINQ to retrieve the AcctgTrans record to be copied
            // Business Purpose: Identifies the accounting transaction to duplicate or reverse
            var acctgTrans = await _context.AcctgTrans
                .Where(at => at.AcctgTransId == fromAcctgTransId)
                .SingleOrDefaultAsync();

            // Check if AcctgTrans was found
            // Technical: Verifies acctgTrans is not null
            // Business Purpose: Prevents copying a non-existent transaction
            if (acctgTrans == null)
            {
                return GeneralServiceResult<string>.Error(
                    $"AccountingTransactionNotFound: acctgTransId={fromAcctgTransId}");
            }

            // Map AcctgTrans to CreateAcctgTransParams for cloning
            // Technical: Copies fields to CreateAcctgTransParams, clearing AcctgTransId
            // Business Purpose: Prepares a new transaction record for creation, preserving original data
            var createParams = new CreateAcctgTransParams
            {
                AcctgTransTypeId = acctgTrans.AcctgTransTypeId,
                GlFiscalTypeId = acctgTrans.GlFiscalTypeId,
                IsPosted = acctgTrans.IsPosted,
                InvoiceId = acctgTrans.InvoiceId,
                PaymentId = acctgTrans.PaymentId,
                PartyId = acctgTrans.PartyId,
                RoleTypeId = acctgTrans.RoleTypeId,
                Description = acctgTrans.Description,
                TransactionDate = DateOnly.FromDateTime(DateTime.UtcNow), // Set to current timestamp
                PostedDate = acctgTrans.PostedDate,
                ScheduledPostingDate = acctgTrans.ScheduledPostingDate,
                GlJournalId = acctgTrans.GlJournalId,
                VoucherRef = acctgTrans.VoucherRef,
                VoucherDate = acctgTrans.VoucherDate,
                GroupStatusId = acctgTrans.GroupStatusId,
                FixedAssetId = acctgTrans.FixedAssetId,
                InventoryItemId = acctgTrans.InventoryItemId,
                PhysicalInventoryId = acctgTrans.PhysicalInventoryId,
                FinAccountTransId = acctgTrans.FinAccountTransId,
                ReceiptId = acctgTrans.ReceiptId,
                TheirAcctgTransId = acctgTrans.TheirAcctgTransId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedTxStamp = DateTime.UtcNow,
                CreatedTxStamp = DateTime.UtcNow
            };

            // Create the new AcctgTrans
            // Technical: Calls CreateAcctgTrans to persist the new transaction
            // Business Purpose: Records the duplicated or reversed transaction in the ledger
            string newAcctgTransId = await _acctgTransService.CreateAcctgTrans(createParams);
            if (string.IsNullOrEmpty(newAcctgTransId))
            {
                return GeneralServiceResult<string>.Error("Failed to create new accounting transaction.");
            }

            // Query related AcctgTransEntry records
            // Technical: Uses LINQ to retrieve all entries for the original transaction
            // Business Purpose: Identifies all accounting entries to duplicate or reverse
            List<AcctgTransEntry> acctgTransEntries = await _context.AcctgTransEntries
                .Where(ate => ate.AcctgTransId == fromAcctgTransId)
                .ToListAsync();

            // Process each AcctgTransEntry
            // Technical: Iterates over entries to clone and adjust as needed
            // Business Purpose: Ensures all ledger entries are copied or reversed correctly
            foreach (var acctgTransEntry in acctgTransEntries)
            {
                // Clone AcctgTransEntry
                // Technical: Copies fields to a new AcctgTransEntry object
                // Business Purpose: Prepares a new entry for creation
                var newAcctgTransEntry = new AcctgTransEntry
                {
                    AcctgTransId = newAcctgTransId,
                    AcctgTransEntrySeqId =
                        acctgTransEntry.AcctgTransEntrySeqId, // Will be reset by CreateAcctgTransEntry
                    DebitCreditFlag = acctgTransEntry.DebitCreditFlag,
                    Amount = acctgTransEntry.Amount,
                    GlAccountId = acctgTransEntry.GlAccountId
                };

                // Adjust debitCreditFlag if revert is true
                // Technical: Swaps D to C or C to D if revert is true
                // Business Purpose: Reverses the accounting entry to negate the original effect
                if (revert)
                {
                    newAcctgTransEntry.DebitCreditFlag = newAcctgTransEntry.DebitCreditFlag == "D" ? "C" : "D";
                }

                // Create the new AcctgTransEntry
                // Technical: Calls CreateAcctgTransEntry to persist the entry
                // Business Purpose: Records the duplicated or reversed entry in the ledger
                var entryResult = await _acctgTransService.CreateAcctgTransEntry(newAcctgTransEntry);
            }

            // Return success with the new acctgTransId
            // Technical: Returns the new transaction ID as ResultData
            // Business Purpose: Confirms the transaction and its entries were copied or reversed
            return GeneralServiceResult<string>.Success();
        }
        catch (Exception ex)
        {
            // Log and return error
            // Technical: Logs exception details
            // Business Purpose: Tracks errors for auditing and troubleshooting
            _logger.LogError(ex, "Error in CopyAcctgTransAndEntries for fromAcctgTransId: {FromAcctgTransId}",
                fromAcctgTransId);
            return GeneralServiceResult<string>.Error(
                "An unexpected error occurred while copying the accounting transaction.");
        }
    }

    private async Task<string> GetCustomerReceivableGlAccountId(
        string organizationPartyId,
        string customerPartyId,
        CancellationToken cancellationToken = default)
    {
        var partyGlAccount = await _context.PartyGlAccounts
            .Where(pga =>
                pga.OrganizationPartyId == organizationPartyId &&
                pga.PartyId == customerPartyId &&
                pga.RoleTypeId == "BILL_TO_CUSTOMER" &&
                pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
            .Select(pga => pga.GlAccountId)
            .FirstOrDefaultAsync(cancellationToken);

        return partyGlAccount ?? "121100";
    }

    public async Task<string> CreateAccountingTransactionForApartmentIncomingPayment(string paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.SalesRequest)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                throw new Exception($"Payment not found: {paymentId}");

            if (payment.SalesRequest == null)
                throw new Exception($"SalesRequest not found for payment: {paymentId}");

            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
            var now = DateTime.UtcNow;
            var transactionDate = payment.EffectiveDate;

            string bankOrCashGlAccountId = payment.PaymentMethod?.GlAccountId
                                           ?? throw new Exception("GL account not configured on payment method");

            // ────────────────────────────────────────────────
            // Determine payment type & related strings
            // ────────────────────────────────────────────────
            string paymentType;
            string paymentTypeDescription;
            string chequeOrTransferRef = string.Empty;

            if (payment.IsBankTransfer == true)
            {
                paymentType = "BankTransfer";
                paymentTypeDescription = "تحويل بنكي";
            }
            else if (!string.IsNullOrEmpty(payment.ChequeNumber))
            {
                paymentType = "Cheque";
                paymentTypeDescription = "شيك";
                chequeOrTransferRef = $"#{payment.ChequeNumber}";
            }
            else
            {
                paymentType = "Cash";
                paymentTypeDescription = "نقدي";
            }

            // Determine credit GL account
            // Keyed off whether this payment actually has an outstanding 124410 reclass posted
            // by RecordSalesRequestCheques.cs - NOT off payment.ChequeNumber being non-empty.
            // ChequeNumber can be set directly on the plain Payment edit form (EditPaymentForm),
            // bypassing "Record Received Cheques" entirely; in that case no money ever moved into
            // 124410, so crediting it here would credit an account nothing ever debited, while
            // leaving the customer's receivable balance wrongly untouched. Checking the actual
            // net-outstanding 124410 balance for this PaymentId is the only reliable signal.
            var outstandingChequeReclassAmount = await _context.AcctgTransEntries
                .Where(e => e.AcctgTrans.PaymentId == payment.PaymentId &&
                            e.AcctgTrans.AcctgTransTypeId == "APARTMENT_SALE_CHEQUE" &&
                            e.GlAccountId == "124410")
                .SumAsync(e => e.DebitCreditFlag == "D" ? e.Amount : -e.Amount);

            string creditGlAccountId;

            if (outstandingChequeReclassAmount > 0)
            {
                // Collecting a cheque that was already reclassified to Cheques Under Collection
                creditGlAccountId = "124410";
            }
            else
            {
                // Direct receipt (cash/bank transfer) → reduce the customer's receivable directly
                creditGlAccountId = await GetCustomerReceivableGlAccountId(
                    organizationPartyId: companyPartyId,
                    customerPartyId: payment.PartyIdFrom,
                    cancellationToken: default);
            }

            // Main transaction description
            var description = $"تحصيل دفعة شقة - {paymentTypeDescription} {chequeOrTransferRef} - " +
                              $"دفعة {payment.PaymentId} - طلب مبيعات {payment.SalesRequestId}";

            var acctgTransParams = new CreateAcctgTransParams
            {
                AcctgTransTypeId = "INCOMING_PAYMENT",
                TransactionDate = transactionDate,
                IsPosted = "Y",
                Description = description,
                GlFiscalTypeId = "ACTUAL",
                PaymentId = payment.PaymentId,
                SalesRequestId = payment.SalesRequestId,
                PartyId = payment.PartyIdFrom
            };

            string acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

            int seq = 0;

            // ────────────────────────────────────────────────
            // Debit entry (incoming money → bank/cash)
            // ────────────────────────────────────────────────
            string debitDescription = paymentType switch
            {
                "BankTransfer" => $"تحويل بنكي مستلم من العميل {chequeOrTransferRef}",
                "Cheque" => $"إيداع بنكي - تحصيل شيك {chequeOrTransferRef}",
                _ => "دفعة نقدية مستلمة من العميل"
            };

            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = bankOrCashGlAccountId,
                DebitCreditFlag = "D",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                ProductId = payment.SalesRequest.ProductId,
                PartyId = payment.PartyIdFrom,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };
            await _acctgTransService.CreateAcctgTransEntry(debitEntry);

            // ────────────────────────────────────────────────
            // Credit entry (reducing receivable or cheques under collection)
            // ────────────────────────────────────────────────
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = creditGlAccountId,
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                ProductId = payment.SalesRequest.ProductId,
                PartyId = payment.PartyIdFrom,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };
            await _acctgTransService.CreateAcctgTransEntry(creditEntry);

            _logger.LogInformation(
                "Accounting transaction {AcctgTransId} created for apartment incoming {PaymentType} payment {PaymentId}. " +
                "Debit: {DebitDesc} | Credit GL {CreditGlAccountId} (ChequeNumber: {ChequeNumber})",
                acctgTransId, paymentTypeDescription, paymentId,
                debitDescription, creditGlAccountId, payment.ChequeNumber);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create accounting transaction for apartment incoming payment {PaymentId}",
                paymentId);
            throw;
        }
    }

    public async Task<string> CreateAccountingTransactionForMaintenanceDepositPayment(string paymentId)
    {
        try
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .Include(p => p.SalesRequest)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == payment.SalesRequest.ProductId);


            if (payment == null)
                throw new Exception($"Payment not found: {paymentId}");

            if (payment.SalesRequest == null)
                throw new Exception($"SalesRequest not found for payment: {paymentId}");

            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();
            var now = DateTime.UtcNow;
            var transactionDate = payment.EffectiveDate;

            string bankOrCashGlAccountId = payment.PaymentMethod?.GlAccountId
                                           ?? throw new Exception("GL account not configured on payment method");

            var receivableGlAccountId = await GetPartyReceivableAccount(
                companyPartyId,
                payment.SalesRequest.FromPartyId!);


            // ────────────────────────────────────────────────
            // Determine payment type for description
            // ────────────────────────────────────────────────
            string paymentTypeDescription;
            string chequeOrTransferRef = string.Empty;

            if (payment.IsBankTransfer == true)
            {
                paymentTypeDescription = "Bank Transfer";
            }
            else if (!string.IsNullOrEmpty(payment.ChequeNumber))
            {
                paymentTypeDescription = "Cheque";
                chequeOrTransferRef = $"#{payment.ChequeNumber}";
            }
            else
            {
                paymentTypeDescription = "Cash";
            }

            var description = $"Maintenance deposit received - {paymentTypeDescription} {chequeOrTransferRef} - " +
                              $"Payment {payment.PaymentId} - SR {payment.SalesRequestId}  - {product.ProductId}";

            var acctgTransParams = new CreateAcctgTransParams
            {
                AcctgTransTypeId = "APARTMENT_MAINTENANCE_DEPOSIT",
                TransactionDate = transactionDate,
                IsPosted = "Y",
                Description = description,
                GlFiscalTypeId = "ACTUAL",
                PaymentId = payment.PaymentId,
                SalesRequestId = payment.SalesRequestId,
                PartyId = payment.PartyIdFrom
            };

            string acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

            int seq = 0;

            // ────────────────────────────────────────────────
            // Debit entry → Bank / Cash (money coming in)
            // ────────────────────────────────────────────────
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = bankOrCashGlAccountId,
                DebitCreditFlag = "D",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                ProductId = payment.SalesRequest.ProductId,
                PartyId = payment.PartyIdFrom,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };
            await _acctgTransService.CreateAcctgTransEntry(debitEntry);

            // ────────────────────────────────────────────────
            // Credit entry → receivableGlAccountId (Maintenance Deposit liability)
            // ────────────────────────────────────────────────
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = receivableGlAccountId,
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                ProductId = payment.SalesRequest.ProductId,
                PartyId = payment.PartyIdFrom,
                CreatedStamp = now,
                LastUpdatedStamp = now
            };
            await _acctgTransService.CreateAcctgTransEntry(creditEntry);

            _logger.LogInformation(
                "Maintenance deposit accounting transaction {AcctgTransId} created for payment {PaymentId}. " +
                "Debit: {BankGl} | Credit: 250130",
                acctgTransId, paymentId, bankOrCashGlAccountId);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create maintenance deposit accounting transaction for payment {PaymentId}",
                paymentId);
            throw;
        }
    }

    public async Task<string> GetPartyReceivableAccount(
        string organizationPartyId,
        string customerPartyId,
        string? roleTypeId = "BILL_TO_CUSTOMER",
        string? defaultAccount = "121100")
    {
        var account = await _context.PartyGlAccounts
            .Where(pga => pga.OrganizationPartyId == organizationPartyId
                          && pga.PartyId == customerPartyId
                          && pga.RoleTypeId == roleTypeId
                          && pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
            .Select(pga => pga.GlAccountId)
            .FirstOrDefaultAsync();

        return account ?? defaultAccount ?? "121100";
    }

    public async Task<string> CreatePostdatedChequeAccountingTransaction(string paymentId)
    {
        try
        {
            // 1. Fetch the payment with necessary includes
            var payment = await _context.Payments
                .FindAsync(paymentId);

            if (payment == null)
                throw new Exception("Payment not found");

            // 2. Early validation – ensure it's an outgoing payment with cheque & override GL account
            if (string.IsNullOrEmpty(payment.OverrideGlAccountId))
                throw new InvalidOperationException("OverrideGlAccountId is required for postdated cheque accounting.");

            if (string.IsNullOrEmpty(payment.ChequeNumber) && payment.ChequeDate == null)
                throw new InvalidOperationException("This method is only for cheque payments.");

            // Assuming outgoing payments have PartyIdFrom = company (or adjust logic as per your model)
            var companyPartyId =
                await _productStoreService.GetProductStorePayToPartId(); // or however you get company party

            if (payment.PartyIdFrom != companyPartyId)
            {
                // Optional: log or throw if not outgoing – depends on your business rules
                _logger.LogWarning("Payment {PaymentId} is not an outgoing payment from company", paymentId);
            }

            var stamp = DateTime.UtcNow;

            var chequeInfo = !string.IsNullOrEmpty(payment.ChequeNumber)
                ? $"Cheque #{payment.ChequeNumber}"
                : $"dated {payment.ChequeDate?.ToString("yyyy-MM-dd")}";


            var description =
                $"Postdated cheque issuance - {chequeInfo} - " +
                $"- Payment {payment.PaymentId} - {payment.Amount:N2}";

            // 3. Create accounting transaction
            var acctgTransParams = new CreateAcctgTransParams
            {
                AcctgTransTypeId = "CHECK_ISSUED", // or "CHECK_ISSUED", adjust as per your system
                TransactionDate = payment.EffectiveDate,
                IsPosted = "Y",
                GlFiscalTypeId = "ACTUAL",
                Description = payment.Comments,
                PaymentId = payment.PaymentId,
                PartyId = payment.PartyIdTo // the recipient
            };

            var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

            var seq = 0;

            // Debit: Override GL Account (usually the expense/liability account you selected)
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
                GlAccountId = payment.OverrideGlAccountId,
                DebitCreditFlag = "D",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                PartyId = payment.PartyIdTo, // the recipient
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(debitEntry);

            // Credit: Postdated Checks Issued (250100)
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
                GlAccountId = "250100", // POSTDATED CHECKS ISSUED
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                PartyId = payment.PartyIdFrom, // usually the company itself
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(creditEntry);

            _logger.LogInformation(
                "Postdated cheque issuance accounting transaction {AcctgTransId} created for payment {PaymentId}",
                acctgTransId, paymentId);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create postdated cheque accounting transaction for payment {PaymentId}",
                paymentId);
            throw;
        }
    }

    /// <summary>
    /// Creates an accounting transaction when a post-dated cheque is issued (status → PMNT_SENT).
    /// 
    /// Accounting effect:
    ///   Debit  → OverrideGlAccountId     (usually expense or liability account chosen by user)
    ///   Credit → 250100                  (POSTDATED CHECKS ISSUED – شيكات صادرة مؤجلة)
    /// </summary>
    /// <param name="paymentId">The ID of the payment that was just marked as sent</param>
    /// <returns>The created AcctgTransId</returns>
    public async Task<string> CreatePostdatedChequeIssuedAccountingTransaction(string paymentId)
    {
        try
        {
            // 1. Load the payment with needed relations
            var payment = await _context.Payments
                .Include(p => p.PaymentMethod)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                throw new Exception($"Payment not found: {paymentId}");
            }

            // 2. Defensive validation (should already be checked before calling, but keep safety)
            if (string.IsNullOrEmpty(payment.OverrideGlAccountId))
            {
                throw new InvalidOperationException(
                    $"Cannot create post-dated cheque transaction: OverrideGlAccountId is missing (Payment {paymentId})");
            }

            if (string.IsNullOrEmpty(payment.ChequeNumber) && !payment.ChequeDate.HasValue)
            {
                throw new InvalidOperationException(
                    $"Cannot create post-dated cheque transaction: No cheque information (Payment {paymentId})");
            }

            if (payment.Amount <= 0)
            {
                throw new InvalidOperationException(
                    $"Cannot create post-dated cheque transaction: Invalid amount (Payment {paymentId})");
            }

            var companyPartyId = await _productStoreService.GetProductStorePayToPartId();

            var stamp = DateTime.UtcNow;

            // Build meaningful description
            var chequeInfo = !string.IsNullOrEmpty(payment.ChequeNumber)
                ? $"Cheque #{payment.ChequeNumber}"
                : $"dated {payment.ChequeDate?.ToString("yyyy-MM-dd")}";

            var description = $"Post-dated cheque issued - {chequeInfo} - " +
                              $"Payment {payment.PaymentId} - {payment.Amount:N2}";

            // 3. Create the accounting transaction header
            var acctgTransParams = new CreateAcctgTransParams
            {
                AcctgTransTypeId = "OUTGOING_PAYMENT", // ← use a specific type if your system has it
                TransactionDate = payment.EffectiveDate,
                IsPosted = "Y",
                Description = payment.Comments,
                GlFiscalTypeId = "ACTUAL",
                PaymentId = payment.PaymentId,
                PartyId = payment.PartyIdTo, // usually the supplier/vendor
            };

            var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);

            int seq = 0;

            // 4. Debit entry – the override GL account (expense / liability / cost center account)
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
                GlAccountId = "250100",
                DebitCreditFlag = "D",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                PartyId = payment.PartyIdTo, // supplier / payee
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(debitEntry);

            // get payment method
            var bankGlAccountId = payment.PaymentMethod?.GlAccountId
                                  ?? throw new Exception("Payment method GL account is not configured");

            // 5. Credit entry – Postdated Checks Issued (liability account)
            var creditEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString().PadLeft(3, '0'),
                GlAccountId = bankGlAccountId,
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = payment.Amount,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = payment.Comments,
                OrganizationPartyId = companyPartyId,
                PartyId = payment.PartyIdFrom, // usually your company
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(creditEntry);

            _logger.LogInformation(
                "Post-dated cheque issuance accounting transaction created: {AcctgTransId} for payment {PaymentId} " +
                "(debit {DebitGl} ← credit 250100)",
                acctgTransId, paymentId, payment.OverrideGlAccountId);

            return acctgTransId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create post-dated cheque issuance accounting transaction for payment {PaymentId}",
                paymentId);

            throw; // or return Result<string>.Failure(...) depending on your error handling pattern
        }
    }

    public async Task<string> CreateAccountingTransactionForEmployeeAdvance(string paymentId)
    {
        var payment = await _context.Payments
            .Include(p => p.PaymentMethod)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

        if (payment == null)
            throw new Exception($"Payment {paymentId} not found");

        if (payment.PartyIdTo == null)
            throw new Exception("Employee (PartyTo) not found on payment");

        var now = DateTime.UtcNow;
        var effectiveDate = payment.EffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        string employeePartyId = payment.PartyIdTo;
        string companyPartyId = payment.PartyIdFrom;

        // ───────────────────────────────────────────────
        // Get the employee-specific Accrued Expenses GL (liability)
        // ───────────────────────────────────────────────
        var employeeAccruedGl = await _context.PartyGlAccounts
            .Where(pga =>
                pga.OrganizationPartyId == companyPartyId &&
                pga.PartyId == employeePartyId &&
                pga.RoleTypeId == "EMPLOYEE" &&
                pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
            .Select(pga => pga.GlAccountId)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(employeeAccruedGl))
        {
            throw new Exception(
                $"No employee-specific accrued expenses GL account found for employee {employeePartyId}");
        }

        // Usually the bank/cash GL comes from payment method
        string debitGlAccountId = payment.PaymentMethod?.GlAccountId
                                  ?? throw new Exception("No GL account configured on payment method");

        string descriptionSuffix = payment.PaymentType?.Description ?? payment.PaymentTypeId;
        var transDescription = payment.Comments;

        var transParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId = "DISBURSEMENT", // or "EMPLOYEE_ADVANCE" if you have custom type
            TransactionDate = effectiveDate,
            IsPosted = "Y",
            Description = transDescription,
            GlFiscalTypeId = "ACTUAL",
            PaymentId = paymentId,
            PartyId = employeePartyId
        };

        string acctgTransId = await _acctgTransService.CreateAcctgTrans(transParams);

        int seq = 0;

        // ───────────────────────────────────────────────
        // Debit: Accrued salaries / Employee payable (liability decreases)
        // ───────────────────────────────────────────────
        var debitEntry = new AcctgTransEntry
        {
            AcctgTransId = acctgTransId,
            AcctgTransEntrySeqId = (++seq).ToString("D3"),
            GlAccountId = employeeAccruedGl,
            DebitCreditFlag = "D",
            Amount = payment.Amount,
            Description = transDescription,
            OrganizationPartyId = companyPartyId,
            PartyId = employeePartyId,
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = now,
            LastUpdatedStamp = now
        };
        await _acctgTransService.CreateAcctgTransEntry(debitEntry);

        // ───────────────────────────────────────────────
        // Credit: Bank / Cash (asset decreases)
        // ───────────────────────────────────────────────
        var creditEntry = new AcctgTransEntry
        {
            AcctgTransId = acctgTransId,
            AcctgTransEntrySeqId = (++seq).ToString("D3"),
            GlAccountId = debitGlAccountId, // bank or cash
            DebitCreditFlag = "C",
            Amount = payment.Amount,
            Description = transDescription,
            OrganizationPartyId = companyPartyId,
            PartyId = employeePartyId,
            ReconcileStatusId = "AES_NOT_RECONCILED",
            CreatedStamp = now,
            LastUpdatedStamp = now
        };
        await _acctgTransService.CreateAcctgTransEntry(creditEntry);

        _logger.LogInformation(
            "Created employee advance accounting trans {TransId} for payment {PaymentId} \u2192 Debit GL {AccruedGl} / Credit GL {BankGl}",
            acctgTransId, paymentId, employeeAccruedGl, debitGlAccountId);

        return acctgTransId;
    }

    public async Task<string> CreateAcctgTransForPayrollInvoice(string invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.InvoiceItems)
            .ThenInclude(ii => ii.InvoiceItemType)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        if (invoice == null)
            throw new Exception($"Invoice {invoiceId} not found");

        var employeePartyId = invoice.PartyIdFrom;
        var organizationPartyId = invoice.PartyId;

        // Categorize invoice items
        // Additions: Basic salary, overtime, bonuses, etc. (IsPositiveAmount == true)
        // Expense-Reducing Deductions: Absence, Penalties (PAYROL_DD_ABSENCE, PAYROL_DD_MISC, etc. and IsPositiveAmount == false)
        // Loan Deductions: PAYROL_DD_ADVANCE (IsPositiveAmount == false)

        var additions = invoice.InvoiceItems
            .Where(ii => ii.InvoiceItemType != null && ii.InvoiceItemType.IsPositiveAmount == true)
            .Sum(ii => (ii.Quantity ?? 1m) * (ii.Amount ?? 0m));

        var expenseReducingDeductions = invoice.InvoiceItems
            .Where(ii => ii.InvoiceItemType != null && ii.InvoiceItemType.IsPositiveAmount == false &&
                         (ii.InvoiceItemTypeId == "PAYROL_DD_ABSENCE" || ii.InvoiceItemTypeId == "PAYROL_DD_MISC"))
            .Sum(ii => (ii.Quantity ?? 1m) * (ii.Amount ?? 0m));

        var loanDeductions = invoice.InvoiceItems
            .Where(ii => ii.InvoiceItemType != null && ii.InvoiceItemType.IsPositiveAmount == false &&
                         ii.InvoiceItemTypeId == "PAYROL_DD_ADVANCE")
            .Sum(ii => (ii.Quantity ?? 1m) * (ii.Amount ?? 0m));

        // Note: Amount in InvoiceItems for deductions is typically stored as positive, but IsPositiveAmount flag is false.
        // We ensure we treat them as absolute values for calculation.

        var debitAmount = additions - expenseReducingDeductions;
        var netSalary = debitAmount - loanDeductions;

        var employee = await _context.Parties.FirstOrDefaultAsync(p => p.PartyId == employeePartyId);
        if (employee == null)
            throw new Exception($"Employee {employeePartyId} not found");

        // Get employee's Loan account (ACCOUNTS_RECEIVABLE)
        var loanGlAccountId = await _context.PartyGlAccounts
            .Where(pga => pga.PartyId == employeePartyId &&
                          pga.OrganizationPartyId == organizationPartyId &&
                          pga.RoleTypeId == "EMPLOYEE" &&
                          pga.GlAccountTypeId == "ACCOUNTS_RECEIVABLE")
            .Select(pga => pga.GlAccountId)
            .FirstOrDefaultAsync();

        // Get employee's Accrued account (ACCOUNTS_PAYABLE)
        var accruedGlAccountId = await _context.PartyGlAccounts
            .Where(pga => pga.PartyId == employeePartyId &&
                          pga.OrganizationPartyId == organizationPartyId &&
                          pga.RoleTypeId == "EMPLOYEE" &&
                          pga.GlAccountTypeId == "ACCOUNTS_PAYABLE")
            .Select(pga => pga.GlAccountId)
            .FirstOrDefaultAsync();

        // We need to check if both accounts are available for the employee and raise an exception if either is not available
        if (string.IsNullOrEmpty(loanGlAccountId))
            throw new Exception($"Loan account (ACCOUNTS_RECEIVABLE) not found for employee {employeePartyId}");

        if (string.IsNullOrEmpty(accruedGlAccountId))
            throw new Exception($"Accrued account (ACCOUNTS_PAYABLE) not found for employee {employeePartyId}");

        // GlAccountIdAdvancedPayment - if available - is to be used in the debit side in place of glAccountId '601000'
        var debitGlAccountId = employee.GlAccountIdAdvancedPayment ?? "601000";

        var acctgTransParams = new CreateAcctgTransParams
        {
            AcctgTransTypeId = "PAYROL_INVOICE",
            TransactionDate = invoice.InvoiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            IsPosted = "Y",
            Description = $"Payroll Invoice - {invoice.InvoiceId}",
            GlFiscalTypeId = "ACTUAL",
            InvoiceId = invoice.InvoiceId,
            PartyId = employeePartyId
        };

        var acctgTransId = await _acctgTransService.CreateAcctgTrans(acctgTransParams);
        var stamp = DateTime.UtcNow;
        var seq = 0;

        // 1. Salaries and Wages (601000 or GlAccountIdAdvancedPayment)
        // This is actual cost for the company: Additions - (Absence + Penalties).
        // When deductions exceed additions, debitAmount is negative, so this leg
        // posts as a credit (reducing expense) instead of being dropped, keeping
        // the transaction balanced against entry 3 below.
        if (debitAmount != 0)
        {
            var debitEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = debitGlAccountId,
                DebitCreditFlag = debitAmount > 0 ? "D" : "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = Math.Abs(debitAmount),
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = $"Salaries and Wages - Invoice {invoice.InvoiceId}",
                OrganizationPartyId = organizationPartyId,
                PartyId = employeePartyId,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(debitEntry);
        }

        // 2. Credit: Loan/Advance (if exists)
        if (loanDeductions > 0 && !string.IsNullOrEmpty(loanGlAccountId))
        {
            var advanceEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = loanGlAccountId,
                DebitCreditFlag = "C",
                AcctgTransEntryTypeId = "_NA_",
                Amount = loanDeductions,
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = $"Deduction for Advance - Invoice {invoice.InvoiceId}",
                OrganizationPartyId = organizationPartyId,
                PartyId = employeePartyId,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(advanceEntry);
        }

        // 3. Net Salary (Accrued Salaries).
        // When loan/expense deductions exceed the salary additions, netSalary is
        // negative (the employee owes the company rather than being owed), so this
        // leg posts as a debit instead of being dropped, keeping the transaction
        // balanced against entry 1 above.
        if (netSalary != 0 && !string.IsNullOrEmpty(accruedGlAccountId))
        {
            var accruedEntry = new AcctgTransEntry
            {
                AcctgTransId = acctgTransId,
                AcctgTransEntrySeqId = (++seq).ToString("D3"),
                GlAccountId = accruedGlAccountId,
                DebitCreditFlag = netSalary > 0 ? "C" : "D",
                AcctgTransEntryTypeId = "_NA_",
                Amount = Math.Abs(netSalary),
                ReconcileStatusId = "AES_NOT_RECONCILED",
                Description = netSalary > 0
                    ? $"Net Salary Payable - Invoice {invoice.InvoiceId}"
                    : $"Net Amount Due from Employee - Invoice {invoice.InvoiceId}",
                OrganizationPartyId = organizationPartyId,
                PartyId = employeePartyId,
                CreatedStamp = stamp,
                LastUpdatedStamp = stamp
            };
            await _acctgTransService.CreateAcctgTransEntry(accruedEntry);
        }

        return acctgTransId;
    }

    public async Task<bool> DeletePostdatedChequeAccountingTransaction(string paymentId)
    {
        var existingTrans = await _context.AcctgTrans
            .Include(t => t.AcctgTransEntries)
            .FirstOrDefaultAsync(t => t.PaymentId == paymentId &&
                                      t.AcctgTransTypeId == "CHECK_ISSUED");

        if (existingTrans == null)
            return false;

        // Option A: Hard delete (common if transaction is not yet posted/final)
        _context.AcctgTransEntries.RemoveRange(existingTrans.AcctgTransEntries);
        _context.AcctgTrans.Remove(existingTrans);

        // Option B (safer audit trail): Reverse it instead of deleting
        // You would create a reversing transaction with opposite D/C and negative amounts
        // or set IsPosted = "N" + add a reversal flag, depending on your system's policy.

        _logger.LogInformation("Deleted existing CHECK_ISSUED transaction for payment {PaymentId}", paymentId);
        return true;
    }
}
using Application.Accounting.Services;
using Application.Core;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects
{
    public class ApproveMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public string WorkEffortId { get; set; }
            public string CompanyId { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.WorkEffortId)
                    .NotEmpty().WithMessage("WorkEffortId is required");
                RuleFor(x => x.CompanyId)
                    .NotEmpty().WithMessage("CompanyId is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<MultiPaymentCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUtilityService _utilityService;
            private readonly IUserAccessor _userAccessor;
            private readonly IAcctgMiscService _acctgMiscService;
            private readonly IInvoiceUtilityService _invoiceUtilityService;


            public Handler(DataContext context, IUtilityService utilityService, IUserAccessor userAccessor,
                IAcctgMiscService acctgMiscService, IInvoiceUtilityService invoiceUtilityService)
            {
                _context = context;
                _utilityService = utilityService;
                _userAccessor = userAccessor;
                _acctgMiscService = acctgMiscService;
                _invoiceUtilityService = invoiceUtilityService;
            }

            public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var certificate = await _context.WorkEfforts
                        .Where(w => w.WorkEffortId == request.WorkEffortId &&
                                    w.WorkEffortTypeId == "PAYMENT_CERTIFICATE")
                        .FirstOrDefaultAsync(cancellationToken);

                    if (certificate == null)
                    {
                        return Result<MultiPaymentCertificateDto>.Failure("Certificate not found");
                    }

                    if (certificate.CurrentStatusId == "WEPR_APPROVED")
                    {
                        return Result<MultiPaymentCertificateDto>.Failure("Certificate is already approved");
                    }

                    // Update certificate status
                    certificate.CurrentStatusId = "WEPR_APPROVED";
                    certificate.LastUpdatedStamp = DateTime.UtcNow;

                    var items = await _context.WorkEfforts
                        .Where(w => w.WorkEffortParentId == request.WorkEffortId &&
                                    w.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                        .ToListAsync(cancellationToken);

                    foreach (var item in items)
                    {
                        item.CurrentStatusId = "WEPR_APPROVED";
                        item.LastUpdatedStamp = DateTime.UtcNow;
                    }

                    var totalAmount = items.Sum(i => i.TotalAmount ?? 0);

                    var updateResult = await _context.SaveChangesAsync(cancellationToken);
                    if (updateResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to approve certificate");
                    }

                    var acctgTransId = await _utilityService.GetNextSequence("AcctgTrans");
                    var acctgTrans = new AcctgTran
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransTypeId = "DISBURSEMENT",
                        Description = $"مستند دفع متعدد {certificate.WorkEffortId}",
                        TransactionDate = DateTime.UtcNow,
                        WorkEffortId = certificate.WorkEffortId,
                        IsPosted = "Y",
                        PostedDate = DateTime.UtcNow,
                        GlFiscalTypeId = "ACTUAL",
                    };
                    _context.AcctgTrans.Add(acctgTrans);

                    var creditEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = "00001",
                        AcctgTransEntryTypeId = "_NA_",
                        Description = $"مستند دفع متعدد {certificate.WorkEffortId}",
                        GlAccountId = certificate.GlAccountId,
                        OrganizationPartyId = request.CompanyId,
                        Amount = totalAmount,
                        CurrencyUomId = "EGP",
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = "EGP",
                        DebitCreditFlag = "C",
                        ReconcileStatusId = "AES_NOT_RECONCILED"
                    };
                    _context.AcctgTransEntries.Add(creditEntry);

                    // ──────────────────────────────────────────────────────────────
                    // Debit entries + optional invoices
                    // ──────────────────────────────────────────────────────────────
                    int entrySeq = 2; // 00002, 00003, ...

                    var currentUsername = _userAccessor.GetUsername();
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserName == currentUsername, cancellationToken);

                    var partyAcctgPreference = await _acctgMiscService.GetPartyAccountingPreferences(
                        user?.OrganizationPartyId ?? "");

                    foreach (var item in items)
                    {
                        var partyId = item.PartyIdSupplier ?? item.PartyIdContractor;
                        bool hasParty = !string.IsNullOrWhiteSpace(partyId);

                        string invoiceId = null;

                        // ─── Create Invoice + Invoice Items only when there is a valid party ───
                        if (hasParty)
                        {
                            var newInvoiceSequence = _invoiceUtilityService.GetNextInvoiceNumber(partyAcctgPreference);
                            invoiceId = partyAcctgPreference.InvoiceIdPrefix + newInvoiceSequence;

                            var invoice = new Invoice
                            {
                                InvoiceId = invoiceId,
                                InvoiceTypeId = "PURCHASE_INVOICE",
                                PartyIdFrom = partyId,
                                PartyId = request.CompanyId,
                                StatusId = "INVOICE_PAID",
                                InvoiceDate = DateTime.UtcNow,
                                CurrencyUomId = "EGP",
                                Description = $"مستند دفع متعدد {certificate.WorkEffortId}",
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.Invoices.Add(invoice);

                            int invoiceItemSeq = 1;

                            var adjustmentTypeDescriptions = new Dictionary<string, string>
                            {
                                { "BASE_AMOUNT", "المبلغ الأساسي" },
                                { "DISCOUNT", "الخصم" },
                                { "TRANSPORTATION", "مصاريف النقل" },
                                { "GRATUITIES", "الإكراميات" }
                            };

                            if (item.Amount != null && item.Amount != 0)
                            {
                                var baseAmountItem = new InvoiceItem
                                {
                                    InvoiceId = invoiceId,
                                    InvoiceItemSeqId = invoiceItemSeq++.ToString("D5"),
                                    InvoiceItemTypeId = "PINV_SPROD_ITEM",
                                    ProductId = item.ProductId,
                                    Quantity = 1,
                                    Amount = Math.Abs(item.Amount.Value),
                                    Description = $"{item.Description} - {adjustmentTypeDescriptions["BASE_AMOUNT"]}",
                                    CreatedStamp = DateTime.UtcNow,
                                    LastUpdatedStamp = DateTime.UtcNow
                                };
                                _context.InvoiceItems.Add(baseAmountItem);
                            }

                            if (item.Discount != null && item.Discount != 0)
                            {
                                var discountItem = new InvoiceItem
                                {
                                    InvoiceId = invoiceId,
                                    InvoiceItemSeqId = invoiceItemSeq++.ToString("D5"),
                                    InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                    ProductId = item.ProductId,
                                    Quantity = 1,
                                    Amount = -Math.Abs(item.Discount.Value),
                                    Description = $"{item.Description} - {adjustmentTypeDescriptions["DISCOUNT"]}",
                                    CreatedStamp = DateTime.UtcNow,
                                    LastUpdatedStamp = DateTime.UtcNow
                                };
                                _context.InvoiceItems.Add(discountItem);
                            }

                            if (item.TransportationExpenses != null && item.TransportationExpenses != 0)
                            {
                                var transportItem = new InvoiceItem
                                {
                                    InvoiceId = invoiceId,
                                    InvoiceItemSeqId = invoiceItemSeq++.ToString("D5"),
                                    InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                    ProductId = item.ProductId,
                                    Quantity = 1,
                                    Amount = Math.Abs(item.TransportationExpenses.Value),
                                    Description =
                                        $"{item.Description} - {adjustmentTypeDescriptions["TRANSPORTATION"]}",
                                    CreatedStamp = DateTime.UtcNow,
                                    LastUpdatedStamp = DateTime.UtcNow
                                };
                                _context.InvoiceItems.Add(transportItem);
                            }

                            if (item.Gratuities != null && item.Gratuities != 0)
                            {
                                var gratuityItem = new InvoiceItem
                                {
                                    InvoiceId = invoiceId,
                                    InvoiceItemSeqId = invoiceItemSeq++.ToString("D5"),
                                    InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                    ProductId = item.ProductId,
                                    Quantity = 1,
                                    Amount = Math.Abs(item.Gratuities.Value),
                                    Description = $"{item.Description} - {adjustmentTypeDescriptions["GRATUITIES"]}",
                                    CreatedStamp = DateTime.UtcNow,
                                    LastUpdatedStamp = DateTime.UtcNow
                                };
                                _context.InvoiceItems.Add(gratuityItem);
                            }
                        }

                        // ─── Always create debit entry ───
                        if (string.IsNullOrEmpty(item.GlAccountId))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<MultiPaymentCertificateDto>.Failure(
                                $"GL Account missing on certificate item {item.WorkEffortId}");
                        }

                        var debitEntry = new AcctgTransEntry
                        {
                            AcctgTransId = acctgTransId,
                            AcctgTransEntrySeqId = entrySeq.ToString("D5"),
                            AcctgTransEntryTypeId = "_NA_",
                            Description = item.Description,
                            GlAccountId = item.GlAccountId,
                            PartyId = hasParty ? partyId : null, // ← null when no party
                            OrganizationPartyId = request.CompanyId,
                            Amount = item.TotalAmount ?? 0,
                            CurrencyUomId = "EGP",
                            OrigAmount = item.TotalAmount ?? 0,
                            OrigCurrencyUomId = "EGP",
                            DebitCreditFlag = "D",
                            ReconcileStatusId = "AES_NOT_RECONCILED"
                        };
                        _context.AcctgTransEntries.Add(debitEntry);

                        entrySeq++;
                    }

                    var acctgSaveResult = await _context.SaveChangesAsync(cancellationToken);
                    if (acctgSaveResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to create accounting transactions");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // ──────────────────────────────────────────────────────────────
                    // Prepare result DTO (unchanged from original)
                    // ──────────────────────────────────────────────────────────────
                    var resultItems = new List<MultiPaymentItemDto>();
                    foreach (var item in items)
                    {
                        var supplier = item.PartyIdSupplier != null
                            ? await _context.Parties
                                .Where(p => p.PartyId == item.PartyIdSupplier)
                                .Select(p => new { p.Description })
                                .FirstOrDefaultAsync(cancellationToken)
                            : null;

                        var contractor = item.PartyIdContractor != null
                            ? await _context.Parties
                                .Where(p => p.PartyId == item.PartyIdContractor)
                                .Select(p => new { p.Description })
                                .FirstOrDefaultAsync(cancellationToken)
                            : null;

                        var service = item.ServiceId != null
                            ? await _context.Products
                                .Where(s => s.ProductId == item.ServiceId)
                                .Select(s => new { s.ProductName })
                                .FirstOrDefaultAsync(cancellationToken)
                            : null;

                        var product = item.ProductId != null
                            ? await _context.Products
                                .Where(pr => pr.ProductId == item.ProductId)
                                .Select(pr => new { pr.ProductName })
                                .FirstOrDefaultAsync(cancellationToken)
                            : null;

                        var itemTypeDescriptions = new Dictionary<string, string>
                        {
                            { "MATERIALS", "المواد" },
                            { "LABOR", "العمالة" },
                            { "EQUIPMENT", "المعدات" },
                            { "EXPENSES", "المصروفات" }
                        };
                        var itemTypeDescription = itemTypeDescriptions.ContainsKey(item.CostType ?? "")
                            ? itemTypeDescriptions[item.CostType]
                            : "";

                        resultItems.Add(new MultiPaymentItemDto
                        {
                            WorkEffortId = item.WorkEffortId,
                            GlAccountId = item.GlAccountId,
                            ItemType = item.CostType,
                            ItemTypeDescription = itemTypeDescription,
                            ServiceId = item.ServiceId,
                            ServiceName = service?.ProductName ?? "",
                            ProductId = item.ProductId,
                            ProductName = product?.ProductName ?? "",
                            Description = item.Description,
                            Amount = item.Amount,
                            Discount = item.Discount,
                            DiscountMode = item.Discount != null && item.Discount > 0 ? "value" : "percentage",
                            TransportationExpenses = item.TransportationExpenses,
                            Gratuities = item.Gratuities,
                            Total = item.TotalAmount,
                            PartyIdSupplier = item.PartyIdSupplier,
                            PartyIdSupplierName = supplier?.Description ?? "",
                            PartyIdContractor = item.PartyIdContractor,
                            PartyIdContractorName = contractor?.Description ?? ""
                        });
                    }

                    var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                    {
                        { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                        { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                        { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                    };

                    var (statusDescription, statusDescriptionArabic) =
                        statusDescriptions.ContainsKey(certificate.CurrentStatusId)
                            ? statusDescriptions[certificate.CurrentStatusId]
                            : ("Unknown", "غير معروف");

                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = certificate.WorkEffortId,
                        Date = certificate.EstimatedStartDate,
                        Description = certificate.Description,
                        CurrentStatusId = certificate.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        Items = resultItems
                    };

                    return Result<MultiPaymentCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure($"Failed to approve certificate: {ex.Message}");
                }
            }
        }
    }
}
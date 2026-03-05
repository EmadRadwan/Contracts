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

            public Handler(DataContext context, IUtilityService utilityService)
            {
                _context = context;
                _utilityService = utilityService;
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

                    // ──────────────────────────────────────────────────────────────
                    // Accounting transaction + entries only — no invoices
                    // ──────────────────────────────────────────────────────────────

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

                    // Credit entry (total)
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

                    // Debit entries — one per certificate item
                    int entrySeq = 2;

                    foreach (var item in items)
                    {
                        if (string.IsNullOrEmpty(item.GlAccountId))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<MultiPaymentCertificateDto>.Failure(
                                $"GL Account missing on certificate item {item.WorkEffortId}");
                        }

                        var partyId = item.PartyIdSupplier ?? item.PartyIdContractor;
                        bool hasParty = !string.IsNullOrWhiteSpace(partyId);

                        var debitEntry = new AcctgTransEntry
                        {
                            AcctgTransId = acctgTransId,
                            AcctgTransEntrySeqId = entrySeq.ToString("D5"),
                            AcctgTransEntryTypeId = "_NA_",
                            Description = item.Description,
                            GlAccountId = item.GlAccountId,
                            PartyId = hasParty ? partyId : null, // still useful for reporting / reconciliation
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
                    // Prepare result DTO (unchanged)
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

                    var employeeParty = certificate.PartyIdEmployee != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdEmployee)
                            .Select(p => new { p.PartyId, p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = certificate.WorkEffortId,
                        Date = certificate.EstimatedStartDate,
                        Description = certificate.Description,
                        PartyIdEmployee = certificate.PartyIdEmployee,
                        PartyName = employeeParty?.Description,
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
using Application.Core;
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

            public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
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

                    var paymentMethod = await _context.PaymentMethods
                        .Where(pm => pm.PaymentMethodId == certificate.PaymentMethodId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (paymentMethod == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Payment method not found");
                    }

                    var paymentId = await _utilityService.GetNextSequence("Payment");
                    var payment = new Payment
                    {
                        PaymentId = paymentId,
                        PaymentTypeId = "VENDOR_PAYMENT",
                        PartyIdFrom = request.CompanyId,
                        PartyIdTo = certificate.PartyIdEmployee,
                        PaymentMethodId = certificate.PaymentMethodId,
                        PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                        Amount = totalAmount,
                        StatusId = "PMNT_SENT",
                        EffectiveDate = certificate.EstimatedStartDate ?? DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);

                    // Create accounting transaction
                    var acctgTransId = await _utilityService.GetNextSequence("AcctgTrans");
                    var employeeParty = await _context.Parties
                        .Where(p => p.PartyId == certificate.PartyIdEmployee)
                        .Select(p => new { p.GlAccountIdAdvancedPayment })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (employeeParty == null || string.IsNullOrEmpty(employeeParty.GlAccountIdAdvancedPayment))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Employee party or advanced payment GL account not found");
                    }

                    var acctgTrans = new AcctgTrans
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransTypeId = "DISBURSEMENT",
                        Description = $"Payment for certificate {certificate.WorkEffortId}",
                        TransactionDate = DateTime.UtcNow,
                        IsPosted = "Y",
                        PostedDate = DateTime.UtcNow,
                        GlFiscalTypeId = "ACTUAL",
                        PaymentId = paymentId,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.AcctgTrans.Add(acctgTrans);

                    // Create accounting transaction entries
                    var entrySeqId1 = "00001";
                    var entrySeqId2 = "00002";

                    // Debit: Employee's advanced payment GL account
                    var debitEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = entrySeqId1,
                        AcctgTransEntryTypeId = "_NA_",
                        Description = $"Advance payment to employee {certificate.PartyIdEmployee} for certificate {certificate.WorkEffortId}",
                        GlAccountId = employeeParty.GlAccountIdAdvancedPayment,
                        OrganizationPartyId = request.CompanyId,
                        Amount = totalAmount,
                        CurrencyUomId = "EGP",
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = "EGP",
                        DebitCreditFlag = "D",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.AcctgTransEntries.Add(debitEntry);

                    // Credit: Payment method's GL account (Cash or Bank)
                    var creditEntry = new AcctgTransEntry
                    {
                        AcctgTransId = acctgTransId,
                        AcctgTransEntrySeqId = entrySeqId2,
                        AcctgTransEntryTypeId = "_NA_",
                        Description = $"Payment from {paymentMethod.Description} for certificate {certificate.WorkEffortId}",
                        GlAccountId = paymentMethod.GlAccountId,
                        OrganizationPartyId = request.CompanyId,
                        Amount = totalAmount,
                        CurrencyUomId = "EGP",
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = "EGP",
                        DebitCreditFlag = "C",
                        ReconcileStatusId = "AES_NOT_RECONCILED",
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.AcctgTransEntries.Add(creditEntry);

                    var updateResult = await _context.SaveChangesAsync(cancellationToken);
                    if (updateResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to approve certificate or create accounting transaction");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var resultItems = new List<MultiPaymentItemDto>();
                    foreach (var item in items)
                    {
                        var project = await _context.WorkEfforts
                            .Where(p => p.WorkEffortId == item.ProjectId)
                            .Select(p => new { p.ProjectName })
                            .FirstOrDefaultAsync(cancellationToken);

                        var subProject = await _context.WorkEfforts
                            .Where(sp => sp.WorkEffortId == item.SubProjectId)
                            .Select(sp => new { sp.SubProjectName })
                            .FirstOrDefaultAsync(cancellationToken);

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
                            ProjectId = item.ProjectId,
                            ProjectName = project?.ProjectName ?? "",
                            SubProjectId = item.SubProjectId,
                            SubProjectName = subProject?.SubProjectName ?? "",
                            ItemType = item.CostType,
                            ItemTypeDescription = itemTypeDescription,
                            ServiceId = item.ServiceId,
                            ServiceName = service?.ProductName ?? "",
                            ProductId = item.ProductId,
                            ProductName = product?.ProductName ?? "",
                            Description = item.Description,
                            Amount = item.TotalAmount,
                            Discount = item.Discount,
                            DiscountMode = item.Discount != null ? "value" : null,
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
                        Code = certificate.CertificateNumber,
                        Date = certificate.EstimatedStartDate,
                        Description = certificate.Description,
                        PaymentMethodId = certificate.PaymentMethodId,
                        ChequeNumber = certificate.ChequeNumber,
                        ChequeDate = certificate.ChequeDate,
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
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

                    var employeeParty = await _context.Parties
                        .Where(p => p.PartyId == certificate.PartyIdEmployee)
                        .Select(p => new { p.GlAccountIdAdvancedPayment })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (employeeParty == null || string.IsNullOrEmpty(employeeParty.GlAccountIdAdvancedPayment))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure(
                            "Employee party or advanced payment GL account not found");
                    }

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
                        GlAccountId = employeeParty.GlAccountIdAdvancedPayment,
                        OrganizationPartyId = request.CompanyId,
                        Amount = totalAmount,
                        CurrencyUomId = "EGP",
                        OrigAmount = totalAmount,
                        OrigCurrencyUomId = "EGP",
                        DebitCreditFlag = "C",
                        ReconcileStatusId = "AES_NOT_RECONCILED"
                    };
                    _context.AcctgTransEntries.Add(creditEntry);

                    // REFACTOR: Move debit entry creation to the invoice loop to associate InvoiceId
                    // This ensures each debit entry can be linked to the corresponding invoice
                    int entrySeq = 2; // Start from 00002 as 00001 is used for credit entry
                    foreach (var item in items)
                    {
                        // Validate PartyIdSupplier or PartyIdContractor
                        var partyId = item.PartyIdSupplier ?? item.PartyIdContractor;
                        if (string.IsNullOrEmpty(partyId))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<MultiPaymentCertificateDto>.Failure(
                                $"No Supplier or Contractor specified for item {item.WorkEffortId}");
                        }

                        var invoiceId = await _utilityService.GetNextSequence("Invoice");
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

                        // REFACTOR: Create debit entry here to associate with InvoiceId and PartyId
                        // This links the debit entry to the invoice and uses the item's PartyIdSupplier or PartyIdContractor
                        var project = await _context.WorkEfforts
                            .Where(p => p.WorkEffortId == item.ProjectId)
                            .Select(p => new { p.GlAccountId })
                            .FirstOrDefaultAsync(cancellationToken);

                        if (project == null || string.IsNullOrEmpty(project.GlAccountId))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<MultiPaymentCertificateDto>.Failure(
                                $"Project or GL account not found for item {item.WorkEffortId}");
                        }

                        var debitEntry = new AcctgTransEntry
                        {
                            AcctgTransId = acctgTransId,
                            AcctgTransEntrySeqId = entrySeq.ToString("D5"),
                            AcctgTransEntryTypeId = "_NA_",
                            Description = item.Description,
                            GlAccountId = project.GlAccountId,
                            // REFACTOR: Use PartyIdSupplier or PartyIdContractor for PartyId
                            // This links the debit entry to the supplier or contractor
                            PartyId = partyId,
                            //InvoiceId = invoiceId, // REFACTOR: Add InvoiceId to debit entry
                            // This associates the debit entry with the created invoice
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
                                InvoiceItemSeqId = invoiceItemSeq.ToString("D5"),
                                InvoiceItemTypeId = "PINV_SPROD_ITEM",
                                ProductId = item.ProductId,
                                Quantity = 1,
                                Amount = Math.Abs(item.Amount.Value),
                                Description = $"{item.Description} - {adjustmentTypeDescriptions["BASE_AMOUNT"]}",
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.InvoiceItems.Add(baseAmountItem);
                            invoiceItemSeq++;
                        }

                        if (item.Discount != null && item.Discount != 0)
                        {
                            var discountItem = new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                InvoiceItemSeqId = invoiceItemSeq.ToString("D5"),
                                InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                ProductId = item.ProductId,
                                Quantity = 1,
                                Amount = -Math.Abs(item.Discount.Value),
                                Description = $"{item.Description} - {adjustmentTypeDescriptions["DISCOUNT"]}",
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.InvoiceItems.Add(discountItem);
                            invoiceItemSeq++;
                        }

                        if (item.TransportationExpenses != null && item.TransportationExpenses != 0)
                        {
                            var transportItem = new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                InvoiceItemSeqId = invoiceItemSeq.ToString("D5"),
                                InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                ProductId = item.ProductId,
                                Quantity = 1,
                                Amount = Math.Abs(item.TransportationExpenses.Value),
                                Description = $"{item.Description} - {adjustmentTypeDescriptions["TRANSPORTATION"]}",
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.InvoiceItems.Add(transportItem);
                            invoiceItemSeq++;
                        }

                        if (item.Gratuities != null && item.Gratuities != 0)
                        {
                            var gratuityItem = new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                InvoiceItemSeqId = invoiceItemSeq.ToString("D5"),
                                InvoiceItemTypeId = "INVOICE_ITM_ADJ",
                                ProductId = item.ProductId,
                                Quantity = 1,
                                Amount = Math.Abs(item.Gratuities.Value),
                                Description = $"{item.Description} - {adjustmentTypeDescriptions["GRATUITIES"]}",
                                CreatedStamp = DateTime.UtcNow,
                                LastUpdatedStamp = DateTime.UtcNow
                            };
                            _context.InvoiceItems.Add(gratuityItem);
                            invoiceItemSeq++;
                        }
                    }

                    var acctgSaveResult = await _context.SaveChangesAsync(cancellationToken);
                    if (acctgSaveResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to create accounting transactions");
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
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

            // REFACTOR: Fetch PaymentMethod to retrieve PaymentMethodTypeId
            // Ensures Payment record includes PaymentMethodTypeId to satisfy ListPayments query joins
            // Improves data integrity by linking Payment to correct PaymentMethodType
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
                // REFACTOR: Set PaymentMethodTypeId from PaymentMethod
                // Ensures Payment record is compatible with ListPayments query by providing non-null PaymentMethodTypeId
                // Prevents join failures in queries expecting PaymentMethodTypeId
                PaymentMethodTypeId = paymentMethod.PaymentMethodTypeId,
                Amount = totalAmount,
                StatusId = "PMNT_SENT",
                EffectiveDate = certificate.EstimatedStartDate ?? DateTime.UtcNow,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            var updateResult = await _context.SaveChangesAsync(cancellationToken);
            if (updateResult <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<MultiPaymentCertificateDto>.Failure("Failed to approve certificate");
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
using Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Domain;

namespace Application.Projects
{
    public class UpdateMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public string WorkEffortId { get; set; }
            public MultiPaymentCertificateDto Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Certificate).NotNull().WithMessage("Certificate cannot be null");
                RuleFor(x => x.Certificate.Items)
                    .Must(items => items != null && items.Any())
                    .WithMessage("At least one certificate item is required");
                RuleFor(x => x.WorkEffortId).NotEmpty().WithMessage("WorkEffortId is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<MultiPaymentCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUtilityService _utilityService;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IUtilityService utilityService, ILogger<Handler> logger)
            {
                _context = context;
                _utilityService = utilityService;
                _logger = logger;
            }

            public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request,
                CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate;
                    var workEffortId = request.WorkEffortId;

                    var existingWorkEffort = await _context.WorkEfforts
                        .FirstOrDefaultAsync(we => we.WorkEffortId == workEffortId
                                                   && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE",
                            cancellationToken);

                    if (existingWorkEffort == null)
                    {
                        _logger.LogWarning("WorkEffortId {WorkEffortId} not found or is not a PAYMENT_CERTIFICATE",
                            workEffortId);
                        return Result<MultiPaymentCertificateDto>.Failure(
                            $"Certificate with WorkEffortId {workEffortId} not found");
                    }

                    existingWorkEffort.EstimatedStartDate = certificate.Date;
                    existingWorkEffort.Description = certificate.Description;
                    existingWorkEffort.Notes = certificate.Notes;
                    existingWorkEffort.PartyIdEmployee = certificate.PartyIdEmployee;
                    existingWorkEffort.GlAccountId = certificate.GlAccountId;
                    existingWorkEffort.LastUpdatedStamp = stamp;

                    _logger.LogInformation("Updating WorkEffort: WorkEffortId={WorkEffortId}, Type={WorkEffortTypeId}",
                        existingWorkEffort.WorkEffortId, existingWorkEffort.WorkEffortTypeId);

                    var existingItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == workEffortId
                                     && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                        .ToListAsync(cancellationToken);

                    _context.WorkEfforts.RemoveRange(existingItems);
                    _logger.LogInformation("Removed {Count} existing items for WorkEffortId={WorkEffortId}",
                        existingItems.Count, workEffortId);

                    foreach (var item in certificate.Items)
                    {
                        var itemWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                        var itemWorkEffort = new WorkEffort
                        {
                            WorkEffortId = item.WorkEffortId ?? itemWorkEffortSerial,
                            WorkEffortParentId = workEffortId,
                            WorkEffortTypeId = "PAYMENT_CERTIFICATE_ITEM",
                            GlAccountId = item.GlAccountId,
                            CostType = item.ItemType,
                            ServiceId = !string.IsNullOrEmpty(item.ServiceId) ? item.ServiceId : null,
                            ProductId = !string.IsNullOrEmpty(item.ProductId) ? item.ProductId : null,
                            Description = item.Description,
                            Discount = item.Discount ?? 0,
                            TransportationExpenses = item.TransportationExpenses ?? 0,
                            Gratuities = item.Gratuities ?? 0,
                            TotalAmount = item.Total,
                            Amount = item.Amount ?? 0,
                            PartyIdSupplier = !string.IsNullOrEmpty(item.PartyIdSupplier) ? item.PartyIdSupplier : null,
                            PartyIdContractor = !string.IsNullOrEmpty(item.PartyIdContractor)
                                ? item.PartyIdContractor
                                : null,
                            CurrentStatusId = existingWorkEffort.CurrentStatusId, // Inherit parent status
                            CostCenterId = item.CostCenterId,
                            ProjectId = item.ProjectId,
                            SubProjectId = item.SubProjectId,
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp
                        };

                        /*if (string.IsNullOrEmpty(itemWorkEffort.ServiceId))
                        {
                            _logger.LogWarning("ServiceId is null for WorkEffortId={WorkEffortId}",
                                itemWorkEffort.WorkEffortId);
                            throw new InvalidOperationException(
                                $"ServiceId cannot be null for WorkEffortId {itemWorkEffort.WorkEffortId}");
                        }

                        var serviceExists =
                            await _context.Products.AnyAsync(p => p.ProductId == itemWorkEffort.ServiceId,
                                cancellationToken);
                        if (!serviceExists)
                        {
                            _logger.LogWarning("Invalid ServiceId={ServiceId} for WorkEffortId={WorkEffortId}",
                                itemWorkEffort.ServiceId, itemWorkEffort.WorkEffortId);
                            throw new InvalidOperationException($"ServiceId {itemWorkEffort.ServiceId} does not exist");
                        }

                        if (!string.IsNullOrEmpty(itemWorkEffort.ProductId))
                        {
                            var productExists =
                                await _context.Products.AnyAsync(p => p.ProductId == itemWorkEffort.ProductId,
                                    cancellationToken);
                            if (!productExists)
                            {
                                _logger.LogWarning("Invalid ProductId={ProductId} for WorkEffortId={WorkEffortId}",
                                    itemWorkEffort.ProductId, itemWorkEffort.WorkEffortId);
                                throw new InvalidOperationException(
                                    $"ProductId {itemWorkEffort.ProductId} does not exist");
                            }
                        }*/

                        _context.WorkEfforts.Add(itemWorkEffort);
                        _logger.LogInformation(
                            "Adding WorkEffort item: WorkEffortId={WorkEffortId}, ServiceId={ServiceId}, ProductId={ProductId}",
                            itemWorkEffort.WorkEffortId, itemWorkEffort.ServiceId ?? "null",
                            itemWorkEffort.ProductId ?? "null");
                    }

                    var updateResult = await _context.SaveChangesAsync(cancellationToken);
                    if (updateResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to update certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var resultItems = new List<MultiPaymentItemDto>();
                    foreach (var item in certificate.Items)
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
                        var itemTypeDescription = itemTypeDescriptions.ContainsKey(item.ItemType ?? "")
                            ? itemTypeDescriptions[item.ItemType]
                            : "";

                        resultItems.Add(new MultiPaymentItemDto
                        {
                            WorkEffortId = item.WorkEffortId,
                            GlAccountId = item.GlAccountId,
                            ItemType = item.ItemType,
                            ItemTypeDescription = itemTypeDescription,
                            ServiceId = item.ServiceId,
                            ServiceName = service?.ProductName ?? "",
                            ProductId = item.ProductId,
                            ProductName = product?.ProductName ?? "",
                            Description = item.Description,
                            Amount = item.Amount,
                            Discount = item.Discount,
                            DiscountMode = item.DiscountMode ?? (item.Discount > 0 ? "value" : "percentage"),
                            TransportationExpenses = item.TransportationExpenses,
                            Gratuities = item.Gratuities,
                            Total = item.Total,
                            PartyIdSupplier = item.PartyIdSupplier,
                            PartyIdSupplierName = supplier?.Description ?? "",
                            PartyIdContractor = item.PartyIdContractor,
                            PartyIdContractorName = contractor?.Description ?? "",
                            CostCenterId = item.CostCenterId,
                            ProjectId = item.ProjectId,
                            SubProjectId = item.SubProjectId
                        });
                    }

                    var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                    {
                        { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                        { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                        { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                    };

                    var (statusDescription, statusDescriptionArabic) =
                        statusDescriptions.ContainsKey(existingWorkEffort.CurrentStatusId)
                            ? statusDescriptions[existingWorkEffort.CurrentStatusId]
                            : ("Unknown", "غير معروف");

                    var employeeParty = existingWorkEffort.PartyIdEmployee != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == existingWorkEffort.PartyIdEmployee)
                            .Select(p => new { p.PartyId, p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = existingWorkEffort.WorkEffortId,
                        Date = existingWorkEffort.EstimatedStartDate,
                        Description = existingWorkEffort.Description,
                        Notes = existingWorkEffort.Notes,
                        PartyIdEmployee = existingWorkEffort.PartyIdEmployee,
                        PartyName = employeeParty?.Description,
                        CurrentStatusId = existingWorkEffort.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        GlAccountId = existingWorkEffort.GlAccountId,
                        Items = resultItems
                    };

                    return Result<MultiPaymentCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Failed to update certificate: WorkEffortId={WorkEffortId}",
                        request.WorkEffortId);
                    return Result<MultiPaymentCertificateDto>.Failure($"Failed to update certificate: {ex.Message}");
                }
            }
        }
    }
}
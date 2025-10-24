using System.Text.Json;
using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Projects
{
    public class CreateMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public MultiPaymentCertificateDto? Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Certificate!.Items)
                    .Must(items => items != null && items.Any())
                    .WithMessage("At least one certificate item is required");
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
                    var certificate = request.Certificate!;
                    var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                    _logger.LogInformation("Received certificate items: {Items}",
                        JsonSerializer.Serialize(certificate.Items));

                    var workEffort = new WorkEffort
                    {
                        WorkEffortId = newWorkEffortSerial,
                        WorkEffortTypeId = "PAYMENT_CERTIFICATE",
                        EstimatedStartDate = certificate.Date,
                        Description = certificate.Description,
                        CurrentStatusId = "WEPR_CREATED",
                        PartyIdEmployee = certificate.PartyIdEmployee,
                        CreatedDate = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.WorkEfforts.Add(workEffort);

                    _logger.LogInformation(
                        "Adding parent WorkEffort: WorkEffortId={WorkEffortId}, Type={WorkEffortTypeId}",
                        workEffort.WorkEffortId, workEffort.WorkEffortTypeId);

                    foreach (var item in certificate.Items!)
                    {
                        var itemWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                        var itemWorkEffort = new WorkEffort
                        {
                            WorkEffortId = itemWorkEffortSerial,
                            WorkEffortParentId = newWorkEffortSerial,
                            WorkEffortTypeId = "PAYMENT_CERTIFICATE_ITEM",
                            ProjectId = item.ProjectId,
                            SubProjectId = !string.IsNullOrEmpty(item.SubProjectId) ? item.SubProjectId : null,
                            CostType = item.ItemType,
                            ServiceId = item.ServiceId,
                            ProductId = !string.IsNullOrEmpty(item.ProductId) ? item.ProductId : null,
                            Description = item.Description,
                            Discount = item.Discount ?? 0,
                            TransportationExpenses = item.TransportationExpenses ?? 0,
                            Gratuities = item.Gratuities ?? 0,
                            TotalAmount = item.Total,
                            PartyIdSupplier = !string.IsNullOrEmpty(item.PartyIdSupplier) ? item.PartyIdSupplier : null,
                            PartyIdContractor = !string.IsNullOrEmpty(item.PartyIdContractor)
                                ? item.PartyIdContractor
                                : null,
                            CurrentStatusId = "WEPR_CREATED",
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp
                        };

                        if (string.IsNullOrEmpty(itemWorkEffort.ServiceId))
                        {
                            _logger.LogWarning(
                                "ServiceId is null or empty for WorkEffortId={WorkEffortId}. ServiceId is mandatory.",
                                itemWorkEffort.WorkEffortId);
                            throw new InvalidOperationException(
                                $"ServiceId cannot be null or empty for WorkEffortId {itemWorkEffort.WorkEffortId}.");
                        }

                        var serviceExists =
                            await _context.Products.AnyAsync(p => p.ProductId == itemWorkEffort.ServiceId,
                                cancellationToken);
                        if (!serviceExists)
                        {
                            _logger.LogWarning(
                                "Invalid ServiceId={ServiceId} for WorkEffortId={WorkEffortId}. No matching PRODUCT_ID in PRODUCT table.",
                                itemWorkEffort.ServiceId, itemWorkEffort.WorkEffortId);
                            throw new InvalidOperationException(
                                $"ServiceId {itemWorkEffort.ServiceId} does not exist in PRODUCT table.");
                        }

                        _logger.LogInformation("Validated ServiceId={ServiceId} for WorkEffortId={WorkEffortId}",
                            itemWorkEffort.ServiceId, itemWorkEffort.WorkEffortId);

                        // REFACTOR: Validate ProductId only if it's not null or empty, as it's optional
                        if (!string.IsNullOrEmpty(itemWorkEffort.ProductId))
                        {
                            var productExists =
                                await _context.Products.AnyAsync(p => p.ProductId == itemWorkEffort.ProductId,
                                    cancellationToken);
                            if (!productExists)
                            {
                                _logger.LogWarning(
                                    "Invalid ProductId={ProductId} for WorkEffortId={WorkEffortId}. No matching PRODUCT_ID in PRODUCT table.",
                                    itemWorkEffort.ProductId, itemWorkEffort.WorkEffortId);
                                throw new InvalidOperationException(
                                    $"ProductId {itemWorkEffort.ProductId} does not exist in PRODUCT table.");
                            }

                            _logger.LogInformation("Validated ProductId={ProductId} for WorkEffortId={WorkEffortId}",
                                itemWorkEffort.ProductId, itemWorkEffort.WorkEffortId);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "ProductId is null or empty for WorkEffortId={WorkEffortId}, skipping validation as it's optional",
                                itemWorkEffort.WorkEffortId);
                        }

                        _context.WorkEfforts.Add(itemWorkEffort);
                        
                        _logger.LogInformation("Adding WorkEffort item: WorkEffortId={WorkEffortId}, ServiceId={ServiceId}, ProductId={ProductId}, ParentId={WorkEffortParentId}",
                            itemWorkEffort.WorkEffortId, itemWorkEffort.ServiceId ?? "null", itemWorkEffort.ProductId ?? "null", itemWorkEffort.WorkEffortParentId);
                    }
                    
                    foreach (var entry in _context.ChangeTracker.Entries<WorkEffort>())
                    {
                        _logger.LogInformation("ChangeTracker Entry: Entity={Entity}, State={State}, ServiceId={ServiceId}, ProductId={ProductId}",
                            nameof(WorkEffort), entry.State, entry.Entity.ServiceId ?? "null", entry.Entity.ProductId ?? "null");
                    }

                    var createResult = await _context.SaveChangesAsync(cancellationToken);
                    if (createResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to create certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Add employee party lookup to retrieve PartyIdEmployee and PartyEmployeeName, matching the Update handler's behavior
                    // This ensures the response DTO includes employee data consistently across Create and Update operations
                    var employeeParty = workEffort.PartyIdEmployee != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffort.PartyIdEmployee)
                            .Select(p => new { p.PartyId, p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultItems = new List<MultiPaymentItemDto>();
                    foreach (var item in certificate.Items!)
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
                        var itemTypeDescription = itemTypeDescriptions.ContainsKey(item.ItemType ?? "")
                            ? itemTypeDescriptions[item.ItemType]
                            : "";

                        resultItems.Add(new MultiPaymentItemDto
                        {
                            WorkEffortId = item.WorkEffortId, // Or generated WorkEffortId
                            ProjectId = item.ProjectId,
                            ProjectName = project?.ProjectName ?? "",
                            SubProjectId = item.SubProjectId,
                            SubProjectName = subProject?.SubProjectName ?? "",
                            ItemType = item.ItemType,
                            ItemTypeDescription = itemTypeDescription,
                            ServiceId = item.ServiceId,
                            ServiceName = service?.ProductName ?? "",
                            ProductId = item.ProductId,
                            ProductName = product?.ProductName ?? "",
                            Description = item.Description,
                            Amount = item.Amount,
                            Discount = item.Discount,
                            DiscountMode = item.DiscountMode,
                            TransportationExpenses = item.TransportationExpenses,
                            Gratuities = item.Gratuities,
                            Total = item.Total,
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
                        statusDescriptions.ContainsKey(workEffort.CurrentStatusId)
                            ? statusDescriptions[workEffort.CurrentStatusId]
                            : ("Unknown", "غير معروف");

                    // REFACTOR: Update the result DTO to include PartyIdEmployee and PartyEmployeeName
                    // This aligns the Create handler's response with the Update handler, ensuring consistent API output
                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        Date = workEffort.EstimatedStartDate,
                        Description = workEffort.Description,
                        CurrentStatusId = workEffort.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        PartyIdEmployee = workEffort.PartyIdEmployee,
                        PartyEmployeeName = employeeParty?.Description ?? null,
                        Items = resultItems
                    };

                    return Result<MultiPaymentCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<MultiPaymentCertificateDto>.Failure($"Failed to create certificate: {ex.Message}");
                }
            }
        }
    }
}
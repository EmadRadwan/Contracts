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
                        Notes = certificate.Notes,
                        PartyIdEmployee = certificate.PartyIdEmployee,
                        CurrentStatusId = "WEPR_CREATED",
                        GlAccountId = certificate.GlAccountId,
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
                            EstimatedStartDate = item.EstimatedStartDate,
                            PartyIdSupplier = !string.IsNullOrEmpty(item.PartyIdSupplier) ? item.PartyIdSupplier : null,
                            PartyIdContractor = !string.IsNullOrEmpty(item.PartyIdContractor)
                                ? item.PartyIdContractor
                                : null,
                            CurrentStatusId = "WEPR_CREATED",
                            CostCenterId = !string.IsNullOrEmpty(item.CostCenterId) ? item.CostCenterId : null,
                            ProjectId = !string.IsNullOrEmpty(item.ProjectId) ? item.ProjectId : null,
                            SubProjectId = !string.IsNullOrEmpty(item.SubProjectId) ? item.SubProjectId : null,
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp
                        };

                        _context.WorkEfforts.Add(itemWorkEffort);

                        _logger.LogInformation(
                            "Adding WorkEffort item: WorkEffortId={WorkEffortId}, ServiceId={ServiceId}, ProductId={ProductId}, ParentId={WorkEffortParentId}",
                            itemWorkEffort.WorkEffortId, itemWorkEffort.ServiceId ?? "null",
                            itemWorkEffort.ProductId ?? "null", itemWorkEffort.WorkEffortParentId);
                    }

                    foreach (var entry in _context.ChangeTracker.Entries<WorkEffort>())
                    {
                        _logger.LogInformation(
                            "ChangeTracker Entry: Entity={Entity}, State={State}, ServiceId={ServiceId}, ProductId={ProductId}",
                            nameof(WorkEffort), entry.State, entry.Entity.ServiceId ?? "null",
                            entry.Entity.ProductId ?? "null");
                    }

                    var createResult = await _context.SaveChangesAsync(cancellationToken);
                    if (createResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to create certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);
                    
                    var resultItems = new List<MultiPaymentItemDto>();
                    var persistedItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == workEffort.WorkEffortId
                                     && we.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                        .ToListAsync(cancellationToken);

                    foreach (var item in persistedItems)
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

                        // Assuming itemTypeDescription is Arabic based on itemType (hardcoded like frontend)
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
                            Amount = (decimal?)item.Amount,
                            Discount = (decimal?)item.Discount,
                            DiscountMode = item.Discount > 0 ? "value" : "percentage",
                            TransportationExpenses = (decimal?)item.TransportationExpenses,
                            Gratuities = (decimal?)item.Gratuities,
                            Total = (decimal?)item.TotalAmount,
                            EstimatedStartDate = item.EstimatedStartDate,
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


                    var employeeParty = workEffort.PartyIdEmployee != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffort.PartyIdEmployee)
                            .Select(p => new { p.PartyId, p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        Date = workEffort.EstimatedStartDate,
                        Description = workEffort.Description,
                        Notes = workEffort.Notes,
                        PartyIdEmployee = workEffort.PartyIdEmployee,
                        PartyName = employeeParty?.Description,
                        CurrentStatusId = workEffort.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        GlAccountId = workEffort.GlAccountId,
                        
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
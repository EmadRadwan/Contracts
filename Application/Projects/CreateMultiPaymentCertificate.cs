using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Projects
{
    public class CreateMultiPaymentCertificate
    {
        public class Command : IRequest<Result<MultiPaymentCertificateDto>>
        {
            public MultiPaymentCertificateDto? Certificate { get; set; }
        }

        // REFACTOR: Added validator to ensure at least one item.
        // Purpose: Prevents creation of empty certificates.
        // Why: Improves data integrity, mirrors example's item requirement.
        // Context: Optional; can be removed if empty certificates are allowed.
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
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;
                    var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");


                    var workEffort = new WorkEffort
                    {
                        WorkEffortId = newWorkEffortSerial,
                        WorkEffortTypeId = "PAYMENT_CERTIFICATE",
                        EstimatedStartDate = certificate.Date,
                        Description = certificate.Description,
                        PaymentMethodId = certificate.PaymentMethodId,
                        ChequeNumber = certificate.ChequeNumber,
                        ChequeDate = certificate.ChequeDate,
                        CurrentStatusId = "WEPR_CREATED",
                        CreatedDate = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.WorkEfforts.Add(workEffort);

                    foreach (var item in certificate.Items!)
                    {
                        var itemWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                        var itemWorkEffort = new WorkEffort
                        {
                            WorkEffortId = itemWorkEffortSerial,
                            WorkEffortParentId = newWorkEffortSerial,
                            WorkEffortTypeId = "PAYMENT_CERTIFICATE_ITEM",
                            ProjectId = item.ProjectId,
                            SubProjectId = item.SubProjectId,
                            CostType = item.ItemType,
                            ServiceId = item.ServiceId,
                            ProductId = !string.IsNullOrEmpty(item.ProductId) ? item.ProductId : null,
                            Description = item.Description,
                            Discount = item.Discount ?? 0,
                            TransportationExpenses = item.TransportationExpenses ?? 0,
                            Gratuities = item.Gratuities ?? 0,
                            TotalAmount = item.Total,
                            PartyIdSupplier = !string.IsNullOrEmpty(item.PartyIdSupplier) ? item.PartyIdSupplier : null,
                            PartyIdContractor = !string.IsNullOrEmpty(item.PartyIdContractor) ? item.PartyIdContractor : null,
                            CurrentStatusId = "WEPR_CREATED",
                            CreatedDate = stamp,
                            LastUpdatedStamp = stamp
                        };
                        _context.WorkEfforts.Add(itemWorkEffort);
                    }

                    var createResult = await _context.SaveChangesAsync(cancellationToken);
                    if (createResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<MultiPaymentCertificateDto>.Failure("Failed to create certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Fetched names for DTO, similar to example.
                    // Purpose: Enriches return DTO with display names.
                    // Why: Ensures consistent output; fetches from DB for accuracy.
                    // Context: Multi-item, so loop to fetch per item; assumes Arabic descriptions hardcoded or fetched.
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

                        // Assuming itemTypeDescription is Arabic based on itemType (hardcoded like frontend)
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
                            WorkEffortId = item.WorkEffortId,  // Or generated WorkEffortId
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

                    // REFACTOR: Constructed result DTO with header and enriched items.
                    // Purpose: Returns complete object for frontend use.
                    // Why: Matches example's structure; includes status translations.
                    // Context: No relatedOrderId since no PO.
                    var resultDto = new MultiPaymentCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        Code = workEffort.CertificateNumber,
                        Date = workEffort.EstimatedStartDate,
                        Description = workEffort.Description,
                        PaymentMethodId = workEffort.PaymentMethodId,
                        ChequeNumber = workEffort.ChequeNumber,
                        ChequeDate = workEffort.ChequeDate,
                        CurrentStatusId = workEffort.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
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
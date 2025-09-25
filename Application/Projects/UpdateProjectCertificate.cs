using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Projects
{
    public class UpdateProjectCertificate
    {
        public class Command : IRequest<Result<ProjectCertificateDto>>
        {
            public ProjectCertificateDto? Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                // REFACTOR: Enhanced validation to match CreateProjectCertificate
                // Purpose: Ensures WorkEffortId and CertificateItems are provided
                // Improvement: Aligns with CreateProjectCertificate's validation rules
                RuleFor(x => x.Certificate!.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
                RuleFor(x => x.Certificate!.CertificateItems)
                    .Must(items => items != null && items.Any())
                    .WithMessage("At least one certificate item is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<ProjectCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly IUtilityService _utilityService;

            public Handler(DataContext context, IUserAccessor userAccessor, IUtilityService utilityService)
            {
                _context = context;
                _userAccessor = userAccessor;
                _utilityService = utilityService;
            }

            public async Task<Result<ProjectCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;

                    // REFACTOR: Simplified WorkEffort query to match CreateProjectCertificate
                    // Purpose: Fetch WorkEffort with CurrentStatus for certificate validation
                    // Improvement: Removes unnecessary StatusItem join, uses Include for efficiency
                    var workEffortQuery = await _context.WorkEfforts
                        .Include(we => we.CurrentStatus)
                        .FirstOrDefaultAsync(we => we.WorkEffortId == certificate.WorkEffortId, cancellationToken);

                    if (workEffortQuery == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Certificate not found");
                    }

                    // REFACTOR: Update certificate header fields, preserving existing if not provided
                    // Purpose: Aligns with CreateProjectCertificate's field updates
                    // Improvement: Ensures consistent field handling and preserves OFBiz conventions
                    workEffortQuery.Description = certificate.Description ?? workEffortQuery.Description;
                    workEffortQuery.EstimatedStartDate = certificate.EstimatedStartDate ?? workEffortQuery.EstimatedStartDate;
                    workEffortQuery.EstimatedCompletionDate = certificate.EstimatedCompletionDate ?? workEffortQuery.EstimatedCompletionDate;
                    workEffortQuery.PartyIdSupplier = certificate.PartyIdSupplier ?? workEffortQuery.PartyIdSupplier;
                    workEffortQuery.PartyIdContractor = certificate.PartyIdContractor ?? workEffortQuery.PartyIdContractor;
                    workEffortQuery.ProjectId = certificate.ProjectId ?? workEffortQuery.ProjectId;
                    workEffortQuery.FacilityId = certificate.FacilityId ?? workEffortQuery.FacilityId;
                    workEffortQuery.LastUpdatedStamp = stamp;

                    var category = workEffortQuery.CertificateCategory;

                    // REFACTOR: Handle certificate items consistently with CreateProjectCertificate
                    // Purpose: Add, update, or remove items, respecting CertificateCategory
                    // Improvement: Matches item field mappings and conditional logic for WORKMANSHIP_CONTRACTING_CERTIFICATE
                    var existingItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == certificate.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    // Remove deleted items
                    foreach (var existingItem in existingItems)
                    {
                        if (!certificate.CertificateItems!.Any(item => item.WorkEffortId == existingItem.WorkEffortId))
                        {
                            _context.WorkEfforts.Remove(existingItem);
                        }
                    }

                    // Update or add items
                    foreach (var item in certificate.CertificateItems!)
                    {
                        var existingItem = existingItems.FirstOrDefault(ei => ei.WorkEffortId == item.WorkEffortId);
                        if (existingItem != null)
                        {
                            // Update existing item
                            existingItem.ProductId = item.ProductId;
                            existingItem.Description = item.Description;
                            existingItem.Quantity = item.Quantity;
                            existingItem.Rate = item.UnitPrice;
                            existingItem.TotalAmount = item.TotalAmount;
                            existingItem.Discount = item.Discount ?? 0;
                            existingItem.Insurance = item.Insurance ?? 0;
                            existingItem.AdditionalInsurance = item.AdditionalInsurance;
                            existingItem.MaterialPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.MaterialPrice : 0;
                            existingItem.LaborPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.LaborPrice : 0;
                            existingItem.QuantityUomId = item.UomId;
                            existingItem.Deductions = item.Deductions ?? 0;
                            existingItem.DeductionDescription = item.DeductionDescription;
                            existingItem.AchievementPercent = item.AchievementPercentage ?? 0;
                            existingItem.Notes = item.Notes;
                            existingItem.ProcurementDate = item.ProcurementDate;
                            existingItem.TransportationExpenses = item.TransportationExpenses ?? 0;
                            existingItem.Gratuities = item.Gratuities ?? 0;
                            existingItem.LastUpdatedStamp = stamp;
                        }
                        else
                        {
                            // Add new item
                            var itemWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                            var itemWorkEffort = new WorkEffort
                            {
                                WorkEffortId = itemWorkEffortSerial,
                                WorkEffortParentId = certificate.WorkEffortId,
                                WorkEffortTypeId = "CERTIFICATE_ITEM",
                                ProductId = item.ProductId,
                                Description = item.Description,
                                Quantity = item.Quantity,
                                Rate = item.UnitPrice,
                                TotalAmount = item.TotalAmount,
                                Discount = item.Discount ?? 0,
                                Insurance = item.Insurance ?? 0,
                                AdditionalInsurance = item.AdditionalInsurance,
                                MaterialPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.MaterialPrice : 0,
                                LaborPrice = category == "WORKMANSHIP_CONTRACTING_CERTIFICATE" ? item.LaborPrice : 0,
                                QuantityUomId = item.UomId,
                                Deductions = item.Deductions ?? 0,
                                DeductionDescription = item.DeductionDescription,
                                AchievementPercent = item.AchievementPercentage ?? 0,
                                Notes = item.Notes,
                                ProcurementDate = item.ProcurementDate,
                                TransportationExpenses = item.TransportationExpenses ?? 0,
                                Gratuities = item.Gratuities ?? 0,
                                CreatedDate = stamp,
                                LastUpdatedStamp = stamp,
                                CurrentStatusId = "WEPR_CREATED"
                            };
                            _context.WorkEfforts.Add(itemWorkEffort);
                        }
                    }

                    // REFACTOR: Persist changes transactionally
                    // Purpose: Save header and item updates in one transaction
                    // Improvement: Aligns with CreateProjectCertificate's save and rollback logic
                    var updateResult = await _context.SaveChangesAsync(cancellationToken);
                    if (updateResult <= 0)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Failed to update certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Construct response DTO with additional details
                    // Purpose: Fetch project, supplier, and contractor names for frontend display
                    // Improvement: Matches CreateProjectCertificate's response structure and status handling
                    var project = await _context.WorkEfforts
                        .Where(p => p.WorkEffortId == workEffortQuery.ProjectId)
                        .Select(p => new { p.ProjectName })
                        .FirstOrDefaultAsync(cancellationToken);

                    var supplier = workEffortQuery.PartyIdSupplier != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffortQuery.PartyIdSupplier)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var contractor = workEffortQuery.PartyIdContractor != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == workEffortQuery.PartyIdContractor)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                    {
                        { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                        { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                        { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                    };

                    var (statusDescription, statusDescriptionArabic) = statusDescriptions.ContainsKey(workEffortQuery.CurrentStatusId)
                        ? statusDescriptions[workEffortQuery.CurrentStatusId]
                        : ("Unknown", "غير معروف");

                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffortQuery.WorkEffortId,
                        CertificateNumber = workEffortQuery.CertificateNumber,
                        WorkEffortTypeId = workEffortQuery.WorkEffortTypeId,
                        CertificateCategory = workEffortQuery.CertificateCategory,
                        ProjectId = workEffortQuery.ProjectId,
                        ProjectName = project?.ProjectName ?? "",
                        PartyIdSupplier = workEffortQuery.PartyIdSupplier,
                        PartyNameSupplier = supplier?.Description,
                        PartyIdContractor = workEffortQuery.PartyIdContractor,
                        PartyNameContractor = contractor?.Description,
                        Description = workEffortQuery.Description,
                        EstimatedStartDate = workEffortQuery.EstimatedStartDate,
                        EstimatedCompletionDate = workEffortQuery.EstimatedCompletionDate,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        CurrentStatusId = workEffortQuery.CurrentStatusId,
                        RelatedOrderId = workEffortQuery.RelatedOrderId,
                        FacilityId = workEffortQuery.FacilityId,
                        CertificateItems = certificate.CertificateItems
                    };

                    return Result<ProjectCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCertificateDto>.Failure($"Failed to update certificate: {ex.Message}");
                }
            }
        }
    }
}
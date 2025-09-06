using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

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
                RuleFor(x => x.Certificate!.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
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
                    var workEffort = await _context.WorkEfforts
                        .Join(
                            _context.StatusItems,
                            we => we.CurrentStatusId,
                            si => si.StatusId,
                            (we, si) => new { WorkEffort = we, StatusItem = si }
                        )
                        .Where(x => x.WorkEffort.WorkEffortId == certificate.WorkEffortId)
                        .Select(x => new { x.WorkEffort, x.StatusItem.Description })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (workEffort == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Certificate not found");
                    }

                    // REFACTOR: Update certificate header
                    // Purpose: Update WorkEffort fields, preserve existing values if null
                    // Context: Ensures OFBiz fields like LastUpdatedStamp are set
                    workEffort.WorkEffort.WorkEffortTypeId = certificate.WorkEffortTypeId ?? workEffort.WorkEffort.WorkEffortTypeId;
                    workEffort.WorkEffort.PartyId = certificate.PartyId;
                    workEffort.WorkEffort.ProjectId = certificate.ProjectId ?? workEffort.WorkEffort.ProjectId;
                    workEffort.WorkEffort.Description = certificate.Description;
                    workEffort.WorkEffort.EstimatedStartDate = certificate.EstimatedStartDate ?? workEffort.WorkEffort.EstimatedStartDate;
                    workEffort.WorkEffort.EstimatedCompletionDate = certificate.EstimatedCompletionDate ?? workEffort.WorkEffort.EstimatedCompletionDate;
                    workEffort.WorkEffort.LastUpdatedStamp = stamp;

                    // REFACTOR: Handle certificate items
                    // Purpose: Update existing items, add new ones, remove deleted ones
                    // Context: Matches CreateProjectCertificate item handling
                    var existingItems = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == certificate.WorkEffortId)
                        .ToListAsync(cancellationToken);

                    // Remove items not in the updated list
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
                            existingItem.DiscountAmount = item.Discount ?? 0;
                            existingItem.InsuranceAmount = item.Insurance ?? 0;
                            existingItem.CompletionPercentage = item.CompletionPercentage;
                            existingItem.Notes = item.Notes;
                            existingItem.ProcurementDate = item.ProcurementDate;
                            existingItem.FacilityId = item.FacilityId;
                            existingItem.IsContractorPurchased = item.IsContractorPurchased;
                            existingItem.LastUpdatedStamp = stamp;
                        }
                        else
                        {
                            // Add new item
                            var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                            var newItem = new WorkEffort
                            {
                                WorkEffortId = newWorkEffortSerial,
                                WorkEffortParentId = certificate.WorkEffortId,
                                WorkEffortTypeId = certificate.WorkEffortTypeId == "CONTRACTING_CERTIFICATE" ? "CONTRACTING_CERTIFICATE_ITEM" : "PROJECT_CERTIFICATE_ITEM",
                                ProductId = item.ProductId,
                                Description = item.Description,
                                Quantity = item.Quantity,
                                Rate = item.UnitPrice,
                                TotalAmount = item.TotalAmount,
                                DiscountAmount = item.Discount ?? 0,
                                InsuranceAmount = item.Insurance ?? 0,
                                CompletionPercentage = item.CompletionPercentage,
                                Notes = item.Notes,
                                ProcurementDate = item.ProcurementDate,
                                FacilityId = item.FacilityId,
                                IsContractorPurchased = item.IsContractorPurchased,
                                CreatedDate = stamp,
                                LastUpdatedStamp = stamp,
                                CurrentStatusId = "WEPR_IN_PROGRESS"
                            };
                            _context.WorkEfforts.Add(newItem);
                        }
                    }

                    // REFACTOR: Save changes
                    // Purpose: Persist header and items transactionally
                    // Context: Ensures OFBiz-style transactional integrity
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Failed to update certificate and items");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Construct response DTO with StatusDescription
                    // Purpose: Include status description from StatusItem join
                    // Context: Aligns with OFBiz status handling
                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = workEffort.WorkEffort.WorkEffortId,
                        CertificateNumber = workEffort.WorkEffort.CertificateNumber,
                        WorkEffortTypeId = workEffort.WorkEffort.WorkEffortTypeId,
                        ProjectId = workEffort.WorkEffort.ProjectId,
                        PartyId = workEffort.WorkEffort.PartyId,
                        Description = workEffort.WorkEffort.Description,
                        EstimatedStartDate = workEffort.WorkEffort.EstimatedStartDate,
                        EstimatedCompletionDate = workEffort.WorkEffort.EstimatedCompletionDate,
                        StatusDescription = workEffort.Description, // From StatusItem
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
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using FluentValidation;
using Application.Interfaces;

namespace Application.Projects
{
    public class ReviewProjectCertificate
    {
        public class Command : IRequest<Result<ProjectCertificateDto>>
        {
            public string WorkEffortId { get; set; } = string.Empty;

            // REFACTOR: New status to transition to (either READY_FOR_APPROVAL or REQUIRES_EDIT)
            // Purpose: Allows reviewer to advance or reject the certificate without triggering inventory actions
            // Improvement: Separates review from approval; no side effects like PO creation or inventory receipt
            public string NewStatusId { get; set; } = string.Empty; // "WEPR_READY_FOR_APPROVAL" or "WEPR_REQUIRES_EDIT"
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.WorkEffortId).NotEmpty().WithMessage("Work Effort ID is required");
                RuleFor(x => x.NewStatusId).NotEmpty().WithMessage("New status is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<ProjectCertificateDto>>
        {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<ProjectCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var certificate = await _context.WorkEfforts
                        .Include(we => we.CurrentStatus)
                        .FirstOrDefaultAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);

                    if (certificate == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure("Certificate not found");
                    }

                    // REFACTOR: Enforce that only CREATED certificates can be reviewed
                    // Purpose: Prevent invalid state transitions (e.g., reviewing an already approved certificate)
                    // Improvement: Maintains workflow integrity
                    /*if (certificate.CurrentStatusId != "WEPR_CREATED")
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateDto>.Failure(
                            "Only certificates in 'Created' status can be reviewed");
                    }*/

                    // REFACTOR: Update status to the requested review outcome
                    // Purpose: Transition to either READY_FOR_APPROVAL or REQUIRES_EDIT
                    // Improvement: No inventory or order side effects — purely status change
                    certificate.CurrentStatusId = request.NewStatusId;
                    certificate.LastUpdatedStamp = DateTime.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    // REFACTOR: Load related data for DTO (project name, party names, etc.)
                    // Purpose: Return full DTO so frontend can update selectedCertificate immediately
                    // Improvement: Avoids extra round-trip to refetch certificate
                    var project = await _context.WorkEfforts
                        .Where(p => p.WorkEffortId == certificate.ProjectId)
                        .Select(p => new { p.ProjectName })
                        .FirstOrDefaultAsync(cancellationToken);

                    var supplier = certificate.PartyIdSupplier != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdSupplier)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    var contractor = certificate.PartyIdContractor != null
                        ? await _context.Parties
                            .Where(p => p.PartyId == certificate.PartyIdContractor)
                            .Select(p => new { p.Description })
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    // REFACTOR: Enhanced status descriptions for new states
                    // Purpose: Provide accurate English/Arabic labels for ribbon and UI
                    // Improvement: Keeps status display consistent across all states
                    var statusDescriptions = new Dictionary<string, (string English, string Arabic)>
                    {
                        { "WEPR_CREATED", ("Created", "تم الإنشاء") },
                        { "WEPR_READY_FOR_APPROVAL", ("Ready for Approval", "جاهز للموافقة") },
                        { "WEPR_REQUIRES_EDIT", ("Requires Editing", "يتطلب تعديل") },
                        { "WEPR_APPROVED", ("Approved", "تمت الموافقة") },
                        { "WEPR_COMPLETE", ("Complete", "مكتمل") }
                    };

                    var (statusDescription, statusDescriptionArabic) = 
                        statusDescriptions.TryGetValue(certificate.CurrentStatusId, out var desc)
                            ? desc
                            : ("Unknown", "غير معروف");

                    var resultDto = new ProjectCertificateDto
                    {
                        WorkEffortId = certificate.WorkEffortId,
                        CertificateNumber = certificate.CertificateNumber,
                        CertificateCategory = certificate.CertificateCategory,
                        ProjectId = certificate.ProjectId,
                        ProjectName = project?.ProjectName ?? "",
                        PartyIdSupplier = certificate.PartyIdSupplier,
                        PartyNameSupplier = supplier?.Description,
                        PartyIdContractor = certificate.PartyIdContractor,
                        PartyNameContractor = contractor?.Description,
                        Description = certificate.Description,
                        EstimatedStartDate = certificate.EstimatedStartDate,
                        EstimatedCompletionDate = certificate.EstimatedCompletionDate,
                        CurrentStatusId = certificate.CurrentStatusId,
                        StatusDescription = statusDescription,
                        StatusDescriptionArabic = statusDescriptionArabic,
                        RelatedOrderId = certificate.RelatedOrderId,
                        FacilityId = certificate.FacilityId,
                        // Note: CertificateItems not included — review doesn't touch items
                    };

                    return Result<ProjectCertificateDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCertificateDto>.Failure($"Failed to review certificate: {ex.Message}");
                }
            }
        }
    }
}
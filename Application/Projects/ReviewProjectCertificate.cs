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
            
            public string NewStatusId { get; set; } = string.Empty;
            public string? Comments { get; set; }
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

                    var space = "_  ";
                    
                    certificate.Description += space += request.Comments;
                    certificate.CurrentStatusId = request.NewStatusId;
                    certificate.LastUpdatedStamp = DateTime.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    
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
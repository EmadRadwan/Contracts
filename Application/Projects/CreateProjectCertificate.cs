using Application.Core;
using FluentValidation;
using MediatR;
using Persistence;
using Domain;
using Application.Interfaces;

namespace Application.Projects
{
    public class CreateProjectCertificate
    {
        public class Command : IRequest<Result<ProjectCertificateRecord>>
        {
            public ProjectCertificateRecord? Certificate { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Certificate!.PartyId).NotEmpty().WithMessage("Party ID is required");
                RuleFor(x => x.Certificate!.Description).NotEmpty().WithMessage("Description is required");
            }
        }

        public class Handler : IRequestHandler<Command, Result<ProjectCertificateRecord>>
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

            public async Task<Result<ProjectCertificateRecord>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var stamp = DateTime.UtcNow;
                    var certificate = request.Certificate!;
                    
                    var newWorkEffortSerial = await _utilityService.GetNextSequence("WorkEffort");
                    var newProjectCertificateSerial = await _utilityService.GetNextSequence("ProjectCertificate");


                    var workEffort = new WorkEffort
                    {
                        WorkEffortId = newWorkEffortSerial,
                        CertificateNumber = newProjectCertificateSerial,
                        WorkEffortTypeId = "PROJECT_CERTIFICATE",
                        PartyId = certificate.PartyId,
                        Description = certificate.Description,
                        EstimatedStartDate = certificate.EstimatedStartDate,
                        EstimatedCompletionDate = certificate.EstimatedCompletionDate,
                        CurrentStatusId = "WEPR_IN_PROGRESS",
                        CreatedDate = stamp,
                        LastUpdatedStamp = stamp
                    };

                    _context.WorkEfforts.Add(workEffort);
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;

                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<ProjectCertificateRecord>.Failure("Failed to create certificate");
                    }

                    await transaction.CommitAsync(cancellationToken);

                    var resultDto = new ProjectCertificateRecord
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        ProjectNum = workEffort.ProjectNum,
                        ProjectName = workEffort.ProjectName,
                        PartyId = workEffort.PartyId,
                        Description = workEffort.Description,
                        EstimatedStartDate = workEffort.EstimatedStartDate,
                        EstimatedCompletionDate = workEffort.EstimatedCompletionDate,
                        StatusDescription = "CREATED"
                    };

                    return Result<ProjectCertificateRecord>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProjectCertificateRecord>.Failure($"Failed to create certificate: {ex.Message}");
                }
            }
        }
    }
}
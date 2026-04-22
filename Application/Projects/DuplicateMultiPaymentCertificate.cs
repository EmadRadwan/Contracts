using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Projects;

public class DuplicateMultiPaymentCertificate
{
    public class Command : IRequest<Result<MultiPaymentCertificateDto>>
    {
        public string OriginalWorkEffortId { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<MultiPaymentCertificateDto>>
    {
        private readonly DataContext _context;
        private readonly IUtilityService _utilityService;
        private readonly ILogger<CreateMultiPaymentCertificate.Handler> _logger;

        public Handler(DataContext context, IUtilityService utilityService, ILogger<CreateMultiPaymentCertificate.Handler> logger)
        {
            _context = context;
            _utilityService = utilityService;
            _logger = logger;
        }

        public async Task<Result<MultiPaymentCertificateDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var original = await _context.WorkEfforts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkEffortId == request.OriginalWorkEffortId && w.WorkEffortTypeId == "PAYMENT_CERTIFICATE", cancellationToken);

            if (original == null)
            {
                return Result<MultiPaymentCertificateDto>.Failure("Original certificate not found");
            }

            var originalItems = await _context.WorkEfforts
                .AsNoTracking()
                .Where(w => w.WorkEffortParentId == request.OriginalWorkEffortId && w.WorkEffortTypeId == "PAYMENT_CERTIFICATE_ITEM")
                .ToListAsync(cancellationToken);

            var createReqDto = new MultiPaymentCertificateDto
            {
                Date = DateTime.UtcNow,
                Description = original.Description != null 
                    ? $"نسخة من {original.WorkEffortId} - {original.Description}" 
                    : $"نسخة من {original.WorkEffortId}",
                Notes = original.Notes,
                PartyIdEmployee = original.PartyIdEmployee,
                GlAccountId = original.GlAccountId,
                Items = originalItems.Select(item => new MultiPaymentItemDto
                {
                    GlAccountId = item.GlAccountId,
                    ItemType = item.CostType,
                    ServiceId = item.ServiceId,
                    ProductId = item.ProductId,
                    Description = item.Description,
                    Amount = (decimal?)item.Amount,
                    Discount = (decimal?)item.Discount,
                    TransportationExpenses = (decimal?)item.TransportationExpenses,
                    Gratuities = (decimal?)item.Gratuities,
                    Total = (decimal?)item.TotalAmount,
                    ProjectId = item.ProjectId,
                    SubProjectId = item.SubProjectId,
                    PartyIdSupplier = item.PartyIdSupplier,
                    PartyIdContractor = item.PartyIdContractor,
                    CostCenterId = item.CostCenterId,
                    EstimatedStartDate = item.EstimatedStartDate
                }).ToList()
            };

            var createCommand = new CreateMultiPaymentCertificate.Command
            {
                Certificate = createReqDto
            };

            return await new CreateMultiPaymentCertificate.Handler(_context, _utilityService, _logger)
                .Handle(createCommand, cancellationToken);
        }
    }
}

using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class CreateSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public SalesCommissionDto Dto { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUtilityService _utilityService;

        public Handler(DataContext context, ILogger<Handler> logger, IUtilityService utilityService)
        {
            _context = context;
            _logger = logger;
            _utilityService = utilityService;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var sr = await _context.SalesRequests
                .Where(x => x.SalesRequestId == dto.SalesRequestId)
                .Join(_context.Products,
                    s => s.ProductId,
                    p => p.ProductId,
                    (s, p) => new { s, p })
                .FirstOrDefaultAsync(cancellationToken);

            if (sr == null)
                return Result<SalesCommissionDto>.Failure("Sales request not found");

            if (sr.s.StatusId != "SALES_REQUEST_APPROVED")
                return Result<SalesCommissionDto>.Failure("Commission can only be created for approved sales requests");

            var existing = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesRequestId == dto.SalesRequestId, cancellationToken);
            if (existing != null)
                return Result<SalesCommissionDto>.Failure("A commission record already exists for this sales request");

            var projectId = sr.p.ProjectId;
            var isIndirect = dto.SaleTypeId == "COMM_SALE_INDIRECT";

            // Party slots are optional, but a party and its percentage must be filled in together —
            // see ValidatePartyPercentPairing for why a lone percentage corrupts the commission reports.
            var partyError = SalesCommissionCalculator.ValidatePartyPercentPairing(dto, isIndirect);
            if (partyError != null)
                return Result<SalesCommissionDto>.Failure(partyError);

            SalesCommissionCalculator.ClearUnassignedPartyPercents(dto);

            // Derive salePrice and collectedAmount from the database — never trust client input
            var salePrice = sr.s.TotalPrice ?? 0m;

            var collectedAmount = await _context.Payments
                .Where(p => p.SalesRequestId == dto.SalesRequestId
                         && p.Amount > 0
                         && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                             || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                         && p.StatusId == "PMNT_RECEIVED")
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            // Threshold rules — collection ratio no longer blocks creation; it only scales the
            // commission factor (0% below the lower threshold, 50% below the upper, 100% at/above it).
            var ratio = salePrice > 0 ? collectedAmount / salePrice : 0m;
            var lowerThreshold = SalesCommissionCalculator.GetLowerThreshold(isIndirect);
            var commissionFactor = SalesCommissionCalculator.ComputeFactor(ratio, lowerThreshold);

            // Validate submitted percentages against the configured project rate
            var configuredRate = projectId != null
                ? await _context.ProjectCommissionRates
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.SaleTypeId == dto.SaleTypeId, cancellationToken)
                : null;

            var rateError = SalesCommissionCalculator.ValidateAgainstConfiguredRate(dto, configuredRate, isIndirect);
            if (rateError != null)
                return Result<SalesCommissionDto>.Failure(rateError);

            var stamp = DateTime.UtcNow;
            var newId = await _utilityService.GetNextSequence("SalesCommission");

            var amounts = SalesCommissionCalculator.CalculateAmounts(dto, salePrice, commissionFactor, isIndirect);

            var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var commission = new Domain.SalesCommission
                {
                    SalesCommissionId = newId,
                    SalesRequestId = dto.SalesRequestId,
                    SaleTypeId = dto.SaleTypeId,
                    StatusId = "COMMISSION_PENDING",
                    CommissionDate = dto.CommissionDate ?? stamp,
                    ProjectId = projectId,
                    SalePrice = salePrice,
                    CollectedAmount = collectedAmount,
                    SalesRepPartyId = dto.SalesRepPartyId,
                    SalesRepPercent = dto.SalesRepPercent,
                    SalesRepAmount = amounts.SalesRepAmount,
                    SalesRepNetAmount = amounts.SalesRepAmount,
                    ManagerPartyId = dto.ManagerPartyId,
                    ManagerPercent = dto.ManagerPercent,
                    ManagerAmount = amounts.ManagerAmount,
                    ManagerNetAmount = amounts.ManagerAmount,
                    SalesRep2PartyId = dto.SalesRep2PartyId,
                    SalesRep2Percent = dto.SalesRep2Percent,
                    SalesRep2Amount = amounts.SalesRep2Amount,
                    SalesRep2NetAmount = amounts.SalesRep2Amount,
                    Manager2PartyId = dto.Manager2PartyId,
                    Manager2Percent = dto.Manager2Percent,
                    Manager2Amount = amounts.Manager2Amount,
                    Manager2NetAmount = amounts.Manager2Amount,
                    ExternalCompanyPartyId = isIndirect ? dto.ExternalCompanyPartyId : null,
                    ExternalCompanyPercent = isIndirect ? dto.ExternalCompanyPercent : null,
                    ExternalCompanyGrossAmount = isIndirect ? amounts.ExternalCompanyGrossAmount : null,
                    ExternalCompanyNetAmount = isIndirect ? amounts.ExternalCompanyNetAmount : null,
                    ExternalSalesRepPartyId = isIndirect ? dto.ExternalSalesRepPartyId : null,
                    ExternalSalesRepPercent = isIndirect ? dto.ExternalSalesRepPercent : null,
                    ExternalSalesRepAmount = isIndirect ? amounts.ExternalSalesRepAmount : null,
                    ExternalSalesRepNetAmount = isIndirect ? amounts.ExternalSalesRepNetAmount : null,
                    HasExternalSalesRepWithholdingTaxExemption = isIndirect && dto.HasExternalSalesRepWithholdingTaxExemption,
                    ExternalSalesRepNationalId = isIndirect ? dto.ExternalSalesRepNationalId : null,
                    ExternalManagerPartyId = isIndirect ? dto.ExternalManagerPartyId : null,
                    ExternalManagerPercent = isIndirect ? dto.ExternalManagerPercent : null,
                    ExternalManagerAmount = isIndirect ? amounts.ExternalManagerAmount : null,
                    ExternalManagerNetAmount = isIndirect ? amounts.ExternalManagerNetAmount : null,
                    HasExternalManagerWithholdingTaxExemption = isIndirect && dto.HasExternalManagerWithholdingTaxExemption,
                    ExternalManagerNationalId = isIndirect ? dto.ExternalManagerNationalId : null,
                    HasVatExemption = dto.HasVatExemption,
                    HasWithholdingTaxExemption = dto.HasWithholdingTaxExemption,
                    VatPercent = dto.VatPercent,
                    WithholdingTaxPercent = dto.WithholdingTaxPercent,
                    Notes = dto.Notes,
                    CreatedStamp = stamp,
                    LastUpdatedStamp = stamp
                };

                _context.SalesCommissions.Add(commission);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                dto.SalesCommissionId = newId;
                dto.StatusId = "COMMISSION_PENDING";
                dto.SalesRepAmount = commission.SalesRepAmount;
                dto.SalesRepNetAmount = commission.SalesRepNetAmount;
                dto.ManagerAmount = commission.ManagerAmount;
                dto.ManagerNetAmount = commission.ManagerNetAmount;
                dto.SalesRep2Amount = commission.SalesRep2Amount;
                dto.SalesRep2NetAmount = commission.SalesRep2NetAmount;
                dto.Manager2Amount = commission.Manager2Amount;
                dto.Manager2NetAmount = commission.Manager2NetAmount;
                dto.ExternalCompanyGrossAmount = commission.ExternalCompanyGrossAmount;
                dto.ExternalCompanyNetAmount = commission.ExternalCompanyNetAmount;
                dto.ExternalSalesRepAmount = commission.ExternalSalesRepAmount;
                dto.ExternalSalesRepNetAmount = commission.ExternalSalesRepNetAmount;
                dto.ExternalManagerAmount = commission.ExternalManagerAmount;
                dto.ExternalManagerNetAmount = commission.ExternalManagerNetAmount;
                dto.HasExternalSalesRepWithholdingTaxExemption = commission.HasExternalSalesRepWithholdingTaxExemption;
                dto.HasExternalManagerWithholdingTaxExemption = commission.HasExternalManagerWithholdingTaxExemption;

                return Result<SalesCommissionDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create sales commission");
                await transaction.RollbackAsync(cancellationToken);
                return Result<SalesCommissionDto>.Failure("Failed to create sales commission");
            }
        }
    }
}

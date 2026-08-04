using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class UpdateSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public SalesCommissionDto Dto { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var commission = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesCommissionId == dto.SalesCommissionId, cancellationToken);

            if (commission == null)
                return Result<SalesCommissionDto>.Failure("Commission record not found");

            if (commission.StatusId != "COMMISSION_PENDING")
                return Result<SalesCommissionDto>.Failure("Only pending commissions can be updated");

            var sr = await _context.SalesRequests
                .Where(x => x.SalesRequestId == commission.SalesRequestId)
                .Join(_context.Products,
                    s => s.ProductId,
                    p => p.ProductId,
                    (s, p) => new { s, p })
                .FirstOrDefaultAsync(cancellationToken);

            if (sr == null)
                return Result<SalesCommissionDto>.Failure("Sales request not found");

            var isIndirect = dto.SaleTypeId == "COMM_SALE_INDIRECT";
            var projectId = commission.ProjectId ?? sr.p.ProjectId;
            var salePrice = sr.s.TotalPrice ?? 0m;

            var partyError = SalesCommissionCalculator.ValidateRequiredParties(dto, isIndirect);
            if (partyError != null)
                return Result<SalesCommissionDto>.Failure(partyError);

            // Validate submitted percentages against the configured project rate
            var configuredRate = projectId != null
                ? await _context.ProjectCommissionRates
                    .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.SaleTypeId == dto.SaleTypeId, cancellationToken)
                : null;

            var rateError = SalesCommissionCalculator.ValidateAgainstConfiguredRate(dto, configuredRate, isIndirect);
            if (rateError != null)
                return Result<SalesCommissionDto>.Failure(rateError);

            var collectedAmount = await _context.Payments
                .Where(p => p.SalesRequestId == commission.SalesRequestId
                         && p.Amount > 0
                         && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                             || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                         && p.StatusId == "PMNT_RECEIVED")
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            // Collection ratio no longer blocks the update; it only scales the commission factor
            // (0% below the lower threshold, 50% below the upper, 100% at/above it).
            var ratio = salePrice > 0 ? collectedAmount / salePrice : 0m;
            var lowerThreshold = SalesCommissionCalculator.GetLowerThreshold(isIndirect);
            var commissionFactor = SalesCommissionCalculator.ComputeFactor(ratio, lowerThreshold);

            var amounts = SalesCommissionCalculator.CalculateAmounts(dto, salePrice, commissionFactor, isIndirect);

            var stamp = DateTime.UtcNow;
            try
            {
                commission.SaleTypeId = dto.SaleTypeId;
                commission.SalePrice = salePrice;
                commission.CollectedAmount = collectedAmount;
                commission.SalesRepPartyId = dto.SalesRepPartyId;
                commission.SalesRepPercent = dto.SalesRepPercent;
                commission.SalesRepAmount = amounts.SalesRepAmount;
                commission.SalesRepNetAmount = amounts.SalesRepAmount;
                commission.ManagerPartyId = dto.ManagerPartyId;
                commission.ManagerPercent = dto.ManagerPercent;
                commission.ManagerAmount = amounts.ManagerAmount;
                commission.ManagerNetAmount = amounts.ManagerAmount;
                commission.SalesRep2PartyId = dto.SalesRep2PartyId;
                commission.SalesRep2Percent = dto.SalesRep2Percent;
                commission.SalesRep2Amount = amounts.SalesRep2Amount;
                commission.SalesRep2NetAmount = amounts.SalesRep2Amount;
                commission.Manager2PartyId = dto.Manager2PartyId;
                commission.Manager2Percent = dto.Manager2Percent;
                commission.Manager2Amount = amounts.Manager2Amount;
                commission.Manager2NetAmount = amounts.Manager2Amount;
                commission.ExternalCompanyPartyId = isIndirect ? dto.ExternalCompanyPartyId : null;
                commission.ExternalCompanyPercent = isIndirect ? dto.ExternalCompanyPercent : null;
                commission.ExternalCompanyGrossAmount = isIndirect ? amounts.ExternalCompanyGrossAmount : null;
                commission.ExternalCompanyNetAmount = isIndirect ? amounts.ExternalCompanyNetAmount : null;
                commission.ExternalSalesRepPartyId = isIndirect ? dto.ExternalSalesRepPartyId : null;
                commission.ExternalSalesRepPercent = isIndirect ? dto.ExternalSalesRepPercent : null;
                commission.ExternalSalesRepAmount = isIndirect ? amounts.ExternalSalesRepAmount : null;
                commission.ExternalSalesRepNetAmount = isIndirect ? amounts.ExternalSalesRepNetAmount : null;
                commission.HasExternalSalesRepWithholdingTaxExemption = isIndirect && dto.HasExternalSalesRepWithholdingTaxExemption;
                commission.ExternalSalesRepNationalId = isIndirect ? dto.ExternalSalesRepNationalId : null;
                commission.ExternalManagerPartyId = isIndirect ? dto.ExternalManagerPartyId : null;
                commission.ExternalManagerPercent = isIndirect ? dto.ExternalManagerPercent : null;
                commission.ExternalManagerAmount = isIndirect ? amounts.ExternalManagerAmount : null;
                commission.ExternalManagerNetAmount = isIndirect ? amounts.ExternalManagerNetAmount : null;
                commission.HasExternalManagerWithholdingTaxExemption = isIndirect && dto.HasExternalManagerWithholdingTaxExemption;
                commission.ExternalManagerNationalId = isIndirect ? dto.ExternalManagerNationalId : null;
                commission.HasVatExemption = dto.HasVatExemption;
                commission.HasWithholdingTaxExemption = dto.HasWithholdingTaxExemption;
                commission.VatPercent = dto.VatPercent;
                commission.WithholdingTaxPercent = dto.WithholdingTaxPercent;
                commission.Notes = dto.Notes;
                commission.LastUpdatedStamp = stamp;

                await _context.SaveChangesAsync(cancellationToken);

                dto.SalesCommissionId = commission.SalesCommissionId;
                dto.StatusId = commission.StatusId;
                dto.SalePrice = salePrice;
                dto.CollectedAmount = collectedAmount;
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
                _logger.LogError(ex, "Failed to update sales commission");
                return Result<SalesCommissionDto>.Failure("Failed to update sales commission");
            }
        }
    }
}

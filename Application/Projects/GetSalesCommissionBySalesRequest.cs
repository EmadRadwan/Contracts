using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class GetSalesCommissionBySalesRequest
{
    public class Query : IRequest<Result<SalesCommissionDto?>>
    {
        public string SalesRequestId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<SalesCommissionDto?>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public async Task<Result<SalesCommissionDto?>> Handle(Query request, CancellationToken cancellationToken)
        {
            var sc = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesRequestId == request.SalesRequestId, cancellationToken);

            if (sc == null)
                return Result<SalesCommissionDto?>.Success(null);

            var dto = new SalesCommissionDto
            {
                SalesCommissionId = sc.SalesCommissionId,
                SalesRequestId = sc.SalesRequestId,
                SaleTypeId = sc.SaleTypeId,
                StatusId = sc.StatusId,
                CommissionDate = sc.CommissionDate,
                ProjectId = sc.ProjectId,
                SalePrice = sc.SalePrice,
                CollectedAmount = sc.CollectedAmount,
                SalesRepPartyId = sc.SalesRepPartyId,
                SalesRepPercent = sc.SalesRepPercent,
                SalesRepAmount = sc.SalesRepAmount,
                SalesRepNetAmount = sc.SalesRepNetAmount,
                ManagerPartyId = sc.ManagerPartyId,
                ManagerPercent = sc.ManagerPercent,
                ManagerAmount = sc.ManagerAmount,
                ManagerNetAmount = sc.ManagerNetAmount,
                SalesRep2PartyId = sc.SalesRep2PartyId,
                SalesRep2Percent = sc.SalesRep2Percent,
                SalesRep2Amount = sc.SalesRep2Amount,
                SalesRep2NetAmount = sc.SalesRep2NetAmount,
                Manager2PartyId = sc.Manager2PartyId,
                Manager2Percent = sc.Manager2Percent,
                Manager2Amount = sc.Manager2Amount,
                Manager2NetAmount = sc.Manager2NetAmount,
                ExternalCompanyPartyId = sc.ExternalCompanyPartyId,
                ExternalCompanyPercent = sc.ExternalCompanyPercent,
                ExternalCompanyGrossAmount = sc.ExternalCompanyGrossAmount,
                ExternalCompanyNetAmount = sc.ExternalCompanyNetAmount,
                ExternalSalesRepPartyId = sc.ExternalSalesRepPartyId,
                ExternalSalesRepPercent = sc.ExternalSalesRepPercent,
                ExternalSalesRepAmount = sc.ExternalSalesRepAmount,
                ExternalSalesRepNetAmount = sc.ExternalSalesRepNetAmount,
                ExternalManagerPartyId = sc.ExternalManagerPartyId,
                ExternalManagerPercent = sc.ExternalManagerPercent,
                ExternalManagerAmount = sc.ExternalManagerAmount,
                ExternalManagerNetAmount = sc.ExternalManagerNetAmount,
                HasVatExemption = sc.HasVatExemption,
                HasWithholdingTaxExemption = sc.HasWithholdingTaxExemption,
                VatPercent = sc.VatPercent,
                WithholdingTaxPercent = sc.WithholdingTaxPercent,
                Notes = sc.Notes
            };

            return Result<SalesCommissionDto?>.Success(dto);
        }
    }
}

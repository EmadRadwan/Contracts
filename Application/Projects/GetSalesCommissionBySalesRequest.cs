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
            var dto = await _context.SalesCommissions
                .Where(sc => sc.SalesRequestId == request.SalesRequestId)
                .GroupJoin(_context.Parties, sc => sc.SalesRepPartyId, p => p.PartyId, (sc, parties) => new { sc, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, salesRepParty) => new { x.sc, salesRepParty })
                .GroupJoin(_context.Parties, x => x.sc.ManagerPartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, managerParty) => new { x.sc, x.salesRepParty, managerParty })
                .GroupJoin(_context.Parties, x => x.sc.SalesRep2PartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, x.managerParty, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, salesRep2Party) => new { x.sc, x.salesRepParty, x.managerParty, salesRep2Party })
                .GroupJoin(_context.Parties, x => x.sc.Manager2PartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, manager2Party) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, manager2Party })
                .GroupJoin(_context.Parties, x => x.sc.ExternalCompanyPartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, x.manager2Party, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, extCompanyParty) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, x.manager2Party, extCompanyParty })
                .GroupJoin(_context.Parties, x => x.sc.ExternalSalesRepPartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, x.manager2Party, x.extCompanyParty, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, extSalesRepParty) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, x.manager2Party, x.extCompanyParty, extSalesRepParty })
                .GroupJoin(_context.Parties, x => x.sc.ExternalManagerPartyId, p => p.PartyId, (x, parties) => new { x.sc, x.salesRepParty, x.managerParty, x.salesRep2Party, x.manager2Party, x.extCompanyParty, x.extSalesRepParty, parties })
                .SelectMany(x => x.parties.DefaultIfEmpty(), (x, extManagerParty) => new SalesCommissionDto
                {
                    SalesCommissionId = x.sc.SalesCommissionId,
                    SalesRequestId = x.sc.SalesRequestId,
                    SaleTypeId = x.sc.SaleTypeId,
                    StatusId = x.sc.StatusId,
                    CommissionDate = x.sc.CommissionDate,
                    ProjectId = x.sc.ProjectId,
                    SalePrice = x.sc.SalePrice,
                    CollectedAmount = x.sc.CollectedAmount,
                    SalesRepPartyId = x.sc.SalesRepPartyId,
                    SalesRepName = x.salesRepParty != null ? x.salesRepParty.Description : null,
                    SalesRepPercent = x.sc.SalesRepPercent,
                    SalesRepAmount = x.sc.SalesRepAmount,
                    SalesRepNetAmount = x.sc.SalesRepNetAmount,
                    ManagerPartyId = x.sc.ManagerPartyId,
                    ManagerName = x.managerParty != null ? x.managerParty.Description : null,
                    ManagerPercent = x.sc.ManagerPercent,
                    ManagerAmount = x.sc.ManagerAmount,
                    ManagerNetAmount = x.sc.ManagerNetAmount,
                    SalesRep2PartyId = x.sc.SalesRep2PartyId,
                    SalesRep2Name = x.salesRep2Party != null ? x.salesRep2Party.Description : null,
                    SalesRep2Percent = x.sc.SalesRep2Percent,
                    SalesRep2Amount = x.sc.SalesRep2Amount,
                    SalesRep2NetAmount = x.sc.SalesRep2NetAmount,
                    Manager2PartyId = x.sc.Manager2PartyId,
                    Manager2Name = x.manager2Party != null ? x.manager2Party.Description : null,
                    Manager2Percent = x.sc.Manager2Percent,
                    Manager2Amount = x.sc.Manager2Amount,
                    Manager2NetAmount = x.sc.Manager2NetAmount,
                    ExternalCompanyPartyId = x.sc.ExternalCompanyPartyId,
                    ExternalCompanyName = x.extCompanyParty != null ? x.extCompanyParty.Description : null,
                    ExternalCompanyPercent = x.sc.ExternalCompanyPercent,
                    ExternalCompanyGrossAmount = x.sc.ExternalCompanyGrossAmount,
                    ExternalCompanyNetAmount = x.sc.ExternalCompanyNetAmount,
                    ExternalSalesRepPartyId = x.sc.ExternalSalesRepPartyId,
                    ExternalSalesRepName = x.extSalesRepParty != null ? x.extSalesRepParty.Description : null,
                    ExternalSalesRepPercent = x.sc.ExternalSalesRepPercent,
                    ExternalSalesRepAmount = x.sc.ExternalSalesRepAmount,
                    ExternalSalesRepNetAmount = x.sc.ExternalSalesRepNetAmount,
                    HasExternalSalesRepWithholdingTaxExemption = x.sc.HasExternalSalesRepWithholdingTaxExemption,
                    ExternalSalesRepNationalId = x.sc.ExternalSalesRepNationalId,
                    ExternalManagerPartyId = x.sc.ExternalManagerPartyId,
                    ExternalManagerName = extManagerParty != null ? extManagerParty.Description : null,
                    ExternalManagerPercent = x.sc.ExternalManagerPercent,
                    ExternalManagerAmount = x.sc.ExternalManagerAmount,
                    ExternalManagerNetAmount = x.sc.ExternalManagerNetAmount,
                    HasExternalManagerWithholdingTaxExemption = x.sc.HasExternalManagerWithholdingTaxExemption,
                    ExternalManagerNationalId = x.sc.ExternalManagerNationalId,
                    HasVatExemption = x.sc.HasVatExemption,
                    HasWithholdingTaxExemption = x.sc.HasWithholdingTaxExemption,
                    VatPercent = x.sc.VatPercent,
                    WithholdingTaxPercent = x.sc.WithholdingTaxPercent,
                    Notes = x.sc.Notes
                })
                .FirstOrDefaultAsync(cancellationToken);

            return Result<SalesCommissionDto?>.Success(dto);
        }
    }
}

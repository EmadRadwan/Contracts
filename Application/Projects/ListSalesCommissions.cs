using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Projects;

public class ListSalesCommissions
{
    public class Query : IRequest<IQueryable<SalesCommissionRecord>>
    {
        public ODataQueryOptions<SalesCommissionRecord>? Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<SalesCommissionRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public Task<IQueryable<SalesCommissionRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.SalesCommissions
                .Join(_context.SalesRequests,
                    sc => sc.SalesRequestId,
                    sr => sr.SalesRequestId,
                    (sc, sr) => new { sc, sr })
                .Join(_context.Products,
                    x => x.sr.ProductId,
                    p => p.ProductId,
                    (x, p) => new { x.sc, x.sr, p })
                .Join(_context.WorkEfforts,
                    x => x.p.ProjectId,
                    we => we.WorkEffortId,
                    (x, we) => new { x.sc, x.sr, x.p, we })
                .GroupJoin(_context.Parties,
                    x => x.sc.SalesRepPartyId,
                    party => party.PartyId,
                    (x, parties) => new { x.sc, x.sr, x.p, x.we, parties })
                .SelectMany(
                    x => x.parties.DefaultIfEmpty(),
                    (x, salesRepParty) => new { x.sc, x.sr, x.p, x.we, salesRepParty })
                .GroupJoin(_context.Parties,
                    x => x.sc.ManagerPartyId,
                    party => party.PartyId,
                    (x, parties) => new { x.sc, x.sr, x.p, x.we, x.salesRepParty, parties })
                .SelectMany(
                    x => x.parties.DefaultIfEmpty(),
                    (x, managerParty) => new { x.sc, x.sr, x.p, x.we, x.salesRepParty, managerParty })
                .GroupJoin(_context.Parties,
                    x => x.sc.ExternalCompanyPartyId,
                    party => party.PartyId,
                    (x, parties) => new { x.sc, x.sr, x.p, x.we, x.salesRepParty, x.managerParty, parties })
                .SelectMany(
                    x => x.parties.DefaultIfEmpty(),
                    (x, extCompanyParty) => new SalesCommissionRecord
                    {
                        SalesCommissionId = x.sc.SalesCommissionId,
                        SalesRequestId = x.sc.SalesRequestId,
                        SaleTypeId = x.sc.SaleTypeId,
                        StatusId = x.sc.StatusId,
                        CommissionDate = x.sc.CommissionDate,
                        ProjectId = x.sc.ProjectId,
                        ProjectName = x.we.WorkEffortName,
                        ApartmentName = x.p.ProductName,
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
                        SalesRep2Percent = x.sc.SalesRep2Percent,
                        SalesRep2Amount = x.sc.SalesRep2Amount,
                        SalesRep2NetAmount = x.sc.SalesRep2NetAmount,
                        Manager2PartyId = x.sc.Manager2PartyId,
                        Manager2Percent = x.sc.Manager2Percent,
                        Manager2Amount = x.sc.Manager2Amount,
                        Manager2NetAmount = x.sc.Manager2NetAmount,
                        ExternalCompanyPartyId = x.sc.ExternalCompanyPartyId,
                        ExternalCompanyName = extCompanyParty != null ? extCompanyParty.Description : null,
                        ExternalCompanyPercent = x.sc.ExternalCompanyPercent,
                        ExternalCompanyGrossAmount = x.sc.ExternalCompanyGrossAmount,
                        ExternalCompanyNetAmount = x.sc.ExternalCompanyNetAmount,
                        ExternalSalesRepPartyId = x.sc.ExternalSalesRepPartyId,
                        ExternalSalesRepPercent = x.sc.ExternalSalesRepPercent,
                        ExternalSalesRepAmount = x.sc.ExternalSalesRepAmount,
                        ExternalSalesRepNetAmount = x.sc.ExternalSalesRepNetAmount,
                        ExternalManagerPartyId = x.sc.ExternalManagerPartyId,
                        ExternalManagerPercent = x.sc.ExternalManagerPercent,
                        ExternalManagerAmount = x.sc.ExternalManagerAmount,
                        ExternalManagerNetAmount = x.sc.ExternalManagerNetAmount,
                        HasVatExemption = x.sc.HasVatExemption,
                        HasWithholdingTaxExemption = x.sc.HasWithholdingTaxExemption,
                        VatPercent = x.sc.VatPercent,
                        WithholdingTaxPercent = x.sc.WithholdingTaxPercent,
                        Notes = x.sc.Notes,
                        CreatedStamp = x.sc.CreatedStamp,
                        LastUpdatedStamp = x.sc.LastUpdatedStamp
                    });

            return Task.FromResult(query);
        }
    }
}

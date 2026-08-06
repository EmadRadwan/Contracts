using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Projects;

public class ListSalesCommissions
{
    public class Query : IRequest<IQueryable<SalesCommissionRecord>>
    {
        public ODataQueryOptions<SalesCommissionRecord>? Options { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<SalesCommissionRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context) => _context = context;

        public Task<IQueryable<SalesCommissionRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Filter by date on the base entity BEFORE projecting. Filtering on the projected
            // SalesCommissionRecord.CommissionDate instead would wrap the whole projection —
            // including the correlated CollectedAmount subquery — in a derived table, which EF Core
            // fails to translate (surfaces only on the Excel export path, which always sends a date
            // range; the paginated grid never sends one).
            var commissions = _context.SalesCommissions.AsQueryable();

            if (request.FromDate.HasValue)
            {
                var from = request.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                commissions = commissions.Where(sc => sc.CommissionDate >= from);
            }
            if (request.ToDate.HasValue)
            {
                var to = request.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                commissions = commissions.Where(sc => sc.CommissionDate <= to);
            }

            var query = (
                from sc in commissions
                join sr in _context.SalesRequests on sc.SalesRequestId equals sr.SalesRequestId
                join p in _context.Products on sr.ProductId equals p.ProductId
                join we in _context.WorkEfforts on sc.ProjectId equals we.WorkEffortId into weGroup
                from we in weGroup.DefaultIfEmpty()
                join salesRepParty in _context.Parties on sc.SalesRepPartyId equals salesRepParty.PartyId into salesRepGroup
                from salesRepParty in salesRepGroup.DefaultIfEmpty()
                join managerParty in _context.Parties on sc.ManagerPartyId equals managerParty.PartyId into managerGroup
                from managerParty in managerGroup.DefaultIfEmpty()
                join salesRep2Party in _context.Parties on sc.SalesRep2PartyId equals salesRep2Party.PartyId into salesRep2Group
                from salesRep2Party in salesRep2Group.DefaultIfEmpty()
                join manager2Party in _context.Parties on sc.Manager2PartyId equals manager2Party.PartyId into manager2Group
                from manager2Party in manager2Group.DefaultIfEmpty()
                join extCompanyParty in _context.Parties on sc.ExternalCompanyPartyId equals extCompanyParty.PartyId into extCompanyGroup
                from extCompanyParty in extCompanyGroup.DefaultIfEmpty()
                join extSalesRepParty in _context.Parties on sc.ExternalSalesRepPartyId equals extSalesRepParty.PartyId into extSalesRepGroup
                from extSalesRepParty in extSalesRepGroup.DefaultIfEmpty()
                join extManagerParty in _context.Parties on sc.ExternalManagerPartyId equals extManagerParty.PartyId into extManagerGroup
                from extManagerParty in extManagerGroup.DefaultIfEmpty()
                select new SalesCommissionRecord
                {
                    SalesCommissionId = sc.SalesCommissionId,
                    SalesRequestId = sc.SalesRequestId,
                    SaleTypeId = sc.SaleTypeId,
                    StatusId = sc.StatusId,
                    CommissionDate = sc.CommissionDate,
                    ProjectId = sc.ProjectId,
                    ProjectName = we != null ? we.ProjectName : null,
                    ApartmentName = p.ProductName,
                    SalePrice = sc.SalePrice,
                    CollectedAmount = _context.Payments
                        .Where(p => p.SalesRequestId == sc.SalesRequestId
                                 && p.Amount > 0
                                 && (p.PaymentTypeId == "RECEIPT_ADVANCE_PAYMENT"
                                     || p.PaymentTypeId == "RECEIPT_DUE_INSTALLMENT")
                                 && p.StatusId == "PMNT_RECEIVED")
                        .Sum(p => (decimal?)p.Amount) ?? 0m,
                    SalesRepPartyId = sc.SalesRepPartyId,
                    SalesRepName = salesRepParty != null ? salesRepParty.Description : null,
                    SalesRepPercent = sc.SalesRepPercent,
                    SalesRepAmount = sc.SalesRepAmount,
                    SalesRepNetAmount = sc.SalesRepNetAmount,
                    ManagerPartyId = sc.ManagerPartyId,
                    ManagerName = managerParty != null ? managerParty.Description : null,
                    ManagerPercent = sc.ManagerPercent,
                    ManagerAmount = sc.ManagerAmount,
                    ManagerNetAmount = sc.ManagerNetAmount,
                    SalesRep2PartyId = sc.SalesRep2PartyId,
                    SalesRep2Name = salesRep2Party != null ? salesRep2Party.Description : null,
                    SalesRep2Percent = sc.SalesRep2Percent,
                    SalesRep2Amount = sc.SalesRep2Amount,
                    SalesRep2NetAmount = sc.SalesRep2NetAmount,
                    Manager2PartyId = sc.Manager2PartyId,
                    Manager2Name = manager2Party != null ? manager2Party.Description : null,
                    Manager2Percent = sc.Manager2Percent,
                    Manager2Amount = sc.Manager2Amount,
                    Manager2NetAmount = sc.Manager2NetAmount,
                    ExternalCompanyPartyId = sc.ExternalCompanyPartyId,
                    ExternalCompanyName = extCompanyParty != null ? extCompanyParty.Description : null,
                    ExternalCompanyPercent = sc.ExternalCompanyPercent,
                    ExternalCompanyGrossAmount = sc.ExternalCompanyGrossAmount,
                    ExternalCompanyNetAmount = sc.ExternalCompanyNetAmount,
                    ExternalSalesRepPartyId = sc.ExternalSalesRepPartyId,
                    ExternalSalesRepName = extSalesRepParty != null ? extSalesRepParty.Description : null,
                    ExternalSalesRepPercent = sc.ExternalSalesRepPercent,
                    ExternalSalesRepAmount = sc.ExternalSalesRepAmount,
                    ExternalSalesRepNetAmount = sc.ExternalSalesRepNetAmount,
                    HasExternalSalesRepWithholdingTaxExemption = sc.HasExternalSalesRepWithholdingTaxExemption,
                    ExternalSalesRepNationalId = sc.ExternalSalesRepNationalId,
                    ExternalManagerPartyId = sc.ExternalManagerPartyId,
                    ExternalManagerName = extManagerParty != null ? extManagerParty.Description : null,
                    ExternalManagerPercent = sc.ExternalManagerPercent,
                    ExternalManagerAmount = sc.ExternalManagerAmount,
                    ExternalManagerNetAmount = sc.ExternalManagerNetAmount,
                    HasExternalManagerWithholdingTaxExemption = sc.HasExternalManagerWithholdingTaxExemption,
                    ExternalManagerNationalId = sc.ExternalManagerNationalId,
                    HasVatExemption = sc.HasVatExemption,
                    HasWithholdingTaxExemption = sc.HasWithholdingTaxExemption,
                    VatPercent = sc.VatPercent,
                    WithholdingTaxPercent = sc.WithholdingTaxPercent,
                    Notes = sc.Notes,
                    CreatedStamp = sc.CreatedStamp,
                    LastUpdatedStamp = sc.LastUpdatedStamp
                }
            ).AsQueryable();

            return Task.FromResult(query);
        }
    }
}

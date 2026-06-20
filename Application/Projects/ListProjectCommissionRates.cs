using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ListProjectCommissionRates
{
    public class Query : IRequest<IQueryable<ProjectCommissionRateRecord>>
    {
        public ODataQueryOptions<ProjectCommissionRateRecord> Options { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, IQueryable<ProjectCommissionRateRecord>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<ProjectCommissionRateRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query =
                from rate in _context.ProjectCommissionRates.AsNoTracking()
                join proj in _context.WorkEfforts.AsNoTracking()
                    on rate.ProjectId equals proj.WorkEffortId into projGroup
                from proj in projGroup.DefaultIfEmpty()
                select new ProjectCommissionRateRecord
                {
                    ProjectCommissionRateId = rate.ProjectCommissionRateId,
                    ProjectId = rate.ProjectId,
                    ProjectName = proj != null ? proj.ProjectName : null,
                    SaleTypeId = rate.SaleTypeId,
                    SalesRepPercent = rate.SalesRepPercent,
                    ManagerPercent = rate.ManagerPercent,
                    ExternalCompanyPercent = rate.ExternalCompanyPercent,
                    ExternalSalesRepPercent = rate.ExternalSalesRepPercent,
                    ExternalManagerPercent = rate.ExternalManagerPercent,
                    LastUpdatedStamp = rate.LastUpdatedStamp,
                    CreatedStamp = rate.CreatedStamp
                };

            return query;
        }
    }
}

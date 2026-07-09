#nullable enable
using Application.Manufacturing;
using AutoMapper;
using FluentValidation.Resources;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Projects;

public class ProjectsList
{
    public class Query : IRequest<IQueryable<WorkEffortRecord>>
    {
        public ODataQueryOptions<WorkEffortRecord> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<WorkEffortRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IQueryable<WorkEffortRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from we in _context.WorkEfforts
                join si in _context.StatusItems 
                    on we.CurrentStatusId equals si.StatusId into statusGroup
                from si in statusGroup.DefaultIfEmpty()

                join ga in _context.GlAccounts 
                    on we.GlAccountId equals ga.GlAccountId into glAccountGroup
                from ga in glAccountGroup.DefaultIfEmpty()

                // Second join for Operating Expense
                join oega in _context.GlAccounts 
                    on we.OperatingExpenseGlAccountId equals oega.GlAccountId into operatingExpenseGroup
                from oega in operatingExpenseGroup.DefaultIfEmpty()

                where we.WorkEffortTypeId == "PROJECT"

                select new WorkEffortRecord
                {
                    WorkEffortId = we.WorkEffortId,
                    ProjectName = we.ProjectName,
                    CurrentStatusId = we.CurrentStatusId,
                    CurrentStatusDescription = si.Description,
                    EstimatedStartDate = we.EstimatedStartDate,
                    EstimatedCompletionDate = we.EstimatedCompletionDate,

                    GlAccountId = we.GlAccountId,
                    OperatingExpenseGlAccountId = we.OperatingExpenseGlAccountId,
                    FacilityId = we.FacilityId,

                    GlAccountName = ga != null 
                        ? $"{(request.Language == "ar" ? ga.AccountNameArabic : ga.AccountName)} - ({ga.AccountCode})" 
                        : null,

                    OperatingExpenseGlAccountName = oega != null 
                        ? $"{(request.Language == "ar" ? oega.AccountNameArabic : oega.AccountName)} - ({oega.AccountCode})" 
                        : null
                };

            return query;
        }
    }
}
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.OrganizationGlSettings;

public class ListCustomTimePeriods
{
    public class Query : IRequest<IQueryable<CustomTimePeriodRecord>>
    {
        public ODataQueryOptions<CustomTimePeriodRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<CustomTimePeriodRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public Task<IQueryable<CustomTimePeriodRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var customTimePeriods = _context.CustomTimePeriods
                .OrderByDescending(x => x.FromDate)
                .Select(x => new CustomTimePeriodRecord
                {
                    CustomTimePeriodId = x.CustomTimePeriodId,
                    ParentPeriodId = x.ParentPeriodId,
                    PeriodTypeId = x.PeriodTypeId,
                    PeriodTypeDescription = x.PeriodType.Description,
                    PeriodNum = x.PeriodNum,
                    PeriodName = x.PeriodName,
                    FromDate = x.FromDate,
                    ThruDate = x.ThruDate,
                    IsClosed = x.IsClosed
                });

            return Task.FromResult(customTimePeriods);
        }
    }
}
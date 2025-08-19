using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.FixedAssets;

public class ListFixedAssets
{
    public class Query : IRequest<IQueryable<FixedAssetRecord>>
    {
        public ODataQueryOptions<FixedAssetRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FixedAssetRecord>>
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

        public async Task<IQueryable<FixedAssetRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from fa in _context.FixedAssets
                    join fat in _context.FixedAssetTypes on fa.FixedAssetTypeId equals fat.FixedAssetTypeId
                    select new FixedAssetRecord
                    {
                        FixedAssetId = fa.FixedAssetId,
                        FixedAssetTypeId = fa.FixedAssetTypeId,
                        FixedAssetTypeDescription = fat.Description,
                        PartyId = fa.PartyId,
                        FixedAssetName = fa.FixedAssetName,
                        DateAcquired = fa.DateAcquired,
                        DateLastServiced = fa.DateLastServiced,
                        DateNextService = fa.DateNextService,
                        ExpectedEndOfLife = fa.ExpectedEndOfLife,
                        ActualEndOfLife = fa.ActualEndOfLife,
                        ProductionCapacity = fa.ProductionCapacity,
                        UomId = fa.UomId,
                        SerialNumber = fa.SerialNumber,
                        LocatedAtFacilityId = fa.LocatedAtFacilityId,
                        LocatedAtLocationSeqId = fa.LocatedAtLocationSeqId,
                        SalvageValue = fa.SalvageValue,
                        Depreciation = fa.Depreciation,
                        PurchaseCost = fa.PurchaseCost,
                        PurchaseCostUomId = fa.PurchaseCostUomId
                    }
                ).AsQueryable();

            return query;
        }
    }
}
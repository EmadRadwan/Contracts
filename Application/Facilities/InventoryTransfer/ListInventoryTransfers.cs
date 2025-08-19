using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Facilities.InventoryTransfer;

public class ListInventoryTransfers
{
    public class Query : IRequest<IQueryable<InventoryTransferRecord>>
    {
        public ODataQueryOptions<InventoryTransferRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<InventoryTransferRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IQueryable<InventoryTransferRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from ifr in _context.InventoryTransfers
                join invi in _context.InventoryItems on ifr.InventoryItemId equals invi.InventoryItemId
                join prd in _context.Products on invi.ProductId equals prd.ProductId
                join f in _context.Facilities on ifr.FacilityId equals f.FacilityId
                join f2 in _context.Facilities on ifr.FacilityIdTo equals f2.FacilityId
                join stsi in _context.StatusItems on ifr.StatusId equals stsi.StatusId
                select new InventoryTransferRecord
                {
                    InventoryTransferId = ifr.InventoryTransferId,
                    StatusId = stsi.Description,
                    InventoryItemId = ifr.InventoryItemId,
                    ProductId = prd.ProductId,
                    ProductName = prd.ProductName,
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    FacilityIdTo = f2.FacilityId,
                    FacilityToName = f2.FacilityName,
                    SendDate = ifr.SendDate,
                    ReceiveDate = ifr.ReceiveDate,
                    Comments = ifr.Comments
                }).AsQueryable();

            return query;
        }
    }
}
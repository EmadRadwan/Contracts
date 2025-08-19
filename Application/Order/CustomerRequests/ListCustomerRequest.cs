using Application.Catalog.Products;
using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.CustomerRequests;

public class ListCustomerRequest
{
    public class Query : IRequest<Result<CustRequestDto2>>
    {
        public string CustomerRequestId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<CustRequestDto2>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<CustRequestDto2>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from rqst in _context.CustRequests
                join pty in _context.Parties on rqst.FromPartyId equals pty.PartyId
                join crt in _context.CustRequestTypes on rqst.CustRequestTypeId equals crt.CustRequestTypeId
                join sts in _context.StatusItems on rqst.StatusId equals sts.StatusId
                //join enm in _context.Enumerations on rqst.SalesChannelEnumId equals enm.EnumId
                join pcm in _context.PartyContactMeches on pty.PartyId equals pcm.PartyId
                join cm in _context.ContactMeches on pcm.ContactMechId equals cm.ContactMechId
                join tn in _context.TelecomNumbers on cm.ContactMechId equals tn.ContactMechId
                join pcmp in _context.PartyContactMechPurposes on new { pcm.PartyId, pcm.ContactMechId } equals
                    new { pcmp.PartyId, pcmp.ContactMechId }
                join cmpt in _context.ContactMechPurposeTypes on pcmp.ContactMechPurposeTypeId equals cmpt
                    .ContactMechPurposeTypeId
                where pcmp.ContactMechPurposeTypeId == "PRIMARY_PHONE"
                      && rqst.CustRequestId == request.CustomerRequestId
                select new CustRequestDto2
                {
                    CustRequestId = rqst.CustRequestId,
                    FromPartyId = new CustRequestPartyDto
                    {
                        FromPartyId = pty.PartyId,
                        FromPartyName = pty.Description
                    },
                    CustRequestDate = DateTime.SpecifyKind(rqst.CustRequestDate.Truncate(TimeSpan.FromSeconds(1)),
                        DateTimeKind.Utc),
                    AllowSubmit = false
                };


            var results = query.ToList();

            var requestItems = (from itm in _context.CustRequestItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                where itm.CustRequestId == request.CustomerRequestId
                select new CustRequestItemDto
                {
                    CustRequestId = itm.CustRequestId,
                    CustRequestItemSeqId = itm.CustRequestItemSeqId,
                    ProductId = new ProductLovDto
                    {
                        ProductId = itm.ProductId,
                        ProductName = prd.ProductName,
                        FacilityName = fac.FacilityName,
                        InventoryItem = inv.InventoryItemId,
                        QuantityOnHandTotal = inv.QuantityOnHandTotal,
                        AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                        Price = prc.PriceWithTax
                    },
                    ProductName = prd.ProductName,
                    Quantity = itm.Quantity,
                    IsProductDeleted = false
                }).ToList();

            var requestToReturn = new CustRequestDto2();

            if (results.Any())
            {
                requestToReturn = results[0];
                if (requestItems.Any()) requestToReturn.CustRequestItems = requestItems;
            }

            return Result<CustRequestDto2>.Success(requestToReturn);
        }
    }
}
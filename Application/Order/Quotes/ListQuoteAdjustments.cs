using AutoMapper;
using MediatR;
using Persistence;

namespace Application.Order.Quotes;

public class ListQuoteAdjustments
{
    public class Query : IRequest<Result<List<QuoteAdjustmentDto2>>>
    {
        public string QuoteId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<QuoteAdjustmentDto2>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<QuoteAdjustmentDto2>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var quoteAdjustments = (from qoti in _context.QuoteItems
                join qadj in _context.QuoteAdjustments on new { qoti.QuoteId, qoti.QuoteItemSeqId } equals
                    new { qadj.QuoteId, qadj.QuoteItemSeqId }
                join oadjt in _context.OrderAdjustmentTypes on qadj.QuoteAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                join prd in _context.Products on qadj.CorrespondingProductId equals prd.ProductId
                where qoti.QuoteId == request.QuoteId
                select new QuoteAdjustmentDto2
                {
                    QuoteAdjustmentId = qadj.QuoteAdjustmentId,
                    QuoteId = qoti.QuoteId,
                    QuoteItemSeqId = qoti.QuoteItemSeqId,
                    QuoteAdjustmentTypeId = qadj.QuoteAdjustmentTypeId,
                    QuoteAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = qadj.CorrespondingProductId,
                    CorrespondingProductName = prd.ProductName,
                    Amount = qadj.Amount,
                    IsManual = qadj.IsManual,
                    IsAdjustmentDeleted = false,
                    SourcePercentage = qadj.SourcePercentage,
                    ProductPromoId = qadj.ProductPromoId,
                    Description = qadj.Description,
                    TaxAuthGeoId = qadj.TaxAuthGeoId,
                    TaxAuthPartyId = qadj.TaxAuthPartyId
                }).ToList();

            var quoteAdjustments2 = (from ord in _context.Quotes
                join qadj in _context.QuoteAdjustments on ord.QuoteId equals qadj.QuoteId
                join oadjt in _context.OrderAdjustmentTypes on qadj.QuoteAdjustmentTypeId equals oadjt
                    .OrderAdjustmentTypeId
                where ord.QuoteId == request.QuoteId && qadj.QuoteItemSeqId == "_NA_"
                select new QuoteAdjustmentDto2
                {
                    QuoteAdjustmentId = qadj.QuoteAdjustmentId,
                    QuoteId = ord.QuoteId,
                    QuoteItemSeqId = "_NA_",
                    QuoteAdjustmentTypeId = qadj.QuoteAdjustmentTypeId,
                    QuoteAdjustmentTypeDescription = oadjt.Description,
                    CorrespondingProductId = qadj.CorrespondingProductId,
                    CorrespondingProductName = null,
                    Amount = qadj.Amount,
                    IsAdjustmentDeleted = false,
                    SourcePercentage = qadj.SourcePercentage,
                    ProductPromoId = qadj.ProductPromoId,
                    Description = qadj.Description,
                    TaxAuthGeoId = qadj.TaxAuthGeoId,
                    TaxAuthPartyId = qadj.TaxAuthPartyId
                }).ToList();


            var quoteToReturn = quoteAdjustments.Union(quoteAdjustments2).ToList();

            return Result<List<QuoteAdjustmentDto2>>.Success(quoteToReturn);
        }
    }
}
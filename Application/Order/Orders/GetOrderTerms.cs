using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class GetOrderTerms
{
    public class Query : IRequest<Result<List<OrderTermDto>>>
    {
        public string OrderId { get; set; }
        public string Langauge { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderTermDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<OrderTermDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Langauge;
            var result = await (from ordt in _context.OrderTerms
                .Where(o => o.OrderId == request.OrderId)
                join tt in _context.TermTypes on ordt.TermTypeId equals tt.TermTypeId
                select new OrderTermDto
                {
                    OrderId = request.OrderId,
                    TermTypeId = ordt.TermTypeId,
                    TermTypeName = language == "ar" ? tt.DescriptionArabic : language == "tr" ? tt.DescriptionTurkish : tt.Description,
                    TermDays = ordt.TermDays,
                    TermValue = ordt.TermValue,
                    TextValue = ordt.TextValue,
                    isNewTerm = "N",
                    Description = language == "ar" ? ordt.DescriptionArabic : language == "tr" ? ordt.DescriptionTurkish : ordt.Description 
                }).ToListAsync();

            return Result<List<OrderTermDto>>.Success(result);
        }
    }
}
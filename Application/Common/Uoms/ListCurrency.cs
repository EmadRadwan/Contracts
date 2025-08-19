using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Uoms;

public class ListCurrency
{
    public class Query : IRequest<Result<List<CurrencyDto>>>
    {
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<CurrencyDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CurrencyDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;
            var query = _context.Uoms
                .Where(z => z.UomTypeId == "CURRENCY_MEASURE")
                .Select(u => new CurrencyDto
                {
                    CurrencyUomId = u.UomId,
                    Description = language == "ar" ? u.DescriptionArabic : language == "tr" ? u.DescriptionTurkish :  u.Description
                })
                .OrderBy(x => x.Description)
                .AsQueryable();


            var currencies = await query
                .ToListAsync();

            return Result<List<CurrencyDto>>.Success(currencies);
        }
    }
}
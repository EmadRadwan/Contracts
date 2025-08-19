using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Common.Uoms;

public class ListConversionDated
{
    public class Query : IRequest<Result<List<UomConversionDatedDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<UomConversionDatedDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<UomConversionDatedDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.UomConversionDateds
                .Select(u => new UomConversionDatedDto
                {
                    UomId = u.UomId,
                    UomIdTo = u.UomIdTo,
                    FromDate = u.FromDate,
                    ThruDate = u.ThruDate,
                    ConversionFactor = u.ConversionFactor,
                    CustomMethodId = u.CustomMethodId,
                    DecimalScale = u.DecimalScale,
                    RoundingMode = u.RoundingMode
                })
                .OrderBy(x => x.UomId)
                .AsQueryable();


            var uomConversionDateds = await query
                .ToListAsync();

            return Result<List<UomConversionDatedDto>>.Success(uomConversionDateds);
        }
    }
}
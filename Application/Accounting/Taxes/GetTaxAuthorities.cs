using Domain;
using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.Taxes;

public class GetTaxAuthorities
{
    public class Query : IRequest<Result<List<TaxAuthDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<TaxAuthDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<TaxAuthDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try 
            {
                var taxAuthorities = await (from tax in _context.TaxAuthorities
                    join pty in _context.Parties on tax.TaxAuthPartyId equals pty.PartyId
                    select new TaxAuthDto
                    {
                        TaxAuthPartyId = tax.TaxAuthPartyId,
                        TaxAuthPartyName = pty.Description,
                    }).ToListAsync(cancellationToken);

                return Result<List<TaxAuthDto>>.Success(taxAuthorities);
            }
            catch (Exception ex)
            {
                return Result<List<TaxAuthDto>>.Failure(ex.Message);
            }
        }
    }
}
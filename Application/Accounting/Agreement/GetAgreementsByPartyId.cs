using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Agreement;

public class GetAgreementsByPartyId
{
    public class Query: IRequest<Result<List<AgreementLovDto>>>
    {
        public string PartyId { get; set; }
        public string OrderType { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AgreementLovDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AgreementLovDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = new List<AgreementLovDto>();
            if (request.OrderType == "PURCHASE_ORDER")
            {
                query = await (from ag in _context.Agreements
                .Where(a => a.PartyIdTo == request.PartyId)
                select new AgreementLovDto
                {
                    AgreementId = ag.AgreementId,
                    Description = ag.Description
                }
                ).ToListAsync(cancellationToken);
            } else {
                query = await (from ag in _context.Agreements
                .Where(a => a.PartyIdFrom == request.PartyId)
                select new AgreementLovDto
                {
                    AgreementId = ag.AgreementId,
                    Description = ag.Description
                }
                ).ToListAsync(cancellationToken);
            }
            return Result<List<AgreementLovDto>>.Success(query);
        }
    }
}
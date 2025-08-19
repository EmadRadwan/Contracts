using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Agreement;

public class ListAgreementTypesLov
{
    public class Query : IRequest<Result<List<AgreementTypeLovDto>>>
    {
        
    }

    public class Handler : IRequestHandler<Query, Result<List<AgreementTypeLovDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<AgreementTypeLovDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var agreementTypes = await (from at in _context.AgreementTypes
                    
                    select new AgreementTypeLovDto
                    {
                        AgreementTypeId = at.AgreementTypeId,
                        Description = at.Description
                    }
                ).ToListAsync(cancellationToken);

            return Result<List<AgreementTypeLovDto>>.Success(agreementTypes);
        }
    }
}
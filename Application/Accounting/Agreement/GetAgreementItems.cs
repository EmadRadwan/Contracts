using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Agreement;

public class GetAgreementItems
{
    public class Query: IRequest<Result<List<AgreementItemDto>>>
    {
        public string AgreementId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AgreementItemDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AgreementItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var agreementItems = await (from ai in _context.AgreementItems 
            .Where(ai => ai.AgreementId == request.AgreementId)
            join ait in _context.AgreementItemTypes on ai.AgreementItemTypeId equals ait.AgreementItemTypeId
            join uoms in _context.Uoms on ai.CurrencyUomId equals uoms.UomId
            select new AgreementItemDto
            {
                AgreementId = ai.AgreementId,
                AgreementItemSeqId = ai.AgreementItemSeqId,
                AgreementItemTypeId = ai.AgreementItemTypeId,
                AgreementItemTypeDescription = ait.Description,
                CurrencyUomId = ai.CurrencyUomId,
                CurrencyUomDescription = uoms.Description
            }).ToListAsync(cancellationToken);

            return Result<List<AgreementItemDto>>.Success(agreementItems);
        }
    }
}
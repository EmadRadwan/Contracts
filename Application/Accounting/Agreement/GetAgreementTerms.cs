using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Agreement;

public class GetAgreementTerms
{
    public class Query: IRequest<Result<List<AgreementTermDto>>>
    {
        public string AgreementId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<AgreementTermDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AgreementTermDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var agreementTerms = await (from at in _context.AgreementTerms 
                    .Where(at => at.AgreementId == request.AgreementId)
                join tt in _context.TermTypes on at.TermTypeId equals tt.TermTypeId
                join iit in _context.InvoiceItemTypes on at.InvoiceItemTypeId equals iit.InvoiceItemTypeId into iitGroup
                from iit in iitGroup.DefaultIfEmpty() 
                select new AgreementTermDto
                {
                    AgreementId = at.AgreementId,
                    AgreementItemSeqId = at.AgreementItemSeqId,
                    AgreementTermId = at.AgreementTermId,
                    Description = at.Description,
                    InvoiceItemTypeId = at.InvoiceItemTypeId,
                    InvoiceItemTypeDescription = iit != null ? iit.Description : null, 
                    FromDate = at.FromDate,
                    ThruDate = at.ThruDate,
                    TermTypeId = at.TermTypeId,
                    TermTypeDescription = tt.Description,
                    TermDays = at.TermDays,
                }).ToListAsync(cancellationToken);

            return Result<List<AgreementTermDto>>.Success(agreementTerms);
        }
    }
}
using Application.Shipments.InvoiceItemTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.InvoiceItemTypeDtos;

public class List
{
    public class Query : IRequest<Result<List<InvoiceItemTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<InvoiceItemTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<InvoiceItemTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.InvoiceItemTypes
                .OrderBy(x => x.Description)
                .Select(x => new InvoiceItemTypeDto
                    {
                        InvoiceItemTypeId = x.InvoiceItemTypeId,
                        ParentTypeId = x.ParentTypeId,
                        Description = x.Description
                    }
                )
                .AsQueryable();


            var invoiceItemTypeDtos = await query
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<InvoiceItemTypeDto>>.Success(invoiceItemTypeDtos);
        }
    }
}
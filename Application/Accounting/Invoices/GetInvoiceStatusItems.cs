using MediatR;
using Persistence;
using AutoMapper;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Accounting.Services;



using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Accounting.Invoices
{
    public class GetInvoiceStatusItems
    {
        public class Query : IRequest<Result<List<StatusItemDto>>>
        {
        }

        public class Handler : IRequestHandler<Query, Result<List<StatusItemDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IInvoiceService _invoiceService;

            public Handler(DataContext context, IMapper mapper, IInvoiceService invoiceService)
            {
                _mapper = mapper;
                _context = context;
                _invoiceService = invoiceService;
            }

            public async Task<Result<List<StatusItemDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var statusItems = await _context.StatusItems
                .Where(si => si.StatusTypeId == "INVOICE_STATUS")
                .Select(x =>
                    new StatusItemDto
                    {
                        StatusItemId = x.StatusId,
                        Description = x.Description
                    }
                ).ToListAsync(cancellationToken);
                statusItems.Insert(0, new StatusItemDto
                {
                    StatusItemId = string.Empty,
                    Description = "All"
                });

                return Result<List<StatusItemDto>>.Success(statusItems);
            }
        }
    }
}

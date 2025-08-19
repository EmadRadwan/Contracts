using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Shipments.PaymentMethodTypes;

public class ListPaymentMethodTypes
{
    public class Query : IRequest<IQueryable<PaymentMethodTypeRecord>>
    {
        public ODataQueryOptions<PaymentMethodTypeRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<PaymentMethodTypeRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<IQueryable<PaymentMethodTypeRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = (from paymentMethodType in _context.PaymentMethodTypes
                select new PaymentMethodTypeRecord
                {
                    PaymentMethodTypeId = paymentMethodType.PaymentMethodTypeId,
                    Description = paymentMethodType.Description,
                    DefaultGlAccountId = paymentMethodType.DefaultGlAccountId,
                    LastUpdatedStamp = paymentMethodType.LastUpdatedStamp,
                    LastUpdatedTxStamp = paymentMethodType.LastUpdatedTxStamp,
                    CreatedStamp = paymentMethodType.CreatedStamp,
                    CreatedTxStamp = paymentMethodType.CreatedTxStamp
                }).AsQueryable();

            return query;
        }
    }
}
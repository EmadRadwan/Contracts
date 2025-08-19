using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Order.Orders.Orders;

public class ListOrders
{
    public class Query : IRequest<IQueryable<OrderRecord>>
    {
        public ODataQueryOptions<OrderRecord> Options { get; set; }
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, IQueryable<OrderRecord>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }


        public async Task<IQueryable<OrderRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;
            var query = from ov in _context.OrderView.AsNoTracking()
                select new OrderRecord
                {
                    OrderId = ov.OrderId,
                    FromPartyName = ov.FromPartyName + " ( " + ov.FromPartyContactNumber + " )",
                    OrderDate = ov.OrderDate,
                    StatusId = ov.OrderStatus,
                    StatusDescription = language == "ar" 
                        ? ov.StatusDescriptionArabic 
                        : language == "tr" 
                            ? ov.StatusDescriptionTurkish 
                            : ov.StatusDescription,
                    BillingAccountId = ov.BillingAccountId,
                    PaymentMethodId = ov.PaymentMethodId,
                    PaymentMethodTypeId = ov.PaymentMethodTypeId,
                    AgreementId = ov.AgreementId,
                    PaymentId = ov.PaymentId,
                    InvoiceId = ov.InvoiceId,
                    GrandTotal = ov.GrandTotal,
                    OrderTypeId = ov.OrderTypeId,
                    OrderTypeDescription = language == "ar" 
                        ? ov.OrderTypeDescriptionArabic 
                        : language == "tr" 
                            ? ov.OrderTypeDescriptionTurkish 
                            : ov.OrderTypeDescription,
                    CurrencyUomId = ov.CurrencyUomId,
                    CurrencyUomDescription = language == "ar" 
                        ? ov.CurrencyUomDescriptionArabic 
                        : language == "tr" 
                            ? ov.CurrencyUomDescriptionTurkish 
                            : ov.CurrencyUomDescription,
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = ov.FromPartyId,
                        FromPartyName = ov.FromPartyNameDescription
                    }
                };


            return query;
        }
    }
}